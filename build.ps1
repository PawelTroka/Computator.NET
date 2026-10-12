[CmdletBinding()]
param(
    [string]$Target = 'Default',
    [ValidateSet('Debug','Release')][string]$Configuration = $(if ($env:configuration) { $env:configuration } elseif ($env:build_config) { $env:build_config } else { 'Release' }),
    [ValidateSet('Quiet','Minimal','Normal','Verbose','Diagnostic')][string]$Verbosity = 'Normal',
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$CakeArguments
)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Cake tool restore failed ($LASTEXITCODE)." }
    & dotnet cake build.cake "--target=$Target" "--configuration=$Configuration" "--verbosity=$Verbosity" @CakeArguments
    if ($LASTEXITCODE -ne 0) { throw "Cake target '$Target' failed ($LASTEXITCODE)." }
}
finally { Pop-Location }
