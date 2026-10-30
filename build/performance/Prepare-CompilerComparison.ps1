#Requires -Version 7.0
<#
.SYNOPSIS
Prepares an isolated, same-runner .NET 10 compiler before/after comparison.
.DESCRIPTION
The historical comparison is a separate experiment. This helper requires two
explicit, clean source checkouts and audits the focused compiler product paths.
It publishes both products, uses one byte-identical driver, and conditions every
selected workload through compare.py before freezing the final manifest.
Conditioning observations are retained separately from the measured process pairs.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$BaselineSourceDirectory,
    [Parameter(Mandatory)][string]$CandidateSourceDirectory,
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{40}$')][string]$ExpectedCandidateCommit,
    [Parameter(Mandatory)][string]$OutputDirectory
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
if (!$IsWindows) { throw 'This diagnostic measures the Windows .NET 10 product and requires Windows.' }

$baselineCommit = 'fe1aba49b5f9538355ffaaa3d1683f69a23d2d9c'
$ExpectedCandidateCommit = $ExpectedCandidateCommit.ToLowerInvariant()
# A harness-only commit may establish current attribution before product edits.
$productPaths = @(
    'Computator.NET.Core/Compilation/NativeCompiler.cs',
    'Computator.NET.Core/Compilation/BuiltinFacadePolicy.cs',
    'Computator.NET.Core/Evaluation/ExpressionsEvaluator.cs',
    'Computator.NET.Core/Evaluation/ScriptEvaluator.cs',
    'build/PrecompiledTslBuiltins.targets',
    'build/GeneratePrecompiledTslBuiltins.cs'
)
$packagePinsPath = 'Directory.Packages.props'
$git = (Get-Command git -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
$dotnet = (Get-Command dotnet -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
$python = (Get-Command python -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
$baselineSource = [IO.Path]::GetFullPath($BaselineSourceDirectory)
$candidateSource = [IO.Path]::GetFullPath($CandidateSourceDirectory)
$output = [IO.Path]::GetFullPath($OutputDirectory)
if ($baselineSource -eq $candidateSource) { throw 'Baseline and candidate must use different source checkouts.' }

function Invoke-Text([string]$Executable, [string[]]$Arguments, [string]$Directory) {
    Push-Location -LiteralPath $Directory
    try {
        $lines = @(& $Executable @Arguments)
        if ($LASTEXITCODE -ne 0) { throw "$([IO.Path]::GetFileName($Executable)) failed ($LASTEXITCODE): $($Arguments -join ' ')" }
        $lines
    } finally { Pop-Location }
}

function Get-SHA256([string]$Path) {
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Write-Json([object]$Value, [string]$Path) {
    $Value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Assert-CleanCheckout([string]$Directory, [string]$ExpectedCommit) {
    if (!(Test-Path -LiteralPath $Directory -PathType Container)) { throw "Missing source checkout: $Directory" }
    $top = [IO.Path]::GetFullPath((Invoke-Text $git @('rev-parse', '--show-toplevel') $Directory))
    if ($top.TrimEnd('\', '/') -ne $Directory.TrimEnd('\', '/')) { throw "Supply the checkout root, not a subdirectory: $Directory" }
    $actual = (Invoke-Text $git @('rev-parse', 'HEAD') $Directory).Trim()
    if ($actual -ne $ExpectedCommit) { throw "Commit mismatch at ${Directory}: expected $ExpectedCommit, found $actual" }
    $changes = @(Invoke-Text $git @('status', '--porcelain=v1', '--untracked-files=all', '--ignore-submodules=none') $Directory)
    if ($changes.Count) { throw "Source checkout must be clean, including nonignored untracked files: $Directory" }
    $unchecked = @(Invoke-Text $git @('ls-files', '-v') $Directory | Where-Object { $_ -cmatch '^[a-zS] ' })
    if ($unchecked.Count) { throw "Sparse/skip-worktree or assume-unchanged paths cannot establish source identity: $Directory" }
    $indirect = @(Invoke-Text $git @('ls-files', '--stage') $Directory | Where-Object { $_ -match '^(120000|160000) ' })
    if ($indirect.Count) { throw "This diagnostic requires ordinary tracked files, without symlinks or submodules: $Directory" }
}

function Get-SourceSnapshot([string]$Directory) {
    $snapshot = [ordered]@{}
    foreach ($relative in @(Invoke-Text $git @('-c', 'core.quotepath=false', 'ls-files') $Directory)) {
        $path = Join-Path $Directory $relative
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Tracked source is missing: $path" }
        if ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Tracked source uses a filesystem redirect: $path" }
        $snapshot[$relative] = Get-SHA256 $path
    }
    $snapshot
}

function Test-AuxiliaryChange([string]$Path) {
    # Test projects are outside the worker's project graph. Shared driver/build
    # inputs are immutable and checked below even when a harness path is allowed.
    if ($Path -match '^Computator\.NET[^/]*Tests/') { return $true }
    if ($Path -match '^(docs/|\.github/)' -or $Path -match '^[^/]+\.(md|slnx)$') { return $true }
    if ($Path -in @('CHANGELOG', 'LICENSE', 'LICENSE.txt', '.gitignore', '.travis.yml', 'appveyor.yml')) { return $true }
    if ($Path -in @('build/performance/DesktopStartup.cs', 'build/performance/CompilationDiagnostics.cs.txt')) { return $true }
    if ($Path -match '^build/performance/diagnostics/translation/') {
        # Retained standalone probe/evidence does not enter Worker.csproj's
        # explicit Compile items. Permit inert project templates, never active
        # build imports, SDK selectors or package-source configuration.
        if ($Path -match '(^|/)(global\.json|nuget\.config)$' -or $Path -match '\.(csproj|props|targets|slnx)$') { return $false }
        return $Path -match '\.(cs|json|py|ps1|sh|md|txt|csv|sha256|csproj\.template)$'
    }
    if ($Path -match '^build/performance/(CompilerComparison.*\.(cs|csproj)|compiler_comparison.*\.py|test_compiler_comparison\.py)$') { return $true }
    if ($Path -match '^build/.*\.(ps1|py|sh|md|yml|yaml)$') { return $true }
    return $false
}

Assert-CleanCheckout $baselineSource $baselineCommit
Assert-CleanCheckout $candidateSource $ExpectedCandidateCommit
if ([IO.Path]::GetFullPath($PSCommandPath) -ne (Join-Path $candidateSource 'build/performance/Prepare-CompilerComparison.ps1')) {
    throw 'Run the preparer from the explicitly pinned candidate checkout.'
}
foreach ($source in @($baselineSource, $candidateSource)) {
    $relative = [IO.Path]::GetRelativePath($source, $output)
    if ($relative -eq '.' -or $source.StartsWith($output.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'OutputDirectory cannot be a source checkout or contain one.'
    }
    if (!$relative.StartsWith('..' + [IO.Path]::DirectorySeparatorChar) -and ![IO.Path]::IsPathRooted($relative)) {
        & $git -C $source check-ignore --quiet -- ($relative.TrimEnd('\', '/') + '/')
        if ($LASTEXITCODE -ne 0) { throw 'An output directory inside a source checkout must be ignored by Git.' }
    }
}
if ((Test-Path -LiteralPath $output) -and @(Get-ChildItem -LiteralPath $output -Force).Count) { throw 'OutputDirectory must be empty.' }
$null = New-Item -ItemType Directory -Path $output -Force
$audit = Join-Path $output 'audit'
$logs = Join-Path $output 'preparation-logs'
$null = New-Item -ItemType Directory -Path $audit, $logs
$beforeFiles = Get-SourceSnapshot $baselineSource
$afterFiles = Get-SourceSnapshot $candidateSource
# This shared build file may only replace Roslyn's existing 5.9.0 literal with
# the same unconditional property used by the build-time helper generator.
# Normalize precisely that refactor, compare every other XML declaration, then
# confirm the version consumed by MSBuild in each actual product project.
$pinAuditText = (Invoke-Text $python @((Join-Path $candidateSource 'build/performance/compiler_comparison_manifest.py'),
    '--baseline-packages', (Join-Path $baselineSource $packagePinsPath),
    '--candidate-packages', (Join-Path $candidateSource $packagePinsPath)) $candidateSource) -join [Environment]::NewLine
$packagePinAudit = $pinAuditText | ConvertFrom-Json -AsHashtable
foreach ($entry in @(@{ label = 'baseline'; source = $baselineSource }, @{ label = 'candidate'; source = $candidateSource })) {
    $evaluationText = (Invoke-Text $dotnet @('msbuild', 'Computator.NET.Core/Computator.NET.Core.csproj', '-nologo', '-getItem:PackageVersion') $entry.source) -join [Environment]::NewLine
    $evaluation = $evaluationText | ConvertFrom-Json -AsHashtable
    $roslyn = @($evaluation.Items.PackageVersion | Where-Object { $_.Identity -eq 'Microsoft.CodeAnalysis.CSharp' })
    if ($roslyn.Count -ne 1 -or $roslyn[0].Version -ne '5.9.0') { throw "Evaluated Roslyn package version changed: $($entry.label)" }
    $packagePinAudit[$entry.label + 'EvaluatedRoslynVersion'] = $roslyn[0].Version
}
$packagePinAudit.baselineSHA256 = $beforeFiles[$packagePinsPath]
$packagePinAudit.candidateSHA256 = $afterFiles[$packagePinsPath]
Write-Json $packagePinAudit (Join-Path $audit 'package-pins.json')
$allPaths = @(@($beforeFiles.Keys) + @($afterFiles.Keys) | Sort-Object -Unique)
$changed = @()
foreach ($relative in $allPaths) {
    $beforeHash = if ($beforeFiles.Contains($relative)) { $beforeFiles[$relative] } else { $null }
    $afterHash = if ($afterFiles.Contains($relative)) { $afterFiles[$relative] } else { $null }
    if ($beforeHash -eq $afterHash) { continue }
    if ($relative -notin $productPaths -and $relative -ne $packagePinsPath -and !(Test-AuxiliaryChange $relative)) {
        throw "Unrelated product/build input changed: $relative. Only the explicitly listed compiler product paths and auxiliary test/docs/harness files are permitted."
    }
    $changed += @{ path = $relative; beforeSHA256 = $beforeHash; afterSHA256 = $afterHash;
        productChange = ($relative -in $productPaths); validatedPackagePinRefactor = ($relative -eq $packagePinsPath) }
}
# Zero product changes are explicitly reported for attribution-only preparations.
$sharedInputs = @(
    'build/performance/Worker.cs', 'build/performance/Worker.csproj', 'build/performance/DirectExpressionEntryPoint.cs', 'build/performance/DesktopStartup.cs',
    'build/performance/compare.py', 'Directory.Build.props', 'Directory.Build.targets',
    'Version.props', 'SharedAssemblyInfo.cs', 'global.json', 'build/ModernDesktop.props', 'build/WindowsAssemblyInfo.cs'
)
$sharedHashes = [ordered]@{}
foreach ($relative in $sharedInputs) {
    if (!$beforeFiles.Contains($relative) -or !$afterFiles.Contains($relative) -or $beforeFiles[$relative] -ne $afterFiles[$relative]) {
        throw "Driver/shared build input differs or is missing: $relative"
    }
    $sharedHashes[$relative] = $beforeFiles[$relative]
}
foreach ($relative in @('build/performance/CompilerComparison.csproj', 'build/performance/CompilerComparisonEntryPoint.cs')) {
    if (!$afterFiles.Contains($relative)) { throw "Common modern driver source must be tracked: $relative" }
    $sharedHashes[$relative] = $afterFiles[$relative]
}
$startupDriverPath = 'build/performance/DesktopStartup.cs'
if (!$beforeFiles.Contains($startupDriverPath) -or !$afterFiles.Contains($startupDriverPath)) { throw 'The startup driver source must exist in both checkouts.' }
$startupDriverAudit = @{
    beforeSHA256 = $beforeFiles[$startupDriverPath]; afterSHA256 = $afterFiles[$startupDriverPath];
    scope = 'Startup source is unchanged; these compiler workloads do not launch the desktop. Both payloads use the same candidate driver bytes.'
}
Write-Json $beforeFiles (Join-Path $audit 'source-files-before.json')
Write-Json $afterFiles (Join-Path $audit 'source-files-after.json')
foreach ($relative in $productPaths) {
    $name = $relative.Replace('/', '__')
    # A new reviewed generator source has no baseline file; its null before hash
    # in changedFiles records that fact. Never fabricate a baseline counterpart.
    if ($beforeFiles.Contains($relative)) {
        Copy-Item -LiteralPath (Join-Path $baselineSource $relative) -Destination (Join-Path $audit ('before__' + $name))
    }
    if (!$afterFiles.Contains($relative)) { throw "Reviewed product/build source is missing in candidate: $relative" }
    Copy-Item -LiteralPath (Join-Path $candidateSource $relative) -Destination (Join-Path $audit ('after__' + $name))
}
Write-Json @{
    diagnostic = 'current-compiler-before-after'; allowedProductPaths = $productPaths;
    baseline = @{ directory = $baselineSource; commit = $baselineCommit; tree = (Invoke-Text $git @('rev-parse', 'HEAD^{tree}') $baselineSource) };
    candidate = @{ directory = $candidateSource; commit = $ExpectedCandidateCommit; tree = (Invoke-Text $git @('rev-parse', 'HEAD^{tree}') $candidateSource) };
    changedFiles = $changed; commonSourceSHA256 = $sharedHashes; startupDriver = $startupDriverAudit; packagePins = $packagePinAudit;
    scope = 'Windows .NET 10 compiler comparison; does not substitute for an authenticated historical-version comparison.'
} (Join-Path $audit 'source-audit.json')

function Invoke-Logged([string]$Executable, [string[]]$Arguments, [string]$Directory, [string]$LogName) {
    Push-Location -LiteralPath $Directory
    try {
        & $Executable @Arguments 2>&1 | Tee-Object -FilePath (Join-Path $logs $LogName)
        if ($LASTEXITCODE -ne 0) { throw "$([IO.Path]::GetFileName($Executable)) failed ($LASTEXITCODE); see $LogName." }
    } finally { Pop-Location }
}

$sdkBefore = (Invoke-Text $dotnet @('--version') $baselineSource).Trim()
$sdkAfter = (Invoke-Text $dotnet @('--version') $candidateSource).Trim()
if ($sdkBefore -ne $sdkAfter -or $sdkBefore -notmatch '^10\.0\.') { throw 'Both source checkouts must resolve the same .NET 10 SDK.' }
$installedRuntimes = @(Invoke-Text $dotnet @('--list-runtimes') $candidateSource)
$payloads = @{ baseline = (Join-Path $output 'baseline'); candidate = (Join-Path $output 'candidate') }
$profiles = @{ baseline = (Join-Path $output 'baseline-profile'); candidate = (Join-Path $output 'candidate-profile') }
$buildCommands = @()
$oldBuildEnvironment = @{}
foreach ($key in @('MSBUILDDISABLENODEREUSE', 'DOTNET_CLI_TELEMETRY_OPTOUT', 'DOTNET_NOLOGO')) {
    $oldBuildEnvironment[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
    [Environment]::SetEnvironmentVariable($key, '1', 'Process')
}
try {
    foreach ($entry in @(@{ name = 'baseline'; source = $baselineSource }, @{ name = 'candidate'; source = $candidateSource })) {
        $arguments = @('publish', (Join-Path $candidateSource 'build/performance/CompilerComparison.csproj'), '-c', 'Release', '-r', 'win-x64',
            '--self-contained', 'false', '-p:PublishReadyToRun=true', ('-p:ProductSourceDirectory=' + $entry.source), '--artifacts-path', (Join-Path $output ('build-' + $entry.name)), '-o', $payloads[$entry.name],
            '--nologo', '-v:minimal', '-m:1', '-p:UseSharedCompilation=false')
        $buildCommands += @{ label = $entry.name; executable = $dotnet; arguments = $arguments; cwd = $entry.source; sdk = $sdkBefore }
        Write-Json $buildCommands (Join-Path $audit 'build-commands.json')
        Invoke-Logged $dotnet $arguments $entry.source ($entry.name + '-publish.txt')
        $null = New-Item -ItemType Directory -Path $profiles[$entry.name]
    }
} finally {
    try { Invoke-Logged $dotnet @('build-server', 'shutdown') $candidateSource 'build-server-shutdown.txt' }
    finally {
        foreach ($key in $oldBuildEnvironment.Keys) { [Environment]::SetEnvironmentVariable($key, $oldBuildEnvironment[$key], 'Process') }
    }
}
Assert-CleanCheckout $baselineSource $baselineCommit
Assert-CleanCheckout $candidateSource $ExpectedCandidateCommit
foreach ($entry in @(@{ source = $baselineSource; snapshot = $beforeFiles }, @{ source = $candidateSource; snapshot = $afterFiles })) {
    $now = Get-SourceSnapshot $entry.source
    if ($now.Count -ne $entry.snapshot.Count) { throw 'Tracked source inventory changed during preparation.' }
    foreach ($relative in $entry.snapshot.Keys) {
        if ($now[$relative] -ne $entry.snapshot[$relative]) { throw "Source changed during preparation: $relative" }
    }
}

# Dependency and runtime policy must agree before using a common driver binary.
# Every product assembly remains from its own publish, including the Engine
# assembly that actually compiles the linked TSLCompiler.cs source.
foreach ($name in @('Computator.Performance.Worker.deps.json', 'Computator.Performance.Worker.runtimeconfig.json')) {
    if ((Get-SHA256 (Join-Path $payloads.baseline $name)) -ne (Get-SHA256 (Join-Path $payloads.candidate $name))) {
        throw "Published dependency/runtime configuration differs: $name"
    }
}
$driverFiles = @('Computator.Performance.Worker.exe', 'Computator.Performance.Worker.dll', 'Computator.Performance.Worker.pdb')
foreach ($name in $driverFiles) {
    Copy-Item -LiteralPath (Join-Path $payloads.candidate $name) -Destination (Join-Path $payloads.baseline $name) -Force
}
$driverHashes = [ordered]@{}
foreach ($name in $driverFiles) {
    $driverHashes[$name] = Get-SHA256 (Join-Path $payloads.candidate $name)
    if ((Get-SHA256 (Join-Path $payloads.baseline $name)) -ne $driverHashes[$name]) { throw "Common driver copy failed: $name" }
}
$baselinePayloadPaths = @(Get-ChildItem -LiteralPath $payloads.baseline -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($payloads.baseline, $_.FullName) } | Sort-Object)
$candidatePayloadPaths = @(Get-ChildItem -LiteralPath $payloads.candidate -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($payloads.candidate, $_.FullName) } | Sort-Object)
if (@(Compare-Object $baselinePayloadPaths $candidatePayloadPaths).Count) { throw 'Published file inventories differ; review the dependency/deployment difference before timing.' }
foreach ($file in @(Get-ChildItem -LiteralPath $payloads.baseline -Recurse -File | Where-Object { $_.Extension -in @('.dll', '.exe') -and $_.Name -notlike 'Computator.*' })) {
    $relative = [IO.Path]::GetRelativePath($payloads.baseline, $file.FullName)
    if ((Get-SHA256 $file.FullName) -ne (Get-SHA256 (Join-Path $payloads.candidate $relative))) { throw "Third-party dependency binary differs: $relative" }
}

function Get-PublicationImage([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $pe = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        if (!$pe.HasMetadata -or $null -eq $pe.PEHeaders.CorHeader) { throw "Not a managed assembly: $Path" }
        $entry = $pe.PEHeaders.CorHeader.ManagedNativeHeaderDirectory
        [uint32]$signature = 0
        if ($entry.Size -ge 4 -and $entry.RelativeVirtualAddress -gt 0) {
            $reader = $pe.GetSectionData($entry.RelativeVirtualAddress).GetReader(0, 4)
            $signature = $reader.ReadUInt32()
        }
        @{
            path = [IO.Path]::GetFullPath($Path); sha256 = (Get-SHA256 $Path);
            bytes = $stream.Length; metadataBytes = $pe.PEHeaders.MetadataSize;
            managedNativeHeaderBytes = $entry.Size; readyToRun = ($signature -eq 0x00525452);
            signature = $signature
        }
    } finally { $pe.Dispose(); $stream.Dispose() }
}

# The common driver is deliberately IL-only. The product and Roslyn retain their
# shipped non-composite ReadyToRun policy, verified from their actual PE headers.
Add-Type -AssemblyName System.Reflection.Metadata
$publicationImages = @()
foreach ($label in @('baseline', 'candidate')) {
    foreach ($name in @('Computator.Performance.Worker.dll', 'Computator.NET.Core.dll',
        'Computator.NET.Engine.dll', 'Microsoft.CodeAnalysis.dll', 'Microsoft.CodeAnalysis.CSharp.dll')) {
        $image = Get-PublicationImage (Join-Path $payloads[$label] $name)
        if ($name -eq 'Computator.Performance.Worker.dll') {
            if ($image.managedNativeHeaderBytes -ne 0) { throw "Common driver unexpectedly has a native header: $label" }
        } elseif (!$image.readyToRun) {
            throw "Required product/compiler assembly is not ReadyToRun: $label/$name"
        }
        $publicationImages += @{ product = $label; assembly = $name; image = $image }
    }
}
Write-Json @{
    policy = 'Same IL-only driver bytes; both product payloads and Roslyn use non-composite ReadyToRun.';
    images = $publicationImages
} (Join-Path $audit 'publication-images.json')

function Get-Artifact([string]$Path, [string]$Role = '') {
    $item = @{ path = [IO.Path]::GetFullPath($Path); sha256 = (Get-SHA256 $Path) }
    if ($Role) { $item.role = $Role }
    $item
}

function Get-PayloadArtifacts([string]$Directory) {
    @(Get-ChildItem -LiteralPath $Directory -Recurse -File | Sort-Object FullName | ForEach-Object {
        $role = ''
        if ($_.FullName -eq (Join-Path $Directory 'Computator.Performance.Worker.dll')) { $role = 'worker' }
        elseif ($_.FullName -eq (Join-Path $Directory 'Computator.NET.Core.dll')) { $role = 'core' }
        Get-Artifact $_.FullName $role
    })
}

$scenariosPath = Join-Path $audit 'scenario-contract.json'
Invoke-Logged $python @((Join-Path $candidateSource 'build/performance/compiler_comparison_manifest.py'), '--scenarios-output', $scenariosPath) $candidateSource 'scenario-contract.txt'
$manifest = @{
    schema = 1; diagnostic = 'current-compiler-before-after'; environment = @{};
    scenarios = (Get-Content -LiteralPath $scenariosPath -Raw | ConvertFrom-Json -AsHashtable)
}
$coreVersions = @{}
foreach ($label in @('baseline', 'candidate')) {
    $core = Join-Path $payloads[$label] 'Computator.NET.Core.dll'
    $engine = Join-Path $payloads[$label] 'Computator.NET.Engine.dll'
    $coreVersions[$label] = [Reflection.AssemblyName]::GetAssemblyName($core).Version.ToString()
    $commit = if ($label -eq 'baseline') { $baselineCommit } else { $ExpectedCandidateCommit }
    $manifest[$label] = @{
        command = @($dotnet, (Join-Path $payloads[$label] 'Computator.Performance.Worker.dll')); cwd = $payloads[$label];
        expected = @{ coreVersion = $coreVersions[$label]; bitness = 64; runtimePrefix = '.NET 10.' };
        environment = @{ COMPUTATOR_DATA_DIRECTORY = $profiles[$label] };
        provenance = @{
            version = ($coreVersions[$label] + ' (' + $label + ' modern compiler comparison)'); commit = $commit;
            source = 'PawelTroka/Computator.NET; same-runner .NET 10 before/after compiler comparison'; buildConfiguration = 'Release';
            deployment = 'framework-dependent win-x64 ReadyToRun (non-composite)'; sdk = $sdkBefore;
            coreSHA256 = (Get-SHA256 $core); engineSHA256 = (Get-SHA256 $engine);
            commonDriver = @{ sourceCommit = $ExpectedCandidateCommit; sourceSHA256 = $sharedHashes;
                startupSourceSHA256 = $afterFiles[$startupDriverPath]; binarySHA256 = $driverHashes }
        };
        artifacts = @(Get-PayloadArtifacts $payloads[$label]) + @(
            Get-Artifact $dotnet
            Get-Artifact (Join-Path $candidateSource 'build/performance/compare.py')
            Get-Artifact $PSCommandPath
        ) + @(Get-ChildItem -LiteralPath $audit -File | Sort-Object FullName | ForEach-Object { Get-Artifact $_.FullName })
    }
}
if ($coreVersions.baseline -ne $coreVersions.candidate) { throw 'The diagnostic requires unchanged product assembly versions.' }
$conditioningManifest = Join-Path $output 'conditioning-manifest.json'
Write-Json $manifest $conditioningManifest
$conditioning = Join-Path $output 'conditioning'
# This separately retained pair checks the complete protocol and conditions the
# OS file cache. It does not contribute observations to the 20 measured pairs.
Invoke-Logged $python @((Join-Path $candidateSource 'build/performance/compare.py'), 'run', '--manifest', $conditioningManifest,
    '--output', $conditioning, '--pairs', '1', '--timeout', '120', '--bootstrap', '50000', '--seed', '191') $candidateSource 'workload-conditioning.txt'
$conditioned = Get-Content -LiteralPath (Join-Path $conditioning 'raw.json') -Raw | ConvertFrom-Json -AsHashtable
$records = @($conditioned.trials | ForEach-Object { $_.invocations } | ForEach-Object { $_.record })
if ($records.Count -ne (2 * $manifest.scenarios.Count) -or @($records.runtime | Sort-Object -Unique).Count -ne 1) { throw 'Conditioning did not establish identical actual runtimes for both workers.' }
$runtime = $records[0].runtime
if ($runtime -notmatch '^\.NET (10\.0\.[0-9]+)$') { throw "Unexpected actual runtime: $runtime" }
$runtimeVersion = $Matches[1]
$runtimeEntry = @($installedRuntimes | Where-Object { $_ -match ('^Microsoft\.NETCore\.App ' + [regex]::Escape($runtimeVersion) + ' \[(.+)\]$') })
if ($runtimeEntry.Count -ne 1 -or $runtimeEntry[0] -notmatch '\[(.+)\]$') { throw 'Cannot identify the actual CLR installation for artifact pinning.' }
$runtimeDirectory = Join-Path $Matches[1] $runtimeVersion
$runtimeArtifacts = @('System.Private.CoreLib.dll', 'coreclr.dll', 'clrjit.dll', 'hostpolicy.dll' | ForEach-Object { Get-Artifact (Join-Path $runtimeDirectory $_) })
$environmentPath = Join-Path $output 'windows-environment.json'
Write-Json @{
    diagnostic = 'current-compiler-before-after'; sdk = $sdkBefore; runtime = $runtime;
    dotnet = (Get-Artifact $dotnet); clrArtifacts = $runtimeArtifacts; installedRuntimes = $installedRuntimes;
    cpu = @(Get-CimInstance Win32_Processor | Select-Object Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed);
    os = Get-CimInstance Win32_OperatingSystem | Select-Object Caption, Version, BuildNumber, OSArchitecture;
    memoryBytes = (Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory;
    powerScheme = @(& powercfg /getactivescheme);
    cacheCondition = 'Fresh measured processes; every selected workload executed once beforehand in each product; OS filesystem cache unflushed. Native profiles conditioned by retained workload screening; desktop startup excluded.';
    profiles = $profiles; conditioning = 'One separately retained process pair per scenario in conditioning/raw.json; excluded from the measured dataset.';
    runtimePolicy = 'Same explicit dotnet host and identical runtimeconfig; packaged tiering/GC defaults; inherited overrides stripped by compare.py.'
} $environmentPath
foreach ($label in @('baseline', 'candidate')) {
    $null = $manifest[$label].expected.Remove('runtimePrefix')
    $manifest[$label].expected.runtime = $runtime
    $manifest[$label].artifacts += $runtimeArtifacts + @(
        Get-Artifact $environmentPath
        Get-Artifact (Join-Path $conditioning 'raw.json')
    )
}
Write-Json $manifest (Join-Path $output 'manifest.json')
Write-Host "Prepared modern compiler comparison: $(Join-Path $output 'manifest.json')"
Write-Host 'Run compare.py with 20 process pairs on this same runner. Conditioning data is separate; do not pool observations across runners or with the historical comparison.'
