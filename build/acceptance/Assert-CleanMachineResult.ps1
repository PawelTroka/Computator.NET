[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ResultPath,
    [Parameter(Mandatory)][string]$CandidateSha256,
    [string]$CandidateBundleSha256,
    [switch]$RequireHistoricalUpgrade,
    [switch]$RequireOfficialBundle
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$report = Get-Content -LiteralPath $ResultPath -Raw | ConvertFrom-Json
if ($CandidateSha256 -notmatch '^[0-9a-fA-F]{64}$' -or $report.schemaVersion -notin @(1,2) -or $report.status -ne 'passed' -or $report.candidateSha256 -ne $CandidateSha256) { throw 'The acceptance report does not establish success for this exact candidate.' }
foreach ($stage in @('clean-runtime-preflight', 'candidate-install-payload-and-launch', 'repair-uninstall-and-user-data-preservation')) {
    if ($stage -notin $report.stages) { throw "Missing acceptance stage: $stage" }
}
if ($RequireHistoricalUpgrade -and ($report.historicalUpgrade -ne 'passed' -or 'historical-install-and-failed-upgrade-rollback' -notin $report.stages)) { throw 'Historical installation, failed-upgrade rollback and successful upgrade were not verified.' }
if ($report.PSObject.Properties['cleanupError']) { throw 'Acceptance cleanup failed; inspect the retained guest diagnostics.' }
if ($RequireOfficialBundle -and $report.schemaVersion -ne 2) { throw 'An extracted-MSI-only report does not establish the official EXE upgrade path.' }
if ($report.schemaVersion -eq 2) {
    $fixture = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'historical-2.3.2.json') -Raw | ConvertFrom-Json
    if ($CandidateBundleSha256 -notmatch '^[0-9a-fA-F]{64}$' -or $report.candidateBundleSha256 -ne $CandidateBundleSha256 -or
        $report.historicalBundleSha256 -ne $fixture.sha256 -or $report.historicalProductCode -ne $fixture.productCode -or
        $report.historicalBundleCode -ne $fixture.bundleCode) { throw 'The report is not bound to the expected candidate EXE and official historical release.' }
    if ($report.environment -ne 'windows-sandbox' -or !$report.noModernRuntimeRequired -or @($report.modernRuntimeLocations).Count -ne 0) {
        throw 'An SDK-equipped runner cannot satisfy clean-machine acceptance.'
    }
    if (@($report.cleanupErrors).Count -ne 0 -or $report.historicalUpgrade -ne 'passed' -or
        $report.historicalPayloadFiles -lt 1 -or $report.candidatePayloadFiles -lt 1 -or @($report.preservedFiles).Count -lt 6) { throw 'Historical acceptance or cleanup is incomplete.' }
    foreach ($stage in @('official-exe-install-profile-and-launch','historical-install-and-failed-upgrade-rollback','real-settings-migration','shortcuts-and-file-associations')) {
        if ($stage -notin $report.stages) { throw "Missing historical EXE acceptance stage: $stage" }
    }
}
Write-Host "Verified clean-machine acceptance for $CandidateSha256; historical upgrade required: $RequireHistoricalUpgrade; official bundle required: $RequireOfficialBundle."
