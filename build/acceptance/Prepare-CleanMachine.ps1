[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$MsiPath,
    [Parameter(Mandatory)][string]$InventoryPath,
    [string]$LegacyMsiPath,
    [string]$LegacySha256,
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
if ($LegacyMsiPath -and $LegacySha256 -notmatch '^[0-9a-fA-F]{64}$') { throw 'Historical upgrade testing requires the owner-verified legacy MSI SHA-256.' }
$legacy = $null
if ($LegacyMsiPath) {
    $legacy = SafeFile $LegacyMsiPath
    if ((Get-FileHash -LiteralPath $legacy -Algorithm SHA256).Hash -ne $LegacySha256) { throw 'Historical MSI hash does not match the approved input.' }
}
$root = Join-Path ([IO.Path]::GetFullPath($OutputRoot)) ([Guid]::NewGuid().ToString('N'))
$inputs = Join-Path $root 'inputs'; $results = Join-Path $root 'results'
New-Item -ItemType Directory -Path $inputs, $results -Force | Out-Null
Copy-Item -LiteralPath $msi -Destination (Join-Path $inputs 'candidate.msi')
Copy-Item -LiteralPath $inventory -Destination (Join-Path $inputs 'payload.xml')
foreach ($script in @('Guest-Acceptance.ps1', 'New-FailingUpgrade.ps1', 'AcceptanceContracts.psm1')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $script) -Destination $inputs }
if ($legacy) { Copy-Item -LiteralPath $legacy -Destination (Join-Path $inputs 'legacy.msi') }
$plan = [ordered]@{
    schemaVersion = 1; candidateProductCode = $product
    candidateSha256 = (Get-FileHash -LiteralPath $msi).Hash
    inventorySha256 = (Get-FileHash -LiteralPath $inventory).Hash
    historicalUpgradeRequested = [bool]$legacy; legacySha256 = $LegacySha256
    expectedUpgradeCode = '{86D89770-B31B-4467-BC5E-B54B0FF263A1}'
    # Both authentic MSI payloads in the v2.3.2 bundle use four components.
    expectedLegacyVersion = '2.3.2.0'; requireNoModernRuntime = $true
}
$plan | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $inputs 'plan.json') -Encoding UTF8
$settings = [xml]'<Configuration><VGpu>Disable</VGpu><Networking>Disable</Networking><ClipboardRedirection>Disable</ClipboardRedirection><AudioInput>Disable</AudioInput><VideoInput>Disable</VideoInput><PrinterRedirection>Disable</PrinterRedirection><MappedFolders><MappedFolder><HostFolder/><SandboxFolder>C:\Inputs</SandboxFolder><ReadOnly>true</ReadOnly></MappedFolder><MappedFolder><HostFolder/><SandboxFolder>C:\Results</SandboxFolder><ReadOnly>false</ReadOnly></MappedFolder></MappedFolders><LogonCommand><Command>powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Inputs\Guest-Acceptance.ps1 -InputDirectory C:\Inputs -ResultDirectory C:\Results -DisposableMachine</Command></LogonCommand></Configuration>'
$settings.Configuration.MappedFolders.MappedFolder[0].HostFolder = $inputs
$settings.Configuration.MappedFolders.MappedFolder[1].HostFolder = $results
$configuration = Join-Path $root 'Computator-Acceptance.wsb'
$settings.Save($configuration)
Write-Host "Prepared only; no installation or Sandbox launch was performed. Review and open: $configuration"
Write-Host "Only the new empty results directory is writable from the guest: $results"
