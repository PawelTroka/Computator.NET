[CmdletBinding()]
param([Parameter(Mandatory)][string]$ResultPath, [Parameter(Mandatory)][string]$CandidateSha256, [switch]$RequireHistoricalUpgrade)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$report = Get-Content -LiteralPath $ResultPath -Raw | ConvertFrom-Json
if ($report.schemaVersion -ne 1 -or $report.status -ne 'passed' -or $report.candidateSha256 -ne $CandidateSha256) { throw 'The acceptance report does not establish success for this exact candidate.' }
foreach ($stage in @('clean-runtime-preflight', 'candidate-install-payload-and-launch', 'repair-uninstall-and-user-data-preservation')) {
    if ($stage -notin $report.stages) { throw "Missing acceptance stage: $stage" }
}
if ($RequireHistoricalUpgrade -and ($report.historicalUpgrade -ne 'passed' -or 'historical-install-and-failed-upgrade-rollback' -notin $report.stages)) { throw 'Historical installation, failed-upgrade rollback and successful upgrade were not verified.' }
if ($report.PSObject.Properties['cleanupError']) { throw 'Acceptance cleanup failed; inspect the retained guest diagnostics.' }
Write-Host "Verified clean-machine acceptance for $CandidateSha256; historical upgrade required: $RequireHistoricalUpgrade."
