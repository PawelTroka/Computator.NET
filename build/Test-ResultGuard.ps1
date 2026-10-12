$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$directory = Join-Path ([IO.Path]::GetTempPath()) ('computator-trx-guard-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $directory | Out-Null
$path = Join-Path $directory 'fixture.trx'
$guard = Join-Path $PSScriptRoot 'Assert-TestResults.ps1'
function Write-Fixture([string]$Outcome = 'Passed', [string]$RunOutcome = 'Completed', [int]$Copies = 1) {
    $countPassed = if ($Outcome -eq 'Passed') { $Copies } else { 0 }
    $entries = (@(1..$Copies | ForEach-Object {
        '<UnitTestResult testId="one" testName="KnownLimitation" outcome="' + $Outcome + '" />'
    }) -join '')
    Set-Content -LiteralPath $path -Value @"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>$entries</Results>
  <TestDefinitions><UnitTest id="one"><TestMethod className="Tests" /></UnitTest></TestDefinitions>
  <ResultSummary outcome="$RunOutcome"><Counters total="$Copies" passed="$countPassed" /></ResultSummary>
</TestRun>
"@
}
function Expect-Rejection([scriptblock]$Action) {
    $rejected = $false
    try { & $Action } catch { $rejected = $true }
    if (!$rejected) { throw 'The result guard accepted an invalid test report.' }
}
try {
    Write-Fixture
    & $guard -Path $path -MinimumTests 1
    Expect-Rejection { & $guard -Path $path -MinimumTests 2 }
    Write-Fixture -Outcome 'NotExecuted'
    & $guard -Path $path -MinimumTests 1 -AllowedSkippedTests @('Tests.KnownLimitation')
    Expect-Rejection { & $guard -Path $path -MinimumTests 1 }
    Write-Fixture -Outcome 'Failed'
    Expect-Rejection { & $guard -Path $path -MinimumTests 1 }
    Write-Fixture -RunOutcome 'Aborted'
    Expect-Rejection { & $guard -Path $path -MinimumTests 1 }
    Write-Fixture -Copies 2
    Expect-Rejection { & $guard -Path $path -MinimumTests 1 }
    Write-Fixture
    (Get-Content -LiteralPath $path -Raw).Replace('total="1"', 'total="2"') | Set-Content -LiteralPath $path
    Expect-Rejection { & $guard -Path $path -MinimumTests 1 }
    Write-Fixture
    (Get-Content -LiteralPath $path -Raw).Replace('<Results>', '<Results><!--').Replace('</Results>', '--></Results>') | Set-Content -LiteralPath $path
    Expect-Rejection { & $guard -Path $path -MinimumTests 1 }
    Expect-Rejection { & $guard -Path (Join-Path $directory 'missing.trx') -MinimumTests 1 }
    Write-Host 'Result guard self-tests passed: valid runs, permitted skips and eight rejection scenarios.'
}
finally { Remove-Item -LiteralPath $directory -Recurse -Force }
