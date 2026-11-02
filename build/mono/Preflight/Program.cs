using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

internal static class Program
{
    private const string ConfigMarker = "<!-- Computator.NET generated process configuration -->";
    private static bool isMac;
    private static string packageRoot;
    private static Manifest manifest;
    private static Dictionary<string, Entry> inventory;
    private static readonly Dictionary<string, string[]> Aliases = new Dictionary<string, string[]>
    {
        { "gsl", new[] { "gsl", "libgsl", "libgsl.so", "libgsl.so.28", "libgsl.dylib", "libgsl.28.dylib" } },
        { "cblas", new[] { "gslcblas", "libgslcblas", "libgslcblas.so", "libgslcblas.so.0", "libgslcblas.dylib", "libgslcblas.0.dylib" } },
        { "gdiplus", new[] { "gdiplus", "gdiplus.dll", "libgdiplus", "libgdiplus.so", "libgdiplus.so.0", "libgdiplus.dylib", "libgdiplus.0.dylib" } },
        { "cairo", new[] { "cairo", "libcairo", "libcairo.so.2", "libcairo-2.dll", "libcairo.dylib", "libcairo.2.dylib" } },
        { "x11", new[] { "X11", "libX11", "libX11.so.6", "libX11.dylib", "libX11.6.dylib" } },
        { "xtst", new[] { "Xtst", "libXtst", "libXtst.so.6", "libXtst.dylib", "libXtst.6.dylib" } },
        { "xinerama", new[] { "Xinerama", "libXinerama", "libXinerama.so.1", "libXinerama.dylib", "libXinerama.1.dylib" } },
        { "xext", new[] { "Xext", "libXext", "libXext.so.6", "libXext.dylib", "libXext.6.dylib" } },
        { "xcursor", new[] { "Xcursor", "libXcursor", "libXcursor.so.1", "libXcursor.dylib", "libXcursor.1.dylib" } },
        { "xfixes", new[] { "Xfixes", "libXfixes", "libXfixes.so.3", "libXfixes.dylib", "libXfixes.3.dylib" } },
        { "xim", new string[0] }
    };

    [STAThread]
    private static int Main(string[] args)
    {
        var report = new Report { StartedUtc = DateTime.UtcNow.ToString("o"), Libraries = new List<Library>() };
        string reportPath = null;
        try
        {
            if (args.Length != 5 || (args[0] != "prepare" && args[0] != "verify") || args[1] != "--package-root" ||
                args[3] != (args[0] == "prepare" ? "--config" : "--report"))
                throw new ArgumentException("Usage: mono Computator.NET.Mono.Preflight.exe prepare --package-root DIR --config PATH | verify --package-root DIR --report PATH");
            if (Type.GetType("Mono.Runtime") == null || IntPtr.Size != 8)
                throw new PlatformNotSupportedException("A matching 64-bit Framework Mono runtime is required.");
            report.Operation = args[0];
            packageRoot = Path.GetFullPath(args[2]).TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(packageRoot) || (File.GetAttributes(packageRoot) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("The package root must be an existing, non-redirected directory.");
            report.PackageRoot = packageRoot;
            reportPath = args[0] == "verify" ? OutsideOutput(args[4]) : null;
            IdentifyRuntime(report);
            ReadAndVerifyManifest(report);
            if (args[0] == "prepare")
            {
                PrepareConfiguration(OutsideOutput(args[4]));
                Console.WriteLine("Prepared package-specific Mono configuration; no system configuration was modified.");
                return 0;
            }
            VerifyRuntime(report);
            report.Passed = true;
        }
        catch (Exception error)
        {
            report.Failure = error.GetBaseException().GetType().Name + ": " + error.GetBaseException().Message;
            Console.Error.WriteLine("Mono package preflight failed: " + report.Failure);
        }
        report.CompletedUtc = DateTime.UtcNow.ToString("o");
        if (reportPath != null)
        {
            try { WriteJson(reportPath, report); }
            catch (Exception error) { Console.Error.WriteLine("Cannot retain preflight report: " + error.Message); return 1; }
        }
        if (report.Passed) Console.WriteLine("Mono package preflight passed for " + report.Rid + "; actual application GUI acceptance remains a separate check.");
        return report.Passed ? 0 : 1;
    }

    private static void IdentifyRuntime(Report report)
    {
        var buffer = Marshal.AllocHGlobal(8192);
        try
        {
            if (uname(buffer) != 0) throw new IOException("Cannot identify the Mono process platform.");
            report.OS = Marshal.PtrToStringAnsi(buffer);
            isMac = report.OS == "Darwin";
            if (!isMac && report.OS != "Linux") throw new PlatformNotSupportedException("Only Linux and macOS packages are supported.");
            var machine = Marshal.PtrToStringAnsi(IntPtr.Add(buffer, isMac ? 4 * 256 : 4 * 65));
            report.Architecture = machine == "x86_64" ? "x64" : machine == "arm64" || machine == "aarch64" ? "arm64" : machine;
            if (isMac)
            {
                int translated;
                var length = new UIntPtr(4);
                report.Rosetta = sysctlbyname("sysctl.proc_translated", out translated, ref length, IntPtr.Zero, UIntPtr.Zero) == 0 && translated != 0;
                if (report.Rosetta) throw new PlatformNotSupportedException("Use the native architecture package and Mono runtime; Rosetta is not an accepted native package lane.");
            }
            var display = Type.GetType("Mono.Runtime").GetMethod("GetDisplayName", BindingFlags.Static | BindingFlags.NonPublic);
            report.Mono = display == null ? Environment.Version.ToString() : (string)display.Invoke(null, null);
            var version = Regex.Match(report.Mono, @"^\d+\.\d+\.\d+(?:\.\d+)?");
            if (!version.Success || new Version(version.Value) < (isMac ? new Version(6, 14, 1) : new Version(6, 8)))
                throw new PlatformNotSupportedException("Use the supported Mono runtime (Linux 6.8 or newer; macOS 6.14.1 or newer).");
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    private static void ReadAndVerifyManifest(Report report)
    {
        var file = PackagePath("mono-package.json");
        if (new FileInfo(file).Length > 16 * 1024 * 1024) throw new IOException("Package manifest is too large.");
        using (var stream = File.OpenRead(file)) manifest = (Manifest)Serializer(typeof(Manifest)).ReadObject(stream);
        if (manifest == null || manifest.SchemaVersion != 1 || string.IsNullOrEmpty(manifest.Version) || manifest.Files == null ||
            manifest.Files.Count < 10 || manifest.Files.Count > 20000 || manifest.Native == null)
            throw new InvalidDataException("Missing or unsupported package manifest.");
        var rid = (isMac ? "osx-" : "linux-") + report.Architecture;
        if (manifest.Rid != rid || !new[] { "linux-x64", "osx-x64", "osx-arm64" }.Contains(rid))
            throw new PlatformNotSupportedException("Package RID " + manifest.Rid + " does not match native Mono process " + rid + ".");
        report.Rid = manifest.Rid;
        report.Version = manifest.Version;
        if (!isMac)
        {
            var release = File.ReadAllLines("/etc/os-release");
            Func<string, string> value = key => (release.FirstOrDefault(line => line.StartsWith(key + "=", StringComparison.Ordinal)) ?? "=")
                .Split(new[] { '=' }, 2)[1].Trim('"', '\'');
            var baseline = value("ID") + "-" + value("VERSION_ID");
            if (!new[] { "ubuntu-22.04", "ubuntu-24.04" }.Contains(baseline) || baseline != manifest.Baseline)
                throw new PlatformNotSupportedException("This native package targets " + manifest.Baseline + "; select the package for " + baseline + ".");
            report.Baseline = baseline;
        }
        inventory = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        long total = 0;
        foreach (var entry in manifest.Files)
        {
            var path = PackagePath(entry.Path);
            if (entry.Size < 0 || entry.Size > 512L * 1024 * 1024 || (total += entry.Size) > 2L * 1024 * 1024 * 1024 ||
                entry.Sha256 == null || !Regex.IsMatch(entry.Sha256, "\\A[0-9a-fA-F]{64}\\z") || inventory.ContainsKey(entry.Path))
                throw new InvalidDataException("Invalid or duplicate package inventory entry.");
            inventory.Add(entry.Path, entry);
            if (!File.Exists(path) || new FileInfo(path).Length != entry.Size || Hash(path) != entry.Sha256.ToLowerInvariant())
                throw new IOException("Package file differs from its inventory: " + entry.Path);
        }
        foreach (var key in new[] { "gsl", "cblas", "gdiplus" })
            if (!manifest.Native.ContainsKey(key)) throw new InvalidDataException("Missing package native library: " + key);
        foreach (var item in manifest.Native)
            if (!Aliases.ContainsKey(item.Key) || !inventory.ContainsKey(item.Value) || inventory[item.Value].Path != item.Value)
                throw new InvalidDataException("Native library is unknown or absent from the inventory: " + item.Key);
        if (manifest.Native.ContainsKey("xim") && (!isMac || !manifest.Native.ContainsKey("x11")))
            throw new InvalidDataException("The XIM ABI shim requires an explicit packaged macOS X11 library.");
        var remaining = new HashSet<string>(manifest.Files.Select(entry => entry.Path), StringComparer.Ordinal);
        var directories = new Stack<string>();
        directories.Push(packageRoot);
        while (directories.Count != 0)
            foreach (var item in Directory.EnumerateFileSystemEntries(directories.Pop()))
            {
                var relative = item.Substring(packageRoot.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
                PackagePath(relative); // Reject links before descending, including unlisted links.
                if (Directory.Exists(item)) directories.Push(item);
                else if (relative != "mono-package.json" && !remaining.Remove(relative))
                    throw new IOException("Unrecorded package file: " + relative);
            }
        if (remaining.Count != 0) throw new IOException("Inventory paths do not exactly match the package files.");
        if (!inventory.ContainsKey("app/Computator.NET.exe") || !inventory.ContainsKey("app/Computator.NET.Mono.Preflight.exe"))
            throw new InvalidDataException("Package does not contain its application and runtime preflight.");
        report.VerifiedFiles = inventory.Count;
        report.ManifestSha256 = Hash(file);
    }

    private static void PrepareConfiguration(string destination)
    {
        var directory = Marshal.PtrToStringAnsi(mono_get_config_dir());
        var source = new[] { Path.Combine(directory ?? "", "mono", "config"), Path.Combine(directory ?? "", "config") }
            .FirstOrDefault(File.Exists);
        if (!File.Exists(source)) throw new IOException("Cannot locate the actual Mono runtime configuration.");
        XDocument config;
        using (var reader = XmlReader.Create(source, new XmlReaderSettings
        { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 2 * 1024 * 1024 }))
            config = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        if (config.Root == null || config.Root.Name != "configuration") throw new InvalidDataException("Invalid Mono runtime configuration.");
        foreach (var library in manifest.Native)
        {
            var aliases = Aliases[library.Key];
            foreach (var map in config.Root.Elements("dllmap").ToArray())
            {
                var dll = (string)map.Attribute("dll") ?? "";
                if (dll.StartsWith("i:", StringComparison.Ordinal)) dll = dll.Substring(2);
                if (aliases.Contains(dll, StringComparer.OrdinalIgnoreCase)) map.Remove();
            }
            foreach (var alias in aliases)
                config.Root.Add(new XElement("dllmap", new XAttribute("dll", alias), new XAttribute("target", PackagePath(library.Value))));
        }
        if (manifest.Native.ContainsKey("xim"))
            foreach (var map in config.Root.Elements("dllmap").Where(node => Aliases["x11"].Contains((string)node.Attribute("dll"))))
                foreach (var entry in new[] { "XGetIMValues", "XGetICValues", "XSetICValues", "XCreateIC", "XVaCreateNestedList" })
                    map.Add(new XElement("dllentry", new XAttribute("name", entry),
                        new XAttribute("dll", PackagePath(manifest.Native["xim"])), new XAttribute("target", "ComputatorMono" + entry)));
        if (File.Exists(destination) && !File.ReadAllText(destination).StartsWith(ConfigMarker, StringComparison.Ordinal))
            throw new IOException("Refusing to overwrite a file that is not a generated package configuration.");
        var builder = new StringBuilder(ConfigMarker).Append('\n');
        // Mono 6.8's native configuration reader needs the ordinary headerless
        // machine-config format. Preserve every unrelated map and omit an XML declaration.
        using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true, NewLineHandling = NewLineHandling.None }))
            config.Save(writer);
        AtomicWrite(destination, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static void VerifyRuntime(Report report)
    {
        if (isMac && Environment.GetEnvironmentVariable("MONO_MWF_MAC_FORCE_X11") != "1")
            throw new InvalidOperationException("The Mac launcher must select Mono's X11 driver before loading WinForms.");
        // Exercise Mono's logical imports first; opening expected paths directly
        // would conceal a broken process dllmap and a fallback to system libraries.
        gsl_set_error_handler_off();
        if (Math.Abs(gsl_sf_gamma(5) - 24) > 1e-11 || Math.Abs(cblas_ddot(3, new[] { 1.0, 2.0, 3.0 }, 1, new[] { 4.0, 5.0, 6.0 }, 1) - 32) > 1e-12)
            throw new InvalidOperationException("Packaged GSL/CBLAS independent known answers failed.");
        using (var bitmap = new Bitmap(320, 80))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var font = new Font(FontFamily.GenericSansSerif, 12))
        {
            graphics.Clear(Color.White);
            foreach (var text in new[] { "before", "\U0001f600", "after \u03c0" })
            {
                var width = graphics.MeasureString(text, font).Width;
                if (width <= 0 || float.IsNaN(width) || float.IsInfinity(width))
                    throw new InvalidOperationException("Drawing stack returned an invalid Unicode text width.");
                graphics.DrawString(text, font, Brushes.Black, 4, 4);
            }
            graphics.FillRectangle(Brushes.Navy, 2, 50, 20, 20);
            if (bitmap.GetPixel(10, 60).ToArgb() != Color.Navy.ToArgb())
                throw new InvalidOperationException("Drawing context was corrupted after Unicode text.");
        }
        var loaded = LoadedLibraries();
        foreach (var key in manifest.Native.Keys)
        {
            var expected = PackagePath(manifest.Native[key]);
            var observed = loaded.Where(p => IsLibrary(key, Path.GetFileName(p))).Select(RealPath).Distinct(StringComparer.Ordinal).ToArray();
            var imported = new[] { "gsl", "cblas", "gdiplus", "cairo", "x11" }.Contains(key);
            if ((imported && observed.Length != 1) || observed.Length > 1 ||
                (observed.Length == 1 && observed[0] != RealPath(expected)))
                throw new InvalidOperationException("Native library selection differs from package " + key + ": " + string.Join(", ", observed));
            var actual = observed.Length == 1 ? observed[0] : RealPath(expected);
            var observation = new Library { Name = key, Path = actual, Sha256 = Hash(actual),
                Scope = observed.Length == 1 ? "loaded by Mono imports/drawing" : "explicit dependency inspection; GUI use is separate" };
            if (observation.Sha256 != inventory[manifest.Native[key]].Sha256.ToLowerInvariant())
                throw new IOException("Loaded native bytes differ from the package: " + key);
            var handle = Open(actual);
            if (key == "gsl")
            {
                var symbol = Symbol(handle, "gsl_version");
                observation.Version = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(symbol));
                if (observation.Version != "2.8") throw new InvalidOperationException("The package must select GSL 2.8.");
            }
            if (key == "gdiplus") Symbol(handle, "GdipCreateFromXDrawable_linux");
            if (key == "xim")
                foreach (var name in new[] { "XGetIMValues", "XGetICValues", "XSetICValues", "XCreateIC", "XVaCreateNestedList" })
                    Symbol(handle, "ComputatorMono" + name);
            report.Libraries.Add(observation);
        }
        foreach (var key in new[] { "cairo", "x11" })
        {
            var modules = LoadedLibraries().Where(p => IsLibrary(key, Path.GetFileName(p))).Select(RealPath).Distinct(StringComparer.Ordinal).ToArray();
            if (modules.Length != 1) throw new InvalidOperationException("The process has a missing or mixed " + key + " dependency.");
            if (key == "cairo") Symbol(Open(modules[0]), "cairo_xlib_surface_create");
            if (!manifest.Native.ContainsKey(key)) report.Libraries.Add(new Library { Name = key, Path = modules[0], Sha256 = Hash(modules[0]), ExternalPrerequisite = true });
        }
        // Loading framework metadata is not a GUI execution claim; that remains
        // the responsibility of the package's real production/startup acceptance.
        Assembly.Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        Assembly.Load("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        var posix = Assembly.Load("Mono.Posix, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
        var stat = posix.GetType("Mono.Unix.Native.Stat", true);
        var fstat = posix.GetType("Mono.Unix.Native.Syscall", true).GetMethod("fstat", new[] { typeof(int), stat.MakeByRefType() });
        using (var input = File.OpenRead(PackagePath("mono-package.json")))
        {
            var values = new[] { (object)input.SafeFileHandle.DangerousGetHandle().ToInt32(), Activator.CreateInstance(stat) };
            if (fstat == null || (int)fstat.Invoke(null, values) != 0)
                throw new InvalidOperationException("The matching Mono.Posix runtime component is required for safe document handling.");
        }
        report.UnixFileIdentity = true;
    }

    private static bool IsLibrary(string key, string name)
    {
        var prefix = key == "cblas" ? "libgslcblas" : key == "x11" ? "libX11" : key == "xtst" ? "libXtst" :
            key == "xinerama" ? "libXinerama" : key == "xext" ? "libXext" : key == "xcursor" ? "libXcursor" :
            key == "xfixes" ? "libXfixes" : key == "xim" ? "libcomputator-mono-xim" : "lib" + key;
        return name == prefix + ".dylib" || name.StartsWith(prefix + ".so", StringComparison.Ordinal) ||
            (name.StartsWith(prefix + ".", StringComparison.Ordinal) && name.EndsWith(".dylib", StringComparison.Ordinal));
    }

    private static string[] LoadedLibraries()
    {
        if (isMac)
        {
            var paths = new List<string>();
            var count = _dyld_image_count();
            for (uint i = 0; i < count; i++)
            {
                var name = Marshal.PtrToStringAnsi(_dyld_get_image_name(i));
                if (name != null && name.StartsWith("/", StringComparison.Ordinal) && File.Exists(name)) paths.Add(name);
            }
            return paths.Distinct().ToArray();
        }
        return File.ReadLines("/proc/self/maps").Select(line => Regex.Match(line, @"^\S+\s+\S+\s+\S+\s+\S+\s+\d+\s+(/.*)$"))
            .Where(match => match.Success).Select(match => Regex.Replace(match.Groups[1].Value, @"\\([0-7]{3})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 8)).ToString()))
            .Where(File.Exists).Distinct().ToArray();
    }

    private static string PackagePath(string relative)
    {
        if (string.IsNullOrEmpty(relative) || Path.IsPathRooted(relative) || relative.Contains('\\') ||
            relative.Split('/').Any(part => part.Length == 0 || part == "." || part == ".."))
            throw new InvalidDataException("Unsafe package path.");
        var path = Path.GetFullPath(Path.Combine(packageRoot, relative));
        if (!path.StartsWith(packageRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new IOException("Package path escapes its root.");
        for (var item = path; item != packageRoot; item = Path.GetDirectoryName(item))
            if ((File.Exists(item) || Directory.Exists(item)) && (File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Package paths must not traverse symbolic links: " + relative);
        return path;
    }

    private static string OutsideOutput(string value)
    {
        var path = Path.GetFullPath(value);
        if (path == packageRoot || path.StartsWith(packageRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            Directory.Exists(path) || (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0))
            throw new IOException("Generated configuration/reports require a regular destination outside the read-only package.");
        return path;
    }

    private static string Hash(string path)
    {
        using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    private static void AtomicWrite(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { file.Write(bytes, 0, bytes.Length); file.Flush(true); }
            if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static DataContractJsonSerializer Serializer(Type type) => new DataContractJsonSerializer(type,
        new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true, MaxItemsInObjectGraph = 200000 });

    private static void WriteJson(string path, Report report)
    {
        using (var memory = new MemoryStream()) { Serializer(typeof(Report)).WriteObject(memory, report); AtomicWrite(path, memory.ToArray()); }
    }

    private static string RealPath(string path)
    {
        var value = realpath(path, IntPtr.Zero);
        if (value == IntPtr.Zero) throw new IOException("Cannot resolve loaded native path: " + path);
        try { return Marshal.PtrToStringAnsi(value); } finally { free(value); }
    }
    private static IntPtr Open(string path)
    {
        var handle = isMac ? MacDlopen(path, 2) : LinuxDlopen(path, 2);
        if (handle == IntPtr.Zero) throw new IOException("Cannot inspect actual native library: " + path);
        return handle;
    }
    private static IntPtr Symbol(IntPtr handle, string name)
    {
        var value = isMac ? MacDlsym(handle, name) : LinuxDlsym(handle, name);
        if (value == IntPtr.Zero) throw new EntryPointNotFoundException(name);
        return value;
    }

    [DataContract] private sealed class Manifest
    {
        [DataMember(Name = "schemaVersion")] public int SchemaVersion { get; set; }
        [DataMember(Name = "version")] public string Version { get; set; }
        [DataMember(Name = "rid")] public string Rid { get; set; }
        [DataMember(Name = "files")] public List<Entry> Files { get; set; }
        [DataMember(Name = "native")] public Dictionary<string, string> Native { get; set; }
        [DataMember(Name = "baseline")] public string Baseline { get; set; }
    }
    [DataContract] private sealed class Entry
    {
        [DataMember(Name = "path")] public string Path { get; set; }
        [DataMember(Name = "sha256")] public string Sha256 { get; set; }
        [DataMember(Name = "size")] public long Size { get; set; }
    }
    [DataContract] private sealed class Report
    {
        [DataMember(Name = "schemaVersion")] public int Schema = 1;
        [DataMember(Name = "startedUtc")] public string StartedUtc;
        [DataMember(Name = "completedUtc")] public string CompletedUtc;
        [DataMember(Name = "passed")] public bool Passed;
        [DataMember(Name = "operation")] public string Operation;
        [DataMember(Name = "failure")] public string Failure;
        [DataMember(Name = "packageRoot")] public string PackageRoot;
        [DataMember(Name = "manifestSha256")] public string ManifestSha256;
        [DataMember(Name = "version")] public string Version;
        [DataMember(Name = "rid")] public string Rid;
        [DataMember(Name = "baseline")] public string Baseline;
        [DataMember(Name = "os")] public string OS;
        [DataMember(Name = "architecture")] public string Architecture;
        [DataMember(Name = "rosetta")] public bool Rosetta;
        [DataMember(Name = "mono")] public string Mono;
        [DataMember(Name = "verifiedFiles")] public int VerifiedFiles;
        [DataMember(Name = "unixFileIdentity")] public bool UnixFileIdentity;
        [DataMember(Name = "libraries")] public List<Library> Libraries;
    }
    [DataContract] private sealed class Library
    {
        [DataMember(Name = "name")] public string Name;
        [DataMember(Name = "path")] public string Path;
        [DataMember(Name = "sha256")] public string Sha256;
        [DataMember(Name = "version")] public string Version;
        [DataMember(Name = "externalPrerequisite")] public bool ExternalPrerequisite;
        [DataMember(Name = "scope")] public string Scope;
    }
    [DllImport("__Internal")] private static extern IntPtr mono_get_config_dir();
    [DllImport("libc")] private static extern int uname(IntPtr buffer);
    [DllImport("libc")] private static extern IntPtr realpath(string path, IntPtr resolved);
    [DllImport("libc")] private static extern void free(IntPtr pointer);
    [DllImport("libc")] private static extern int sysctlbyname(string name, out int oldValue, ref UIntPtr oldLength, IntPtr newValue, UIntPtr newLength);
    [DllImport("gsl", CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr gsl_set_error_handler_off();
    [DllImport("gsl", CallingConvention = CallingConvention.Cdecl)] private static extern double gsl_sf_gamma(double value);
    [DllImport("gslcblas", CallingConvention = CallingConvention.Cdecl)] private static extern double cblas_ddot(int count, double[] left, int leftStride, double[] right, int rightStride);
    [DllImport("libdl.so.2", EntryPoint = "dlopen")] private static extern IntPtr LinuxDlopen(string path, int flags);
    [DllImport("libdl.so.2", EntryPoint = "dlsym")] private static extern IntPtr LinuxDlsym(IntPtr handle, string name);
    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "dlopen")] private static extern IntPtr MacDlopen(string path, int flags);
    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "dlsym")] private static extern IntPtr MacDlsym(IntPtr handle, string name);
    [DllImport("/usr/lib/libSystem.B.dylib")] private static extern uint _dyld_image_count();
    [DllImport("/usr/lib/libSystem.B.dylib")] private static extern IntPtr _dyld_get_image_name(uint index);
}
