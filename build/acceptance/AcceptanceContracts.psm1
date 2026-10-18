Set-StrictMode -Version Latest
function Assert-TransactionalUpgradeSequence([int]$Initialize, [int]$RemoveExisting, [int]$InstallFiles, [int]$Finalize) {
    # The deliberate error must follow an actual removal inside the transaction.
    # A late RemoveExistingProducts would leave the old product untouched and
    # make the old registration/hash checks falsely appear to prove rollback.
    if ($Initialize -le 0 -or $RemoveExisting -le $Initialize -or
        $InstallFiles -le $RemoveExisting -or $Finalize -le $InstallFiles) {
        throw 'Rollback acceptance requires InstallInitialize < RemoveExistingProducts < InstallFiles < InstallFinalize.'
    }
}
function Assert-HistoricalRollbackLog([string]$Log, [string]$LegacyCode, [string]$CandidateCode) {
    # A nested uninstall does not necessarily emit the top-level ARP summary
    # "Removal completed successfully". Require its actual execution instead.
    $removeStart = [regex]::Match($Log, '(?im)^Action start [^\r\n]*: RemoveExistingProducts\.')
    $removeEnd = [regex]::Match($Log, '(?im)^Action ended [^\r\n]*: RemoveExistingProducts\. Return value 1\.')
    $failureStart = [regex]::Match($Log, '(?im)^Action start [^\r\n]*: ComputatorAcceptanceFailure\.')
    $failureEnd = [regex]::Match($Log, '(?im)^Action ended [^\r\n]*: ComputatorAcceptanceFailure\. Return value 3\.')
    if (!$removeStart.Success -or !$removeEnd.Success -or !$failureStart.Success -or !$failureEnd.Success -or
        $removeEnd.Index -le $removeStart.Index -or $failureStart.Index -le $removeEnd.Index -or $failureEnd.Index -le $failureStart.Index) {
        throw 'MSI log must show completed old-product removal before the intentional action fails.'
    }
    $removal = $Log.Substring($removeStart.Index, $removeEnd.Index - $removeStart.Index)
    foreach ($pattern in @(
        ('(?im)^Property\(N\): ProductCode = ' + [regex]::Escape($LegacyCode) + '\s*$'),
        ('(?im)^Property\(N\): UPGRADINGPRODUCTCODE = ' + [regex]::Escape($CandidateCode) + '\s*$'),
        '(?im)^Property\(N\): REMOVE = ALL\s*$',
        '(?im)^Action ended [^\r\n]*: INSTALL\. Return value 1\.',
        'Executing op: FileRemove\([^\r\n]*FileName=Computator\.NET\.exe[,)]'
    )) {
        if ($removal -notmatch $pattern) { throw 'MSI log does not prove successful nested removal of the pinned historical product and executable.' }
    }
    $failure = $Log.Substring($failureStart.Index, $failureEnd.Index - $failureStart.Index)
    if (!$failure.Contains('Intentional disposable-machine rollback test')) { throw 'The failing action did not report the expected injected error.' }
    $rollback = $Log.Substring($failureEnd.Index + $failureEnd.Length)
    if (!$rollback.Contains('Executing op: RollbackInfo(')) { throw 'MSI log does not show rollback execution after the intentional failure.' }
    return @{
        removedProductCode = $LegacyCode; upgradingProductCode = $CandidateCode
        failureAction = 'ComputatorAcceptanceFailure'; failureReturnValue = 3
        # Preserve engine diagnostics for review; registry, file, settings and
        # launch assertions must still independently establish actual recovery.
        engineDiagnostics = @([regex]::Matches($rollback, '(?im)^[^\r\n]*Error in rollback skipped[^\r\n]*') | ForEach-Object { $_.Value })
    }
}
Export-ModuleMember -Function Assert-TransactionalUpgradeSequence, Assert-HistoricalRollbackLog
