param([Parameter(Mandatory)][string]$PublishDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = (Resolve-Path -LiteralPath $PublishDirectory).Path
$exe = Join-Path $root 'computator.dll'
$script:cases = 0
function Invoke-Cli {
    param([string[]]$Arguments, [int]$ExpectedExit = 0)
    $info = [Diagnostics.ProcessStartInfo]::new('dotnet')
    $info.UseShellExecute = $false
    $info.RedirectStandardError = $true
    $info.RedirectStandardOutput = $true
    $info.StandardOutputEncoding = [Text.Encoding]::UTF8
    $info.StandardErrorEncoding = [Text.Encoding]::UTF8
    $info.ArgumentList.Add($exe)
    foreach ($argument in $Arguments) { $info.ArgumentList.Add($argument) }
    $child = [Diagnostics.Process]::Start($info)
    try {
        # Drain both pipes before waiting. ReadToEnd before WaitForExit made the
        # previous timeout ineffective when a regression never closed stderr.
        $stdout = $child.StandardOutput.ReadToEndAsync()
        $stderr = $child.StandardError.ReadToEndAsync()
        if (!$child.WaitForExit(30000)) { throw 'Published CLI check timed out.' }
        if (![Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout, $stderr), 5000)) { throw 'CLI pipes did not close.' }
        if ($child.ExitCode -ne $ExpectedExit) { throw "CLI exit $($child.ExitCode), expected $ExpectedExit`: $($stderr.Result)" }
        if ($ExpectedExit -eq 0) {
            if ($stderr.Result) { throw 'Successful CLI command wrote to stderr.' }
            $value = $stdout.Result | ConvertFrom-Json
            if ($value.result -isnot [string]) { throw 'CLI result contract changed.' }
        } else {
            if ($stdout.Result) { throw 'Failed CLI command wrote a success response.' }
            $value = $stderr.Result | ConvertFrom-Json
            if (!$value.error) { throw 'CLI error contract changed.' }
            if ($ExpectedExit -eq 2 -and ($value.error -ne 'compilation' -or @($value.diagnostics).Count -eq 0)) { throw 'Compilation diagnostics were lost.' }
        }
        $script:cases++
        return $value
    } finally {
        if (!$child.HasExited) {
            $child.Kill($true)
            if (!$child.WaitForExit(10000)) { throw 'Owned CLI process did not terminate.' }
        }
        $child.Dispose()
    }
}
function Assert-Number {
    param([string[]]$Arguments, [double]$Expected, [double]$Tolerance = 1e-8)
    $value = Invoke-Cli -Arguments $Arguments
    $actual = [double]::Parse($value.result, [Globalization.CultureInfo]::InvariantCulture)
    if (![double]::IsFinite($actual) -or [Math]::Abs($actual - $Expected) -gt $Tolerance) { throw "Incorrect numerical CLI result: $actual, expected $Expected." }
}
$temp = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Portable-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    Assert-Number -Arguments @('calculate','--expression','2x+3','--x','7') -Expected 17
    Assert-Number -Arguments @('calculate','--expression','x/y','--x','-7.5','--y','2') -Expected -3.75
    $functions = Join-Path $temp 'Żółć functions.tslf'
    $scriptFile = Join-Path $temp 'script λ.tsl'
    [IO.File]::WriteAllText($functions, 'static real Probe(real x) { return x/2; }', [Text.UTF8Encoding]::new($true))
    [IO.File]::WriteAllText($scriptFile, 'write(Probe(6)); write("zażółć λ");', [Text.UTF8Encoding]::new($true))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile,'--functions',$functions)
    if ($value.result -ne '3zażółć λ') { throw 'Published portable UTF-8 document execution failed.' }
    # Ordinary trusted code and referenced libraries may write to Console. Those
    # messages must not become extra JSON documents or corrupt structured errors.
    [IO.File]::WriteAllText($scriptFile, 'Console.WriteLine("discarded stdout λ"); Console.Error.WriteLine("discarded stderr λ"); write("kept λ");', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile)
    if ($value.result -ne 'kept λ') { throw 'Console isolation changed TSL write output.' }
    [IO.File]::WriteAllText($scriptFile, 'Console.WriteLine("discarded stdout"); Console.Error.WriteLine("discarded stderr"); throw new InvalidOperationException("expected execution failure");', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile) -ExpectedExit 1
    if ($value.error -ne 'InvalidOperationException' -or $value.message -ne 'expected execution failure') { throw 'Console isolation lost the execution error.' }
    [IO.File]::WriteAllText($functions, 'static real Probe(real x) { Console.WriteLine("discarded stdout"); Console.Error.WriteLine("discarded stderr"); return x/2; }', [Text.UTF8Encoding]::new($false))
    Assert-Number -Arguments @('calculate','--expression','Probe(6)','--functions',$functions) -Expected 3
    Assert-Number -Arguments @('integrate','--expression','Probe(x)','--a','0','--b','6','--steps','2','--functions',$functions) -Expected 9 -Tolerance 1e-10
    [IO.File]::WriteAllText($functions, 'static real Probe(real x) { Console.WriteLine("discarded stdout"); Console.Error.WriteLine("discarded stderr"); throw new InvalidOperationException("expected function failure"); }', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('calculate','--expression','Probe(6)','--functions',$functions) -ExpectedExit 1
    if ($value.error -ne 'InvalidOperationException' -or $value.message -ne 'expected function failure') { throw 'Console isolation lost the custom function error.' }
    # Keep the protocol writers usable even when trusted code replaces Console.
    $redirect = 'Console.SetOut(new System.IO.StringWriter()); Console.SetError(new System.IO.StringWriter()); '
    [IO.File]::WriteAllText($scriptFile, $redirect + 'write("restored");', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile)
    if ($value.result -ne 'restored') { throw 'The result protocol writer was replaced.' }
    [IO.File]::WriteAllText($scriptFile, $redirect + 'throw new InvalidOperationException("restored failure");', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile) -ExpectedExit 1
    if ($value.error -ne 'InvalidOperationException' -or $value.message -ne 'restored failure') { throw 'The error protocol writer was replaced.' }
    # ProcessExit runs after Main returns; ordinary shutdown logging must stay
    # outside the protocol too, on both the success and failure paths.
    $shutdown = 'AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => { Console.WriteLine("ordinary shutdown log"); Console.Error.WriteLine("ordinary shutdown warning"); }; '
    [IO.File]::WriteAllText($scriptFile, $shutdown + 'write("42");', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile)
    if ($value.result -ne '42') { throw 'Shutdown logging corrupted the result.' }
    [IO.File]::WriteAllText($scriptFile, $shutdown + 'throw new InvalidOperationException("expected shutdown failure");', [Text.UTF8Encoding]::new($false))
    $value = Invoke-Cli -Arguments @('script','--file',$scriptFile) -ExpectedExit 1
    if ($value.error -ne 'InvalidOperationException' -or $value.message -ne 'expected shutdown failure') { throw 'Shutdown logging corrupted the structured error.' }
    [IO.File]::WriteAllText($functions, 'static real Probe(real x) { return x/2; }', [Text.UTF8Encoding]::new($true))
    Assert-Number -Arguments @('integrate','--expression','x*x','--a','0','--b','3','--steps','100') -Expected 9 -Tolerance 1e-10
    Assert-Number -Arguments @('integrate','--expression','Probe(x)','--a','0','--b','6','--functions',$functions) -Expected 9 -Tolerance 1e-10
    Assert-Number -Arguments @('differentiate','--expression','x*x','--x','3') -Expected 6 -Tolerance 1e-6
    Assert-Number -Arguments @('differentiate','--expression','x*x','--x','3','--order','2') -Expected 2 -Tolerance 1e-5
    Assert-Number -Arguments @('root','--expression','x*x-2','--a','0','--b','2','--tolerance','1e-10','--iterations','100') -Expected ([Math]::Sqrt(2)) -Tolerance 1e-8
    $null = Invoke-Cli -Arguments @('calculate','--expression','missing(2)') -ExpectedExit 2
    $null = Invoke-Cli -Arguments @('integrate','--expression','missing(x)','--a','0','--b','1') -ExpectedExit 2
    foreach ($arguments in @(
        @('unknown'),
        @('calculate','--expression','1','--order','2'),
        @('calculate','--expression','x','--x','1','--x','2'),
        @('calculate','--expression'),
        @('calculate','--expression','x','--x','NaN'),
        @('calculate','--expression','x','--x','Infinity'),
        @('calculate','--expression','x','--x','1e999'),
        @('calculate','--expression','x','--x','3,5'),
        @('integrate','--expression','x','--a','0'),
        @('integrate','--expression','x','--a','1','--b','1'),
        @('integrate','--expression','x','--a','0','--b','1','--steps','3'),
        @('integrate','--expression','x','--a','0','--b','1','--steps','2.5'),
        @('differentiate','--expression','x','--x','0','--order','0'),
        @('root','--expression','x','--a','-1','--b','1','--tolerance','0'),
        @('root','--expression','x','--a','-1','--b','1','--iterations','1000001')
    )) { $null = Invoke-Cli -Arguments $arguments -ExpectedExit 1 }
    foreach ($bytes in @(
        [byte[]]@(0xc3,0x28),
        [byte[]]@(0xff,0xfe,0x31,0x00),
        [Text.Encoding]::UTF8.GetBytes((' ' * 262145)),
        [Text.Encoding]::UTF8.GetBytes((' ' * 65537))
    )) {
        [IO.File]::WriteAllBytes($scriptFile, $bytes)
        $null = Invoke-Cli -Arguments @('script','--file',$scriptFile) -ExpectedExit 1
    }
    # Failure must not poison the next independent invocation.
    Assert-Number -Arguments @('integrate','--expression','x','--a','0','--b','2','--steps','2') -Expected 2 -Tolerance 1e-12
    if ($script:cases -lt 39) { throw "Expected at least 39 published CLI process checks, but ran $script:cases." }
    Write-Host "Published portable CLI passed $script:cases process checks: numerical commands, documents, strict options, Console isolation, errors and recovery."
} finally { Remove-Item -LiteralPath $temp -Recurse -Force }
