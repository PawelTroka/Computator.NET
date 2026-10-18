[CmdletBinding()]
param([Parameter(Mandatory)][string]$Source, [Parameter(Mandatory)][string]$Destination)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'AcceptanceContracts.psm1') -Force
if ($env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1') { throw 'Negative MSI generation is permitted only inside the explicit disposable acceptance guest.' }
if (Test-Path -LiteralPath $Destination) { throw 'Refusing to overwrite a negative-test MSI.' }
Copy-Item -LiteralPath $Source -Destination $Destination
$installer = $null; $database = $null; $summary = $null
function Query([string]$Sql) {
    $view = $database.OpenView($Sql)
    try {
        # COM void returns must not join the view in the PowerShell pipeline.
        [void]$view.Execute()
        return $view
    } catch {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)
        throw
    }
}
function ReadValue([string]$Sql, [switch]$Integer) {
    $view = Query $Sql; $record = $null
    try {
        $record = $view.Fetch()
        if ($null -eq $record) { return $null }
        if ($Integer) { return [int]$record.IntegerData(1) }
        return [string]$record.StringData(1)
    } finally {
        if ($null -ne $record) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        [void]$view.Close()
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)
    }
}
function ExecuteSql([string]$Sql) {
    $view = Query $Sql
    try { [void]$view.Close() }
    finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
}
try {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.OpenDatabase([IO.Path]::GetFullPath($Destination), 1)
    $after = [int](ReadValue 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''InstallFiles''' -Integer)
    $finalize = [int](ReadValue 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''InstallFinalize''' -Integer)
    $initialize = [int](ReadValue 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''InstallInitialize''' -Integer)
    $removeExisting = [int](ReadValue 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''RemoveExistingProducts''' -Integer)
    Assert-TransactionalUpgradeSequence $initialize $removeExisting $after $finalize
    $sequence = $after + 1
    while ($sequence -lt $finalize) {
        $occupied = ReadValue ('SELECT `Action` FROM `InstallExecuteSequence` WHERE `Sequence` = {0}' -f $sequence)
        if ($null -eq $occupied) { break }
        $sequence++
    }
    if ($sequence -ge $finalize) { throw 'No transactional sequence slot is available for a deliberate failure.' }
    # Type 19 deliberately fails inside the MSI transaction. Never ship this copy.
    ExecuteSql 'INSERT INTO `CustomAction` (`Action`, `Type`, `Target`) VALUES (''ComputatorAcceptanceFailure'', 19, ''Intentional disposable-machine rollback test'')'
    ExecuteSql ('INSERT INTO `InstallExecuteSequence` (`Action`, `Condition`, `Sequence`) VALUES (''ComputatorAcceptanceFailure'', ''1'', {0})' -f $sequence)
    $summary = $database.SummaryInformation(20)
    [void]$summary.GetType().InvokeMember('Property', [Reflection.BindingFlags]::SetProperty, $null, $summary, @(9, [Guid]::NewGuid().ToString('B')))
    [void]$summary.Persist()
    [void]$database.Commit()
} finally {
    # Close() ends a view's result set but does not release its COM handle. Every
    # record/view above must be released too, or the generated MSI remains locked.
    if ($null -ne $summary) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary) }
    if ($null -ne $database) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) }
    if ($null -ne $installer) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) }
}
