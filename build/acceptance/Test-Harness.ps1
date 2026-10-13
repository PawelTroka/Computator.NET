$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
foreach ($file in Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.ps1') {
    $tokens = $null; $errors = $null
    [Management.Automation.Language.Parser]::ParseFile($file.FullName, [ref]$tokens, [ref]$errors) | Out-Null
    if ($errors.Count) { throw "PowerShell parse errors in $($file.Name): $errors" }
}
$root = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Harness-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
function Reject([scriptblock]$Action) { $failed = $false; try { & $Action } catch { $failed = $true }; if (!$failed) { throw 'An unsafe/incomplete acceptance input was accepted.' } }
try {
    $msi = Join-Path $root 'candidate.msi'; $payload = Join-Path $root 'payload.xml'
    Set-Content -LiteralPath $msi 'plan-only fixture: never executed as an MSI'
    Set-Content -LiteralPath $payload '<Payload ProductCode="{28D0C198-6106-46F7-B01B-767AF587D28D}" />'
    & (Join-Path $PSScriptRoot 'Prepare-CleanMachine.ps1') -MsiPath $msi -InventoryPath $payload -OutputRoot (Join-Path $root 'output & spaces')
    $config = @(Get-ChildItem $root -Recurse -Filter '*.wsb')
    if ($config.Count -ne 1) { throw 'Expected one isolated Sandbox plan.' }
    [xml]$xml = Get-Content -LiteralPath $config[0].FullName -Raw
    if ($xml.Configuration.Networking -ne 'Disable' -or $xml.Configuration.ClipboardRedirection -ne 'Disable') { throw 'Sandbox networking/clipboard must be disabled.' }
    $folders = @($xml.Configuration.MappedFolders.MappedFolder)
    if ($folders.Count -ne 2 -or $folders[0].ReadOnly -ne 'true' -or $folders[1].ReadOnly -ne 'false') { throw 'Only input/output mappings are permitted.' }
    if (@(Get-ChildItem -LiteralPath $folders[1].HostFolder).Count -ne 0) { throw 'Writable guest output mapping must start empty.' }
    Reject { & (Join-Path $PSScriptRoot 'Prepare-CleanMachine.ps1') -MsiPath $msi -InventoryPath $payload -LegacyMsiPath $msi -OutputRoot $root }
    Reject { & (Join-Path $PSScriptRoot 'Prepare-CleanMachine.ps1') -MsiPath $msi -InventoryPath $payload -LegacyMsiPath $msi -LegacySha256 ('0' * 64) -OutputRoot $root }
    $result = Join-Path $root 'result.json'; $hash = (Get-FileHash -LiteralPath $msi).Hash
    $report = @{ schemaVersion=1;status='passed';candidateSha256=$hash;historicalUpgrade='not-requested';stages=@('clean-runtime-preflight','candidate-install-payload-and-launch','repair-uninstall-and-user-data-preservation') }
    $report | ConvertTo-Json | Set-Content -LiteralPath $result
    & (Join-Path $PSScriptRoot 'Assert-CleanMachineResult.ps1') -ResultPath $result -CandidateSha256 $hash
    Reject { & (Join-Path $PSScriptRoot 'Assert-CleanMachineResult.ps1') -ResultPath $result -CandidateSha256 $hash -RequireHistoricalUpgrade }
    Reject { & (Join-Path $PSScriptRoot 'Assert-CleanMachineResult.ps1') -ResultPath $result -CandidateSha256 ('0' * 64) }
    $report.status='failed'; $report | ConvertTo-Json | Set-Content -LiteralPath $result
    Reject { & (Join-Path $PSScriptRoot 'Assert-CleanMachineResult.ps1') -ResultPath $result -CandidateSha256 $hash }
    Write-Host 'Acceptance harness checks passed: syntax, restricted mappings, input hash requirements and incomplete/failed result rejection. No MSI was installed.'
}
finally { Remove-Item -LiteralPath $root -Recurse -Force }
