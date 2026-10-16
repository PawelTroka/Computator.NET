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
try {
    # Import only the production helper functions, never the destructive scripts.
    foreach ($definition in @(@('Test-HistoricalUpgrade.ps1','Property'), @('New-FailingUpgrade.ps1','Query'))) {
        $tokens = $null; $errors = $null
        $ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot $definition[0]), [ref]$tokens, [ref]$errors)
        if ($errors.Count -ne 0) { throw "Acceptance source failed parsing: $($definition[0])" }
        $name = $definition[1]
        $function = $ast.Find({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name }, $true)
        if (!$function) { throw "Missing MSI helper: $name" }
        . ([scriptblock]::Create($function.Extent.Text))
    }
    $path = Join-Path $root 'read-only-contract.msi'
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.OpenDatabase($path, 3)
    $sql = 'CREATE TABLE `Property` (`Property` CHAR(72) NOT NULL, `Value` CHAR(0) LOCALIZABLE PRIMARY KEY `Property`)'
    $view = $database.OpenView($sql)
    try { [void]$view.Execute() } finally { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
    $expected = [ordered]@{
        ProductCode = '{5EE11F5A-4F1E-FC4E-A296-BC713710E32E}'
        UpgradeCode = '{86D89770-B31B-4467-BC5E-B54B0FF263A1}'
        ProductVersion = '3.0.0'
    }
    foreach ($name in $expected.Keys) {
        $view = $database.OpenView(('INSERT INTO `Property` (`Property`, `Value`) VALUES (''{0}'', ''{1}'')' -f $name, $expected[$name]))
        try { [void]$view.Execute() } finally { [void]$view.Close(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) }
    }
    [void]$database.Commit()
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database); $database = $null
    foreach ($name in $expected.Keys) {
        $values = @(Property $path $name)
        if ($values.Count -ne 1 -or $values[0] -isnot [string] -or $values[0] -cne $expected[$name]) {
            throw "MSI reader must return exactly the property string, without COM null outputs: $name; count=$($values.Count)"
        }
    }
    $missingRejected = $false
    try { Property $path 'MissingProperty' | Out-Null } catch {
        if ($_.Exception.Message -notlike '*Missing MSI property*') { throw }
        $missingRejected = $true
    }
    if (!$missingRejected) { throw 'The MSI reader accepted a missing property.' }
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
    Write-Host "Windows Installer COM regressions passed on PowerShell $($PSVersionTable.PSVersion): three scalar properties, missing-property rejection and one query view. No product was installed."
} finally {
    if ($null -ne $database) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) }
    if ($null -ne $installer) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer) }
    Remove-Item -LiteralPath $root -Recurse -Force
}
