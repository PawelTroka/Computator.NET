[CmdletBinding()]
param([Parameter(Mandatory)][string]$InputDirectory, [Parameter(Mandatory)][string]$ResultDirectory, [switch]$DisposableMachine)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$DisposableMachine -or $env:USERNAME -ne 'WDAGUtilityAccount') { throw 'This entry point only runs in an explicitly launched Windows Sandbox guest.' }
$plan = Get-Content -LiteralPath (Join-Path $InputDirectory 'plan.json') -Raw | ConvertFrom-Json
if ($plan.schemaVersion -ne 2 -or !$plan.historicalUpgradeRequested -or !$plan.requireNoModernRuntime) { throw 'Expected a complete no-SDK historical bundle acceptance plan.' }
foreach ($entry in $plan.inputs) {
    if ($entry.name -ne [IO.Path]::GetFileName($entry.name)) { throw 'Acceptance input escapes its read-only directory.' }
    $path = Join-Path $InputDirectory $entry.name
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $entry.sha256) { throw "Acceptance input hash differs: $($entry.name)" }
}
$env:COMPUTATOR_DISPOSABLE_INSTALL_TEST = '1'
& (Join-Path $InputDirectory 'Test-HistoricalUpgrade.ps1') -MsiPath (Join-Path $InputDirectory 'candidate.msi') -InventoryPath (Join-Path $InputDirectory 'payload.xml') -CandidateBundlePath (Join-Path $InputDirectory 'candidate.exe') -HistoricalBundlePath (Join-Path $InputDirectory 'historical.exe') -ResultDirectory $ResultDirectory -DisposableMachine -RequireNoModernRuntime
