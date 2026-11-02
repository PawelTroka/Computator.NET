using System;
using System.Diagnostics;
using System.Threading.Tasks;

// Test infrastructure only. External X11 tools must not inherit application
// loader tracing or block the native UI test while a redirected pipe fills.
internal static class NativeToolProcess
{
    internal sealed class Output
    {
        internal string StandardOutput;
        internal string StandardError;
    }

    internal static Output Run(ProcessStartInfo start, int timeoutMilliseconds)
    {
        if (timeoutMilliseconds <= 0 || timeoutMilliseconds > 6000)
            throw new ArgumentOutOfRangeException("timeoutMilliseconds", "Native input tools have a maximum six-second deadline.");
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;
        // Removing this in a shell wrapper is too late: the interpreter itself
        // has already inherited DYLD's potentially pipe-filling diagnostics.
        start.EnvironmentVariables.Remove("DYLD_PRINT_LIBRARIES");
        using (var process = Process.Start(start))
        {
            if (process == null) throw new InvalidOperationException("Cannot start native X11 tool: " + start.FileName);
            var watch = Stopwatch.StartNew();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            var readers = new Task[] { stdout, stderr };
            if (!process.WaitForExit(timeoutMilliseconds))
            {
                try { process.Kill(); } catch (InvalidOperationException) { }
                process.WaitForExit(1000);
                Task.WaitAll(readers, 1000);
                throw new TimeoutException("Native X11 tool exceeded its bounded deadline: " + start.FileName + " " + start.Arguments +
                    ". stderr: " + (stderr.IsCompleted ? stderr.GetAwaiter().GetResult() : "still open after bounded cleanup"));
            }
            int remaining = Math.Max(0, timeoutMilliseconds - (int)watch.ElapsedMilliseconds);
            if (!Task.WaitAll(readers, remaining))
                throw new TimeoutException("Native X11 tool output remained open past its bounded deadline: " + start.FileName);
            var result = new Output { StandardOutput = stdout.GetAwaiter().GetResult(), StandardError = stderr.GetAwaiter().GetResult() };
            if (process.ExitCode != 0)
                throw new InvalidOperationException("Native X11 tool failed with exit " + process.ExitCode + ": " + start.FileName + " " +
                    start.Arguments + Environment.NewLine + result.StandardError + Environment.NewLine + result.StandardOutput);
            return result;
        }
    }
}
