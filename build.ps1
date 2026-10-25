[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$Target = 'Default',
    [ValidateSet('Debug','Release')][string]$Configuration = $(if ($env:configuration) { $env:configuration } elseif ($env:build_config) { $env:build_config } else { 'Release' }),
    [ValidateSet('Quiet','Minimal','Normal','Verbose','Diagnostic')][string]$Verbosity = 'Normal',
    [Alias('h')][switch]$Help,
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$CakeArguments
)
$ErrorActionPreference = 'Stop'

# Do not restore tools or create build output just to explain how to get started.
if ($Help -or $CakeArguments -contains '--help') {
    Get-Content -LiteralPath (Join-Path $PSScriptRoot 'build/cli-help.txt')
    return
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'The .NET SDK is not on PATH. Install the SDK selected by global.json, restart your terminal, and retry. Run ./build.ps1 -Help for setup and target guidance.'
}
Push-Location $PSScriptRoot
try {
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Cake tool restore failed ($LASTEXITCODE)." }
    & dotnet cake build.cake "--target=$Target" "--configuration=$Configuration" "--verbosity=$Verbosity" @CakeArguments
    if ($LASTEXITCODE -ne 0) { throw "Cake target '$Target' failed ($LASTEXITCODE)." }
}
finally { Pop-Location }
