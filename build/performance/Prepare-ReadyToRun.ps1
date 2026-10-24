#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$OutputDirectory,
    [switch]$DisposableProfile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (!$IsWindows -or !$DisposableProfile) { throw 'Use a disposable Windows account and pass -DisposableProfile.' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
if ((Test-Path -LiteralPath $output) -and @(Get-ChildItem -LiteralPath $output -Force).Count) { throw 'OutputDirectory must be empty.' }
New-Item -ItemType Directory -Path $output -Force | Out-Null
$ilDirectory = Join-Path $output 'il'
$r2rDirectory = Join-Path $output 'r2r'
& (Join-Path $PSScriptRoot 'Prepare-Comparison.ps1') -OutputDirectory $ilDirectory -DisposableProfile -ReadyToRun:$false
& (Join-Path $PSScriptRoot 'Prepare-Comparison.ps1') -OutputDirectory $r2rDirectory -DisposableProfile -ReadyToRun:$true
$il = Get-Content -LiteralPath (Join-Path $ilDirectory 'manifest.json') -Raw | ConvertFrom-Json -AsHashtable
$r2r = Get-Content -LiteralPath (Join-Path $r2rDirectory 'manifest.json') -Raw | ConvertFrom-Json -AsHashtable
if ($il.candidate.provenance.commit -ne $r2r.candidate.provenance.commit) { throw 'Both publishes must use the same clean commit.' }

function Get-Pin([string]$Path) {
    @{ path=[IO.Path]::GetFullPath($Path); sha256=(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
}
function Get-ImageRecord([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $pe = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        if (!$pe.HasMetadata -or $null -eq $pe.PEHeaders.CorHeader) { throw "Not a managed assembly: $Path" }
        $entry = $pe.PEHeaders.CorHeader.ManagedNativeHeaderDirectory
        [uint32]$signature = 0
        if ($entry.Size -ge 4 -and $entry.RelativeVirtualAddress -gt 0) {
            $reader = $pe.GetSectionData($entry.RelativeVirtualAddress).GetReader(0,4)
            $signature = $reader.ReadUInt32()
        }
        $record = Get-Pin $Path
        $record.bytes = $stream.Length
        $record.machine = $pe.PEHeaders.CoffHeader.Machine.ToString()
        $record.managedNativeHeaderBytes = $entry.Size
        $record.readyToRun = $signature -eq 0x00525452 # RTR\0 native-header signature.
        $record.signature = $signature
        $record
    } finally { $pe.Dispose(); $stream.Dispose() }
}
$images = @()
foreach ($payload in @('candidate','candidate-desktop')) {
    foreach ($name in @('Computator.NET.Core.dll','Computator.NET.Engine.dll','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')) {
        $before = Get-ImageRecord (Join-Path $ilDirectory ($payload + '/' + $name))
        $after = Get-ImageRecord (Join-Path $r2rDirectory ($payload + '/' + $name))
        if (!$after.readyToRun) { throw "Publish did not precompile the required assembly: $($after.path)" }
        if ($name -eq 'Computator.NET.Core.dll' -and $before.readyToRun) { throw 'IL control unexpectedly contains a precompiled Core image.' }
        $images += @{ payload=$payload; assembly=$name; il=$before; readyToRun=$after }
    }
}
$payloadBytes = @{}
foreach ($kind in @('il','r2r')) {
    $payloadBytes[$kind] = @{}
    foreach ($payload in @('candidate','candidate-desktop')) {
        $payloadBytes[$kind][$payload] = [long](Get-ChildItem -LiteralPath (Join-Path $output ($kind + '/' + $payload)) -Recurse -File | Measure-Object Length -Sum).Sum
    }
}
# Publishing R2R may alter native image layout but must not silently select a
# different runtime/GC/tiering configuration for the comparison.
foreach ($payload in @('candidate','candidate-desktop')) {
    $name = if ($payload -eq 'candidate') { 'Computator.Performance.Worker.runtimeconfig.json' } else { 'Computator.NET.runtimeconfig.json' }
    $a = Get-Content -LiteralPath (Join-Path $ilDirectory ($payload + '/' + $name)) -Raw
    $b = Get-Content -LiteralPath (Join-Path $r2rDirectory ($payload + '/' + $name)) -Raw
    if ($a -ne $b) { throw "Runtime policy differs between IL and R2R: $payload" }
}
$auditPath = Join-Path $output 'publication-audit.json'
@{
    schema=1; sourceCommit=$r2r.candidate.provenance.commit; images=$images; payloadBytes=$payloadBytes;
    historical='Authenticated official 2.3.2.0; never retargeted or republished.';
    runtimePolicy='Default tiering/PGO, identical runtimeconfig, same source and SDK; IL-only vs non-composite ReadyToRun.';
    conditioning='Both published desktops exercised before collection; every observation is a new process. No OS file-cache purge.'
} | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $auditPath -Encoding utf8

# A causal modern IL/R2R experiment, independent from historical comparison.
# Both startup targets use the exact same IL observer, outside the timed product.
$modern = @{
    schema=1; environment=@{};
    baseline=$il.candidate; candidate=$r2r.candidate;
    scenarios=($il.scenarios | ConvertTo-Json -Depth 12 | ConvertFrom-Json -AsHashtable)
}
$modern.baseline.provenance.version += ' (IL-only)'
$modern.candidate.provenance.version += ' (ReadyToRun)'
$observer = $il.candidate.command[0]
$modern.scenarios.startup.commands = @{ baseline=@($observer); candidate=@($observer) }
$modern.scenarios.startup.args = @{
    baseline=$il.scenarios.startup.args.candidate;
    candidate=$r2r.scenarios.startup.args.candidate
}
# Pin the observer's entire IL payload in the candidate as well as its own R2R
# payload. Do not duplicate the unique primary worker/core roles.
$observerPins = @()
foreach ($file in Get-ChildItem -LiteralPath $il.candidate.cwd -Recurse -File) {
    $pin = Get-Pin $file.FullName
    if ($file.FullName -eq $observer) { $pin.role='observer' }
    $observerPins += $pin
}
$modern.candidate.artifacts += $observerPins
$modern.baseline.artifacts += @(Get-Pin $auditPath)
$modern.candidate.artifacts += @(Get-Pin $auditPath)
$modern | ConvertTo-Json -Depth 18 | Set-Content -LiteralPath (Join-Path $output 'modern-manifest.json') -Encoding utf8
# Reread to avoid retaining shared PowerShell hashtable references changed while
# labeling the causal comparison. The historical candidate keeps its plain version.
$r2r = Get-Content -LiteralPath (Join-Path $r2rDirectory 'manifest.json') -Raw | ConvertFrom-Json -AsHashtable
$r2r.baseline.artifacts += @(Get-Pin $auditPath)
$r2r.candidate.artifacts += @(Get-Pin $auditPath)
$r2r | ConvertTo-Json -Depth 18 | Set-Content -LiteralPath (Join-Path $output 'historical-manifest.json') -Encoding utf8
Write-Host "Verified source and native images. Historical manifest: $(Join-Path $output 'historical-manifest.json')"
Write-Host "Causal IL/R2R manifest: $(Join-Path $output 'modern-manifest.json')"
Get-Content -LiteralPath $auditPath
