# Exercise the actual Windows Installer COM adapter without installing a product.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) {
    Write-Host 'Windows Installer COM regression tests require Windows; not executed on this host.'
    return
}
$root = Join-Path ([IO.Path]::GetTempPath()) ('Computator-MsiInterop-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
$installer = $null; $database = $null
function HelperDefinition([string]$File, [string]$Name) {
    $tokens = $null; $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot $File), [ref]$tokens, [ref]$errors)
    if ($errors.Count -ne 0) { throw "Acceptance source failed parsing: $File" }
    $function = $ast.Find({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $Name }, $true)
    if (!$function) { throw "Missing MSI helper: $Name" }
    return [scriptblock]::Create($function.Extent.Text)
}
try {
    # Import only helpers. The lifecycle scripts themselves must never run here.
    . (HelperDefinition 'New-FailingUpgrade.ps1' 'Query')
    . (HelperDefinition 'New-FailingUpgrade.ps1' 'ReadValue')
    $path = Join-Path $root 'read-only-contract.msi'
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.OpenDatabase($path, 3)
    foreach ($sql in @(
        'CREATE TABLE `Property` (`Property` CHAR(72) NOT NULL, `Value` CHAR(0) LOCALIZABLE PRIMARY KEY `Property`)',
        'CREATE TABLE `InstallExecuteSequence` (`Action` CHAR(72) NOT NULL, `Condition` CHAR(255), `Sequence` SHORT PRIMARY KEY `Action`)',
        'CREATE TABLE `CustomAction` (`Action` CHAR(72) NOT NULL, `Type` SHORT NOT NULL, `Source` CHAR(72), `Target` CHAR(255) PRIMARY KEY `Action`)'
    )) {
        $view = $database.OpenView($sql)
        try { [void]$view.Execute() } finally { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
    }
    $expected = [ordered]@{
        ProductCode = '{5EE11F5A-4F1E-FC4E-A296-BC713710E32E}'
        UpgradeCode = '{86D89770-B31B-4467-BC5E-B54B0FF263A1}'
        ProductVersion = '3.0.0'
    }
    foreach ($name in $expected.Keys) {
        $view = $database.OpenView(('INSERT INTO `Property` (`Property`, `Value`) VALUES (''{0}'', ''{1}'')' -f $name, $expected[$name]))
        try { [void]$view.Execute() } finally { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
    }
    foreach ($entry in @(@('InstallInitialize',1500),@('RemoveExistingProducts',1501),@('InstallFiles',4000),@('OccupiedSlot',4001),@('InstallFinalize',6600))) {
        $view = $database.OpenView(('INSERT INTO `InstallExecuteSequence` (`Action`, `Sequence`) VALUES (''{0}'', {1})' -f $entry[0], $entry[1]))
        try { [void]$view.Execute() } finally { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
    }
    [void]$database.Commit()
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database); $database = $null
    foreach ($source in @('Test-HistoricalUpgrade.ps1','Guest-Acceptance.ps1')) {
        . (HelperDefinition $source 'Property')
        foreach ($name in $expected.Keys) {
            $values = @(Property $path $name)
            if ($values.Count -ne 1 -or $values[0] -isnot [string] -or $values[0] -cne $expected[$name]) {
                throw "MSI reader must return exactly the property string, without COM null outputs: $source/$name; count=$($values.Count)"
            }
        }
        $missingRejected = $false
        try { Property $path 'MissingProperty' | Out-Null } catch {
            if ($_.Exception.Message -notlike '*Missing MSI property*') { throw }
            $missingRejected = $true
        }
        if (!$missingRejected) { throw "The MSI reader accepted a missing property: $source" }
    }
    $database = $installer.OpenDatabase($path, 0)
    $values = @(Query 'SELECT `Value` FROM `Property` WHERE `Property` = ''ProductVersion''')
    if ($values.Count -ne 1 -or ![Runtime.InteropServices.Marshal]::IsComObject($values[0])) { throw 'The negative-fixture query did not return exactly one COM view.' }
    $view = $values[0]; $record = $null
    try {
        $record = $view.Fetch()
        if (!$record -or $record.StringData(1) -cne '3.0.0') { throw 'The negative-fixture query returned the wrong record.' }
    } finally {
        if ($null -ne $record) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)
    }
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database); $database = $null
    $sourceHash = (Get-FileHash -LiteralPath $path).Hash
    $negative = Join-Path $root 'never-install-this-contract.msi'
    $consent = $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST
    try {
        # Only the non-installable synthetic database in this temporary fixture is
        # modified. No real installer or msiexec is invoked by these contracts.
        $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST = '1'
        & (Join-Path $PSScriptRoot 'New-FailingUpgrade.ps1') -Source $path -Destination $negative
    } finally { [Environment]::SetEnvironmentVariable('COMPUTATOR_DISPOSABLE_INSTALL_TEST', $consent) }
    if ((Get-FileHash -LiteralPath $path).Hash -ne $sourceHash -or (Get-FileHash -LiteralPath $negative).Hash -eq $sourceHash) { throw 'Negative fixture changed its source or did not change the copy.' }
    $exclusive = [IO.File]::Open($negative, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    $exclusive.Dispose()
    $database = $installer.OpenDatabase($negative, 0)
    if ((ReadValue 'SELECT `Type` FROM `CustomAction` WHERE `Action` = ''ComputatorAcceptanceFailure''' -Integer) -ne 19 -or
        (ReadValue 'SELECT `Sequence` FROM `InstallExecuteSequence` WHERE `Action` = ''ComputatorAcceptanceFailure''' -Integer) -ne 4002) { throw 'Negative fixture did not add the intentional failure in the first free transactional slot.' }
    Write-Host "Windows Installer COM regressions passed on PowerShell $($PSVersionTable.PSVersion): both readers, missing-property rejection, one query view, negative fixture sequence, unchanged source and no file lock. No product was installed."
} finally {
    if ($null -ne $database) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) }
    if ($null -ne $installer) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) }
    Remove-Item -LiteralPath $root -Recurse -Force
}
