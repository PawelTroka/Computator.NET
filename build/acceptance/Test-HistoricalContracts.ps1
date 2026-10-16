$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'AcceptanceContracts.psm1') -Force
$root = Join-Path ([IO.Path]::GetTempPath()) ('Computator-HistoricalContracts-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
function Reject([scriptblock]$Action) {
    $rejected = $false
    try { & $Action } catch { $rejected = $true }
    if (!$rejected) { throw 'Invalid historical acceptance evidence/input was accepted.' }
}
try {
    $fixture = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'historical-2.3.2.json') -Raw | ConvertFrom-Json
    if ($fixture.sha256 -ne '30b107dced1c147b6f3ba9089b97d2131dd43aa195d5a23c3a95d9b150149d4a' -or $fixture.size -ne 28069455 -or $fixture.productVersion -ne '2.3.2.0') { throw 'Historical fixture pin differs from the inspected official release.' }
    # A trimmed reproduction of the nested log structure observed in run
    # 38904951253. This parser fixture is not evidence of machine acceptance.
    $candidateCode = '{5EE11F5A-4F1E-FC4E-A296-BC713710E32E}'
    $nestedLog = @'
Action start 13:07:17: RemoveExistingProducts.
MSI (s) (10:20) [13:07:18:000]: Executing op: FileRemove(,FileName=Computator.NET.exe,,ComponentId=fixture)
Action ended 13:07:18: INSTALL. Return value 1.
Property(N): ProductCode = {BDA7D43C-B1D1-4844-AC3F-198D33E3F237}
Property(N): UPGRADINGPRODUCTCODE = {5EE11F5A-4F1E-FC4E-A296-BC713710E32E}
Property(N): REMOVE = ALL
Action ended 13:07:18: RemoveExistingProducts. Return value 1.
Action start 13:07:18: ComputatorAcceptanceFailure.
MSI (s) (10:20) [13:07:18:001]: Product: Computator.NET -- Intentional disposable-machine rollback test
Action ended 13:07:18: ComputatorAcceptanceFailure. Return value 3.
MSI (s) (10:20) [13:07:18:002]: Executing op: RollbackInfo(,RollbackAction=1)
MSI (s) (10:20) [13:07:19:000]: Error in rollback skipped. Return: 5
'@
    foreach ($log in @($nestedLog, $nestedLog.Replace("`r`n", "`n").Replace("`n", "`r`n"))) {
        $proof = Assert-HistoricalRollbackLog $log $fixture.productCode $candidateCode
        if ($proof.removedProductCode -ne $fixture.productCode -or $proof.failureReturnValue -ne 3 -or
            $proof.engineDiagnostics.Count -ne 1 -or $proof.engineDiagnostics[0] -notlike '*Error in rollback skipped*') { throw 'Rollback parser lost its identities or engine diagnostics.' }
    }
    foreach ($pair in @(
        @($fixture.productCode, '{00000000-0000-0000-0000-000000000000}'),
        @($candidateCode, '{00000000-0000-0000-0000-000000000000}'),
        @('REMOVE = ALL', 'REMOVE = SomeFeature'),
        @('FileName=Computator.NET.exe', 'FileName=unrelated.exe'),
        @('INSTALL. Return value 1.', 'INSTALL. Return value 3.'),
        @('RemoveExistingProducts. Return value 1.', 'RemoveExistingProducts. Return value 3.'),
        @('ComputatorAcceptanceFailure. Return value 3.', 'ComputatorAcceptanceFailure. Return value 1.'),
        @('Intentional disposable-machine rollback test', 'An unrelated installation error'),
        @('Executing op: RollbackInfo(', 'Rollback was never executed(')
    )) {
        $changedLog = $nestedLog.Replace($pair[0], $pair[1])
        Reject { Assert-HistoricalRollbackLog $changedLog $fixture.productCode $candidateCode | Out-Null }
    }
    $split = $nestedLog.IndexOf('Action start 13:07:18: ComputatorAcceptanceFailure.')
    $wrongOrder = $nestedLog.Substring($split) + "`n" + $nestedLog.Substring(0, $split)
    Reject { Assert-HistoricalRollbackLog $wrongOrder $fixture.productCode $candidateCode | Out-Null }
    $msi = Join-Path $root 'candidate.msi'; $inventory = Join-Path $root 'payload.xml'; $exe = Join-Path $root 'not-a-real-installer.exe'
    Set-Content -LiteralPath $msi 'Never execute this plan-only fixture.'
    Set-Content -LiteralPath $exe 'Never execute this plan-only fixture.'
    Set-Content -LiteralPath $inventory '<Payload ProductCode="{28D0C198-6106-46F7-B01B-767AF587D28D}" />'
    $prepare = Join-Path $PSScriptRoot 'Prepare-CleanMachine.ps1'
    Reject { & $prepare -MsiPath $msi -InventoryPath $inventory -CandidateBundlePath $exe -OutputRoot $root }
    Reject { & $prepare -MsiPath $msi -InventoryPath $inventory -HistoricalBundlePath $exe -OutputRoot $root }
    Reject { & $prepare -MsiPath $msi -InventoryPath $inventory -CandidateBundlePath $exe -HistoricalBundlePath $exe -OutputRoot $root }
    Reject { & $prepare -MsiPath $msi -InventoryPath $inventory -CandidateBundlePath $exe -HistoricalBundlePath $exe -LegacyMsiPath $msi -OutputRoot $root }
    $reportPath = Join-Path $root 'evidence.json'
    $msiHash = '1' * 64; $bundleHash = '2' * 64
    $valid = @{
        schemaVersion = 2; status = 'passed'; historicalUpgrade = 'passed'; candidateSha256 = $msiHash; candidateBundleSha256 = $bundleHash
        historicalBundleSha256 = $fixture.sha256; historicalProductCode = $fixture.productCode; historicalBundleCode = $fixture.bundleCode
        environment = 'windows-sandbox'; noModernRuntimeRequired = $true; modernRuntimeLocations = @(); cleanupErrors = @()
        historicalPayloadFiles = 20; candidatePayloadFiles = 40
        preservedFiles = @('a.tsl','a.tslf','b.tsl','b.tslf','settings.dat','settings.json') | ForEach-Object { @{path = $_;sha256 = '3' * 64} }
        stages = @('clean-runtime-preflight','official-exe-install-profile-and-launch','historical-install-and-failed-upgrade-rollback',
            'candidate-install-payload-and-launch','real-settings-migration','shortcuts-and-file-associations','repair-uninstall-and-user-data-preservation')
    }
    $verifier = Join-Path $PSScriptRoot 'Assert-CleanMachineResult.ps1'
    $valid | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath
    & $verifier -ResultPath $reportPath -CandidateSha256 $msiHash -CandidateBundleSha256 $bundleHash -RequireHistoricalUpgrade -RequireOfficialBundle
    Reject { & $verifier -ResultPath $reportPath -CandidateSha256 $msiHash -CandidateBundleSha256 ('4' * 64) -RequireOfficialBundle }
    Reject { & $verifier -ResultPath $reportPath -CandidateSha256 ('4' * 64) -CandidateBundleSha256 $bundleHash -RequireOfficialBundle }
    foreach ($mutation in @(
        @{ key='schemaVersion'; value=1 }, @{ key='status'; value='failed' }, @{ key='historicalUpgrade'; value='not-requested' },
        @{ key='historicalBundleSha256'; value=('4' * 64) }, @{ key='environment'; value='github-hosted-sdk-equipped' },
        @{ key='noModernRuntimeRequired'; value=$false }, @{ key='modernRuntimeLocations'; value=@('C:\Program Files\dotnet') },
        @{ key='cleanupErrors'; value=@('uninstall failed') }, @{ key='historicalPayloadFiles'; value=0 }, @{ key='preservedFiles'; value=@() }
    )) {
        $changed = ($valid | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
        $changed.($mutation.key) = $mutation.value
        $changed | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath
        Reject { & $verifier -ResultPath $reportPath -CandidateSha256 $msiHash -CandidateBundleSha256 $bundleHash -RequireHistoricalUpgrade -RequireOfficialBundle }
    }
    foreach ($stage in $valid.stages) {
        $changed = ($valid | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
        $changed.stages = @($valid.stages | Where-Object { $_ -ne $stage })
        $changed | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath
        Reject { & $verifier -ResultPath $reportPath -CandidateSha256 $msiHash -CandidateBundleSha256 $bundleHash -RequireHistoricalUpgrade -RequireOfficialBundle }
    }
    Write-Host 'Historical contract tests passed. Fixtures are synthetic report/plan/log inputs; no product was installed and no machine acceptance is claimed.'
} finally { Remove-Item -LiteralPath $root -Recurse -Force }
