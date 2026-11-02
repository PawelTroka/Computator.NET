using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

// Test-only native drawable readback. Root-screen composition and occlusion are
// separate facts, and are deliberately not inferred from this image.
internal static class MacDrawableCapture
{
    public static Bitmap Capture(IntPtr handle)
    {
        var info = new int[6];
        if (Info(new UIntPtr(unchecked((ulong)handle.ToInt64())), info, new UIntPtr(6)) != 0 ||
            info[0] <= 0 || info[1] <= 0 || info[0] > 8192 || info[1] > 8192 || info[2] != 2)
            throw new InvalidOperationException("The actual X11 drawable is not mapped and sized.");
        Console.WriteLine("Actual native drawable: handle=0x" + handle.ToInt64().ToString("x") +
            " size=" + info[0] + "x" + info[1] + " map-state=" + info[2] + " depth=" + info[3]);
        int count = checked(info[0] * info[1]);
        var pixels = new int[count];
        var captured = new int[4];
        string evidence = Environment.GetEnvironmentVariable("COMPUTATOR_MAC_DRAWABLE_EVIDENCE_DIRECTORY");
        string ppm = String.IsNullOrEmpty(evidence) ? null : Path.Combine(evidence, "native-xgetpixel-" + Guid.NewGuid().ToString("N") + ".ppm");
        int result = Argb(new UIntPtr(unchecked((ulong)handle.ToInt64())), pixels, new UIntPtr((uint)count), captured, ppm);
        Console.WriteLine("Actual XImage: width=" + captured[0] + " height=" + captured[1] + " pitch=" + captured[2] +
            " bits=" + captured[3] + " result=" + result);
        if (ppm != null) Console.WriteLine("Native C XGetPixel PPM before managed marshalling: " + ppm);
        if (result != 0)
            throw new InvalidOperationException("XGetImage did not return the mapped drawable's actual pixels.");
        var image = new Bitmap(captured[0], captured[1], PixelFormat.Format32bppArgb);
        var data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Console.WriteLine("Actual managed bitmap: width=" + image.Width + " height=" + image.Height + " stride=" + data.Stride);
        try
        {
            for (int row = 0; row < image.Height; row++)
                Marshal.Copy(pixels, row * image.Width, IntPtr.Add(data.Scan0, row * data.Stride), image.Width);
        }
        finally { image.UnlockBits(data); }
        return image;
    }
    [DllImport("computator_mac_display", EntryPoint="ComputatorMacDrawableInfo")]
    private static extern int Info(UIntPtr handle, [Out] int[] info, UIntPtr count);
    [DllImport("computator_mac_display", EntryPoint="ComputatorMacDrawableArgb")]
    private static extern int Argb(UIntPtr handle, [Out] int[] pixels, UIntPtr count, [Out] int[] info, string ppm);
}
