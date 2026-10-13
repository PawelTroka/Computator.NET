[CmdletBinding()]
param([Parameter(Mandatory)][string]$PackagePath, [Parameter(Mandatory)][string]$ExpectedSignerThumbprint,
    [string]$BaselinePackagePath, [switch]$DisposableMachine)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$IsWindows -or !$DisposableMachine -or $env:COMPUTATOR_DISPOSABLE_INSTALL_TEST -ne '1') {
    throw 'Package activation acceptance requires an explicitly disposable interactive Windows account.'
}
if (!(Test-Path (Join-Path $env:USERPROFILE 'Computator-MSIX-Acceptance-Account.txt'))) {
    throw 'Create the acceptance marker only inside a dedicated disposable Windows account, not your everyday profile.'
}
$name = 'TROKASoftware.Computator.NET'
if (Get-AppxPackage -Name $name) { throw 'Refusing to replace an existing installation.' }
function ApprovedPackage([string]$Path) {
    $full = (Resolve-Path -LiteralPath $Path).Path
    $signature = Get-AuthenticodeSignature -LiteralPath $full
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Thumbprint -ne $ExpectedSignerThumbprint) {
        throw 'The owner-approved, already-trusted signing identity is required. No certificate will be imported.'
    }
    return $full
}
$candidate = ApprovedPackage $PackagePath
$baseline = if ($BaselinePackagePath) { ApprovedPackage $BaselinePackagePath } else { $null }
Add-Type -Path (Join-Path $PSScriptRoot 'PackageActivation.cs')
$installed = $null
$owned = Join-Path $env:TEMP ('Computator-MSIX-Activation-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $owned | Out-Null
$evidence = [Collections.Generic.List[object]]::new()
function VerifyActivation($package, [string]$document) {
    $manifest = Get-AppxPackageManifest -Package $package.PackageFullName
    $app = @($manifest.Package.Applications.Application)[0]
    $aumid = $package.PackageFamilyName + '!' + $app.Id
    $started = Get-Date
    $number = [Computator.Acceptance.PackageActivation]::LaunchAndVerify($aumid, $package.PackageFullName, $document)
    $process = Get-Process -Id $number
    # Do not close an unrelated pre-existing application returned by activation.
    if ($process.StartTime -lt $started.AddSeconds(-2)) { throw 'Activation reused a pre-existing process; close it manually in the disposable account.' }
    try {
        if (!$process.WaitForInputIdle(30000)) { throw 'The activated package did not become responsive.' }
        $deadline = (Get-Date).AddSeconds(30)
        do { $process.Refresh(); if ($process.MainWindowHandle -ne 0) { break }; Start-Sleep -Milliseconds 100 } while ((Get-Date) -lt $deadline)
        if ($process.MainWindowHandle -eq 0) { throw 'No main window after package activation.' }
        if (!$process.CloseMainWindow() -or !$process.WaitForExit(15000)) { throw 'Package did not close normally after activation.' }
        if ($process.ExitCode -ne 0) { throw 'Activated package exited unsuccessfully.' }
        $evidence.Add([ordered]@{ package=$package.PackageFullName; mode=$(if ($document) {'Windows.File'} else {'Application'}); success=$true })
    } finally { if (!$process.HasExited) { $process.Kill($true); $process.WaitForExit() }; $process.Dispose() }
}
try {
    if ($baseline) {
        Add-AppxPackage -Path $baseline
        $installed = Get-AppxPackage -Name $name
        VerifyActivation $installed $null
        $oldVersion = [Version]$installed.Version
    }
    Add-AppxPackage -Path $candidate
    $installed = Get-AppxPackage -Name $name
    if (!$installed) { throw 'The candidate did not install under the preserved identity.' }
    if ($baseline -and [Version]$installed.Version -le $oldVersion) { throw 'The candidate did not advance the baseline version.' }
    VerifyActivation $installed $null
    foreach ($extension in @('.tsl', '.tslf')) {
        $file = Join-Path $owned ('Żółć activation' + $extension)
        # Opening inert trusted content exercises the file contract, not arbitrary code.
        [IO.File]::WriteAllText($file, '// package activation acceptance')
        VerifyActivation $installed $file
    }
    Remove-AppxPackage -Package $installed.PackageFullName
    $installed = $null
    if (Get-AppxPackage -Name $name) { throw 'Package removal failed.' }
    $evidence | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $owned 'activation.json') -Encoding utf8
    Write-Host "Signed package identity and file-contract acceptance recorded in $owned"
} finally {
    if ($installed) { Remove-AppxPackage -Package $installed.PackageFullName }
    # Keep this small private evidence directory. Never collect the account's real documents.
}
