[CmdletBinding(DefaultParameterSetName = 'Official')]
param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../../artifacts/performance/historical'),
    [Parameter(Mandatory = $true, ParameterSetName = 'Local')][string]$ArchivePath,
    [Parameter(Mandatory = $true, ParameterSetName = 'Local')]
    [ValidatePattern('^[a-fA-F0-9]{64}$')][string]$ExpectedSHA256,
    [Parameter(Mandatory = $true, ParameterSetName = 'Local')]
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')][string]$ExpectedVersion,
    [Parameter(ParameterSetName = 'Local')][string]$SourceDescription = 'Caller-supplied historical archive'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# This acquires bytes and inspects metadata; it neither installs nor runs the product.
# Windows PowerShell provides a separate reflection-only inspection process even
# when the caller is PowerShell 7. The historical assemblies never enter this host.
if ($env:OS -ne 'Windows_NT') { throw 'Historical Framework baseline validation requires Windows.' }
$metadataHost = Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe'
if (-not (Test-Path -LiteralPath $metadataHost -PathType Leaf)) {
    throw 'Windows PowerShell 5.1 is required for reflection-only Framework metadata inspection.'
}

function Assert-UnredirectedPath([string]$Path) {
    for ($part = [IO.Path]::GetFullPath($Path); $part; $part = [IO.Path]::GetDirectoryName($part)) {
        if ((Test-Path -LiteralPath $part) -and
            ((Get-Item -LiteralPath $part -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Historical paths must not contain redirected ancestors: $part"
        }
    }
}

$destination = [IO.Path]::GetFullPath($OutputDirectory)
Assert-UnredirectedPath $destination
if (Test-Path -LiteralPath $destination) {
    if (-not (Test-Path -LiteralPath $destination -PathType Container) -or
        @(Get-ChildItem -LiteralPath $destination -Force).Count -ne 0) {
        throw 'Historical output must be absent or empty; existing payloads are never overwritten.'
    }
}

$official = $PSCmdlet.ParameterSetName -eq 'Official'
$url = 'https://github.com/PawelTroka/Computator.NET/releases/download/v2.3.2/Computator.NET.v2.3.2.zip'
if ($official) {
    $ExpectedSHA256 = '21a64b059d78176b975d066a6459e4467f9a523c997f57981ceca49485df8b2d'
    $ExpectedVersion = '2.3.2.0'
    $SourceDescription = 'Official GitHub v2.3.2 portable release asset 5646130'
} else {
    $ArchivePath = [IO.Path]::GetFullPath($ArchivePath)
    Assert-UnredirectedPath $ArchivePath
    if (-not (Test-Path -LiteralPath $ArchivePath -PathType Leaf)) { throw 'Historical archive does not exist.' }
    if ((Get-Item -LiteralPath $ArchivePath).Length -gt 256MB) { throw 'Historical archive exceeds the 256 MiB input limit.' }
}

$parent = [IO.Path]::GetDirectoryName($destination)
if (-not $parent) { throw 'Choose an output subdirectory, not a drive root.' }
[IO.Directory]::CreateDirectory($parent) | Out-Null
$stage = Join-Path $parent ('.historical-acquire-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($stage) | Out-Null
try {
    $archive = Join-Path $stage 'baseline.zip'
    if ($official) {
        Add-Type -AssemblyName System.Net.Http
        $previousSecurityProtocol = [Net.ServicePointManager]::SecurityProtocol
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $client = New-Object Net.Http.HttpClient
        try {
            $client.Timeout = [TimeSpan]::FromSeconds(60)
            $client.MaxResponseContentBufferSize = 12002377
            $bytes = $client.GetByteArrayAsync($url).GetAwaiter().GetResult()
            if ($bytes.Length -ne 12002377) { throw 'Official archive size differs from the reviewed release asset.' }
            [IO.File]::WriteAllBytes($archive, $bytes)
        } finally {
            $client.Dispose()
            [Net.ServicePointManager]::SecurityProtocol = $previousSecurityProtocol
        }
    } else {
        [IO.File]::Copy($ArchivePath, $archive, $false)
    }
    $archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($archiveHash -ne $ExpectedSHA256.ToLowerInvariant()) {
        throw 'Historical archive SHA-256 mismatch. Do not replace the expected pin automatically.'
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $payload = Join-Path $stage 'payload'
    [IO.Directory]::CreateDirectory($payload) | Out-Null
    $prefix = $payload + [IO.Path]::DirectorySeparatorChar
    $zip = [IO.Compression.ZipFile]::OpenRead($archive)
    try {
        if ($zip.Entries.Count -gt 10000) { throw 'Historical archive contains too many entries.' }
        $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
        [long]$expandedSize = 0
        foreach ($entry in $zip.Entries) {
            $name = $entry.FullName.Replace('/', '\')
            if ($name.Contains(':') -or [IO.Path]::IsPathRooted($name) -or
                ((($entry.ExternalAttributes -shr 16) -band 61440) -eq 40960) -or
                @($name.Split('\') | Where-Object { $_ -eq '..' -or $_ -eq '.' -or $_ -match '[. ]$' }).Count -ne 0) {
                throw "Unsupported archive entry: $($entry.FullName)"
            }
            $path = [IO.Path]::GetFullPath((Join-Path $payload $name))
            if (-not $path.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase) -or -not $seen.Add($path)) {
                throw "Escaping or duplicate archive entry: $($entry.FullName)"
            }
            $expandedSize += $entry.Length
            if ($expandedSize -gt 512MB) { throw 'Historical archive exceeds the 512 MiB extraction limit.' }
            if ($name.EndsWith('\')) {
                [IO.Directory]::CreateDirectory($path) | Out-Null
                continue
            }
            [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
            $inputStream = $entry.Open()
            $outputStream = [IO.File]::Open($path, [IO.FileMode]::CreateNew)
            try { $inputStream.CopyTo($outputStream) }
            finally { $outputStream.Dispose(); $inputStream.Dispose() }
        }
    } finally { $zip.Dispose() }

    $helper = Join-Path $stage 'Read-Metadata.ps1'
    @'
param([string]$Directory)
$ErrorActionPreference = 'Stop'
$result = foreach ($name in @('Computator.NET.exe', 'Computator.NET.Core.dll', 'Computator.NET.DataTypes.dll')) {
    $path = Join-Path $Directory $name
    $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($path)
    $attributes = $assembly.GetCustomAttributesData()
    $target = @($attributes | Where-Object { $_.AttributeType.FullName -eq 'System.Runtime.Versioning.TargetFrameworkAttribute' })
    $configuration = @($attributes | Where-Object { $_.AttributeType.FullName -eq 'System.Reflection.AssemblyConfigurationAttribute' })
    if ($target.Count -ne 1 -or $configuration.Count -ne 1) { throw "Missing or ambiguous assembly metadata: $name" }
    $kind = [Reflection.PortableExecutableKinds]0
    $machine = [Reflection.ImageFileMachine]::I386
    $assembly.ManifestModule.GetPEKind([ref]$kind, [ref]$machine)
    $fileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion
    [pscustomobject]@{
        file = $name
        sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        assemblyName = $assembly.GetName().Name
        assemblyVersion = $assembly.GetName().Version.ToString()
        fileVersion = $fileVersion
        targetFramework = [string]$target[0].ConstructorArguments[0].Value
        configuration = [string]$configuration[0].ConstructorArguments[0].Value
        machine = $machine.ToString()
        portableExecutableKinds = $kind.ToString()
        requires32Bit = [bool]($kind -band [Reflection.PortableExecutableKinds]::Required32Bit)
        prefers32Bit = [bool]($kind -band [Reflection.PortableExecutableKinds]::Preferred32Bit)
    }
}
ConvertTo-Json -InputObject @($result) -Depth 5 -Compress
'@ | Set-Content -LiteralPath $helper -Encoding UTF8
    $metadataJson = & $metadataHost -NoProfile -NonInteractive -File $helper -Directory $payload
    if ($LASTEXITCODE -ne 0) { throw 'Historical assembly metadata inspection failed.' }
    $assemblies = @($metadataJson | ConvertFrom-Json)
    if ($assemblies.Count -ne 3) { throw 'Historical assembly inspection returned an incomplete result.' }
    $officialHashes = @{
        'Computator.NET.exe' = '63fe05835b1b31ba16a4f2c7fd6101bda5f1a8dd3aba41eeef8e352f0862c290'
        'Computator.NET.Core.dll' = 'aefa901c609e40eafff49e5526e308d75557a82aeee2904db797659f88c58c2b'
        'Computator.NET.DataTypes.dll' = '29ff67f8b09dacabd02e2662823bf5cd74d6f75447e3c213ffd704b658517c8b'
    }
    foreach ($assembly in $assemblies) {
        if ($assembly.assemblyName -ne [IO.Path]::GetFileNameWithoutExtension($assembly.file) -or
            $assembly.assemblyVersion -ne $ExpectedVersion -or $assembly.fileVersion -ne $ExpectedVersion) {
            throw "Historical assembly identity does not match requested version ${ExpectedVersion}: $($assembly.file)"
        }
        if ($assembly.targetFramework -notmatch '^\.NETFramework,Version=v4\.' -or
            $assembly.configuration -notin @('Retail', 'Release')) {
            throw "Expected a Release/Retail .NET Framework historical build: $($assembly.file)"
        }
        if ($official -and ($assembly.sha256 -ne $officialHashes[$assembly.file] -or
            $assembly.targetFramework -ne '.NETFramework,Version=v4.6.1' -or
            $assembly.machine -ne 'I386' -or $assembly.portableExecutableKinds -ne 'ILOnly' -or
            $assembly.requires32Bit -or $assembly.prefers32Bit)) {
            throw "Official historical payload metadata or hash differs from the reviewed asset: $($assembly.file)"
        }
    }
    $provenance = [ordered]@{
        schemaVersion = 1
        acquiredUtc = [DateTime]::UtcNow.ToString('o')
        kind = $(if ($official) { 'official-release' } else { 'caller-supplied' })
        description = $SourceDescription
        version = $ExpectedVersion
        archiveSha256 = $archiveHash
        archiveBytes = (Get-Item -LiteralPath $archive).Length
        source = $(if ($official) { $url } else { [IO.Path]::GetFileName($ArchivePath) })
        releaseAssetId = $(if ($official) { 5646130 } else { $null })
        releaseTag = $(if ($official) { 'v2.3.2' } else { $null })
        releaseTagCommit = $(if ($official) { '56fed3363c07026d7fb290e40f852468dfe158ba' } else { $null })
        provenanceNote = 'Hashes pin acquired bytes. They are not a publisher-signed checksum or proof of a reproducible source build. A caller-supplied version is accepted only after matching its actual assembly metadata.'
        assemblies = $assemblies
    }
    $provenance | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $payload 'baseline-provenance.json') -Encoding UTF8
    Assert-UnredirectedPath $destination
    if (Test-Path -LiteralPath $destination) { [IO.Directory]::Delete($destination, $false) }
    [IO.Directory]::Move($payload, $destination)
    Write-Host "Verified historical $ExpectedVersion payload: $destination"
    [pscustomobject]$provenance
} finally {
    if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
}
