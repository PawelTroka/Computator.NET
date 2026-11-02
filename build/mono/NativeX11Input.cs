using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

// Acceptance infrastructure only. XSync acknowledges server requests, not the
// application's event handlers. Observe the actual owned XIDs and UI events.
internal static class NativeX11Input
{
    internal static void FocusOwnedWindow(IntPtr owner, Action pump)
    {
        if (owner.ToInt64() <= 1) throw new ArgumentException("An owned X11 window ID is required.", nameof(owner));
        var display = Open();
        try { Focus(display, owner, pump); }
        finally { XCloseDisplay(display); }
    }

    internal static void ClickOnce(IntPtr owner, IntPtr target, Func<int> observedClicks, Action pump)
    {
        if (owner.ToInt64() <= 1 || target.ToInt64() <= 1) throw new ArgumentException("Owned form and target X11 window IDs are required.");
        var display = Open();
        try
        {
            Focus(display, owner, pump);
            Wait(display, owner, target, () => Viewable(display, target), pump, "target mapping");
            if (!Within(display, target, owner)) throw new InvalidOperationException("Native input target does not belong to the owned form.");
            XWindowAttributes attributes;
            int x, y; IntPtr child;
            if (XGetWindowAttributes(display, target, out attributes) == 0 ||
                XTranslateCoordinates(display, target, attributes.Root, attributes.Width / 2, attributes.Height / 2, out x, out y, out child) == 0)
                throw new InvalidOperationException("Cannot resolve the owned button's actual X11 center. " + Describe(display, owner, target));
            if (XTestFakeMotionEvent(display, XDefaultScreen(display), x, y, UIntPtr.Zero) == 0 ||
                XTestFakeButtonEvent(display, 1, 1, UIntPtr.Zero) == 0 ||
                XTestFakeButtonEvent(display, 1, 0, UIntPtr.Zero) == 0)
                throw new InvalidOperationException("Cannot send the single native XTEST click. " + Describe(display, owner, target));
            XSync(display, 0);
            Wait(display, owner, target, () => observedClicks() != 0, pump, "one real Click event (count remains zero)");
            // A queued duplicate must fail too. Never send another button pair
            // to compensate for missing application-side acknowledgement.
            var settle = Stopwatch.StartNew();
            while (settle.ElapsedMilliseconds < 100) { pump(); Thread.Sleep(10); }
            if (observedClicks() != 1)
                throw new InvalidOperationException("Expected exactly one real Click event, observed " + observedClicks() + ". " + Describe(display, owner, target));
            Console.WriteLine("Native X11 click acknowledged once: owner=0x" + owner.ToInt64().ToString("x") +
                " target=0x" + target.ToInt64().ToString("x") + " root-center=" + x + "," + y);
        }
        finally { XCloseDisplay(display); }
    }

    private static void Focus(IntPtr display, IntPtr owner, Action pump)
    {
        Wait(display, owner, owner, () => Viewable(display, owner), pump, "owned form mapping");
        XSetInputFocus(display, owner, 2, UIntPtr.Zero); // RevertToParent, CurrentTime
        XSync(display, 0);
        Wait(display, owner, owner, () =>
        {
            IntPtr focus; int revert; XGetInputFocus(display, out focus, out revert);
            return Within(display, focus, owner);
        }, pump, "owned form input focus");
    }

    private static void Wait(IntPtr display, IntPtr owner, IntPtr target, Func<bool> ready, Action pump, string operation)
    {
        var watch = Stopwatch.StartNew();
        while (!ready() && watch.ElapsedMilliseconds < 5000) { pump(); Thread.Sleep(10); }
        if (!ready()) throw new TimeoutException("Timed out waiting for " + operation + ". " + Describe(display, owner, target));
    }

    private static bool Viewable(IntPtr display, IntPtr window)
    {
        XWindowAttributes attributes;
        return window != IntPtr.Zero && XGetWindowAttributes(display, window, out attributes) != 0 &&
            attributes.MapState == 2 && attributes.Width > 0 && attributes.Height > 0;
    }

    private static bool Within(IntPtr display, IntPtr window, IntPtr owner)
    {
        for (var depth = 0; depth < 64 && window.ToInt64() > 1; depth++)
        {
            if (window == owner) return true;
            IntPtr root, parent, children; uint count;
            if (XQueryTree(display, window, out root, out parent, out children, out count) == 0) return false;
            if (children != IntPtr.Zero) XFree(children);
            if (parent == window) return false;
            window = parent;
        }
        return false;
    }

    private static string Describe(IntPtr display, IntPtr owner, IntPtr target)
    {
        XWindowAttributes attributes; IntPtr focus; int revert;
        var status = XGetWindowAttributes(display, target, out attributes);
        XGetInputFocus(display, out focus, out revert);
        return "owner=0x" + owner.ToInt64().ToString("x") + " target=0x" + target.ToInt64().ToString("x") +
            " attributes=" + status + " map-state=" + attributes.MapState + " native-size=" + attributes.Width + "x" + attributes.Height +
            " focus=0x" + focus.ToInt64().ToString("x") + " revert=" + revert;
    }

    private static IntPtr Open()
    {
        var display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero) throw new InvalidOperationException("Cannot open the acceptance test's authenticated X11 display.");
        return display;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XWindowAttributes
    {
        public int X, Y, Width, Height, BorderWidth, Depth;
        public IntPtr Visual, Root;
        public int Class, BitGravity, WinGravity, BackingStore;
        public UIntPtr BackingPlanes, BackingPixel;
        public int SaveUnder;
        public IntPtr Colormap;
        public int MapInstalled, MapState;
        public IntPtr AllEventMasks, YourEventMask, DoNotPropagateMask;
        public int OverrideRedirect;
        public IntPtr Screen;
    }

    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr XOpenDisplay(IntPtr name);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XCloseDisplay(IntPtr display);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XSync(IntPtr display, int discard);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XDefaultScreen(IntPtr display);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XGetWindowAttributes(IntPtr display, IntPtr window, out XWindowAttributes attributes);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XTranslateCoordinates(IntPtr display, IntPtr source, IntPtr destination, int x, int y, out int rootX, out int rootY, out IntPtr child);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XSetInputFocus(IntPtr display, IntPtr window, int revert, UIntPtr time);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XGetInputFocus(IntPtr display, out IntPtr window, out int revert);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XQueryTree(IntPtr display, IntPtr window, out IntPtr root, out IntPtr parent, out IntPtr children, out uint count);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XFree(IntPtr pointer);
    [DllImport("libXtst.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XTestFakeMotionEvent(IntPtr display, int screen, int x, int y, UIntPtr delay);
    [DllImport("libXtst.so.6", CallingConvention = CallingConvention.Cdecl)] private static extern int XTestFakeButtonEvent(IntPtr display, uint button, int pressed, UIntPtr delay);
}
