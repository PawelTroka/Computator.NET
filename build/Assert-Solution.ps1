[CmdletBinding()]
param([string]$Root = (Join-Path $PSScriptRoot '..'))
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$rootPath = (Resolve-Path -LiteralPath $Root).Path
Push-Location $rootPath
try {
    $tracked = @(& git ls-files)
    if ($LASTEXITCODE -ne 0) { throw 'Cannot enumerate tracked repository files.' }
    $solutions = @($tracked | Where-Object { $_ -match '\.(sln|slnx|slnf)$' })
    if ($solutions.Count -ne 1 -or $solutions[0] -ne 'Computator.NET.slnx') {
        throw "Expected only Computator.NET.slnx; found $($solutions -join ', ')."
    }
    [xml]$solution = Get-Content -LiteralPath 'Computator.NET.slnx' -Raw
    $projects = @($solution.SelectNodes('//Project') | ForEach-Object { $_.Path.Replace('\', '/') })
    $historical = @('Computator.NET/Computator.NET.net40.csproj', 'Computator.NET.Desktop.UITests/Computator.NET.Desktop.UITests.csproj')
    # These active probes require the explicit ComputatorMono=true graph and
    # belong to Mono-Validate, rather than the default Windows solution build.
    $monoProbes = @('build/mono/SmokeTests/Computator.NET.Mono.SmokeTests.csproj', 'build/mono/StartupProbe/Computator.NET.Mono.StartupProbe.csproj')
    $outsideDefaultBuild = $historical + $monoProbes
    $expected = @($tracked | Where-Object { $_.EndsWith('.csproj') -and $_ -notin $outsideDefaultBuild })
    $difference = @(Compare-Object ($expected | Sort-Object) ($projects | Sort-Object))
    if ($difference.Count) { throw "Solution project inventory differs from the repository: $($difference | Out-String)" }
    if (@($projects | Select-Object -Unique).Count -ne $projects.Count) { throw 'Duplicate projects in the solution.' }
    foreach ($node in $solution.SelectNodes('//Project|//File')) {
        if (![IO.File]::Exists((Join-Path $rootPath $node.Path))) { throw "Missing solution item: $($node.Path)" }
    }
    $items = @($solution.SelectNodes('//File') | ForEach-Object { $_.Path.Replace('\', '/') })
    if (@($items | Select-Object -Unique).Count -ne $items.Count) { throw 'Duplicate linked files in the solution.' }
    $folders = @($solution.SelectNodes('//Folder') | ForEach-Object { $_.Name })
    if (@($folders | Select-Object -Unique).Count -ne $folders.Count) { throw 'Duplicate folder names in the solution.' }
    # Guard discoverability as the repository grows, not just paths already in
    # the solution. Binary archives/documents stay out of this source inventory.
    $developerFiles = @($tracked | Where-Object {
        ($_ -match '^(build/|\.github/|\.config/|Shared/)' -and -not $_.EndsWith('.csproj')) -or
        ($_ -match '^docs/' -and ($_ -match '\.(md|json|py|rtf)$' -or $_ -match '/SHA256SUMS$')) -or
        ($_ -notmatch '/' -and $_ -match '\.(md|props|targets|DotSettings)$')
    })
    foreach ($path in @('build.cake', 'build.ps1', 'build.cmd', 'build.sh', 'Directory.Packages.props') + $outsideDefaultBuild + $developerFiles) {
        if ($path -notin $items) { throw "Required developer/historical solution item is missing: $path" }
    }
    Write-Host "One solution; $($projects.Count) buildable projects; $($items.Count) linked developer and historical items."
}
finally { Pop-Location }
