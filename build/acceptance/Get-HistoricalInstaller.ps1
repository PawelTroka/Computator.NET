[CmdletBinding()]
param([string]$OutputDirectory = (Join-Path $PSScriptRoot '../../artifacts/installer/historical'))
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$fixture = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'historical-2.3.2.json') -Raw | ConvertFrom-Json
$root = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $root -Force | Out-Null
for ($part = Get-Item -LiteralPath $root; $null -ne $part; $part = $part.Parent) {
    if ($part.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Historical input directory must not contain redirected ancestors.' }
}
$destination = Join-Path $root $fixture.fileName
function Assert-Download([string]$Path) {
    $item = Get-Item -LiteralPath $Path
    if ($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $item.Length -ne $fixture.size -or (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -ne $fixture.sha256) {
        throw 'The historical release asset does not match the reviewed size/SHA-256. Do not refresh the pin automatically.'
    }
}
if (Test-Path -LiteralPath $destination) { Assert-Download $destination }
else {
    $temporary = Join-Path $root ([Guid]::NewGuid().ToString('N') + '.download')
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -UseBasicParsing -Uri $fixture.downloadUrl -OutFile $temporary
        Assert-Download $temporary
        [IO.File]::Move($temporary, $destination)
    } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force } }
}
Write-Host "Verified official historical EXE (not installed): $destination"
Write-Host "SHA-256: $($fixture.sha256); asset ID: $($fixture.assetId)"
