[CmdletBinding()]
param([Parameter(Mandatory)][string]$PackagePath, [switch]$DisposableMachine)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$DisposableMachine -or $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1') { throw 'Use an explicitly disposable Windows test account/machine.' }
$name = 'TROKASoftware.Computator.NET'
if (Get-AppxPackage -Name $name) { throw 'An existing Store installation is present; refusing to replace it automatically.' }
$path = (Resolve-Path -LiteralPath $PackagePath).Path
$signature = Get-AuthenticodeSignature -LiteralPath $path
if ($signature.Status -ne 'Valid') { throw 'Use an owner-signed package whose certificate is already trusted. This test never imports signing keys/certificates.' }
$installed = $null
try {
    Add-AppxPackage -Path $path
    $installed = Get-AppxPackage -Name $name
    if (!$installed) { throw 'The package did not install under the preserved identity.' }
    $manifest = Get-AppxPackageManifest -Package $installed.PackageFullName
    if (!$manifest.Package.Applications.Application) { throw 'Installed activation entry is missing.' }
    $executable = Join-Path $installed.InstallLocation 'Computator.NET.exe'
    if (!(Test-Path -LiteralPath $executable)) { throw 'Installed desktop payload is missing.' }
    # Direct launch verifies the immutable installed payload; Store activation and
    # document activation require separate interactive acceptance, not a false claim.
    & (Join-Path $PSScriptRoot '../Test-PublishedNet10Desktop.ps1') -PublishDirectory $installed.InstallLocation
    Remove-AppxPackage -Package $installed.PackageFullName
    $installed = $null
    if (Get-AppxPackage -Name $name) { throw 'The test package remained installed after removal.' }
    Write-Host 'Signed MSIX install, installed-payload launch and uninstall passed.'
}
finally {
    if ($installed) { Remove-AppxPackage -Package $installed.PackageFullName }
}
