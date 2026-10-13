[CmdletBinding()]
param([Parameter(Mandatory)][string]$PublishDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$directory = (Resolve-Path -LiteralPath $PublishDirectory).Path
$profile = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Desktop-Smoke-' + [Guid]::NewGuid().ToString('N'))
$start = [Diagnostics.ProcessStartInfo]::new((Join-Path $directory 'Computator.NET.exe'))
$start.WorkingDirectory = $directory
$start.UseShellExecute = $false
$start.Environment['COMPUTATOR_DATA_DIRECTORY'] = $profile
$process = [Diagnostics.Process]::new()
$process.StartInfo = $start
$started = $false
try {
    $started = $process.Start()
    if (!$started) { throw 'Could not start the published desktop executable.' }
    $deadline = [DateTime]::UtcNow.AddSeconds(60)
    $ready = $false
    do {
        $process.Refresh()
        if ($process.HasExited) { throw "Desktop exited before showing its main window: $($process.ExitCode)" }
        $ready = $process.MainWindowHandle -ne [IntPtr]::Zero -and $process.MainWindowTitle.StartsWith('Computator.NET')
        if (!$ready) { Start-Sleep -Milliseconds 250 }
    } while (!$ready -and [DateTime]::UtcNow -lt $deadline)
    if (!$ready) { throw "Published desktop did not show its main window. Last window: $($process.MainWindowTitle)" }
    if (!$process.WaitForInputIdle(10000)) { throw 'Published desktop did not become responsive.' }
    if (!$process.CloseMainWindow()) { throw 'Could not request a normal desktop shutdown.' }
    if (!$process.WaitForExit(10000)) { throw 'Published desktop did not close normally.' }
    if ($process.ExitCode -ne 0) { throw "Published desktop failed during shutdown: $($process.ExitCode)" }
    Write-Host 'Published NET 10 desktop passed: real executable, splash/startup, main window and normal shutdown.'
}
finally {
    if ($started -and !$process.HasExited) { $process.Kill($true); $process.WaitForExit() }
    $process.Dispose()
    if (Test-Path -LiteralPath $profile) { Remove-Item -LiteralPath $profile -Recurse -Force }
}
