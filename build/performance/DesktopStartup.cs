using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

internal static class DesktopStartup
{
    internal sealed class Result
    {
        internal double ElapsedMilliseconds;
        internal Dictionary<string, object> Details;
        internal string CoreVersion;
        internal string Runtime;
    }

    private delegate bool WindowCallback(IntPtr window, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool EnumWindows(WindowCallback callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, WindowCallback callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder text, int maximum);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, StringBuilder text, int maximum);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool IsWow64Process(IntPtr process, out bool wow64);
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SendMessageTimeout(
        IntPtr window, uint message, UIntPtr wParam, IntPtr lParam, uint flags, uint timeout, out UIntPtr result);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    internal static Result Measure(string executable)
    {
        executable = Path.GetFullPath(executable);
        if (!File.Exists(executable)) throw new FileNotFoundException("Desktop executable missing.", executable);
        // Warm the observer's native entry points before timing the
        // product. The main form must be identified, not the separate splash.
        EnumWindows((window, parameter) => true, IntPtr.Zero);
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = Path.GetDirectoryName(executable), UseShellExecute = false
        };
        using (var process = new Process { StartInfo = start })
        {
            bool started = false;
            bool normalExit = false;
            try
            {
                var clock = Stopwatch.StartNew();
                started = process.Start();
                if (!started) throw new InvalidOperationException("Could not start desktop.");
                IntPtr main = IntPtr.Zero;
                while (clock.ElapsedMilliseconds < 60000 && main == IntPtr.Zero)
                {
                    if (process.HasExited) throw new InvalidOperationException("Desktop exited before its main form: " + process.ExitCode);
                    EnumWindows((window, parameter) =>
                    {
                        uint owner;
                        GetWindowThreadProcessId(window, out owner);
                        if (owner != process.Id || !IsWindowVisible(window)) return true;
                        var title = new StringBuilder(256);
                        GetWindowText(window, title, title.Capacity);
                        if (title.ToString() != "Computator.NET © Pawel Troka") return true;
                        bool tabsReady = false;
                        EnumChildWindows(window, (child, unused) =>
                        {
                            var className = new StringBuilder(256);
                            GetClassName(child, className, className.Capacity);
                            if (className.ToString().IndexOf("SysTabControl32", StringComparison.OrdinalIgnoreCase) < 0) return true;
                            UIntPtr count;
                            // TCM_GETITEMCOUNT proves the six main workflow tabs
                            // exist. The splash has no such tab control.
                            if (SendMessageTimeout(child, 0x1304, UIntPtr.Zero, IntPtr.Zero, 2, 1000, out count) != IntPtr.Zero && count.ToUInt64() >= 6)
                                tabsReady = true;
                            return !tabsReady;
                        }, IntPtr.Zero);
                        if (!tabsReady) return true;
                        UIntPtr response;
                        if (SendMessageTimeout(window, 0, UIntPtr.Zero, IntPtr.Zero, 2, 1000, out response) == IntPtr.Zero) return true;
                        main = window;
                        return false;
                    }, IntPtr.Zero);
                    if (main == IntPtr.Zero) Thread.Sleep(10);
                }
                if (main == IntPtr.Zero) throw new TimeoutException("No responsive MainView within 60 seconds.");
                double elapsed = clock.Elapsed.TotalMilliseconds;
                bool wow64;
                if (!Environment.Is64BitOperatingSystem || !IsWow64Process(process.Handle, out wow64) || wow64)
                    throw new InvalidOperationException("Desktop did not run as a 64-bit process.");
                string runtime = null;
                foreach (ProcessModule module in process.Modules)
                {
                    if (string.Equals(module.ModuleName, "clr.dll", StringComparison.OrdinalIgnoreCase))
                        runtime = ".NET Framework CLR " + module.FileVersionInfo.FileVersion;
                    if (string.Equals(module.ModuleName, "coreclr.dll", StringComparison.OrdinalIgnoreCase))
                        runtime = ".NET " + module.FileVersionInfo.FileVersion;
                }
                if (runtime == null) throw new InvalidOperationException("Could not identify the desktop CLR module.");
                var result = new Result
                {
                    ElapsedMilliseconds = elapsed,
                    CoreVersion = System.Reflection.AssemblyName.GetAssemblyName(Path.Combine(Path.GetDirectoryName(executable), "Computator.NET.Core.dll")).Version.ToString(),
                    Runtime = runtime,
                    Details = new Dictionary<string, object>
                    {
                        { "readiness", "owned visible exact-title main window, six workflow tabs, successful WM_NULL reply" },
                        { "targetBitness", 64 }, { "targetRuntimeSource", "loaded CLR module file version" },
                        { "pollIntervalMs", 10 }, { "privateBytesAtReady", process.PrivateMemorySize64 },
                        { "workingSetBytesAtReady", process.WorkingSet64 },
                        { "filesystemCache", "unflushed; fresh process, reused profile after conditioning" }
                    }
                };
                if (!PostMessage(main, 0x0010, IntPtr.Zero, IntPtr.Zero)) throw new InvalidOperationException("Could not request normal close.");
                if (!process.WaitForExit(10000)) throw new TimeoutException("Desktop did not close normally.");
                if (process.ExitCode != 0) throw new InvalidOperationException("Desktop exit code: " + process.ExitCode);
                normalExit = true;
                result.Details["normalShutdown"] = true;
                return result;
            }
            finally
            {
                if (started && !normalExit && !process.HasExited)
                {
                    process.Kill();
                    if (!process.WaitForExit(10000)) throw new TimeoutException("Owned desktop did not terminate.");
                }
            }
        }
    }
}
