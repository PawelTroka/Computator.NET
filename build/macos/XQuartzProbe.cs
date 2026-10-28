// Standalone macOS dependency/display diagnostic; intentionally has no reference
// to Computator.NET or either 3D implementation. Compile with Framework Mono mcs.
// Native loading, an X11 connection and actual screen pixels are separate checks.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

internal static class XQuartzProbe
{
    private const string LibSystem = "/usr/lib/libSystem.B.dylib";
    private static readonly Color Navy = Color.FromArgb(26, 53, 92);
    private static readonly Color Orange = Color.FromArgb(231, 127, 35);
    private static readonly Color Green = Color.FromArgb(42, 151, 105);

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2 || (args[0] != "headless" && args[0] != "display" && args[0] != "gui"))
        {
            Console.Error.WriteLine("Run through build/macos/probe-xquartz.sh.");
            return 2;
        }

        // Avoid relying on GNU timeout, which macOS does not supply by default.
        var watchdog = new Thread(delegate()
        {
            Thread.Sleep(30000);
            Console.Error.WriteLine("FAIL: diagnostic exceeded 30 seconds; no GUI acceptance was established.");
            Environment.Exit(1);
        });
        watchdog.IsBackground = true;
        watchdog.Start();

        try
        {
            Require(Type.GetType("Mono.Runtime") != null, "Framework Mono is required.");
            Require(IntPtr.Size == 8, "A 64-bit Mono process is required.");
            Require(Environment.GetEnvironmentVariable("MONO_MWF_MAC_FORCE_X11") == "1",
                "Set MONO_MWF_MAC_FORCE_X11=1 before loading System.Drawing or WinForms.");
            ReportArchitecture();

            Require(XInitThreads() != 0, "The Mono-selected libX11 lacks working thread initialization.");
            ReportLibrary("libX11");
            CheckDrawingLibrary();
            CheckGsl();
            // Cairo can pull in another Xlib build. Detect an accidentally mixed
            // Homebrew/XQuartz client stack after all the drawing dependencies load.
            ReportLibrary("libX11");
            Console.WriteLine("PASS native-dependencies: actual 64-bit Mono loading, drawing, X11 export and GSL answers.");

            if (args[0] != "headless") CheckDisplay();
            if (args[0] == "gui") CheckWindow(args[1]);

            Console.WriteLine("Scope: standalone probe only. Computator.NET startup, editors, documents, full numerical catalogue,");
            Console.WriteLine("charts, input, clipboard, accessibility and physical Mac acceptance remain separate checks.");
            if (args[0] == "headless") Console.WriteLine("DISPLAY and GUI were not tested in headless mode.");
            else if (args[0] == "display") Console.WriteLine("A server connection was checked; no GUI window was tested.");
            return 0;
        }
        catch (Exception error)
        {
            Exception cause = error.GetBaseException();
            Console.Error.WriteLine("FAIL: " + cause.GetType().Name + ": " + cause.Message);
            Console.Error.WriteLine("Check the actual loaded library paths and matching architecture. Mono's loader configuration");
            Console.Error.WriteLine("can select a different library from the one installed by Homebrew. For loader detail, rerun with");
            Console.Error.WriteLine("MONO_LOG_LEVEL=debug MONO_LOG_MASK=dll. No application GUI compatibility is certified by this failure.");
            return 1;
        }
    }

    private static void ReportArchitecture()
    {
        IntPtr buffer = Marshal.AllocHGlobal(8192);
        try
        {
            Require(uname(buffer) == 0, "uname failed.");
            Require(Marshal.PtrToStringAnsi(buffer) == "Darwin", "This diagnostic requires macOS.");
            // Darwin's utsname contains five 256-byte fields. Inspect only machine,
            // not the computer's hostname. This is inside the actual Mono process.
            string machine = Marshal.PtrToStringAnsi(IntPtr.Add(buffer, 4 * 256));
            Require(machine == "arm64" || machine == "x86_64", "Unexpected process architecture: " + machine);
            Console.WriteLine("Mono process uname machine: " + machine + "; pointer bits: " + (8 * IntPtr.Size));
            int translated;
            UIntPtr size = new UIntPtr(4);
            int result = sysctlbyname("sysctl.proc_translated", out translated, ref size, IntPtr.Zero, UIntPtr.Zero);
            Console.WriteLine("Rosetta translation: " + (result == 0 ? (translated != 0 ? "yes" : "no") : "not reported"));
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    private static void CheckDrawingLibrary()
    {
        // This forces System.Drawing to resolve its actual native dependency.
        // Successful offscreen pixels alone do not prove the X11 entry point exists.
        using (var bitmap = new Bitmap(64, 64))
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                using (var brush = new SolidBrush(Navy)) graphics.FillRectangle(brush, 8, 8, 32, 32);
            }
            Require(bitmap.GetPixel(20, 20).ToArgb() == Navy.ToArgb(), "Offscreen GDI+ drawing produced wrong pixels.");
            Require(bitmap.GetPixel(0, 0).ToArgb() == Color.White.ToArgb(), "Offscreen GDI+ background was incorrect.");
        }
        string library = ReportLibrary("libgdiplus");
        IntPtr handle = dlopen(library, 2); // RTLD_NOW; this library is already loaded.
        Require(handle != IntPtr.Zero, "Cannot reopen the actual libgdiplus: " + LastDlError());
        try
        {
            Require(dlsym(handle, "GdipCreateFromXDrawable_linux") != IntPtr.Zero,
                "The loaded libgdiplus has no GdipCreateFromXDrawable_linux export. An X11-enabled libgdiplus " +
                "and Cairo with Xlib support are required. Homebrew's stock mono-libgdiplus uses --without-x11; " +
                "installing XQuartz or passing a bitmap test cannot restore this missing native entry point.");
        }
        finally { dlclose(handle); }
    }

    private static void CheckGsl()
    {
        // The production Mono target also imports the logical library name "gsl".
        gsl_set_error_handler_off();
        double gamma = gsl_sf_gamma(5);
        double bessel = gsl_sf_bessel_J0(0);
        Require(Math.Abs(gamma - 24) < 1e-11, "Native GSL gamma(5) should equal 24; observed " + gamma);
        Require(Math.Abs(bessel - 1) < 1e-13, "Native GSL J0(0) should equal 1; observed " + bessel);
        ReportLibrary("libgsl");
        Console.WriteLine("GSL independent known answers: gamma(5) = 24; J0(0) = 1.");
    }

    private static void CheckDisplay()
    {
        string displayName = Environment.GetEnvironmentVariable("DISPLAY");
        Require(!String.IsNullOrWhiteSpace(displayName),
            "DISPLAY is empty. Start XQuartz and use its terminal, or a login shell with XQuartz's DISPLAY. " +
            "Do not assume DISPLAY=:0; first installation may require logout/login. --headless skips this check.");
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        Require(display != IntPtr.Zero,
            "Mono could not connect to DISPLAY=" + displayName + ". Check the running XQuartz session and xauth; do not disable access control.");
        try
        {
            int major, firstEvent, firstError;
            Require(XQueryExtension(display, "RENDER", out major, out firstEvent, out firstError) != 0,
                "The selected X11 server does not advertise the RENDER extension.");
            int screen = XDefaultScreen(display);
            Console.WriteLine("DISPLAY: " + displayName + "; vendor: " + Marshal.PtrToStringAnsi(XServerVendor(display)));
            Console.WriteLine("X11 screen: " + XDisplayWidth(display, screen) + " x " + XDisplayHeight(display, screen));
            Console.WriteLine("PASS display-connection: real X11 connection and RENDER extension; window usability remains untested.");
        }
        finally { XCloseDisplay(display); }
    }

    private static void CheckWindow(string screenshotPath)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Exception failure = null;
        bool captured = false;
        int paints = 0;
        using (var window = new Form())
        using (var timer = new System.Windows.Forms.Timer())
        {
            window.Text = "Computator.NET prerequisite probe (separate test window)";
            window.ClientSize = new Size(560, 320);
            window.StartPosition = FormStartPosition.CenterScreen;
            window.TopMost = true;
            window.Paint += delegate(object sender, PaintEventArgs e)
            {
                paints++;
                e.Graphics.Clear(Color.White);
                using (var font = new Font(FontFamily.GenericSansSerif, 13))
                {
                    e.Graphics.DrawString("Mono + XQuartz native drawing probe", font, Brushes.Black, 24, 24);
                    e.Graphics.DrawString("Screen capture follows; this is not the application.", font, Brushes.Black, 24, 256);
                }
                using (var brush = new SolidBrush(Navy)) e.Graphics.FillRectangle(brush, 24, 90, 150, 130);
                using (var brush = new SolidBrush(Orange)) e.Graphics.FillRectangle(brush, 194, 90, 150, 130);
                using (var brush = new SolidBrush(Green)) e.Graphics.FillRectangle(brush, 364, 90, 150, 130);
            };
            timer.Interval = 1000;
            timer.Tick += delegate
            {
                timer.Stop();
                try
                {
                    window.Refresh();
                    Application.DoEvents();
                    Require(window.Visible && window.IsHandleCreated && paints > 0, "WinForms did not paint a visible window.");
                    using (var capture = new Bitmap(window.ClientSize.Width, window.ClientSize.Height))
                    {
                        using (Graphics graphics = Graphics.FromImage(capture))
                            graphics.CopyFromScreen(window.PointToScreen(Point.Empty), Point.Empty, capture.Size);
                        capture.Save(screenshotPath, ImageFormat.Png);
                        Require(Near(capture.GetPixel(99, 155), Navy) && Near(capture.GetPixel(269, 155), Orange)
                            && Near(capture.GetPixel(439, 155), Green) && Near(capture.GetPixel(10, 10), Color.White),
                            "Real screen pixels do not match the calibration window. Check visibility/occlusion and XQuartz capture support; " +
                            "the screenshot is diagnostic evidence, not a passing render.");
                    }
                    captured = true;
                }
                catch (Exception error) { failure = error; }
                finally { window.Close(); }
            };
            window.Shown += delegate { timer.Start(); };
            Application.Run(window);
        }
        if (failure != null) throw new InvalidOperationException("WinForms screen probe failed.", failure);
        Require(captured, "The probe window closed before its real-screen capture completed.");
        Console.WriteLine("PASS winforms-window-pixels: a small actual window painted and matching screen pixels were captured.");
        Console.WriteLine("Capture: " + Path.GetFullPath(screenshotPath));
        Console.WriteLine("Keyboard, pointer, clipboard, DPI, accessibility and Computator.NET workflows were not exercised.");
    }

    private static bool Near(Color actual, Color expected)
    {
        return Math.Abs(actual.R - expected.R) <= 12 && Math.Abs(actual.G - expected.G) <= 12
            && Math.Abs(actual.B - expected.B) <= 12;
    }

    private static string ReportLibrary(string name)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);
        uint count = _dyld_image_count();
        for (uint i = 0; i < count; i++)
        {
            string path = Marshal.PtrToStringAnsi(_dyld_get_image_name(i));
            if (path != null && Path.GetFileName(path).StartsWith(name + ".", StringComparison.Ordinal)) paths.Add(path);
        }
        Require(paths.Count == 1, "Expected exactly one loaded " + name + " dylib, observed " + paths.Count + ". Check native loader configuration.");
        foreach (string path in paths)
        {
            Console.WriteLine("Loaded " + name + ": " + path);
            return path;
        }
        throw new InvalidOperationException("No native library path found.");
    }

    private static string LastDlError() { return Marshal.PtrToStringAnsi(dlerror()) ?? "unknown loader error"; }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

    [DllImport(LibSystem)] private static extern int uname(IntPtr buffer);
    [DllImport(LibSystem)] private static extern int sysctlbyname(string name, out int value, ref UIntPtr size, IntPtr newValue, UIntPtr newSize);
    [DllImport(LibSystem)] private static extern uint _dyld_image_count();
    [DllImport(LibSystem)] private static extern IntPtr _dyld_get_image_name(uint index);
    [DllImport(LibSystem)] private static extern IntPtr dlopen(string path, int flags);
    [DllImport(LibSystem)] private static extern IntPtr dlsym(IntPtr handle, string symbol);
    [DllImport(LibSystem)] private static extern int dlclose(IntPtr handle);
    [DllImport(LibSystem)] private static extern IntPtr dlerror();
    [DllImport("libX11")] private static extern int XInitThreads();
    [DllImport("libX11")] private static extern IntPtr XOpenDisplay(IntPtr name);
    [DllImport("libX11")] private static extern int XCloseDisplay(IntPtr display);
    [DllImport("libX11")] private static extern int XDefaultScreen(IntPtr display);
    [DllImport("libX11")] private static extern IntPtr XServerVendor(IntPtr display);
    [DllImport("libX11")] private static extern int XDisplayWidth(IntPtr display, int screen);
    [DllImport("libX11")] private static extern int XDisplayHeight(IntPtr display, int screen);
    [DllImport("libX11")] private static extern int XQueryExtension(IntPtr display, string name, out int major, out int firstEvent, out int firstError);
    [DllImport("gsl")] private static extern IntPtr gsl_set_error_handler_off();
    [DllImport("gsl")] private static extern double gsl_sf_gamma(double x);
    [DllImport("gsl")] private static extern double gsl_sf_bessel_J0(double x);
}
