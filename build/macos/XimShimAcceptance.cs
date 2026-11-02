using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// This exercises the actual native Mono fixed-arity imports and Xlib XIM
// objects. It is not an assertion that a physical Mac IME composed text.
internal static class XimShimAcceptance
{
    [StructLayout(LayoutKind.Sequential)]
    private sealed class Point { public short X; public short Y; }

    public static void Verify()
    {
        if (Environment.GetEnvironmentVariable("COMPUTATOR_MAC_XIM_SHIM") != "1") return;
        Require(Environment.GetEnvironmentVariable("MONO_WINFORMS_XIM_STYLE") != "disabled", "Shim acceptance must enable the actual XIM path.");
        VerifyMonoImports();
        Require(AbiVersion() == 1, "Unsupported packaged XIM ABI shim.");
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        Require(display != IntPtr.Zero, "XIM contract needs the actual authenticated display.");
        IntPtr window = IntPtr.Zero, im = IntPtr.Zero, first = IntPtr.Zero, second = IntPtr.Zero;
        try
        {
            window = XCreateSimpleWindow(display, XDefaultRootWindow(display), 10, 10, 64, 64, 0, UIntPtr.Zero, UIntPtr.Zero);
            Require(window != IntPtr.Zero, "Cannot create the XIM contract client window.");
            im = XOpenIM(display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            Require(im != IntPtr.Zero, "The actual XQuartz/Xlib input method could not open.");
            IntPtr styles;
            Require(GetIMValues(im, "queryInputStyle", out styles, IntPtr.Zero) == null && styles != IntPtr.Zero,
                "Fixed XGetIMValues did not return actual input styles.");
            XFree(styles);
            // Root style is defined by Xlib and universally required by XIM.
            const int rootStyle = 0x0008 | 0x0400;
            first = CreateIC2(im, "inputStyle", rootStyle, "clientWindow", window, IntPtr.Zero);
            second = CreateIC3(im, "inputStyle", rootStyle, "clientWindow", window, "focusWindow", window, IntPtr.Zero);
            Require(first != IntPtr.Zero && second != IntPtr.Zero, "One of the two exact Mono XCreateIC overloads failed.");
            IntPtr mask = Marshal.AllocHGlobal(8);
            try
            {
                Marshal.WriteInt32(mask, 0, 0);
                Marshal.WriteInt32(mask, 4, unchecked((int)0x5ab47e19));
                Require(GetICValues(first, "filterEvents", mask, IntPtr.Zero) == null, "Fixed XGetICValues failed.");
                Require(Marshal.ReadInt32(mask, 4) == unchecked((int)0x5ab47e19), "Xlib's LP64 result overwrote Mono's 32-bit EventMask boundary.");
            }
            finally { Marshal.FreeHGlobal(mask); }
            SetICValues(first, "focusWindow", window, IntPtr.Zero);
            // The native list API records attribute/value pairs. A real Point
            // class marshals as the same pointer used by Mono's private imports.
            var point = new Point { X = 12, Y = 21 };
            IntPtr name = Marshal.StringToHGlobalAnsi("spotLocation");
            IntPtr pointMemory = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.WriteInt16(pointMemory, 0, point.X); Marshal.WriteInt16(pointMemory, 2, point.Y);
                IntPtr one = Nested1(0, name, point, IntPtr.Zero);
                IntPtr two = Nested2(0, name, point, name, pointMemory, IntPtr.Zero);
                IntPtr four = Nested4(0, name, pointMemory, name, pointMemory, name, pointMemory, name, pointMemory, IntPtr.Zero);
                Require(one != IntPtr.Zero && two != IntPtr.Zero && four != IntPtr.Zero, "An exact Mono nested-list arity failed.");
                XFree(one); XFree(two); XFree(four);
            }
            finally { Marshal.FreeHGlobal(pointMemory); Marshal.FreeHGlobal(name); }
            for (int operation = 0; operation < 8; operation++)
                Require(CallCount(operation) > 0, "XIM fixed-arity branch was not exercised: " + operation);
            Console.WriteLine("PASS XIM ABI contract: all eight fixed-arity branches, real XIM contexts and bounded EventMask write.");
        }
        finally
        {
            if (second != IntPtr.Zero) XDestroyIC(second);
            if (first != IntPtr.Zero) XDestroyIC(first);
            if (im != IntPtr.Zero) XCloseIM(im);
            if (window != IntPtr.Zero) XDestroyWindow(display, window);
            XCloseDisplay(display);
        }
    }

    private static void VerifyMonoImports()
    {
        Type keyboard = typeof(Form).Assembly.GetType("System.Windows.Forms.X11Keyboard", true);
        foreach (var entry in new[] { Tuple.Create("XCreateIC", new[] { 6, 8 }), Tuple.Create("XVaCreateNestedList", new[] { 4, 6, 10 }),
                                      Tuple.Create("XGetIMValues", new[] { 4 }), Tuple.Create("XGetICValues", new[] { 4 }), Tuple.Create("XSetICValues", new[] { 4 }) })
        {
            var methods = keyboard.GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == entry.Item1).ToArray();
            Require(methods.Select(m => m.GetParameters().Length).OrderBy(n => n).SequenceEqual(entry.Item2),
                "Mono XIM imports differ from the reviewed 6.14.1 ABI: " + entry.Item1);
            foreach (var method in methods)
            {
                var import = (DllImportAttribute)Attribute.GetCustomAttribute(method, typeof(DllImportAttribute));
                Require(import != null && import.Value == "libX11" && import.EntryPoint == entry.Item1, "Unexpected Mono native import metadata.");
            }
        }
        Type nativePoint = typeof(Form).Assembly.GetType("System.Windows.Forms.XPoint", true);
        Require(nativePoint.IsClass && Marshal.SizeOf(nativePoint) == 4, "Mono XPoint must remain a four-byte sequential class passed by pointer.");
        Type mask = typeof(Form).Assembly.GetType("System.Windows.Forms.EventMask", true);
        Type style = typeof(Form).Assembly.GetType("System.Windows.Forms.XIMProperties", true);
        Require(Enum.GetUnderlyingType(mask) == typeof(int) && Enum.GetUnderlyingType(style) == typeof(int), "Mono XIM enum widths changed.");
    }

    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    [DllImport("computator_xim", EntryPoint="ComputatorMonoXimAbiVersion")] private static extern int AbiVersion();
    [DllImport("computator_xim", EntryPoint="ComputatorMonoXimCallCount")] private static extern uint CallCount(int operation);
    [DllImport("libX11")] private static extern IntPtr XOpenDisplay(IntPtr name);
    [DllImport("libX11")] private static extern int XCloseDisplay(IntPtr display);
    [DllImport("libX11")] private static extern IntPtr XDefaultRootWindow(IntPtr display);
    [DllImport("libX11")] private static extern IntPtr XCreateSimpleWindow(IntPtr display, IntPtr root, int x, int y, uint width, uint height, uint border, UIntPtr borderPixel, UIntPtr background);
    [DllImport("libX11")] private static extern int XDestroyWindow(IntPtr display, IntPtr window);
    [DllImport("libX11")] private static extern IntPtr XOpenIM(IntPtr display, IntPtr database, IntPtr resourceName, IntPtr resourceClass);
    [DllImport("libX11")] private static extern int XCloseIM(IntPtr im);
    [DllImport("libX11")] private static extern void XDestroyIC(IntPtr ic);
    [DllImport("libX11")] private static extern int XFree(IntPtr memory);
    [DllImport("libX11", EntryPoint="XGetIMValues")] private static extern string GetIMValues(IntPtr im, string name, out IntPtr value, IntPtr end);
    [DllImport("libX11", EntryPoint="XGetICValues")] private static extern string GetICValues(IntPtr ic, string name, IntPtr value, IntPtr end);
    [DllImport("libX11", EntryPoint="XSetICValues")] private static extern void SetICValues(IntPtr ic, string name, IntPtr value, IntPtr end);
    [DllImport("libX11", EntryPoint="XCreateIC")] private static extern IntPtr CreateIC2(IntPtr im, string name, int style, string name2, IntPtr value2, IntPtr end);
    [DllImport("libX11", EntryPoint="XCreateIC")] private static extern IntPtr CreateIC3(IntPtr im, string name, int style, string name2, IntPtr value2, string name3, IntPtr value3, IntPtr end);
    [DllImport("libX11", EntryPoint="XVaCreateNestedList")] private static extern IntPtr Nested1(int dummy, IntPtr name, Point point, IntPtr end);
    [DllImport("libX11", EntryPoint="XVaCreateNestedList")] private static extern IntPtr Nested2(int dummy, IntPtr name, Point point, IntPtr name2, IntPtr value2, IntPtr end);
    [DllImport("libX11", EntryPoint="XVaCreateNestedList")] private static extern IntPtr Nested4(int dummy, IntPtr name, IntPtr value, IntPtr name2, IntPtr value2, IntPtr name3, IntPtr value3, IntPtr name4, IntPtr value4, IntPtr end);
}
