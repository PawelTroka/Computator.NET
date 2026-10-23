[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$InputDirectory,
    [Parameter(Mandatory = $true)][string]$OutputDirectory,
    [string]$Dotnet = 'dotnet'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($env:OS -ne 'Windows_NT' -or $PSVersionTable.PSVersion.Major -lt 7) {
    throw 'Run this diagnostic with PowerShell 7 on the Windows comparison host.'
}
$inputRoot = [IO.Path]::GetFullPath($InputDirectory)
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
if ((Test-Path -LiteralPath $outputRoot) -and
    (-not (Test-Path -LiteralPath $outputRoot -PathType Container) -or
        @(Get-ChildItem -LiteralPath $outputRoot -Force).Count -ne 0)) {
    throw 'The diagnostic output must be absent or empty.'
}
if ($outputRoot -eq $inputRoot -or
    $outputRoot.StartsWith($inputRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Keep diagnostic output outside the authenticated input directory.'
}
$manifestPath = Join-Path $inputRoot 'manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$worker = [string]$manifest.candidate.command[0]
if ($manifest.candidate.command.Count -ne 1 -or
    [IO.Path]::GetFileName($worker) -ne 'Computator.Performance.Worker.exe') {
    throw 'Expected the existing candidate Worker executable from Prepare-Comparison.ps1.'
}
$workerDirectory = [string]$manifest.candidate.cwd
$dotnetPath = (Get-Command -Name $Dotnet -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
$toolVersion = '10.0.750501'
$toolSourceCommit = 'ad724c4b85e7ae775e42f393c6e95a7557a57954'
$toolPackageSource = 'https://api.nuget.org/v3/index.json'
$toolPackagePage = 'https://www.nuget.org/packages/dotnet-trace/10.0.750501'
$source = 'https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace'
$profiles = 'dotnet-sampled-thread-time,dotnet-common'
$utf8 = [Text.UTF8Encoding]::new($false)

function Assert-CandidateArtifacts {
    foreach ($artifact in $manifest.candidate.artifacts) {
        $actual = (Get-FileHash -LiteralPath $artifact.path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne $artifact.sha256) { throw "Candidate artifact changed: $($artifact.path)" }
    }
}
function Invoke-OwnedProcess([string]$Executable, [string[]]$Arguments,
    [string]$WorkingDirectory, [string]$LogName, [int]$TimeoutSeconds,
    [hashtable]$Controls = @{}) {
    $callPath = Join-Path $outputRoot ($LogName + '.call.json')
    [ordered]@{ command = @($Executable) + $Arguments; cwd = $WorkingDirectory;
        logName = $LogName; timeoutSeconds = $TimeoutSeconds; controls = $Controls } |
            ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $callPath -Encoding utf8
    & $pythonPath $processAdapter $supervisorPath $callPath $outputRoot
    if ($LASTEXITCODE -ne 0) { throw "Diagnostic $LogName failed; retain its trace and logs." }
    return [IO.File]::ReadAllText((Join-Path $outputRoot ($LogName + '.stdout.txt')))
}

Assert-CandidateArtifacts
[IO.Directory]::CreateDirectory($outputRoot) | Out-Null
$pythonPath = (Get-Command python -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
$supervisorPath = Join-Path $PSScriptRoot 'compare.py'
$processAdapter = Join-Path $outputRoot 'owned-process.py'
@'
import importlib.util, json, pathlib, sys
supervisor, call, output = map(pathlib.Path, sys.argv[1:])
spec = importlib.util.spec_from_file_location('performance_supervisor', supervisor)
module = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = module
spec.loader.exec_module(module)
request = json.loads(call.read_text(encoding='utf-8-sig'))
environment, controls = module.controlled_environment({'environment': {}}, False)
environment.update(request['controls'])
record = module.run_child(request['command'], request['cwd'], environment, request['timeoutSeconds'])
record['environmentOverrides'] = {**controls, **request['controls']}
record['scope'] = 'Owned diagnostic command; JSON worker parsing notices are expected for tool commands.'
name = request['logName']
for stream in ('stdout', 'stderr'):
    (output / (name + '.' + stream + '.txt')).write_text(record[stream], encoding='utf-8')
(output / (name + '.command.json')).write_text(json.dumps(record, indent=2) + '\n', encoding='utf-8')
fatal = [error for error in record['errors'] if not error.startswith('Worker stdout must be one JSON record:')]
raise SystemExit(1 if fatal or record.get('unsafeToContinue') or record['timedOut'] or record['exitCode'] != 0 else 0)
'@ | Set-Content -LiteralPath $processAdapter -Encoding utf8
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $outputRoot 'input-manifest.json')
$toolDirectory = Join-Path $outputRoot 'tool'
$nugetConfig = Join-Path $outputRoot 'NuGet.Config'
@'
<configuration>
  <packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources>
</configuration>
'@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8
$null = Invoke-OwnedProcess $dotnetPath @('tool', 'install', 'dotnet-trace', '--version', $toolVersion,
    '--tool-path', $toolDirectory, '--configfile', $nugetConfig, '--no-cache') $outputRoot 'tool-install' 180
$trace = Join-Path $toolDirectory 'dotnet-trace.exe'
$reportedVersion = Invoke-OwnedProcess $trace @('--version') $outputRoot 'tool-version' 30
if ($reportedVersion -notmatch ('(?m)^' + [Regex]::Escape($toolVersion) + '(?:\s|\+)')) {
    throw "Installed profiler version differs from the reviewed pin: $reportedVersion"
}
$null = Invoke-OwnedProcess $trace @('list-profiles') $outputRoot 'tool-profiles' 30
$controls = @{}
foreach ($property in $manifest.candidate.environment.PSObject.Properties) {
    if ($property.Name -ne 'COMPUTATOR_DATA_DIRECTORY') { throw 'Unexpected candidate environment override.' }
    $controls[$property.Name] = [string]$property.Value
}
$records = @()
foreach ($scenario in @('real', 'script')) {
    $tracePath = Join-Path $outputRoot ($scenario + '.nettrace')
    # Child startup suspension is handled by dotnet-trace. The executable and
    # scenario are exactly the existing common worker; no benchmark source changes.
    $stdout = Invoke-OwnedProcess $trace @('collect', '--profile', $profiles,
        '--buffersize', '64', '--output', $tracePath, '--show-child-io', '--',
        $worker, '--scenario', $scenario) $workerDirectory ($scenario + '-collect') 180 $controls
    $observations = @($stdout -split '\r?\n' | Where-Object { $_.StartsWith('{') } |
        ForEach-Object { $_ | ConvertFrom-Json })
    if ($observations.Count -ne 1 -or -not $observations[0].correctness -or
        $observations[0].scenario -ne $scenario -or $observations[0].coreVersion -ne '3.0.0.0' -or
        $observations[0].bitness -ne 64 -or $observations[0].runtime -notlike '.NET 10.*') {
        throw "Profiled worker did not preserve its identity and correctness: $scenario"
    }
    if (-not (Test-Path -LiteralPath $tracePath -PathType Leaf)) { throw "Missing $scenario nettrace" }
    $null = Invoke-OwnedProcess $trace @('convert', $tracePath, '--format', 'Speedscope',
        '--output', (Join-Path $outputRoot $scenario)) $outputRoot ($scenario + '-convert') 180
    if (@(Get-ChildItem -LiteralPath $outputRoot -Filter ($scenario + '*.speedscope.json')).Count -ne 1) {
        throw "Missing or ambiguous Speedscope conversion: $scenario"
    }
    $records += [pscustomobject]@{ scenario = $scenario; worker = $observations[0];
        scope = 'Profiled attribution run; timings are not benchmark observations.' }
    Assert-CandidateArtifacts
}
$files = @(Get-ChildItem -LiteralPath $toolDirectory -Recurse -File | ForEach-Object {
    [pscustomobject]@{ path = $_.FullName; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
})
[ordered]@{
    schema = 1; createdUtc = [DateTime]::UtcNow.ToString('o');
    scope = 'Separate .NET 10 Windows attribution diagnostic; no historical trace or benchmark timing claim.';
    tool = @{ package = 'dotnet-trace'; version = $toolVersion; packagePage = $toolPackagePage;
        source = $toolPackageSource; sourceCommit = $toolSourceCommit; reportedVersion = $reportedVersion.Trim(); files = $files };
    documentation = $source; profiles = $profiles; workerObservations = $records;
    supervisorSha256 = (Get-FileHash -LiteralPath $supervisorPath -Algorithm SHA256).Hash.ToLowerInvariant();
    interpretation = 'Sampled .NET thread stacks include off-CPU time; do not label sample proportions as precise CPU percentages. Keep nettrace: Speedscope loses runtime event detail.';
    candidateArtifactsVerifiedBeforeAndAfter = $true
} | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $outputRoot 'profiling-provenance.json') -Encoding utf8
Write-Host "Preserved compilation traces, converted stacks and child correctness records in $outputRoot"
