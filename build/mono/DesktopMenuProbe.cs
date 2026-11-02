// Build-time acceptance fixture. The installed application needs no C# compiler.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;

internal static class DesktopMenuProbe
{
    [StructLayout(LayoutKind.Sequential)]
    private struct GError { public uint Domain; public int Code; public IntPtr Message; }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Launched(IntPtr app, int pid, IntPtr data);
    [DllImport("libgio-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_desktop_app_info_new_from_filename(string path);
    [DllImport("libgio-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern int g_desktop_app_info_launch_uris_as_manager(IntPtr app, IntPtr uris, IntPtr context,
        int flags, IntPtr setup, IntPtr setupData, Launched launched, IntPtr data, out IntPtr error);
    [DllImport("libgobject-2.0.so.0")] private static extern void g_object_unref(IntPtr value);
    [DllImport("libglib-2.0.so.0")] private static extern void g_error_free(IntPtr error);
    [DllImport("libc")] private static extern int kill(int pid, int signal);
    [DllImport("libc")] private static extern int waitpid(int pid, out int status, int options);

    private static int Main(string[] args)
    {
        if (args.Length < 2 || args.Length > 3 || (args.Length == 3 && args[2] != "--expect-error"))
        { Console.Error.WriteLine("Usage: DesktopMenuProbe.exe ENTRY RESULTS [--expect-error]"); return 2; }
        var results = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(results);
        var expectError = args.Length == 3;
        var label = expectError ? "desktop-menu-error" : "desktop-menu";
        var clock = Stopwatch.StartNew();
        var launchers = new List<int>();
        Exception failure = null;
        try
        {
            var entry = Path.GetFullPath(args[0]);
            if (!File.ReadAllLines(entry).Contains("Terminal=false"))
                throw new InvalidOperationException("The delivered menu entry still requires a terminal emulator.");
            var app = g_desktop_app_info_new_from_filename(entry);
            if (app == IntPtr.Zero) throw new InvalidOperationException("GIO rejected the delivered desktop entry.");
            try
            {
                IntPtr error;
                Launched callback = (info, pid, data) => { if (pid > 1) launchers.Add(pid); };
                // SEARCH_PATH | DO_NOT_REAP_CHILD: record and reap exactly the
                // process this test launched, rather than searching/killing apps.
                var ok = g_desktop_app_info_launch_uris_as_manager(app, IntPtr.Zero, IntPtr.Zero,
                    6, IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero, out error);
                GC.KeepAlive(callback);
                if (ok == 0)
                {
                    var message = error == IntPtr.Zero ? "Unknown launch error." :
                        Marshal.PtrToStringAnsi(((GError)Marshal.PtrToStructure(error, typeof(GError))).Message);
                    if (error != IntPtr.Zero) g_error_free(error);
                    throw new InvalidOperationException("Desktop menu could not launch: " + message);
                }
            }
            finally { g_object_unref(app); }
            if (launchers.Count != 1) throw new InvalidOperationException("Expected exactly one owned launcher process.");

            string window = null;
            Dictionary<string, int> geometry = null;
            while (clock.Elapsed.TotalSeconds < 30 && window == null)
            {
                foreach (var pid in Descendants(launchers[0]).Concat(launchers).Distinct())
                {
                    foreach (var id in Xdo("search --onlyvisible --pid " + pid + " --name Computator.NET").Split('\n'))
                    {
                        long numeric;
                        if (!long.TryParse(id.Trim(), out numeric)) continue;
                        var title = Xdo("getwindowname " + numeric).Trim();
                        var candidate = Geometry(numeric.ToString(CultureInfo.InvariantCulture));
                        if (expectError ? title == "Computator.NET could not start" :
                            title.StartsWith("Computator.NET", StringComparison.Ordinal) &&
                            title.IndexOf("could not start", StringComparison.Ordinal) < 0 &&
                            candidate.ContainsKey("WIDTH") && candidate["WIDTH"] >= 800 && candidate["HEIGHT"] >= 500)
                        { window = numeric.ToString(CultureInfo.InvariantCulture); geometry = candidate; break; }
                    }
                    if (window != null) break;
                }
                if (window == null) Thread.Sleep(100);
            }
            if (window == null) throw new TimeoutException("The delivered menu entry did not show its expected window within 30 seconds.");
            Thread.Sleep(300); // retain an actual painted native window, not only a handle
            using (var bitmap = new Bitmap(geometry["WIDTH"], geometry["HEIGHT"]))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(geometry["X"], geometry["Y"], 0, 0, bitmap.Size);
                bitmap.Save(Path.Combine(results, label + ".png"), ImageFormat.Png);
            }
            if (expectError)
            {
                Xdo("windowfocus --sync " + window);
                Xdo("key Return");
                var cache = Path.Combine(Environment.GetEnvironmentVariable("XDG_CACHE_HOME"), "Computator.NET");
                if (!Directory.GetFiles(cache, "startup-errors.*.log").Any(path =>
                    File.ReadAllText(path).Contains("fixture runtime unavailable")))
                    throw new InvalidOperationException("The visible startup error did not retain its diagnostic log.");
                int status;
                var wait = Stopwatch.StartNew();
                int reaped;
                while ((reaped = waitpid(launchers[0], out status, 1)) == 0 && wait.Elapsed.TotalSeconds < 5) Thread.Sleep(50);
                if (reaped != launchers[0] || (status >> 8) == 0)
                    throw new InvalidOperationException("The failed desktop launch did not exit with an error after dismissing its dialog.");
                launchers.Clear(); // this owned child has already been reaped
            }
            Console.WriteLine("PASS " + label + ": actual delivered entry displayed native window " + window +
                "; no terminal emulator or build tools required.");
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine("FAIL " + label + ": " + error); }
        finally
        {
            foreach (var pid in launchers)
            {
                // The separate production StartupProbe verifies graceful Form.Close.
                // This launch probe cancels only its owned fresh-profile process tree.
                int status;
                if (waitpid(pid, out status, 1) == 0)
                {
                    // Signal the launcher first so its cancellation handler can
                    // suppress crash dialogs and forward TERM to its child.
                    kill(pid, 15);
                    var until = Stopwatch.StartNew();
                    int reaped;
                    while ((reaped = waitpid(pid, out status, 1)) == 0 && until.Elapsed.TotalSeconds < 5) Thread.Sleep(50);
                    if (reaped == 0)
                    {
                        foreach (var child in Descendants(pid).AsEnumerable().Reverse()) kill(child, 15);
                        kill(pid, 9); waitpid(pid, out status, 0);
                    }
                }
            }
        }
        using (var writer = XmlWriter.Create(Path.Combine(results, label + "-results.xml"), new XmlWriterSettings { Indent = true }))
        {
            writer.WriteStartElement("testsuite"); writer.WriteAttributeString("name", label);
            writer.WriteAttributeString("tests", "1"); writer.WriteAttributeString("failures", failure == null ? "0" : "1");
            writer.WriteAttributeString("skipped", "0"); writer.WriteStartElement("testcase"); writer.WriteAttributeString("name", label);
            if (failure != null) writer.WriteElementString("failure", failure.ToString());
            writer.WriteEndElement(); writer.WriteEndElement();
        }
        return failure == null ? 0 : 1;
    }

    private static Dictionary<string, int> Geometry(string window)
    {
        var result = new Dictionary<string, int>();
        foreach (var line in Xdo("getwindowgeometry --shell " + window).Split('\n'))
        {
            var pair = line.Trim().Split('='); int value;
            if (pair.Length == 2 && int.TryParse(pair[1], out value)) result[pair[0]] = value;
        }
        return result;
    }

    private static string Xdo(string arguments)
    {
        using (var process = Process.Start(new ProcessStartInfo("xdotool", arguments)
        { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true }))
        {
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(2000))
            { process.Kill(); process.WaitForExit(1000); throw new TimeoutException("X11 window query did not finish."); }
            error.GetAwaiter().GetResult();
            return process.ExitCode == 0 ? output.GetAwaiter().GetResult() : "";
        }
    }

    private static List<int> Descendants(int parent)
    {
        var result = new List<int>();
        var parents = new Dictionary<int, int>();
        foreach (var directory in Directory.GetDirectories("/proc"))
        {
            int pid;
            if (!int.TryParse(Path.GetFileName(directory), out pid)) continue;
            try
            {
                var line = File.ReadAllLines(Path.Combine(directory, "status")).FirstOrDefault(s => s.StartsWith("PPid:"));
                if (line != null) parents[pid] = int.Parse(line.Substring(5).Trim(), CultureInfo.InvariantCulture);
            }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        var pending = new Queue<int>(); pending.Enqueue(parent);
        while (pending.Count != 0)
        {
            var pid = pending.Dequeue();
            foreach (var child in parents.Where(pair => pair.Value == pid).Select(pair => pair.Key))
                if (!result.Contains(child)) { result.Add(child); pending.Enqueue(child); }
        }
        return result;
    }
}
