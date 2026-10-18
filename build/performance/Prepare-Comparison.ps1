[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$HistoricalArchive,
    [string]$HistoricalSHA256,
    [string]$HistoricalVersion = '2.3.2.0',
    [string]$HistoricalSource = '',
    [switch]$DisposableProfile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (!$IsWindows) { throw 'The historical Framework product requires Windows.' }
if (!$DisposableProfile) {
    throw 'Run under a disposable Windows account/VM and pass -DisposableProfile: the unmodified historical application uses Documents/Computator.NET.'
}
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) {
    if (@(Get-ChildItem -LiteralPath $output -Force).Count) { throw 'OutputDirectory must be empty.' }
}
$null = New-Item -ItemType Directory -Path $output -Force
$historical = Join-Path $output 'historical'
$arguments = @{ OutputDirectory = $historical }
if ($HistoricalArchive) {
    $arguments.ArchivePath = $HistoricalArchive
    $arguments.ExpectedSHA256 = $HistoricalSHA256
    $arguments.ExpectedVersion = $HistoricalVersion
    $arguments.SourceDescription = $HistoricalSource
} elseif ($HistoricalVersion -ne '2.3.2.0') {
    throw 'Only the official 2.3.2 control can be downloaded automatically. Supply an authenticated archive for another version.'
}
& (Join-Path $PSScriptRoot 'Get-HistoricalBaseline.ps1') @arguments
$provenance = Get-Content -LiteralPath (Join-Path $historical 'baseline-provenance.json') -Raw | ConvertFrom-Json
$baseline = Join-Path $output 'baseline'
$candidate = Join-Path $output 'candidate'
$desktop = Join-Path $output 'candidate-desktop'
$candidateProfile = Join-Path $output 'candidate-profile'
$null = New-Item -ItemType Directory -Path $baseline
Copy-Item -Path (Join-Path $historical '*') -Destination $baseline -Recurse

function Invoke-Dotnet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed ($LASTEXITCODE): $($Arguments -join ' ')" }
}
Push-Location $root
try {
    $env:MSBUILDDISABLENODEREUSE = '1'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $env:DOTNET_NOLOGO = '1'
    $worker = Join-Path $PSScriptRoot 'Worker.csproj'
    Invoke-Dotnet @('build', $worker, '-c', 'Release', "-p:HistoricalDirectory=$historical", '-o', $baseline, '--nologo', '-v:minimal', '-m:1')
    # Preserve the complete official payload and its runtime/binding policy.
    Copy-Item -Path (Join-Path $historical '*') -Destination $baseline -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $historical 'Computator.NET.exe.config') -Destination (Join-Path $baseline 'Computator.Performance.Worker.exe.config') -Force
    Invoke-Dotnet @('publish', $worker, '-c', 'Release', '-r', 'win-x64', '--self-contained', 'false', '-o', $candidate, '--nologo', '-v:minimal', '-m:1')
    Invoke-Dotnet @('publish', 'Computator.NET/Computator.NET.csproj', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'false', '-o', $desktop, '--nologo', '-v:minimal', '-m:1')
    Invoke-Dotnet @('build-server', 'shutdown')
    $commit = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot identify candidate commit.' }
    $changes = @(& git status --porcelain --untracked-files=no)
    if ($changes.Count) { throw 'Build from a clean commit; candidate tracked sources changed.' }
} finally { Pop-Location }

function Get-Artifact([string]$Path, [string]$RoleMode = 'none') {
    $item = @{ path = [IO.Path]::GetFullPath($Path); sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
    if ($RoleMode -eq 'primary') {
        if ([IO.Path]::GetFileName($Path) -eq 'Computator.Performance.Worker.exe') { $item.role = 'worker' }
        elseif ([IO.Path]::GetFileName($Path) -eq 'Computator.NET.Core.dll') { $item.role = 'core' }
    } elseif ($RoleMode -eq 'observer' -and [IO.Path]::GetFileName($Path) -eq 'Computator.Performance.Worker.exe') { $item.role = 'observer' }
    $item
}
function Get-Artifacts([string]$Directory, [string]$RoleMode = 'none') {
    @(Get-ChildItem -LiteralPath $Directory -Recurse -File | Sort-Object FullName | ForEach-Object { Get-Artifact $_.FullName $RoleMode })
}
# Run all setup before timings; then reuse each version's own profile/native
# extraction cache. First installation/disk-cache-cold startup is a separate test.
foreach ($entry in @(
    @{ worker = Join-Path $candidate 'Computator.Performance.Worker.exe'; desktop = Join-Path $baseline 'Computator.NET.exe'; label = 'baseline' },
    @{ worker = Join-Path $candidate 'Computator.Performance.Worker.exe'; desktop = Join-Path $desktop 'Computator.NET.exe'; label = 'candidate' }
)) {
    Remove-Item Env:COMPUTATOR_GSL_DIRECTORY -ErrorAction SilentlyContinue
    Remove-Item Env:COMPUTATOR_DATA_DIRECTORY -ErrorAction SilentlyContinue
    if ($entry.label -eq 'candidate') { $env:COMPUTATOR_DATA_DIRECTORY = $candidateProfile }
    $stdout = Join-Path $output ($entry.label + '-conditioning.stdout.json')
    $stderr = Join-Path $output ($entry.label + '-conditioning.stderr.txt')
    $child = Start-Process -FilePath $entry.worker -ArgumentList @('--scenario', 'startup', '--desktop-exe', ('"' + $entry.desktop + '"')) -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    if (!$child.WaitForExit(90000)) {
        & taskkill /PID $child.Id /T /F | Out-Null
        throw "Conditioning timed out: $($entry.label). See retained logs."
    }
    if ($child.ExitCode -ne 0) { throw "Conditioning failed: $($entry.label). See retained logs." }
    $record = Get-Content -LiteralPath $stdout -Raw | ConvertFrom-Json
    if (!$record.correctness -or $record.bitness -ne 64) { throw 'Invalid conditioning result.' }
}
Remove-Item Env:COMPUTATOR_DATA_DIRECTORY -ErrorAction SilentlyContinue

$scenarios = [ordered]@{}
function Add-Scenario([string]$Name, [hashtable]$Operations) {
    $script:scenarios[$Name] = @{ metrics = @($Operations.Keys | Sort-Object); operations = $Operations }
}
Add-Scenario 'real' @{
    'evaluator.create.ms'=1; 'first.interaction.ms'=1;
    'engine.initialize.ms'=1; 'compile.first.ms'=1; 'evaluate.first.ms'=1; 'evaluate.typed.batch.ms'=65536;
    'evaluate.public.batch.ms'=65536; 'compile.same.warm.ms'=1; 'compile.custom.changed.ms'=1; 'integral.simpson.batch.ms'=32
}
Add-Scenario 'complex' @{
    'evaluator.create.ms'=1; 'first.interaction.ms'=1;
    'engine.initialize.ms'=1; 'compile.first.ms'=1; 'evaluate.first.ms'=1; 'evaluate.typed.batch.ms'=65536; 'compile.same.warm.ms'=1
}
Add-Scenario 'surface' @{
    'evaluator.create.ms'=1; 'first.interaction.ms'=1;
    'engine.initialize.ms'=1; 'compile.first.ms'=1; 'evaluate.first.ms'=1; 'chart.grid.sample.ms'=4096; 'compile.same.warm.ms'=1
}
Add-Scenario 'script' @{
    'evaluator.create.ms'=1; 'first.interaction.ms'=1;
    'engine.initialize.ms'=1; 'compile.first.ms'=1; 'script.first.ms'=1; 'script.repeated.batch.ms'=32; 'compile.same.warm.ms'=1
}
Add-Scenario 'math' @{ 'engine.initialize.ms'=1; 'math.special.batch.ms'=16384; 'math.native.gsl.batch.ms'=16384; 'math.elementary.batch.ms'=16384 }
Add-Scenario 'translation' @{ 'translation.first.ms'=1; 'translation.repeated.batch.ms'=256; 'translation.empty.batch.ms'=256 }
Add-Scenario 'startup' @{ 'desktop.main.ready.ms'=1 }
$scenarios.startup.args = @{
    baseline = @('--desktop-exe', (Join-Path $baseline 'Computator.NET.exe'))
    candidate = @('--desktop-exe', (Join-Path $desktop 'Computator.NET.exe'))
}
$scenarios.startup.commands = @{
    baseline = @((Join-Path $candidate 'Computator.Performance.Worker.exe'))
    candidate = @((Join-Path $candidate 'Computator.Performance.Worker.exe'))
}
$candidateVersion = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $candidate 'Computator.NET.Core.dll')).Version.ToString()
$baselineIdentity = @{
    version = $HistoricalVersion; source = $provenance.source; buildConfiguration = 'Release';
    commitUnknownReason = 'Official shipped binary is authoritative; a source tag is not proof of build reproducibility.';
    releaseSourceTag = '56fed3363c07026d7fb290e40f852468dfe158ba';
    archiveSHA256 = $provenance.archiveSHA256
}
if ($HistoricalArchive) {
    $baselineIdentity.source = $HistoricalSource
    $baselineIdentity.Remove('releaseSourceTag')
    $baselineIdentity.commitUnknownReason = 'Caller supplied an authenticated binary without a reproducible source build mapping.'
}
$hardware = @{
    cpu = @(Get-CimInstance Win32_Processor | Select-Object Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed);
    os = Get-CimInstance Win32_OperatingSystem | Select-Object Caption, Version, BuildNumber, OSArchitecture;
    memoryBytes = (Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory;
    frameworkRelease = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full').Release;
    installedRuntimes = @(& dotnet --list-runtimes);
    powerScheme = @(& powercfg /getactivescheme);
    cacheCondition = 'fresh processes; profiles/native extraction conditioned; OS filesystem cache unflushed';
    guiProfile = 'Disposable account: legacy uses normal Documents path; candidate uses a separate COMPUTATOR_DATA_DIRECTORY.'
}
$hardware | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $output 'windows-environment.json') -Encoding utf8
$manifest = @{
    schema = 1;
    baseline = @{
        command = @((Join-Path $baseline 'Computator.Performance.Worker.exe')); cwd = $baseline;
        expected = @{ coreVersion = $HistoricalVersion; bitness = 64; runtimePrefix = '.NET Framework CLR ' };
        provenance = $baselineIdentity; artifacts = @((Get-Artifacts $baseline 'primary') + (Get-Artifacts $candidate 'observer'))
    };
    candidate = @{
        command = @((Join-Path $candidate 'Computator.Performance.Worker.exe')); cwd = $candidate;
        expected = @{ coreVersion = $candidateVersion; bitness = 64; runtimePrefix = '.NET 10.' };
        environment = @{ COMPUTATOR_DATA_DIRECTORY = $candidateProfile };
        provenance = @{ version=$candidateVersion; commit=$commit; source='PawelTroka/Computator.NET'; buildConfiguration='Release'; deployment='framework-dependent win-x64' };
        artifacts = @((Get-Artifacts $candidate 'primary') + (Get-Artifacts $desktop))
    };
    scenarios = $scenarios;
    environment = @{}
}
$manifestPath = Join-Path $output 'manifest.json'
$manifest | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $manifestPath -Encoding utf8
Write-Host "Prepared comparison: $manifestPath"
