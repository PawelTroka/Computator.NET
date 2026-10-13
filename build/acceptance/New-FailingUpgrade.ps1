[CmdletBinding()]
param([Parameter(Mandatory)][string]$Source, [Parameter(Mandatory)][string]$Destination)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1') { throw 'Negative MSI generation is permitted only inside the explicit disposable acceptance guest.' }
if (Test-Path -LiteralPath $Destination) { throw 'Refusing to overwrite a negative-test MSI.' }
Copy-Item -LiteralPath $Source -Destination $Destination
$installer = New-Object -ComObject WindowsInstaller.Installer
$database = $installer.OpenDatabase([IO.Path]::GetFullPath($Destination), 1)
function Query([string]$Sql) {
    $view = $database.OpenView($Sql); $view.Execute(); return $view
}
$view = Query 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''InstallFiles'''
$record = $view.Fetch(); $after = [int]$record.IntegerData(1); $view.Close()
$view = Query 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''InstallFinalize'''
$record = $view.Fetch(); $finalize = [int]$record.IntegerData(1); $view.Close()
$sequence = $after + 1
while ($sequence -lt $finalize) {
    $view = Query ('SELECT `Action` FROM `InstallExecuteSequence` WHERE `Sequence` = {0}' -f $sequence)
    $record = $view.Fetch(); $view.Close()
    if ($null -eq $record) { break }
    $sequence++
}
if ($sequence -ge $finalize) { throw 'No transactional sequence slot is available for a deliberate failure.' }
# Type 19 reports an error to Windows Installer. This copy is never a release
# artifact; its sole purpose is proving failed-upgrade transaction rollback.
$view = Query 'INSERT INTO `CustomAction` (`Action`, `Type`, `Target`) VALUES (''ComputatorAcceptanceFailure'', 19, ''Intentional disposable-machine rollback test'')'; $view.Close()
$view = Query ('INSERT INTO `InstallExecuteSequence` (`Action`, `Condition`, `Sequence`) VALUES (''ComputatorAcceptanceFailure'', ''1'', {0})' -f $sequence); $view.Close()
$database.Commit()
$summary = $database.SummaryInformation(20)
$summary.GetType().InvokeMember('Property', [Reflection.BindingFlags]::SetProperty, $null, $summary, @(9, [Guid]::NewGuid().ToString('B'))) | Out-Null
$summary.Persist()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary) | Out-Null
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) | Out-Null
