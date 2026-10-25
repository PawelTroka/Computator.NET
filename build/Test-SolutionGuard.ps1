$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$guard = Join-Path $PSScriptRoot 'Assert-Solution.ps1'
$source = Join-Path $PSScriptRoot '../Computator.NET.slnx'
$directory = Join-Path ([IO.Path]::GetTempPath()) ('computator-solution-guard-' + [Guid]::NewGuid().ToString('N'))
$solutionPath = Join-Path $directory 'Computator.NET.slnx'
$baseline = Get-Content -LiteralPath $source -Raw

function Invoke-Git([string[]]$Arguments) {
    & git -C $directory @Arguments | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Fixture git command failed: $Arguments" }
}
function Add-FixtureFile([string]$Path) {
    $fullPath = Join-Path $directory $Path
    New-Item -ItemType Directory -Path (Split-Path $fullPath -Parent) -Force | Out-Null
    Set-Content -LiteralPath $fullPath -Value ''
    Invoke-Git @('add', '--', $Path)
}
function Expect-Rejection([string]$Reason) {
    $message = $null
    try { & $guard -Root $directory } catch { $message = $_.Exception.Message }
    if (!$message -or $message -notlike "*$Reason*") { throw "Expected '$Reason' rejection; got '$message'." }
}
try {
    New-Item -ItemType Directory -Path $directory | Out-Null
    Invoke-Git @('init', '--quiet')
    [xml]$solution = $baseline
    foreach ($node in $solution.SelectNodes('//Project|//File')) { Add-FixtureFile $node.Path }
    Set-Content -LiteralPath $solutionPath -Value $baseline
    Invoke-Git @('add', '--', 'Computator.NET.slnx')
    & $guard -Root $directory

    # An otherwise-valid solution used to accept newly invisible docs/workflows.
    foreach ($path in @('docs/new-developer-guide.md', '.github/workflows/new-validation.yml')) {
        Add-FixtureFile $path
        Expect-Rejection 'Required developer/historical solution item is missing'
        Invoke-Git @('rm', '--force', '--', $path)
    }

    [xml]$solution = $baseline
    $file = $solution.SelectSingleNode('//File[@Path="build/PrecompiledTslBuiltins.targets"]')
    $file.ParentNode.RemoveChild($file) | Out-Null
    $solution.Save($solutionPath)
    Expect-Rejection 'Required developer/historical solution item is missing'

    [xml]$solution = $baseline
    $file = $solution.SelectSingleNode('//File[@Path="README.md"]')
    $file.ParentNode.AppendChild($file.CloneNode($true)) | Out-Null
    $solution.Save($solutionPath)
    Expect-Rejection 'Duplicate linked files'

    [xml]$solution = $baseline
    $project = $solution.SelectSingleNode('//Project')
    $project.ParentNode.RemoveChild($project) | Out-Null
    $solution.Save($solutionPath)
    Expect-Rejection 'Solution project inventory differs'

    Set-Content -LiteralPath $solutionPath -Value $baseline
    Add-FixtureFile 'second.sln'
    Expect-Rejection 'Expected only Computator.NET.slnx'
    Invoke-Git @('rm', '--force', '--', 'second.sln')

    Remove-Item -LiteralPath (Join-Path $directory 'README.md')
    Expect-Rejection 'Missing solution item'
    Write-Host 'Solution guard tests passed: valid grouped solution and seven rejection scenarios.'
}
finally { if (Test-Path -LiteralPath $directory) { Remove-Item -LiteralPath $directory -Recurse -Force } }
