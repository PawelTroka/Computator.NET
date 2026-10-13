$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$source = Join-Path $root 'artifacts/local-web'
$destination = Join-Path $root 'artifacts/local-web-review'
if (Test-Path $destination) { Remove-Item -LiteralPath $destination -Recurse -Force }
[IO.Compression.ZipFile]::ExtractToDirectory((Join-Path $root 'publish/Computator.NET.LocalWeb.win-x64.zip'), $destination)
$expected = @(Get-ChildItem $source -File -Recurse | ForEach-Object { [IO.Path]::GetRelativePath($source, $_.FullName) } | Sort-Object)
$actual = @(Get-ChildItem $destination -File -Recurse | ForEach-Object { [IO.Path]::GetRelativePath($destination, $_.FullName) } | Sort-Object)
if (Compare-Object $expected $actual) { throw 'The local package inventory differs from staging.' }
foreach ($name in $expected) {
    if ((Get-FileHash (Join-Path $source $name)).Hash -ne (Get-FileHash (Join-Path $destination $name)).Hash) { throw "Package hash mismatch: $name" }
}
Write-Host "Verified and extracted $($actual.Count) local-package files for real browser testing."
