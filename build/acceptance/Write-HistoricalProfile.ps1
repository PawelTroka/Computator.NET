# Run only in a disposable Windows guest, using Windows PowerShell 5.1.
# This deliberately uses the historical assembly, not a recreated NRBF schema.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$InstallDirectory, [switch]$VerifyOnly)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($PSVersionTable.PSEdition -ne 'Desktop' -or $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1') {
    throw 'Historical profile fixtures require Windows PowerShell 5.1 in the explicit disposable acceptance environment.'
}
$profile = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Computator.NET'
$settingsPath = Join-Path $profile 'settings.dat'
if (!$VerifyOnly -and (Test-Path -LiteralPath $settingsPath)) { throw 'Refusing to overwrite an existing historical profile.' }
if (Test-Path -LiteralPath (Join-Path $profile 'settings.json')) { throw 'Historical fixture requires a profile without modern settings.' }
New-Item -ItemType Directory -Path $profile -Force | Out-Null
[Environment]::SetEnvironmentVariable('COMPUTATOR_DATA_DIRECTORY', $null)
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $InstallDirectory 'Computator.NET.Core.dll'))
$type = $assembly.GetType('Computator.NET.Core.Properties.Settings', $true)
$settings = $type.GetProperty('Default').GetValue($null, $null)
$expected = @{
    ScriptingDirectory = (Join-Path $profile 'Acceptance scripts')
    CustomFunctionsDirectory = (Join-Path $profile 'Acceptance functions')
    WorkingDirectory = (Join-Path $profile 'Acceptance workspace')
}
if (!$VerifyOnly) {
    $settings.Language = New-Object Globalization.CultureInfo('pl-PL')
    $settings.ShowReturnTypeInExpression = $true
    $settings.ShowParametersTypeInExpression = $false
    $settings.ShowReturnTypeInScripting = $false
    $settings.ShowParametersTypeInScripting = $false
    foreach ($key in $expected.Keys) { $settings.$key = $expected[$key]; New-Item -ItemType Directory -Path $expected[$key] -Force | Out-Null }
    $settings.ExpressionFont = New-Object Drawing.Font('Cambria', 18.25, [Drawing.FontStyle]::Bold)
    $settings.ScriptingFont = New-Object Drawing.Font('Consolas', 14.5, [Drawing.FontStyle]::Italic)
    $settings.Save()
}
if ($settings.Language.Name -ne 'pl-PL' -or !$settings.ShowReturnTypeInExpression -or
    $settings.ShowParametersTypeInExpression -or $settings.ShowReturnTypeInScripting -or $settings.ShowParametersTypeInScripting -or
    $settings.ExpressionFont.Size -ne 18.25 -or $settings.ScriptingFont.Size -ne 14.5) { throw 'Historical settings did not retain the non-default fixture values.' }
foreach ($key in $expected.Keys) { if ($settings.$key -ne $expected[$key]) { throw "Historical directory setting differs: $key" } }
if (!(Test-Path -LiteralPath $settingsPath)) { throw 'Historical settings were not persisted by the actual 2.3.2 application assembly.' }
Write-Host "Actual 2.3.2 settings fixture verified: $settingsPath"
