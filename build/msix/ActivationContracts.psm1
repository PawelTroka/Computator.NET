Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'Msix.psm1') -Force
function Read-ActivationPackageIdentity([string]$Path) {
    # Inspect the signed input before Add-AppxPackage, so an unrelated package
    # signed with the same certificate cannot be deployed or left behind.
    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($archive.Entries | Where-Object { $_.FullName -ceq 'AppxManifest.xml' })
        if ($entries.Count -ne 1 -or $entries[0].Length -gt 1MB) { throw 'Expected one bounded MSIX application manifest.' }
        $stream = $entries[0].Open()
        try {
            $settings = [Xml.XmlReaderSettings]::new()
            $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
            $reader = [Xml.XmlReader]::Create($stream, $settings)
            try { $manifest = [xml]::new(); $manifest.Load($reader) } finally { $reader.Dispose() }
        } finally { $stream.Dispose() }
    } finally { $archive.Dispose() }
    [xml]$expected = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'AppxManifest.xml') -Raw
    Assert-MsixIdentity $manifest $expected
    $identities = @($manifest.SelectNodes('/*[local-name()="Package"]/*[local-name()="Identity"]'))
    $applications = @($manifest.SelectNodes('/*[local-name()="Package"]/*[local-name()="Applications"]/*[local-name()="Application"]'))
    if ($identities.Count -ne 1 -or $applications.Count -ne 1) { throw 'Expected one package identity and desktop application.' }
    foreach ($attribute in @('Name', 'Publisher', 'ProcessorArchitecture')) {
        if ($identities[0].GetAttribute($attribute) -cne $expected.Package.Identity.GetAttribute($attribute)) {
            throw "The approved package must preserve the exact $attribute."
        }
    }
    if ($applications[0].GetAttribute('Id') -cne $expected.Package.Applications.Application.GetAttribute('Id')) {
        throw 'The approved package must preserve the exact application ID.'
    }
    return [pscustomobject]@{
        Name = $identities[0].GetAttribute('Name'); Publisher = $identities[0].GetAttribute('Publisher')
        Version = $identities[0].GetAttribute('Version'); Architecture = $identities[0].GetAttribute('ProcessorArchitecture')
        ApplicationId = $applications[0].GetAttribute('Id')
    }
}
function Assert-ActivationUpgrade($Candidate, $Baseline) {
    foreach ($field in @('Name', 'Publisher', 'Architecture', 'ApplicationId')) {
        if ($Candidate.$field -cne $Baseline.$field) { throw "The baseline and candidate differ in $field." }
    }
    if ([Version]$Candidate.Version -le [Version]$Baseline.Version) { throw 'The candidate must advance the baseline version before deployment.' }
}
function Assert-ActivationDocumentPreserved([string]$Path, [string]$ExpectedSha256) {
    if (!(Test-Path -LiteralPath $Path -PathType Leaf) -or
        (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -ne $ExpectedSha256) {
        throw 'Package acceptance changed or removed a preservation fixture.'
    }
}
Export-ModuleMember -Function Read-ActivationPackageIdentity, Assert-ActivationUpgrade, Assert-ActivationDocumentPreserved
