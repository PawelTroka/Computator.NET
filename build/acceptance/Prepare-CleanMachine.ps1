[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$MsiPath,
    [Parameter(Mandatory)][string]$InventoryPath,
    [string]$LegacyMsiPath,
    [string]$LegacySha256,
    [string]$CandidateBundlePath,
    [string]$HistoricalBundlePath,
    [string]$OutputRoot = (Join-Path $PSScriptRoot '../../artifacts/clean-machine')
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function SafeFile([string]$Path) {
    $item = Get-Item -LiteralPath $Path
    if ($item.PSIsContainer) { throw "Expected a file: $Path" }
    for ($part = $item; $null -ne $part; $part = if ($part -is [IO.FileInfo]) { $part.Directory } else { $part.Parent }) {
        if ($part.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Redirected acceptance input: $Path" }
    }
    return $item.FullName
}
$msi = SafeFile $MsiPath
$inventory = SafeFile $InventoryPath
if ([IO.Path]::GetExtension($msi) -ne '.msi') { throw 'Provide an actual compiled MSI, not the bootstrapper or WiX source.' }
[xml]$payload = Get-Content -LiteralPath $inventory -Raw
$product = [Guid]::Parse($payload.Payload.ProductCode).ToString('B')
$bundleMode = [bool]$CandidateBundlePath -or [bool]$HistoricalBundlePath
if ($bundleMode -and (!$CandidateBundlePath -or !$HistoricalBundlePath -or $LegacyMsiPath)) { throw 'Supply both actual EXEs, or use the separate legacy-MSI mode; do not mix modes.' }
$historical = $null; $candidateBundle = $null
if ($bundleMode) {
    $historical = SafeFile $HistoricalBundlePath; $candidateBundle = SafeFile $CandidateBundlePath
    if ([IO.Path]::GetExtension($historical) -ne '.exe' -or [IO.Path]::GetExtension($candidateBundle) -ne '.exe') { throw 'Bundle mode requires real EXE inputs.' }
    $fixture = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'historical-2.3.2.json') -Raw | ConvertFrom-Json
    if ((Get-Item -LiteralPath $historical).Length -ne $fixture.size -or (Get-FileHash -LiteralPath $historical).Hash -ne $fixture.sha256) { throw 'Historical EXE does not match the pinned official release asset.' }
}
if ($LegacyMsiPath -and $LegacySha256 -notmatch '^[0-9a-fA-F]{64}$') { throw 'Historical upgrade testing requires the owner-verified legacy MSI SHA-256.' }
$legacy = $null
if ($LegacyMsiPath) {
    $legacy = SafeFile $LegacyMsiPath
    if ((Get-FileHash -LiteralPath $legacy -Algorithm SHA256).Hash -ne $LegacySha256) { throw 'Historical MSI hash does not match the approved input.' }
}
$root = Join-Path ([IO.Path]::GetFullPath($OutputRoot)) ([Guid]::NewGuid().ToString('N'))
$inputs = Join-Path $root 'inputs'; $results = Join-Path $root 'results'
New-Item -ItemType Directory -Path $inputs, $results -Force | Out-Null
for ($part = Get-Item -LiteralPath $root; $null -ne $part; $part = $part.Parent) {
    if ($part.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Acceptance output must not have redirected ancestors.' }
}
Copy-Item -LiteralPath $msi -Destination (Join-Path $inputs 'candidate.msi')
Copy-Item -LiteralPath $inventory -Destination (Join-Path $inputs 'payload.xml')
$scripts = @('Guest-Acceptance.ps1', 'New-FailingUpgrade.ps1', 'AcceptanceContracts.psm1')
$entryPoint = 'Guest-Acceptance.ps1'
if ($bundleMode) {
    $entryPoint = 'Guest-HistoricalUpgrade.ps1'
    $scripts = @($entryPoint, 'Test-HistoricalUpgrade.ps1', 'Write-HistoricalProfile.ps1', 'New-FailingUpgrade.ps1', 'AcceptanceContracts.psm1', 'historical-2.3.2.json')
    Copy-Item -LiteralPath $historical -Destination (Join-Path $inputs 'historical.exe')
    Copy-Item -LiteralPath $candidateBundle -Destination (Join-Path $inputs 'candidate.exe')
}
foreach ($script in $scripts) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $script) -Destination $inputs }
if ($legacy) { Copy-Item -LiteralPath $legacy -Destination (Join-Path $inputs 'legacy.msi') }
$plan = [ordered]@{
    schemaVersion = $(if ($bundleMode) { 2 } else { 1 }); candidateProductCode = $product
    candidateSha256 = (Get-FileHash -LiteralPath $msi).Hash
    inventorySha256 = (Get-FileHash -LiteralPath $inventory).Hash
    historicalUpgradeRequested = [bool]($legacy -or $bundleMode); legacySha256 = $LegacySha256
    expectedUpgradeCode = '{86D89770-B31B-4467-BC5E-B54B0FF263A1}'
    expectedLegacyVersion = '2.3.2.0'; requireNoModernRuntime = $true
}
if ($bundleMode) {
    $plan.candidateBundleSha256 = (Get-FileHash -LiteralPath $candidateBundle).Hash
    $plan.historicalBundleSha256 = $fixture.sha256
    $plan.inputs = @(Get-ChildItem -LiteralPath $inputs -File | Sort-Object Name | ForEach-Object { @{ name = $_.Name; sha256 = (Get-FileHash -LiteralPath $_.FullName).Hash } })
}
$plan | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $inputs 'plan.json') -Encoding UTF8
$settings = [xml]'<Configuration><VGpu>Disable</VGpu><Networking>Disable</Networking><ClipboardRedirection>Disable</ClipboardRedirection><AudioInput>Disable</AudioInput><VideoInput>Disable</VideoInput><PrinterRedirection>Disable</PrinterRedirection><MappedFolders><MappedFolder><HostFolder/><SandboxFolder>C:\Inputs</SandboxFolder><ReadOnly>true</ReadOnly></MappedFolder><MappedFolder><HostFolder/><SandboxFolder>C:\Results</SandboxFolder><ReadOnly>false</ReadOnly></MappedFolder></MappedFolders><LogonCommand><Command/></LogonCommand></Configuration>'
# Typed node setters avoid Windows PowerShell's XmlNode adapter rejecting a
# pipeline-wrapped string. InnerText still performs correct XML escaping.
$settings.SelectSingleNode('/Configuration/MappedFolders/MappedFolder[1]/HostFolder').InnerText = [string]$inputs
$settings.SelectSingleNode('/Configuration/MappedFolders/MappedFolder[2]/HostFolder').InnerText = [string]$results
$settings.SelectSingleNode('/Configuration/LogonCommand/Command').InnerText = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Inputs\$entryPoint -InputDirectory C:\Inputs -ResultDirectory C:\Results -DisposableMachine"
$configuration = Join-Path $root 'Computator-Acceptance.wsb'
$settings.Save($configuration)
Write-Host "Prepared only; no installation or Sandbox launch was performed. Review and open: $configuration"
Write-Host "Only the new empty results directory is writable from the guest: $results"
