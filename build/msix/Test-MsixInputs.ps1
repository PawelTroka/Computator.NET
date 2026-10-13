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
