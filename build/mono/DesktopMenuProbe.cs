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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
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

    [DataContract]
    private sealed class Inventory
    {
        [DataMember(Name = "ownership", IsRequired = true)] public string Ownership { get; set; }
        [DataMember(Name = "pid", IsRequired = true)] public int Pid { get; set; }
        [DataMember(Name = "complete", IsRequired = true)] public bool Complete { get; set; }
        [DataMember(Name = "windows", IsRequired = true)] public NativeWindow[] Windows { get; set; }
    }

    [DataContract]
    private sealed class NativeWindow
    {
        [DataMember(Name = "xid", IsRequired = true)] public long Id { get; set; }
        [DataMember(Name = "ownerPid", IsRequired = true)] public int Pid { get; set; }
        [DataMember(Name = "mapState", IsRequired = true)] public int MapState { get; set; }
        [DataMember(Name = "width", IsRequired = true)] public int Width { get; set; }
        [DataMember(Name = "height", IsRequired = true)] public int Height { get; set; }
        [DataMember(Name = "canClose", IsRequired = true)] public bool CanClose { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
    }

    [DataContract]
    private sealed class NormalClose
    {
        [DataMember(Name = "pid", IsRequired = true)] public int Pid { get; set; }
        [DataMember(Name = "xid", IsRequired = true)] public long Id { get; set; }
        [DataMember(Name = "protocol", IsRequired = true)] public string Protocol { get; set; }
        [DataMember(Name = "sent", IsRequired = true)] public bool Sent { get; set; }
        [DataMember(Name = "xerror", IsRequired = true)] public int Error { get; set; }
    }

    private static int Main(string[] args)
    {
        if (args.Length < 3 || args.Length > 4 || (args.Length == 4 && args[3] != "--expect-error"))
        { Console.Error.WriteLine("Usage: DesktopMenuProbe.exe ENTRY RESULTS X11-TOOL [--expect-error]"); return 2; }
        var results = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(results);
        var x11Tool = Path.GetFullPath(args[2]);
        var expectError = args.Length == 4;
        var label = expectError ? "desktop-menu-error" : "desktop-menu";
        var clock = Stopwatch.StartNew();
        var launchers = new List<int>();
        var owned = new Dictionary<int, string>();
        var cache = Path.Combine(Environment.GetEnvironmentVariable("XDG_CACHE_HOME"), "Computator.NET");
        var previousLogs = new HashSet<string>(ErrorLogs(cache), StringComparer.Ordinal);
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
                Launched callback = (info, pid, data) =>
                {
                    if (pid <= 1) return;
                    launchers.Add(pid);
                    var identity = ProcessIdentity(pid);
                    if (identity != null) owned[pid] = identity;
                };
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

            NativeWindow selected = null;
            string window = null;
            while (clock.Elapsed.TotalSeconds < 30 && window == null)
            {
                foreach (var pid in RememberOwned(launchers[0], owned))
                {
                    // Mono does not reliably set _NET_WM_PID. XRes establishes
                    // ownership before titles/geometry select the owned window.
                    var inventory = ReadInventory(x11Tool, pid, null);
                    var candidates = inventory.Windows.Where(candidate => candidate.MapState == 2 &&
                        (expectError ? candidate.Name == "Computator.NET could not start" :
                         candidate.Name != null && candidate.Name.StartsWith("Computator.NET", StringComparison.Ordinal) &&
                         candidate.Name.IndexOf("could not start", StringComparison.Ordinal) < 0 &&
                         candidate.Width >= 800 && candidate.Height >= 500 && candidate.CanClose)).ToArray();
                    if (candidates.Length > 1)
                        throw new InvalidOperationException("The owned process has more than one matching top-level window.");
                    if (candidates.Length == 1)
                    { selected = candidates[0]; window = selected.Id.ToString(CultureInfo.InvariantCulture); }
                    if (window != null) break;
                }
                if (window == null) Thread.Sleep(100);
            }
            if (window == null) throw new TimeoutException("The delivered menu entry did not show its expected window within 30 seconds.");
            Thread.Sleep(300); // retain an actual painted native window, not only a handle
            var geometry = Geometry(window);
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
                if (!ErrorLogs(cache).Except(previousLogs).Any(path =>
                    File.ReadAllText(path).Contains("fixture runtime unavailable")))
                    throw new InvalidOperationException("The visible startup error did not retain its diagnostic log.");
                if (WaitForLauncher(launchers, owned, 5000) == 0)
                    throw new InvalidOperationException("The failed desktop launch did not exit with an error after dismissing its dialog.");
            }
            else
            {
                if (!owned.ContainsKey(selected.Pid) || ProcessIdentity(selected.Pid) != owned[selected.Pid] ||
                    !Descendants(launchers[0]).Contains(selected.Pid))
                    throw new InvalidOperationException("The selected application process is no longer owned by this launch.");
                var beforeClose = ReadInventory(x11Tool, selected.Pid, Path.Combine(results, "desktop-menu-before-close.json"));
                if (!beforeClose.Windows.Any(candidate => candidate.Id == selected.Id && candidate.MapState == 2 &&
                    candidate.CanClose && candidate.Width == selected.Width && candidate.Height == selected.Height))
                    throw new InvalidOperationException("The owned main window changed before normal close.");
                var closeJson = NativeToolProcess.Run(new ProcessStartInfo(x11Tool,
                    "close " + selected.Pid + " " + window), 5000).StandardOutput;
                File.WriteAllText(Path.Combine(results, "desktop-menu-close.json"), closeJson);
                var close = ReadJson<NormalClose>(closeJson);
                if (close.Pid != selected.Pid || close.Id != selected.Id || close.Protocol != "WM_DELETE_WINDOW" ||
                    !close.Sent || close.Error != 0)
                    throw new InvalidOperationException("The native tool did not deliver the owned normal-close request.");
                var status = WaitForLauncher(launchers, owned, 20000);
                if (status != 0)
                    throw new InvalidOperationException("The production launcher failed normal shutdown; raw wait status=" + status + ".");
                if (ErrorLogs(cache).Except(previousLogs).Any())
                    throw new InvalidOperationException("Normal shutdown retained an unexpected startup-error log.");
            }
            var survivors = owned.Where(process => ProcessIdentity(process.Key) == process.Value).Select(process => process.Key).ToArray();
            if (survivors.Length != 0)
                throw new InvalidOperationException("Owned processes survived launcher exit: " + string.Join(", ", survivors));
            Console.WriteLine("PASS " + label + ": actual delivered entry displayed native window " + window +
                (expectError ? "; startup failure was visible and retained." : "; XRes-owned WM_DELETE_WINDOW, zero exit, no error log or surviving process."));
        }
        catch (Exception error)
        {
            failure = error;
            Console.Error.WriteLine("FAIL " + label + ": " + error);
            foreach (var path in ErrorLogs(cache).Except(previousLogs))
                try { File.Copy(path, Path.Combine(results, Path.GetFileName(path)), true); }
                catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        finally
        {
            foreach (var pid in launchers)
            {
                // Failure cleanup is never accepted as normal shutdown. Only the
                // exact launcher and observed descendants belong to this probe.
                RememberOwned(pid, owned);
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
                        foreach (var process in owned.Where(process => process.Key != pid))
                            if (ProcessIdentity(process.Key) == process.Value) kill(process.Key, 15);
                        kill(pid, 9); waitpid(pid, out status, 0);
                    }
                }
            }
            if (failure != null)
                foreach (var process in owned)
                    if (ProcessIdentity(process.Key) == process.Value) kill(process.Key, 9);
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

    private static string[] ErrorLogs(string cache)
    {
        return Directory.Exists(cache) ? Directory.GetFiles(cache, "startup-errors.*.log") : new string[0];
    }

    private static T ReadJson<T>(string json)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
    }

    private static Inventory ReadInventory(string tool, int pid, string evidence)
    {
        var json = NativeToolProcess.Run(new ProcessStartInfo(tool, "inventory " + pid), 5000).StandardOutput;
        if (evidence != null) File.WriteAllText(evidence, json);
        var inventory = ReadJson<Inventory>(json);
        if (!inventory.Complete || inventory.Ownership != "XRes-local-client-pid" || inventory.Pid != pid ||
            inventory.Windows == null || inventory.Windows.Any(window => window.Pid != pid))
            throw new InvalidOperationException("Incomplete or inconsistent XRes process ownership inventory.");
        return inventory;
    }

    private static int WaitForLauncher(List<int> launchers, Dictionary<int, string> owned, int milliseconds)
    {
        var clock = Stopwatch.StartNew();
        while (clock.ElapsedMilliseconds < milliseconds)
        {
            RememberOwned(launchers[0], owned);
            int status;
            var reaped = waitpid(launchers[0], out status, 1);
            if (reaped == launchers[0])
            {
                launchers.Clear();
                return status;
            }
            if (reaped != 0) throw new InvalidOperationException("Could not reap the owned launcher.");
            Thread.Sleep(50);
        }
        throw new TimeoutException("The production launcher did not exit after the native close request.");
    }

    private static int[] RememberOwned(int launcher, Dictionary<int, string> owned)
    {
        string launcherIdentity;
        if (!owned.TryGetValue(launcher, out launcherIdentity) || ProcessIdentity(launcher) != launcherIdentity)
            return new int[0];
        var processes = Descendants(launcher).Concat(new[] { launcher }).Distinct().ToArray();
        foreach (var pid in processes)
        {
            var identity = ProcessIdentity(pid);
            if (identity != null) owned[pid] = identity;
        }
        return processes;
    }

    private static string ProcessIdentity(int pid)
    {
        try
        {
            var stat = File.ReadAllText("/proc/" + pid + "/stat");
            // comm may contain spaces or parentheses. starttime is field 22;
            // retain it so cleanup cannot signal a recycled PID.
            return stat.Substring(stat.LastIndexOf(')') + 2).Split(' ')[19];
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
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
        return NativeToolProcess.Run(new ProcessStartInfo("xdotool", arguments), 2000).StandardOutput;
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
