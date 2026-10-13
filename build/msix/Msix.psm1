Set-StrictMode -Version Latest
function Assert-MsixVersion([string]$Version) {
    if ($Version -notmatch '^\d+\.\d+\.\d+\.0$') { throw 'Store package version must contain four numeric components with a zero revision.' }
    $parsed = [Version]::Parse($Version)
    if ($parsed.Major -lt 1 -or @($parsed.Major, $parsed.Minor, $parsed.Build, $parsed.Revision | Where-Object { $_ -gt 65535 }).Count -gt 0) {
        throw 'MSIX version components must be within the Store package limits.'
    }
}
function Assert-MsixIdentity([xml]$Manifest, [xml]$Legacy) {
    $identity = $Manifest.SelectSingleNode('/*[local-name()="Package"]/*[local-name()="Identity"]')
    $oldIdentity = $Legacy.SelectSingleNode('/*[local-name()="Package"]/*[local-name()="Identity"]')
    foreach ($attribute in @('Name', 'Publisher', 'ProcessorArchitecture')) {
        if ($identity.GetAttribute($attribute) -ne $oldIdentity.GetAttribute($attribute)) { throw "Historical package identity changed: $attribute" }
    }
    $application = $Manifest.SelectSingleNode('//*[local-name()="Application"]')
    $oldApplication = $Legacy.SelectSingleNode('//*[local-name()="Application"]')
    if ($application.GetAttribute('Id') -ne $oldApplication.GetAttribute('Id') -or
        $application.GetAttribute('Executable') -ne 'Computator.NET.exe' -or
        $application.GetAttribute('EntryPoint') -ne 'Windows.FullTrustApplication') { throw 'The desktop activation identity changed.' }
    if (!$Manifest.SelectSingleNode('//*[local-name()="Capability" and @Name="runFullTrust"]')) { throw 'The full-trust desktop capability is missing.' }
    $types = @($Manifest.SelectNodes('//*[local-name()="FileType"]') | ForEach-Object { $_.InnerText })
    if (@($types | Sort-Object -Unique).Count -ne 2 -or '.tsl' -notin $types -or '.tslf' -notin $types) { throw 'Document associations are incomplete.' }
    Assert-MsixVersion $identity.GetAttribute('Version')
}
function Get-MakeAppxPath {
    $kits = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots' -ErrorAction Stop).KitsRoot10
    $candidate = Get-ChildItem (Join-Path $kits 'bin') -Directory | Where-Object {
        $_.Name -match '^10\.0\.\d+\.\d+$' -and (Test-Path (Join-Path $_.FullName 'x64\makeappx.exe'))
    } | Sort-Object { [Version]$_.Name } -Descending | Select-Object -First 1
    if (!$candidate) { throw 'Install a Windows 10/11 SDK containing x64 MakeAppx; validation is never disabled.' }
    return Join-Path $candidate.FullName 'x64\makeappx.exe'
}
Export-ModuleMember -Function Assert-MsixVersion, Assert-MsixIdentity, Get-MakeAppxPath
