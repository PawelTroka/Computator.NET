[CmdletBinding()]
param([Parameter(Mandatory)][string]$PublishDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$directory = (Resolve-Path -LiteralPath $PublishDirectory).Path
$configuration = Get-Content (Join-Path $directory 'Computator.NET.WebApi.runtimeconfig.json') -Raw | ConvertFrom-Json
if ($configuration.runtimeOptions.tfm -ne 'net10.0') { throw 'The published API is not a .NET 10 application.' }
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start(); $port = $listener.LocalEndpoint.Port; $listener.Stop()
$address = "http://127.0.0.1:$port"
$start = [Diagnostics.ProcessStartInfo]::new('dotnet')
$start.UseShellExecute = $false; $start.WorkingDirectory = $directory
$start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
$start.ArgumentList.Add((Join-Path $directory 'Computator.NET.WebApi.dll'))
$start.ArgumentList.Add('--urls'); $start.ArgumentList.Add($address)
$start.Environment['ASPNETCORE_ENVIRONMENT'] = 'Production'
$start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
# Exercise the package's default resolver, never a developer's opt-in build tree.
[void]$start.Environment.Remove('COMPUTATOR_GSL_DIRECTORY')
$token = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$start.Environment['ExecutionAccess__Token'] = $token
$profile = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Api-Smoke-' + [Guid]::NewGuid().ToString('N'))
$start.Environment['COMPUTATOR_DATA_DIRECTORY'] = $profile
$process = [Diagnostics.Process]::new(); $process.StartInfo = $start
$client = [Net.Http.HttpClient]::new(); $client.Timeout = [TimeSpan]::FromSeconds(30)
$client.DefaultRequestHeaders.Add('X-Computator-Session', $token)
$started = $false; $output = $null; $errors = $null
try {
    $started = $process.Start()
    if (!$started) { throw 'Could not start the published API.' }
    $output = $process.StandardOutput.ReadToEndAsync(); $errors = $process.StandardError.ReadToEndAsync()
    $ready = $false; $deadline = [DateTime]::UtcNow.AddSeconds(45)
    do {
        if ($process.HasExited) { throw "Published API exited before readiness: $($process.ExitCode)" }
        try {
            $response = $client.GetAsync("$address/api/numerical-calculations/integral/list-methods").GetAwaiter().GetResult()
            try { $ready = $response.IsSuccessStatusCode } finally { $response.Dispose() }
        } catch [Net.Http.HttpRequestException] { }
        if (!$ready) { Start-Sleep -Milliseconds 250 }
    } while (!$ready -and [DateTime]::UtcNow -lt $deadline)
    if (!$ready) { throw 'Published API readiness timed out.' }
    $anonymous = [Net.Http.HttpClient]::new()
    try {
        $response = $anonymous.GetAsync("$address/api/calculate/real/2x/21").GetAwaiter().GetResult()
        try { if ([int]$response.StatusCode -ne 403) { throw 'Published API accepted an anonymous execution request.' } } finally { $response.Dispose() }
    } finally { $anonymous.Dispose() }
    $result = $client.GetStringAsync("$address/api/calculate/real/2x/21").GetAwaiter().GetResult()
    if ($result -ne '42') { throw "Published calculation regression: $result" }
    $script = [Uri]::EscapeDataString('write(42);')
    $result = $client.GetStringAsync("$address/api/script/$script").GetAwaiter().GetResult()
    if ($result -ne '42') { throw "Published scripting regression: $result" }
    # Inspect the already-loaded module after real native-backed calculations.
    # Explicitly loading a DLL here would bypass the resolver we need to verify.
    $gslScript = @'
var gamma = gammaP(1,1);
var airy = AiPrime(0);
using var current = System.Diagnostics.Process.GetCurrentProcess();
System.Diagnostics.ProcessModule gsl = null;
foreach (System.Diagnostics.ProcessModule module in current.Modules)
    if (module.ModuleName.Equals("gsl.dll", System.StringComparison.OrdinalIgnoreCase)) gsl = module;
if (gsl == null) throw new System.Exception("GSL was not loaded by the numerical functions.");
var version = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(
    System.Runtime.InteropServices.Marshal.ReadIntPtr(
        System.Runtime.InteropServices.NativeLibrary.GetExport(gsl.BaseAddress, "gsl_version")));
write(System.Text.Json.JsonSerializer.Serialize(new { pid = current.Id, path = gsl.FileName, version, gamma, airy }));
'@
    $gsl = $client.GetStringAsync("$address/api/script/$([Uri]::EscapeDataString($gslScript))").GetAwaiter().GetResult() | ConvertFrom-Json
    $expectedGsl = [IO.Path]::GetFullPath((Join-Path $directory 'native/gsl/gsl.dll'))
    if ($gsl.pid -eq $process.Id -or $gsl.pid -le 0) { throw 'Published GSL scripting did not execute in an owned worker.' }
    if ($gsl.version -ne '2.8' -or ![string]::Equals([IO.Path]::GetFullPath($gsl.path), $expectedGsl, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Published worker selected GSL $($gsl.version) from '$($gsl.path)', expected packaged GSL 2.8 at '$expectedGsl'."
    }
    if (![double]::IsFinite($gsl.gamma) -or [Math]::Abs($gsl.gamma - (1 - [Math]::Exp(-1))) -gt 1e-12 -or
        ![double]::IsFinite($gsl.airy) -or [Math]::Abs($gsl.airy - (-0.2588194037928068)) -gt 1e-12) {
        throw 'Published GSL worker numerical regression.'
    }
    foreach ($format in @('png', 'bmp')) {
        $bytes = $client.GetByteArrayAsync("$address/api/chart/$format/300/300/0/5/0/5/x").GetAwaiter().GetResult()
        if ($bytes.Length -le 24) { throw 'Published chart payload is incomplete.' }
        if ($format -eq 'png' -and [Convert]::ToHexString($bytes[0..7]) -ne '89504E470D0A1A0A') { throw 'Published chart is not a PNG.' }
        if ($format -eq 'bmp' -and ($bytes[0] -ne 66 -or $bytes[1] -ne 77 -or $bytes.Length -lt 65536)) { throw 'Published large BMP response is invalid.' }
    }
    $swagger = $client.GetStringAsync("$address/swagger/v1/swagger.json").GetAwaiter().GetResult() | ConvertFrom-Json
    if (!$swagger.paths -or $swagger.info.title -ne 'Computator.NET.Web API') { throw 'Published OpenAPI document is invalid.' }
    Write-Host 'Published API passed: token protection, real calculation/script, packaged GSL 2.8 worker, PNG, BMP and OpenAPI.'
}
finally {
    $client.Dispose()
    if ($started -and !$process.HasExited) { $process.Kill($true); $process.WaitForExit() }
    # Runtime logs do not need and must never retain the private test token.
    if ($null -ne $output) { $output.GetAwaiter().GetResult().Replace($token, '[redacted]') | Set-Content (Join-Path $directory 'api-smoke.stdout.log') }
    if ($null -ne $errors) { $errors.GetAwaiter().GetResult().Replace($token, '[redacted]') | Set-Content (Join-Path $directory 'api-smoke.stderr.log') }
    $process.Dispose()
    if (Test-Path -LiteralPath $profile) { Remove-Item -LiteralPath $profile -Recurse -Force }
}
