[CmdletBinding()]
param([Parameter(Mandatory)][string]$MsiPath, [Parameter(Mandatory)][string]$InventoryPath, [switch]$DisposableMachine)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$DisposableMachine -or $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1') {
    throw 'Installation tests require an explicitly disposable Windows machine and COMPUTATOR_DISPOSABLE_INSTALL_TEST=1.'
}
if (!(New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'MSI lifecycle validation requires elevation on the disposable machine.'
}
# Registry entries without DisplayName are valid. Avoid both StrictMode property
# errors and Win32_Product, whose enumeration can unexpectedly repair products.
$installed = @('HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*', 'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*') |
    ForEach-Object { Get-ItemProperty $_ -ErrorAction SilentlyContinue } | Where-Object {
        $_.PSObject.Properties['DisplayName'] -and $_.DisplayName -eq 'Computator.NET'
    }
if ($installed) { throw 'An existing Computator.NET installation is present; refusing to run destructive lifecycle tests.' }
[xml]$inventory = Get-Content -LiteralPath $InventoryPath -Raw
$productCode = [Guid]::Parse($inventory.Payload.ProductCode).ToString('B')
$msi = (Resolve-Path -LiteralPath $MsiPath).Path
$temporaryDirectory = (Resolve-Path -LiteralPath $env:TEMP).Path
$root = Join-Path $temporaryDirectory ('Computator-Installer-' + [Guid]::NewGuid().ToString('N'))
$application = Join-Path $root 'application'
$profile = Join-Path $root 'profile'
New-Item -ItemType Directory -Path $profile -Force | Out-Null
$sentinel = Join-Path $profile 'keep.tslf'
Set-Content -LiteralPath $sentinel 'static real Probe(real x) { return x; }'
function Invoke-Msi([string[]]$Arguments) {
    $process = Start-Process msiexec.exe -ArgumentList $Arguments -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -notin @(0,3010)) { throw "Windows Installer failed with exit code $($process.ExitCode)." }
}
$didInstall = $false
try {
    Invoke-Msi @('/i', ('"' + $msi + '"'), '/qn', '/norestart', ('INSTALLDIR="' + $application + '"'), '/l*v', ('"' + $root + '\install.log"'))
    $didInstall = $true
    foreach ($file in $inventory.Payload.File) {
        $path = Join-Path $application $file.Path
        if (!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.SHA256) { throw "Installed payload mismatch: $($file.Path)" }
    }
    & (Join-Path $PSScriptRoot '../Test-PublishedNet10Desktop.ps1') -PublishDirectory $application
    Remove-Item -LiteralPath (Join-Path $application 'Static/functions.xml')
    Invoke-Msi @('/fa', $productCode, '/qn', '/norestart', '/l*v', ('"' + $root + '\repair.log"'))
    if (!(Test-Path -LiteralPath (Join-Path $application 'Static/functions.xml'))) { throw 'Repair did not restore the application payload.' }
    Invoke-Msi @('/x', $productCode, '/qn', '/norestart', '/l*v', ('"' + $root + '\uninstall.log"'))
    $didInstall = $false
    if (Test-Path -LiteralPath (Join-Path $application 'Computator.NET.exe')) { throw 'Uninstall left the main executable behind.' }
    if (!(Test-Path -LiteralPath $sentinel)) { throw 'Uninstall removed user data.' }
    Write-Host 'MSI installation, payload hashes, launch, repair, uninstall and external user-data preservation passed.'
}
finally {
    try {
        if ($didInstall) { Invoke-Msi @('/x', $productCode, '/qn', '/norestart') }
    }
    finally {
        $logs = Join-Path $PSScriptRoot '../../artifacts/installer/logs'
        New-Item -ItemType Directory -Path $logs -Force | Out-Null
        Get-ChildItem -LiteralPath $root -Filter '*.log' -ErrorAction SilentlyContinue | Copy-Item -Destination $logs
        if (Test-Path -LiteralPath $root) {
            $resolvedRoot = (Resolve-Path -LiteralPath $root).Path
            $temporaryPrefix = $temporaryDirectory.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
            if (!$resolvedRoot.StartsWith($temporaryPrefix, [StringComparison]::OrdinalIgnoreCase) -or
                ((Get-Item -LiteralPath $resolvedRoot).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
                throw "Refusing to remove a redirected test directory or a path outside the temporary directory: $resolvedRoot"
            }
            Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
        }
    }
}
