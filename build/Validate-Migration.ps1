[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [ValidateSet('All', 'Computator.NET.Core.Tests', 'Computator.NET.Core.IntegrationTests',
        'Computator.NET.WebApi.IntegrationTests', 'Computator.NET.Desktop.Tests')]
    [string]$TestSuite = 'All',
    [switch]$UpdateLocks
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) {
    throw 'The desktop and mathematical API require Windows with .NET Framework 4.8.1. The WebClient can be built independently on Linux/macOS.'
}
function Invoke-Dotnet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE." }
}
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & (Join-Path $PSScriptRoot 'Test-ResultGuard.ps1')
    [xml]$modern = Get-Content 'Computator.NET.Modern.slnx' -Raw
    $modernProjects = @($modern.SelectNodes('//Project') | ForEach-Object { $_.Path.Replace('\', '/') } | Sort-Object)
    $classicProjects = @([regex]::Matches((Get-Content 'Computator.NET.sln' -Raw), '"([^"\r\n]+\.csproj)"') |
        ForEach-Object { $_.Groups[1].Value.Replace('\', '/') } | Sort-Object)
    if (Compare-Object $modernProjects $classicProjects) { throw 'The .sln and .slnx project lists differ.' }
    $restore = @('restore', 'Computator.NET.sln', '-p:WarningsAsErrors=NU1900%3BNU1901%3BNU1902%3BNU1903%3BNU1904')
    if (!$UpdateLocks) { $restore += '--locked-mode' }
    Invoke-Dotnet $restore
    Invoke-Dotnet @('build', 'Computator.NET.sln', '-c', $Configuration, '--no-restore', '-bl:desktop.binlog')
    $minimumTests = @{
        'Computator.NET.Core.Tests' = 223
        'Computator.NET.Core.IntegrationTests' = 947
        'Computator.NET.WebApi.IntegrationTests' = 52
        'Computator.NET.Desktop.Tests' = 5
    }
    # These three tests were already ignored on master. A newly ignored test
    # must fail validation even when the adapter reports an exit code of zero.
    $knownApiSkips = @(
        'Computator.NET.WebApi.IntegrationTests.CalculateApiTests.DivideSignInExpressionWorks',
        'Computator.NET.WebApi.IntegrationTests.ScriptApiTests.DivideSignInExpressionWorks',
        'Computator.NET.WebApi.IntegrationTests.ChartApiTests.ReturnsHttpOkForChart3D'
    )
    $projects = @('Computator.NET.Core.Tests', 'Computator.NET.WebApi.IntegrationTests',
        'Computator.NET.Desktop.Tests', 'Computator.NET.Core.IntegrationTests')
    if ($TestSuite -ne 'All') { $projects = @($TestSuite) }
    foreach ($project in $projects) {
        $results = Join-Path 'TestResults' "$project.trx"
        if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results }
        Invoke-Dotnet @('test', "$project/$project.csproj", '-c', $Configuration, '--no-build',
            '--blame-hang-timeout', '120s', '--blame-hang-dump-type', 'none',
            '--logger', 'console;verbosity=normal', '--logger', "trx;LogFileName=$project.trx",
            '--results-directory', 'TestResults')
        $allowedSkips = @()
        if ($project -eq 'Computator.NET.WebApi.IntegrationTests') { $allowedSkips = $knownApiSkips }
        & (Join-Path $PSScriptRoot 'Assert-TestResults.ps1') -Path $results -MinimumTests $minimumTests[$project] -AllowedSkippedTests $allowedSkips
    }
    if ($TestSuite -in @('All', 'Computator.NET.Desktop.Tests')) {
        Invoke-Dotnet @('publish', 'Computator.NET/Computator.NET.csproj', '-c', $Configuration,
            '--no-restore', '--output', 'artifacts/desktop')
        foreach ($file in @('Computator.NET.exe', 'Computator.NET.exe.config', 'Microsoft.CodeAnalysis.CSharp.dll',
            'MathNet.Numerics.dll', 'Meta.Numerics.dll', 'Static/functions.xml', 'Static/TSL-Syntax.xshd',
            'pl/Computator.NET.Localization.resources.dll', 'CHANGELOG')) {
            if (!(Test-Path (Join-Path 'artifacts/desktop' $file))) { throw "Missing published file: $file" }
        }
    }
}
finally { Pop-Location }
