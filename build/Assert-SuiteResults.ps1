[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Computator.NET.Core.Tests','Computator.NET.Core.IntegrationTests','Computator.NET.WebApi.IntegrationTests','Computator.NET.Desktop.Tests')][string]$Suite,
    [Parameter(Mandatory)][int]$MinimumTests
)
$ErrorActionPreference = 'Stop'
$skips = @()
if ($Suite -eq 'Computator.NET.WebApi.IntegrationTests') {
    $skips = @(
        'Computator.NET.WebApi.IntegrationTests.CalculateApiTests.DivideSignInExpressionWorks',
        'Computator.NET.WebApi.IntegrationTests.ScriptApiTests.DivideSignInExpressionWorks',
        'Computator.NET.WebApi.IntegrationTests.ChartApiTests.ReturnsHttpOkForChart3D'
    )
}
& (Join-Path $PSScriptRoot 'Assert-TestResults.ps1') -Path (Join-Path $PSScriptRoot "../TestResults/$Suite.trx") -MinimumTests $MinimumTests -AllowedSkippedTests $skips
