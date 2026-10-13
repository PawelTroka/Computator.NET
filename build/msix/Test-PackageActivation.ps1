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
Import-Module (Join-Path $PSScriptRoot 'ActivationContracts.psm1') -Force
function ApprovedPackage([string]$Path) {
    $full = (Resolve-Path -LiteralPath $Path).Path
    $signature = Get-AuthenticodeSignature -LiteralPath $full
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Thumbprint -ne $ExpectedSignerThumbprint) {
        throw 'The owner-approved, already-trusted signing identity is required. No certificate will be imported.'
    }
    return [pscustomobject]@{ Path=$full; Identity=(Read-ActivationPackageIdentity $full); Sha256=(Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash }
}
$candidate = ApprovedPackage $PackagePath
$baseline = if ($BaselinePackagePath) { ApprovedPackage $BaselinePackagePath } else { $null }
if ($baseline) { Assert-ActivationUpgrade $candidate.Identity $baseline.Identity }
Add-Type -Path (Join-Path $PSScriptRoot 'PackageActivation.cs')
$installed = $null
$owned = Join-Path $env:TEMP ('Computator-MSIX-Activation-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $owned | Out-Null
$evidence = [Collections.Generic.List[object]]::new()
$report = [ordered]@{ schemaVersion=1; status='running'; startedUtc=[DateTime]::UtcNow.ToString('o')
    candidateSha256=$candidate.Sha256; candidateIdentity=$candidate.Identity
    baselineSha256=$(if ($baseline) { $baseline.Sha256 } else { $null })
    historicalUpgrade=$(if ($baseline) { 'requested' } else { 'not-requested' }); stages=@() }
$documents = @()
foreach ($extension in @('.TSL', '.TsLf')) {
    $file = Join-Path $owned ('Żółć activation' + $extension)
    # Synthetic inert content only. The hashes must survive both opening and removal.
    [IO.File]::WriteAllText($file, '// package activation acceptance')
    $documents += [pscustomobject]@{ Path=$file; Hash=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash }
}
function AssertInstalledIdentity($package, $expected) {
    if (!$package -or @($package).Count -ne 1 -or $package.Name -cne $expected.Name -or
        $package.Publisher -cne $expected.Publisher -or [string]$package.Architecture -ine $expected.Architecture -or
        [Version]$package.Version -ne [Version]$expected.Version) { throw 'Installed identity differs from the approved input manifest.' }
}
function VerifyActivation($package, [string]$document) {
    $manifest = Get-AppxPackageManifest -Package $package.PackageFullName
    $app = @($manifest.Package.Applications.Application)[0]
    $aumid = $package.PackageFamilyName + '!' + $app.Id
    $started = Get-Date
    $number = [Computator.Acceptance.PackageActivation]::LaunchAndVerify($aumid, $package.PackageFullName, $document)
    $process = Get-Process -Id $number
    # Do not close an unrelated pre-existing application returned by activation.
    if ($process.StartTime -lt $started.AddSeconds(-2)) {
        $process.Dispose()
        throw 'Activation reused a pre-existing process; close it manually in the disposable account.'
    }
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
        Add-AppxPackage -Path $baseline.Path
        $installed = Get-AppxPackage -Name $name
        AssertInstalledIdentity $installed $baseline.Identity
        VerifyActivation $installed $null
        $stateDirectory = Join-Path $env:LOCALAPPDATA ('Packages/' + $installed.PackageFamilyName + '/LocalState')
        New-Item -ItemType Directory -Path $stateDirectory -Force | Out-Null
        $stateFile = Join-Path $stateDirectory ('Computator-Acceptance-' + [Guid]::NewGuid().ToString('N') + '.txt')
        [IO.File]::WriteAllText($stateFile, 'synthetic state that must survive an upgrade')
        $stateHash = (Get-FileHash -LiteralPath $stateFile -Algorithm SHA256).Hash
    }
    Add-AppxPackage -Path $candidate.Path
    $installed = Get-AppxPackage -Name $name
    AssertInstalledIdentity $installed $candidate.Identity
    VerifyActivation $installed $null
    foreach ($document in $documents) {
        VerifyActivation $installed $document.Path
        Assert-ActivationDocumentPreserved $document.Path $document.Hash
    }
    if ($baseline) {
        Assert-ActivationDocumentPreserved $stateFile $stateHash
        $report.historicalUpgrade = 'passed'
        $report.stages += 'baseline-state-preserved-through-upgrade'
    }
    Remove-AppxPackage -Package $installed.PackageFullName
    $installed = $null
    if (Get-AppxPackage -Name $name) { throw 'Package removal failed.' }
    foreach ($document in $documents) { Assert-ActivationDocumentPreserved $document.Path $document.Hash }
    $report.stages += 'external-documents-preserved-after-removal'
    $report.status = 'passed'
} catch {
    $report.status = 'failed'; $report.error = $_.Exception.Message
    Write-Error $_ -ErrorAction Continue
} finally {
    try {
        # Re-query after deployment errors: Add-AppxPackage can have registered the
        # approved product before a later check fails. Never remove another identity.
        $versions = @($candidate.Identity.Version)
        if ($baseline) { $versions += $baseline.Identity.Version }
        foreach ($remaining in @(Get-AppxPackage -Name $name)) {
            if ($remaining.Publisher -cne $candidate.Identity.Publisher -or [string]$remaining.Version -notin $versions) {
                throw 'Refusing to remove a package that differs from the approved inputs.'
            }
            Remove-AppxPackage -Package $remaining.PackageFullName
        }
        if (Get-AppxPackage -Name $name) { throw 'Acceptance cleanup left the package installed.' }
    } catch {
        $report.status = 'failed'; $report.cleanupError = $_.Exception.Message
    } finally {
        $report.completedUtc = [DateTime]::UtcNow.ToString('o')
        $report.activations = @($evidence.ToArray())
        $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $owned 'activation.json') -Encoding utf8
    }
    # Keep this small private evidence directory. Never collect the account's real documents.
}
if ($report.status -ne 'passed') { throw "Signed package acceptance failed; inspect $owned" }
Write-Host "Signed package identity, file-contract and synthetic preservation acceptance recorded in $owned"
