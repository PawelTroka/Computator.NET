[CmdletBinding()]
param([string]$Root = (Join-Path $PSScriptRoot '..'))
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$rootPath = (Resolve-Path -LiteralPath $Root).Path

function Read-Items([string]$Project) {
    # Evaluation only: no restore, build, desktop runtime or generated binaries.
    $output = @(& dotnet msbuild (Join-Path $rootPath "$Project/$Project.csproj") -nologo '-getItem:Compile,None,Content,EmbeddedResource,Page')
    if ($LASTEXITCODE -ne 0) { throw "Cannot evaluate project items for ${Project}: $($output -join [Environment]::NewLine)" }
    return (($output -join [Environment]::NewLine) | ConvertFrom-Json -AsHashtable).Items
}

# Representative real project contracts: WinForms, WPF, resource designers and
# an existing explicit resource relationship must all survive SDK evaluation.
$relationships = [ordered]@{
    'Computator.NET.Desktop' = @{
        'Views/MainView.Designer.cs' = 'MainView.cs'
        'Benchmarking/BenchmarkForm.Designer.cs' = 'BenchmarkForm.cs'
    }
    'Computator.NET.Charting' = @{
        'Chart3D/UI/Chart3DControl.xaml.cs' = 'Chart3DControl.xaml'
        'EditChartProperties.designer.cs' = 'EditChartProperties.cs'
    }
    'Computator.NET.Localization' = @{
        'Strings.Designer.cs' = 'Strings.resx'
        'Views/MainView.Designer.cs' = 'MainView.resx'
    }
    'Computator.NET.Core' = @{ 'Properties/Resources.Designer.cs' = 'Resources.resx' }
    'Computator.NET.DataTypes' = @{ 'Properties/GraphicsResources.Designer.cs' = 'GraphicsResources.resx' }
}
foreach ($project in $relationships.Keys) {
    $items = Read-Items $project
    foreach ($path in $relationships[$project].Keys) {
        $matches = @($items.Compile | Where-Object { $_.Identity.Replace('\', '/') -eq $path })
        if ($matches.Count -ne 1 -or $matches[0]['DependentUpon'] -ne $relationships[$project][$path]) {
            throw "Missing or incorrect IDE parent for $project/$path."
        }
    }
}

$web = Read-Items 'Computator.NET.WebClient'
$sourceAssets = @('package.json', 'package-lock.json', 'tsconfig.json', 'wwwroot/favicon.ico', 'wwwroot/img/Square50x50Logo.png')
foreach ($path in $sourceAssets) {
    $matches = @($web.None | Where-Object { $_.Identity.Replace('\', '/') -eq $path })
    if ($matches.Count -ne 1 -or $matches[0]['CopyToOutputDirectory'] -ne 'Never' -or $matches[0]['CopyToPublishDirectory'] -ne 'Never') {
        throw "Web source input must be visible exactly once without being copied to output/publish: $path"
    }
    if (@($web.Content | Where-Object { $_.Identity.Replace('\', '/') -eq $path }).Count) {
        throw "Web source input unexpectedly became published content: $path"
    }
}
$lock = @($web.None | Where-Object { $_.Identity -eq 'package-lock.json' })[0]
if ($lock['DependentUpon'] -ne 'package.json') { throw 'The npm lockfile is not grouped with package.json.' }
Write-Host 'IDE item contracts passed: six evaluated projects, designer parents and five visible frontend inputs.'
