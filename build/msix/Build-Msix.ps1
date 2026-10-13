[CmdletBinding()]
param([Parameter(Mandatory)][string]$PayloadDirectory, [Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'Msix.psm1') -Force
Assert-MsixVersion $Version
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$payload = (Resolve-Path -LiteralPath $PayloadDirectory).Path
$artifact = Join-Path $root 'artifacts/msix'
$layout = Join-Path $artifact 'layout'
$unpacked = Join-Path $artifact 'unpacked'
foreach ($owned in @($layout, $unpacked)) {
    if (Test-Path -LiteralPath $owned) { Remove-Item -LiteralPath $owned -Recurse -Force }
    New-Item -ItemType Directory -Path $owned -Force | Out-Null
}
foreach ($item in Get-ChildItem -LiteralPath $payload -Recurse -Force) {
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Redirected files/directories must not enter a Store package.' }
    if (!$item.PSIsContainer -and $item.Extension -in @('.pfx','.p12','.snk','.pem')) { throw 'Signing material must not enter a Store package.' }
}
Copy-Item (Join-Path $payload '*') -Destination $layout -Recurse -Force
Copy-Item (Join-Path $root 'Graphics/Assets') -Destination (Join-Path $layout 'Assets') -Recurse -Force
[xml]$manifest = Get-Content (Join-Path $PSScriptRoot 'AppxManifest.xml') -Raw
[xml]$legacy = Get-Content (Join-Path $root 'build-uwp/AppxManifest.xml') -Raw
$manifest.Package.Identity.SetAttribute('Version', $Version)
Assert-MsixIdentity $manifest $legacy
$manifest.Save((Join-Path $layout 'AppxManifest.xml'))
foreach ($file in @('Computator.NET.exe','Computator.NET.dll','Computator.NET.runtimeconfig.json','coreclr.dll','hostfxr.dll','Static/functions.xml','LICENSE')) {
    if (!(Test-Path -LiteralPath (Join-Path $layout $file))) { throw "Missing MSIX payload: $file" }
}
foreach ($attribute in $manifest.SelectNodes('//@*[contains(local-name(),"Logo")]')) {
    if (!(Test-Path -LiteralPath (Join-Path $layout $attribute.Value))) { throw "Missing manifest image: $($attribute.Value)" }
}
foreach ($logo in $manifest.SelectNodes('//*[local-name()="Logo"]')) {
    if (!(Test-Path -LiteralPath (Join-Path $layout $logo.InnerText))) { throw "Missing manifest logo: $($logo.InnerText)" }
}
$files = @(Get-ChildItem -LiteralPath $layout -Recurse -File | ForEach-Object {
    [ordered]@{ path = [IO.Path]::GetRelativePath($layout, $_.FullName).Replace('\','/'); sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
})
$tool = Get-MakeAppxPath
$package = Join-Path $artifact "Computator.NET.$Version.win-x64.msix"
& $tool pack /d $layout /p $package /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx pack/validation failed: $LASTEXITCODE" }
& $tool unpack /p $package /d $unpacked /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx unpack failed: $LASTEXITCODE" }
foreach ($file in $files) {
    $path = Join-Path $unpacked $file.path
    if (!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.sha256) {
        throw "Packaged file differs from the staged file: $($file.path)"
    }
}
[xml]$result = Get-Content (Join-Path $unpacked 'AppxManifest.xml') -Raw
Assert-MsixIdentity $result $legacy
[ordered]@{
    schemaVersion = 1; package = [IO.Path]::GetFileName($package); version = $Version;
    identity = $manifest.Package.Identity.GetAttribute('Name'); publisher = $manifest.Package.Identity.GetAttribute('Publisher');
    sdkToolVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($tool).FileVersion;
    packageSha256 = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash;
    signed = $false; files = $files
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $artifact 'package-validation.json') -Encoding utf8
Write-Host "Validated unsigned MSIX: $package ($($files.Count) files round-tripped without changes)."
