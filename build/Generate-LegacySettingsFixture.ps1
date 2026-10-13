[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$OutputPath)
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Use Windows PowerShell 5.1 for the historical Framework fixture writer.' }
Add-Type -Path (Join-Path $PSScriptRoot 'fixtures/LegacySettings.cs') -ReferencedAssemblies 'System.Drawing.dll' -IgnoreWarnings
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
[Computator.NET.Core.Properties.Settings]::Write($OutputPath)
if (!(Test-Path -LiteralPath $OutputPath)) { throw 'Legacy fixture generation failed.' }
