using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Acceptance-only display readback. Linux retains its existing real X11 root
// capture. XQuartz rootless backing windows require a calibrated read of the
// actual macOS compositor; an offscreen control render is never substituted.
internal static class NativeDesktopCapture
{
    private static readonly bool IsMac = IdentifyMac();

    internal static void CopyFromScreen(Graphics destination, Point source, Point target, Size size, Control anchor)
    {
        if (!IsMac)
        {
            destination.CopyFromScreen(source, target, size);
            return;
        }
        if (anchor == null || anchor.IsDisposed || !anchor.IsHandleCreated || !anchor.Visible)
            throw new InvalidOperationException("Mac compositor acceptance requires an actual visible native anchor.");
        using (var capture = MacDesktopCapture.Capture(new Rectangle(source, size), anchor))
        {
            if (capture == null || capture.Size != size)
                throw new InvalidOperationException("Mac compositor capture must preserve the calibrated logical rectangle.");
            destination.DrawImageUnscaled(capture, target);
        }
    }

    private static bool IdentifyMac()
    {
        var buffer = Marshal.AllocHGlobal(8192);
        try
        {
            if (uname(buffer) != 0) throw new InvalidOperationException("Cannot identify the native desktop capture platform.");
            return Marshal.PtrToStringAnsi(buffer) == "Darwin";
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    [DllImport("libc")] private static extern int uname(IntPtr buffer);
}
