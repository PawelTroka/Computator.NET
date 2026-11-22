param([Parameter(Mandatory)][string]$PublishDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$exe = Join-Path (Resolve-Path -LiteralPath $PublishDirectory).Path 'computator.dll'
$script:cases = 0

function Invoke-Symbolic {
    param([string[]]$Arguments, [int]$ExpectedExit = 0)
    $info = [Diagnostics.ProcessStartInfo]::new('dotnet')
    $info.UseShellExecute = $false
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.StandardOutputEncoding = [Text.Encoding]::UTF8
    $info.StandardErrorEncoding = [Text.Encoding]::UTF8
    foreach ($argument in @($exe, 'symbolic') + $Arguments) { $info.ArgumentList.Add($argument) }
    $child = [Diagnostics.Process]::Start($info)
    try {
        $stdout = $child.StandardOutput.ReadToEndAsync()
        $stderr = $child.StandardError.ReadToEndAsync()
        if (!$child.WaitForExit(30000)) { throw 'Published symbolic CLI did not finish within its process deadline.' }
        if (![Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout, $stderr), 5000)) { throw 'Symbolic CLI pipes did not close.' }
        if ($child.ExitCode -ne $ExpectedExit) { throw "Symbolic exit $($child.ExitCode), expected $ExpectedExit`: $($stderr.Result)" }
        $script:cases++
        if ($ExpectedExit -ne 0) {
            if ($stdout.Result) { throw 'A failed symbolic request emitted a success response.' }
            $value = $stderr.Result | ConvertFrom-Json
            if (!$value.error -or !$value.message) { throw 'A symbolic input error lost its diagnostic.' }
            return $value
        }
        if ($stderr.Result) { throw 'A successful symbolic command wrote to stderr.' }
        $value = $stdout.Result | ConvertFrom-Json
        if ($value.result -isnot [string] -or $value.latex -isnot [string] -or
            $value.status -notin @('exact', 'approximate', 'unevaluated', 'undefined') -or
            $value.warnings -isnot [array]) { throw 'Symbolic result/status/LaTeX/warnings contract changed.' }
        return $value
    } finally {
        if (!$child.HasExited) {
            $child.Kill($true)
            if (!$child.WaitForExit(10000)) { throw 'Owned symbolic CLI process survived termination.' }
        }
        $child.Dispose()
    }
}
function Assert-Exact {
    param([string[]]$Arguments, [string]$Expected)
    $value = Invoke-Symbolic -Arguments $Arguments
    if ($value.status -ne 'exact' -or ($value.result -replace '\s', '') -cne $Expected) {
        throw "Symbolic answer '$($value.result)' ($($value.status)), expected exact '$Expected'."
    }
    return $value
}

$null = Assert-Exact @('--operation','simplify','--expression','1/3+2/3') '1'
$null = Assert-Exact @('--operation','simplify','--expression','sin(π/2)') '1'
$null = Assert-Exact @('--operation','differentiate','--expression','x³','--order','3') '6'
$null = Assert-Exact @('--operation','differentiate','--expression','x*y','--variable','y','--order','2') '0'
$null = Assert-Exact @('--operation','definite-integrate','--expression','x^2','--lower','0','--upper','3') '9'
$null = Assert-Exact @('--operation','limit','--expression','sin(x)/x','--point','0') '1'
$null = Assert-Exact @('--operation','substitute','--expression','x+y','--substitutions','{"x":"1/3","y":"2/3"}') '1'
$null = Assert-Exact @('--operation','determinant','--expression','[[1,2],[3,4]]') '-2'
$null = Assert-Exact @('--operation','trace','--expression','[[1,2],[3,4]]') '5'

# The process contract carries every supported operation, with independent
# scalar values after a round trip instead of depending on cosmetic term order.
foreach ($example in @(
    @{ Operation='expand'; Expression='(x+1)^3'; Expected='27' },
    @{ Operation='factor'; Expression='x^2-1'; Expected='3' }
)) {
    $value = Invoke-Symbolic @('--operation',$example.Operation,'--expression',$example.Expression)
    $null = Assert-Exact @('--operation','substitute','--expression',$value.result,'--substitutions','{"x":"2"}') $example.Expected
}
$integral = Invoke-Symbolic @('--operation','integrate','--expression','2*x')
$derivative = Invoke-Symbolic @('--operation','differentiate','--expression',$integral.result)
$null = Assert-Exact @('--operation','substitute','--expression',$derivative.result,'--substitutions','{"x":"3"}') '6'
$series = Invoke-Symbolic @('--operation','taylor','--expression','exp(x)','--order','6')
$null = Assert-Exact @('--operation','substitute','--expression',$series.result,'--substitutions','{"x":"0"}') '1'
$roots = Invoke-Symbolic @('--operation','solve','--expression','x^2=1')
$rootText = $roots.result -replace '\s', ''
if ($roots.status -ne 'exact' -or $rootText -notin @('{-1,1}','{1,-1}')) { throw 'Exact equation roots were lost.' }
$none = Invoke-Symbolic @('--operation','solve','--expression','x^2+1=0','--domain','real')
if ($none.status -ne 'exact' -or ($none.result -replace '\s', '') -ne '{}') { throw 'Real and complex solving domains were confused.' }
$system = Invoke-Symbolic @('--operation','solve-system','--equations','["x+y=3","x-y=1"]','--variables','["x","y"]')
if ($system.status -ne 'exact' -or $system.result -notmatch '2' -or $system.result -notmatch '1') { throw 'The actual system solver did not return its solution.' }
$inverse = Invoke-Symbolic @('--operation','inverse','--expression','[[1,0],[0,1]]')
$null = Assert-Exact @('--operation','determinant','--expression',$inverse.result) '1'
$transpose = Invoke-Symbolic @('--operation','transpose','--expression','[[1,2],[3,4]]')
$null = Assert-Exact @('--operation','determinant','--expression',$transpose.result) '-2'
$approximate = Invoke-Symbolic @('--operation','evaluate','--expression','pi','--precision','40')
if ($approximate.status -ne 'approximate' -or !$approximate.result.StartsWith('3.141592653589793238462643383279502884')) { throw 'Requested arbitrary precision was lost.' }

foreach ($arguments in @(
    @(),
    @('--operation','missing','--expression','x'),
    @('--operation','simplify'),
    @('--operation','simplify','--expression','('),
    @('--operation','simplify','--expression','x','--operation','factor'),
    @('--operation','simplify','--expression','x','--order','2'),
    @('--operation','differentiate','--expression','x','--order','2.5'),
    @('--operation','differentiate','--expression','x','--order','21'),
    @('--operation','simplify','--expression','x','--domain','unknown'),
    @('--operation','limit','--expression','x','--direction','up'),
    @('--operation','definite-integrate','--expression','x','--lower','0'),
    @('--operation','substitute','--expression','x'),
    @('--operation','substitute','--expression','x','--substitutions','{"x":"1","x":"2"}'),
    @('--operation','substitute','--expression','x','--substitutions','{"x":2}'),
    @('--operation','substitute','--expression','x','--substitutions','[]'),
    @('--operation','substitute','--expression','x','--substitutions','{'),
    @('--operation','solve-system','--equations','[2]','--variables','["x"]'),
    @('--operation','solve-system','--equations','["x=1"]','--variables','{}'),
    @('--operation','solve-system','--equations','["x=1"]','--variables','["x"]','--expression','x'),
    @('--operation','evaluate','--expression','pi','--precision','101'),
    @('--operation','simplify','--expression',('x' * 8193)),
    @('--operation','simplify','--expression','x','--functions','not-a-symbolic-function-file.tslf')
)) { $null = Invoke-Symbolic -Arguments $arguments -ExpectedExit 1 }

# A rejected request must not break a later published-process invocation.
$null = Assert-Exact @('--operation','simplify','--expression','2+3') '5'
if ($script:cases -lt 48) { throw "Expected at least 48 symbolic CLI process checks; ran $script:cases." }
Write-Host "Published symbolic CLI passed $script:cases process checks, including all operations, exact/approximate results, Unicode, domains, strict options and recovery."
