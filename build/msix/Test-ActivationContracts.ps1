$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -Path (Join-Path $PSScriptRoot 'PackageActivation.cs')
Import-Module (Join-Path $PSScriptRoot 'ActivationContracts.psm1') -Force
function Reject([scriptblock]$Action) {
    $rejected = $false; try { & $Action } catch { $rejected = $true }
    if (!$rejected) { throw 'An invalid activation input was accepted.' }
}
$fullName = 'TROKASoftware.Computator.NET_3.0.0.0_x64__abcdefghijklm'
$applicationId = 'TROKASoftware.Computator.NET_abcdefghijklm!Computator.NET'
[Computator.Acceptance.PackageActivation]::ValidateIdentity($applicationId, $fullName)
foreach ($bad in @('', 'other!App', 'TROKASoftware.Computator.NET_abcdefghijklm', ($applicationId + '!Other'),
    'TROKASoftware.Computator.NET_otherpublisher!Computator.NET', 'TROKASoftware.Computator.NET_abcdefghijklm!Other')) {
    Reject { [Computator.Acceptance.PackageActivation]::ValidateIdentity($bad, $fullName) }
}
Reject { [Computator.Acceptance.PackageActivation]::ValidateIdentity($applicationId, 'TROKASoftware.Computator.NET_invalid') }
Reject { [Computator.Acceptance.PackageActivation]::LaunchAndVerify($applicationId, $fullName, 'missing-relative-file.tsl') }
$root = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Activation-Contracts-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $root | Out-Null
function Fixture([xml]$Manifest) {
    $path = Join-Path $root ([Guid]::NewGuid().ToString('N') + '.msix')
    $archive = [IO.Compression.ZipFile]::Open($path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $writer = [IO.StreamWriter]::new($archive.CreateEntry('AppxManifest.xml').Open())
        try { $writer.Write($Manifest.OuterXml) } finally { $writer.Dispose() }
    } finally { $archive.Dispose() }
    return $path
}
try {
    foreach ($extension in @('.tsl', '.TSL', '.tslf', '.TsLf')) {
        $path = Join-Path $root ('Żółć activation' + $extension)
        [IO.File]::WriteAllText($path, '// inert fixture')
        [Computator.Acceptance.PackageActivation]::ValidateDocument($path)
    }
    $unrelated = Join-Path $root 'unrelated.txt'; [IO.File]::WriteAllText($unrelated, 'data')
    Reject { [Computator.Acceptance.PackageActivation]::ValidateDocument($unrelated) }
    $hash = (Get-FileHash -LiteralPath $unrelated -Algorithm SHA256).Hash
    Assert-ActivationDocumentPreserved $unrelated $hash
    [IO.File]::WriteAllText($unrelated, 'changed')
    Reject { Assert-ActivationDocumentPreserved $unrelated $hash }
    Reject { Assert-ActivationDocumentPreserved (Join-Path $root 'missing.tsl') $hash }
    [xml]$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'AppxManifest.xml') -Raw
    $baseline = Read-ActivationPackageIdentity (Fixture $manifest)
    $manifest.Package.Identity.Version = ([Version]::new(([Version]$baseline.Version).Major + 1, 0, 0, 0)).ToString()
    $candidate = Read-ActivationPackageIdentity (Fixture $manifest)
    Assert-ActivationUpgrade $candidate $baseline
    Reject { Assert-ActivationUpgrade $baseline $baseline }
    Reject { Assert-ActivationUpgrade $baseline $candidate }
    foreach ($attribute in @('Name', 'Publisher', 'ProcessorArchitecture')) {
        [xml]$wrong = $manifest.OuterXml; $wrong.Package.Identity.SetAttribute($attribute, 'unrelated')
        $fixture = Fixture $wrong
        Reject { Read-ActivationPackageIdentity $fixture }
    }
    $manifest.Package.Applications.Application.SetAttribute('Id', 'Other')
    $fixture = Fixture $manifest
    Reject { Read-ActivationPackageIdentity $fixture }
    Write-Host 'Activation interop, package identity/version and document contracts passed; no package was installed or activated.'
} finally {
    $resolvedRoot = (Resolve-Path -LiteralPath $root).Path
    $temporaryPrefix = (Resolve-Path -LiteralPath ([IO.Path]::GetTempPath())).Path.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    if (!$resolvedRoot.StartsWith($temporaryPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        ((Get-Item -LiteralPath $resolvedRoot).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Refusing redirected/out-of-temp test cleanup.' }
    Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
}
