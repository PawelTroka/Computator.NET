$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Msix.psm1') -Force
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
[xml]$manifest = Get-Content (Join-Path $PSScriptRoot 'AppxManifest.xml') -Raw
[xml]$legacy = Get-Content (Join-Path $root 'build-uwp/AppxManifest.xml') -Raw
Assert-MsixIdentity $manifest $legacy
Assert-MsixVersion '2.4.0.0'
foreach ($bad in @('2.4.0', '2.4.0.1', '0.4.0.0', '65536.1.1.0', '2.-1.0.0', '2.4.preview.0')) {
    $rejected = $false
    try { Assert-MsixVersion $bad } catch { $rejected = $true }
    if (!$rejected) { throw "Version guard accepted $bad" }
}
foreach ($attribute in @('Name', 'Publisher', 'ProcessorArchitecture')) {
    [xml]$changed = $manifest.OuterXml
    $changed.Package.Identity.SetAttribute($attribute, 'unexpected')
    $rejected = $false
    try { Assert-MsixIdentity $changed $legacy } catch { $rejected = $true }
    if (!$rejected) { throw "Identity guard accepted changed $attribute" }
}
Write-Host 'MSIX input guards passed: historical identity, associations, version limits and rejection cases.'
$temporarySdk = Join-Path ([IO.Path]::GetTempPath()) ('Computator-SDK-' + [Guid]::NewGuid().ToString('N'))
try {
    foreach ($relative in @('first/bin/10.0.19041.0/x64', 'second/bin/10.0.26100.0/x64', 'second/bin/10.0.30000.0/arm64')) {
        $directory = Join-Path $temporarySdk $relative
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $directory 'makeappx.exe') | Out-Null
    }
    $selected = Get-MakeAppxPath -SdkRoots @((Join-Path $temporarySdk 'missing'), (Join-Path $temporarySdk 'first'), (Join-Path $temporarySdk 'second'))
    if ($selected -ne (Join-Path $temporarySdk 'second/bin/10.0.26100.0/x64/makeappx.exe')) {
        throw 'SDK discovery did not select the newest existing x64 tool across all registered roots.'
    }
    $rejected = $false
    try { Get-MakeAppxPath -SdkRoots @((Join-Path $temporarySdk 'missing')) } catch { $rejected = $true }
    if (!$rejected) { throw 'SDK discovery accepted a missing SDK installation.' }
    Write-Host 'MSIX SDK discovery passed: stale roots, multiple versions, architecture and missing-tool rejection.'
} finally {
    if (Test-Path -LiteralPath $temporarySdk) { Remove-Item -LiteralPath $temporarySdk -Recurse -Force }
}
