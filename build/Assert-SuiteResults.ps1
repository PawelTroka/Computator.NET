[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Computator.NET.Core.Tests','Computator.NET.Core.IntegrationTests','Computator.NET.WebApi.IntegrationTests','Computator.NET.Desktop.Tests')][string]$Suite,
    [Parameter(Mandatory)][int]$MinimumTests
)
$ErrorActionPreference = 'Stop'
$skips = @()
& (Join-Path $PSScriptRoot 'Assert-TestResults.ps1') -Path (Join-Path $PSScriptRoot "../TestResults/$Suite.trx") -MinimumTests $MinimumTests -AllowedSkippedTests $skips
