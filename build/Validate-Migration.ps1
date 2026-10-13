# Compatibility shim for existing callers. All build/test/publish orchestration is Cake.
[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [string]$TestSuite = 'All',
    [switch]$UpdateLocks
)
if ($UpdateLocks) { Write-Warning 'NuGet lockfiles are no longer used. Update Directory.Packages.props and run Cake CI.' }
& (Join-Path $PSScriptRoot '../build.ps1') -Target CI -Configuration $Configuration "--suite=$TestSuite"
