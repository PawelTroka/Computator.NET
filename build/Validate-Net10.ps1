[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [ValidateSet('All','Computator.NET.Core.Tests','Computator.NET.Core.IntegrationTests','Computator.NET.WebApi.IntegrationTests','Computator.NET.Desktop.Tests')][string]$TestSuite = 'All',
    [switch]$UpdateLocks
)
$ErrorActionPreference = 'Stop'
if (!$IsWindows) { throw 'Desktop and chart-backed API validation requires Windows.' }
$oldData = $env:COMPUTATOR_DATA_DIRECTORY
$oldFixture = $env:COMPUTATOR_LEGACY_FIXTURE
$root = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Net10-Validation-' + [Guid]::NewGuid().ToString('N'))
try {
    $env:COMPUTATOR_DATA_DIRECTORY = Join-Path $root 'profile'
    $env:COMPUTATOR_LEGACY_FIXTURE = Join-Path $root 'fixture/settings.dat'
    & powershell.exe -NoProfile -File (Join-Path $PSScriptRoot 'Generate-LegacySettingsFixture.ps1') -OutputPath $env:COMPUTATOR_LEGACY_FIXTURE
    if ($LASTEXITCODE -ne 0) { throw 'The legacy fixture writer failed.' }
    & (Join-Path $PSScriptRoot 'Validate-Migration.ps1') -Configuration $Configuration -TestSuite $TestSuite -UpdateLocks:$UpdateLocks
}
finally {
    $env:COMPUTATOR_DATA_DIRECTORY = $oldData
    $env:COMPUTATOR_LEGACY_FIXTURE = $oldFixture
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
