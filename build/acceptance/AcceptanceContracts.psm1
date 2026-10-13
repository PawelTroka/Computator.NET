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
Export-ModuleMember -Function Assert-TransactionalUpgradeSequence
