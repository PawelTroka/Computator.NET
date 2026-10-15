[CmdletBinding(DefaultParameterSetName = 'Smoke')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Smoke')][string]$PublishDirectory,
    [Parameter(Mandatory, ParameterSetName = 'Contracts')][switch]$TestContracts,
    [string]$DiagnosticsDirectory = (Join-Path $PSScriptRoot '../TestResults/desktop-smoke')
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$IsWindows) { throw 'Desktop process validation requires Windows.' }

function Get-SmokeCleanupPath([string]$Path) {
    $resolved = (Resolve-Path -LiteralPath $Path).Path.TrimEnd('\', '/')
    $temporaryPrefix = (Resolve-Path -LiteralPath ([IO.Path]::GetTempPath())).Path.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    if (!$resolved.StartsWith($temporaryPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        ((Get-Item -LiteralPath $resolved).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw 'Refusing to remove a redirected smoke directory or a path outside the temporary directory.'
    }
    return $resolved
}

function Test-SmokeProcessIdentity([Diagnostics.Process]$Process, [string]$StartedUtc) {
    if ($Process.HasExited) { return $false }
    try {
        return $Process.StartTime.ToUniversalTime().ToString('o') -eq $StartedUtc -and !$Process.HasExited
    } catch [InvalidOperationException] {
        if ($Process.HasExited) { return $false }
        throw
    }
}

# No dumps, screenshots, environment variables, window captions, user documents
# or settings are captured. Each invocation has its own result, even in aggregates.
if ($TestContracts) {
    $temporary = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Smoke-Contracts-' + [Guid]::NewGuid().ToString('N'))
    $results = Join-Path ([IO.Path]::GetFullPath($DiagnosticsDirectory)) ('contracts-' + [Guid]::NewGuid().ToString('N'))
    $null = [IO.Directory]::CreateDirectory($temporary)
    try {
        # Exercise only the path validator for rejected locations, never deletion.
        # The checkout itself may legitimately be beneath TEMP.
        foreach ($path in @([IO.Path]::GetTempPath(), [IO.Path]::GetPathRoot([IO.Path]::GetTempPath()))) {
            $rejected = $false
            try { $null = Get-SmokeCleanupPath $path } catch { $rejected = $true }
            if (!$rejected) { throw 'Smoke cleanup accepted an unowned location.' }
        }
        $current = [Diagnostics.Process]::GetCurrentProcess()
        try {
            $startedUtc = $current.StartTime.ToUniversalTime().ToString('o')
            if (!(Test-SmokeProcessIdentity $current $startedUtc) -or (Test-SmokeProcessIdentity $current '2000-01-01T00:00:00.0000000Z')) {
                throw 'Smoke process identity must distinguish reuse of a PID from the owned instance.'
            }
        } finally { $current.Dispose() }
        # Framework is only a synthetic WinForms fixture producer, as in the
        # existing settings-fixture tests. No dependency is added to the product.
        $source = Join-Path $temporary 'Fixture.cs'
        [IO.File]::WriteAllText($source, @'
using System;
using System.IO;
using System.Windows.Forms;
class Fixture {
    [STAThread] static void Main() {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        if (File.Exists(Path.Combine(root, "early-exit"))) { Environment.Exit(23); return; }
        using (Form form = new Form()) {
            form.Text = "Computator.NET synthetic smoke fixture";
            form.FormClosing += delegate(object sender, FormClosingEventArgs e) {
                if (File.Exists(Path.Combine(root, "refuse-close"))) e.Cancel = true;
                if (File.Exists(Path.Combine(root, "bad-exit"))) Environment.ExitCode = 17;
            };
            Application.Run(form);
        }
    }
}
'@)
        $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
        $fixture = Join-Path $temporary 'fixture.exe'
        & $compiler /nologo /target:winexe "/out:$fixture" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll $source
        if ($LASTEXITCODE -ne 0) { throw 'Could not compile the synthetic desktop fixture.' }
        foreach ($mode in @('normal', 'refuse-close', 'early-exit', 'bad-exit')) {
            $directory = Join-Path $temporary $mode
            $output = Join-Path $results $mode
            $null = [IO.Directory]::CreateDirectory($directory)
            Copy-Item -LiteralPath $fixture -Destination (Join-Path $directory 'Computator.NET.exe')
            if ($mode -ne 'normal') { [IO.File]::WriteAllText((Join-Path $directory $mode), '') }
            $info = [Diagnostics.ProcessStartInfo]::new((Join-Path $PSHOME 'pwsh.exe'))
            $info.UseShellExecute = $false
            $info.CreateNoWindow = $true
            $info.RedirectStandardOutput = $true
            $info.RedirectStandardError = $true
            foreach ($argument in @('-NoProfile', '-File', $PSCommandPath, '-PublishDirectory', $directory, '-DiagnosticsDirectory', $output)) { $info.ArgumentList.Add($argument) }
            $child = [Diagnostics.Process]::Start($info)
            $childStartedUtc = $child.StartTime.ToUniversalTime().ToString('o')
            try {
                $stdout = $child.StandardOutput.ReadToEndAsync()
                $stderr = $child.StandardError.ReadToEndAsync()
                if (!$child.WaitForExit(100000)) { throw "Synthetic smoke check timed out: $mode" }
                if (Test-SmokeProcessIdentity $child $childStartedUtc) { throw 'An exited process was reported as a live owned instance.' }
                if (![Threading.Tasks.Task]::WaitAll([Threading.Tasks.Task[]]@($stdout, $stderr), 5000)) { throw 'Synthetic smoke output did not close.' }
                if (($child.ExitCode -eq 0) -ne ($mode -eq 'normal')) { throw "Incorrect smoke exit status for $mode`: $($stderr.Result)" }
                $files = @(Get-ChildItem -LiteralPath $output -Recurse -Filter result.json)
                if ($files.Count -ne 1) { throw 'Expected one invocation-specific diagnostic report.' }
                $report = Get-Content -LiteralPath $files[0].FullName -Raw | ConvertFrom-Json
                if ($report.executableSha256 -ne (Get-FileHash -LiteralPath $fixture -Algorithm SHA256).Hash) { throw 'Evidence does not identify the tested executable.' }
                if ($report.cleanupErrors.Count -ne 0) { throw 'Synthetic fixture cleanup failed.' }
                if ($mode -eq 'normal') {
                    if (!$report.passed -or !$report.normalShutdown -or $report.forcedTermination) { throw 'Normal shutdown evidence is incorrect.' }
                } else {
                    if ($report.passed -or $report.normalShutdown) { throw 'Failed fixture was reported as successful.' }
                    $before = @($report.samples | Where-Object label -eq 'failure-before-cleanup')
                    if ($before.Count -ne 1) { throw 'Failure was not captured before cleanup.' }
                    if ($mode -eq 'refuse-close' -and (!$report.forcedTermination -or $before[0].hasExited -or $report.failure.phase -ne 'shutdown')) { throw 'Refused normal close was hidden or misreported.' }
                    if ($mode -eq 'early-exit' -and ($before[0].exitCode -ne 23 -or $report.failure.phase -ne 'startup')) { throw 'Early exit was not preserved.' }
                    if ($mode -eq 'bad-exit' -and ($before[0].exitCode -ne 17 -or $report.failure.phase -ne 'shutdown')) { throw 'Nonzero shutdown was not preserved.' }
                }
                $remaining = Get-Process -Id $report.processId -ErrorAction SilentlyContinue
                if ($null -ne $remaining) {
                    try {
                        if (Test-SmokeProcessIdentity $remaining $report.processStartedUtc) { throw 'The owned fixture is still running after cleanup.' }
                    } finally { $remaining.Dispose() }
                }
                $raw = Get-Content -LiteralPath $files[0].FullName -Raw
                if ($raw.Contains($directory) -or $raw.Contains('synthetic smoke fixture') -or $raw.Contains('EnvironmentVariables')) { throw 'Diagnostics exposed a path, window caption or environment dump.' }
            } finally {
                if (!$child.HasExited) { $child.Kill($true); $null = $child.WaitForExit(10000) }
                $child.Dispose()
            }
        }
        Write-Host 'Desktop smoke contracts passed: normal close, refused close, early exit and nonzero shutdown. Negative-fixture reports are expected failures, not application results.'
    } finally {
        $cleanupPath = Get-SmokeCleanupPath $temporary
        Remove-Item -LiteralPath $cleanupPath -Recurse -Force
    }
    return
}

$run = Join-Path ([IO.Path]::GetFullPath($DiagnosticsDirectory)) ([Guid]::NewGuid().ToString('N'))
$null = [IO.Directory]::CreateDirectory($run)
$reportPath = Join-Path $run 'result.json'
$clock = [Diagnostics.Stopwatch]::StartNew()
$profile = Join-Path ([IO.Path]::GetTempPath()) ('Computator-Desktop-Smoke-' + [Guid]::NewGuid().ToString('N'))
$evidence = [ordered]@{
    schemaVersion = 1; startedUtc = [DateTime]::UtcNow.ToString('o'); completedUtc = $null
    executableSha256 = $null; processId = $null; processStartedUtc = $null; passed = $false; normalShutdown = $false
    closeRequested = $false; forcedTermination = $false; failure = $null
    startupTimeoutMs = 60000; idleTimeoutMs = 10000; shutdownTimeoutMs = 10000
    samples = [Collections.Generic.List[object]]::new(); cleanupErrors = [Collections.Generic.List[string]]::new()
}
$process = [Diagnostics.Process]::new()
$started = $false
$failure = $null
$phase = 'prepare'
function Save-Evidence {
    $evidence | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding utf8
}
function Save-Snapshot([string]$Label) {
    $sample = [ordered]@{ label = $Label; phase = $phase; elapsedMs = $clock.ElapsedMilliseconds }
    if ($started) {
        try {
            $process.Refresh()
            $sample.hasExited = $process.HasExited
            if ($process.HasExited) { $sample.exitCode = $process.ExitCode }
            else {
                $sample.mainWindowHandle = $process.MainWindowHandle.ToInt64()
                $sample.privateBytes = $process.PrivateMemorySize64
                $sample.workingSetBytes = $process.WorkingSet64
                $sample.cpuMs = $process.TotalProcessorTime.TotalMilliseconds
                $sample.threadCount = $process.Threads.Count
                $sample.threads = @($process.Threads | Select-Object -First 64 | ForEach-Object {
                    $thread = [ordered]@{ id = $_.Id; state = $_.ThreadState.ToString() }
                    if ($_.ThreadState -eq [Diagnostics.ThreadState]::Wait) { $thread.waitReason = $_.WaitReason.ToString() }
                    $thread
                })
            }
        } catch { $sample.sampleError = $_.Exception.GetType().Name }
    }
    $evidence.samples.Add($sample)
}
try {
    $directory = (Resolve-Path -LiteralPath $PublishDirectory).Path
    $executable = Join-Path $directory 'Computator.NET.exe'
    $evidence.executableSha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = $directory
    $start.UseShellExecute = $false
    $start.Environment['COMPUTATOR_DATA_DIRECTORY'] = $profile
    $process.StartInfo = $start
    $phase = 'startup'
    $started = $process.Start()
    if (!$started) { throw 'Could not start the published desktop executable.' }
    $evidence.processId = $process.Id
    $evidence.processStartedUtc = $process.StartTime.ToUniversalTime().ToString('o')
    Save-Snapshot 'started'
    Save-Evidence
    $deadline = [DateTime]::UtcNow.AddSeconds(60)
    $ready = $false
    do {
        $process.Refresh()
        if ($process.HasExited) { throw "Desktop exited before showing its main window: $($process.ExitCode)" }
        $ready = $process.MainWindowHandle -ne [IntPtr]::Zero -and $process.MainWindowTitle.StartsWith('Computator.NET')
        if (!$ready) { Start-Sleep -Milliseconds 250 }
    } while (!$ready -and [DateTime]::UtcNow -lt $deadline)
    if (!$ready) { throw 'Published desktop did not show its main window.' }
    Save-Snapshot 'main-window'
    $phase = 'input-idle'
    if (!$process.WaitForInputIdle(10000)) { throw 'Published desktop did not become responsive.' }
    Save-Snapshot 'input-idle'
    $phase = 'shutdown'
    $evidence.closeRequested = $process.CloseMainWindow()
    if (!$evidence.closeRequested) { throw 'Could not request a normal desktop shutdown.' }
    if (!$process.WaitForExit(10000)) { throw 'Published desktop did not close normally.' }
    if ($process.ExitCode -ne 0) { throw "Published desktop failed during shutdown: $($process.ExitCode)" }
    $evidence.normalShutdown = $true
    $evidence.passed = $true
    Save-Snapshot 'normal-exit'
} catch {
    $failure = $_
    # Record only the error type and stage in the artifact. Exception messages
    # remain in the failing job, not copied into a potentially shared report.
    $evidence.failure = @{ phase = $phase; errorType = $_.Exception.GetType().Name }
    Save-Snapshot 'failure-before-cleanup'
    try { Save-Evidence } catch { Write-Warning 'Could not persist pre-cleanup desktop diagnostics.' }
} finally {
    try {
        if ($started -and !$process.HasExited) {
            $evidence.forcedTermination = $true
            try { $process.Kill($true) } catch [InvalidOperationException] { if (!$process.HasExited) { throw } }
            if (!$process.WaitForExit(10000)) { throw 'Owned desktop did not terminate within its cleanup deadline.' }
        }
        Save-Snapshot 'after-cleanup'
    } catch { $evidence.cleanupErrors.Add($_.Exception.GetType().Name) }
    finally { $process.Dispose() }
    try {
        if (Test-Path -LiteralPath $profile) {
            $cleanupPath = Get-SmokeCleanupPath $profile
            Remove-Item -LiteralPath $cleanupPath -Recurse -Force
        }
    }
    catch { $evidence.cleanupErrors.Add($_.Exception.GetType().Name) }
    if ($evidence.cleanupErrors.Count -ne 0) { $evidence.passed = $false }
    $evidence.completedUtc = [DateTime]::UtcNow.ToString('o')
    try { Save-Evidence } catch { if ($null -eq $failure) { $failure = $_ } }
    Write-Host "Desktop smoke evidence: $reportPath"
}
if ($null -ne $failure) { throw $failure }
if (!$evidence.passed) { throw 'Desktop smoke cleanup failed; see the invocation evidence.' }
Write-Host 'Published NET 10 desktop passed: real executable, splash/startup, main window and normal shutdown.'
