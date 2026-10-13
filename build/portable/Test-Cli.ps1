param([Parameter(Mandatory)][string]$PublishDirectory)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $PublishDirectory).Path
$exe = Join-Path $root 'computator.dll'
$output = & dotnet $exe calculate --expression '2x+3' --x 7
if ($LASTEXITCODE -ne 0 -or ($output | ConvertFrom-Json).result -ne '17') { throw 'Published portable calculation failed.' }
$temp = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Portable-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $functions = Join-Path $temp 'probe.tslf'
    $script = Join-Path $temp 'probe.tsl'
    Set-Content -LiteralPath $functions -Value 'static real Probe(real x) { return x/2; }' -Encoding utf8
    Set-Content -LiteralPath $script -Value 'write(Probe(6));' -Encoding utf8
    $output = & dotnet $exe script --file $script --functions $functions
    if ($LASTEXITCODE -ne 0 -or ($output | ConvertFrom-Json).result -ne '3') { throw 'Published portable script failed.' }
    $process = [Diagnostics.ProcessStartInfo]::new('dotnet')
    $process.UseShellExecute = $false
    $process.RedirectStandardError = $true
    foreach ($arg in @($exe,'calculate','--expression','missing(2)')) { $process.ArgumentList.Add($arg) }
    $child = [Diagnostics.Process]::Start($process)
    try {
        $errorText = $child.StandardError.ReadToEnd()
        if (!$child.WaitForExit(30000)) { $child.Kill($true); throw 'Invalid-program check timed out.' }
        if ($child.ExitCode -ne 2 -or ($errorText | ConvertFrom-Json).error -ne 'compilation') { throw 'CLI did not return structured compilation diagnostics and nonzero exit status.' }
    } finally { $child.Dispose() }
    Write-Host 'Published portable CLI passed calculation, UTF-8 document execution and invalid-program diagnostics.'
} finally { Remove-Item -LiteralPath $temp -Recurse -Force }
