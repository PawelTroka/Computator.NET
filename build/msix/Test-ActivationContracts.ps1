$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'PackageActivation.cs')
[Computator.Acceptance.PackageActivation]::ValidateIdentity('TROKASoftware.Computator.NET_example!App', 'TROKASoftware.Computator.NET_3.0.0.0_x64__example')
foreach ($bad in @('', 'other!App', 'TROKASoftware.Computator.NET_example', 'TROKASoftware.Computator.NET_example!App!Other')) {
    $rejected = $false
    try { [Computator.Acceptance.PackageActivation]::ValidateIdentity($bad, 'TROKASoftware.Computator.NET_3.0.0.0_x64__example') } catch { $rejected = $true }
    if (!$rejected) { throw "Invalid activation identity was accepted: $bad" }
}
$rejected = $false
try { [Computator.Acceptance.PackageActivation]::LaunchAndVerify('TROKASoftware.Computator.NET_example!App', 'TROKASoftware.Computator.NET_3.0.0.0_x64__example', 'missing-relative-file.tsl') } catch { $rejected = $true }
if (!$rejected) { throw 'Missing activation file was accepted.' }
Write-Host 'Activation interop compiles and input contracts pass; no package was installed or activated.'
