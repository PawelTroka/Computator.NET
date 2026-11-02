using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

// Acceptance-only readback from the actual macOS compositor. XQuartz rootless
// XGetImage can return black/BadMatch or corrupt a clipped backing-store stride.
// Its published native-window ID supplies a measured X11 -> Quartz transform;
// the full display capture retains occlusion by every other visible window.
internal static class MacDesktopCapture
{
    private static int sequence;

    internal static Bitmap Capture(Rectangle desktopRectangle, Control anchor)
    {
        if (anchor == null || anchor.IsDisposed || !anchor.IsHandleCreated || !anchor.Visible ||
            desktopRectangle.Width <= 0 || desktopRectangle.Height <= 0)
            throw new InvalidOperationException("Compositor capture requires a visible native anchor and a positive rectangle.");

        string directory = Environment.GetEnvironmentVariable("COMPUTATOR_MAC_COMPOSITOR_EVIDENCE_DIRECTORY");
        if (String.IsNullOrWhiteSpace(directory))
            directory = Path.Combine(Path.GetTempPath(), "computator-mac-compositor-" + Process.GetCurrentProcess().Id);
        directory = Path.GetFullPath(directory);
        Directory.CreateDirectory(directory);
        string prefix = Path.Combine(directory, "capture-" + Process.GetCurrentProcess().Id + "-" +
            Interlocked.Increment(ref sequence).ToString("D4", CultureInfo.InvariantCulture));
        double[] geometry = new double[26];
        int result = ComputatorMacDesktopGeometry(anchor.Handle, geometry, new UIntPtr((uint)geometry.Length), prefix + "-native.json");
        if (result != 0) throw new InvalidOperationException("Cannot map the visible X11 anchor to its real macOS window; native geometry status " + result + ".");
        for (int i = 0; i < geometry.Length; i++)
            if (Double.IsNaN(geometry[i]) || Double.IsInfinity(geometry[i]))
                throw new InvalidOperationException("Native compositor geometry contains a non-finite field.");

        double outerWidth = geometry[4] + 2 * geometry[6], outerHeight = geometry[5] + 2 * geometry[6];
        if (outerWidth <= 0 || outerHeight <= 0 || geometry[15] <= 0 || geometry[16] <= 0)
            throw new InvalidOperationException("Native compositor geometry has invalid window/display dimensions.");
        double scaleX = geometry[11] / outerWidth, scaleY = geometry[12] / outerHeight;
        if (scaleX <= 0 || Math.Abs(scaleX - scaleY) > 0.01 || scaleX > 4 || scaleX < 0.25)
            throw new InvalidOperationException("X11 and Quartz native frame scales disagree: " + scaleX + " / " + scaleY + ".");
        Point anchorOrigin = anchor.PointToScreen(Point.Empty);
        double quartzX = geometry[9] + (geometry[7] + geometry[6] + desktopRectangle.X - anchorOrigin.X) * scaleX;
        double quartzY = geometry[10] + (geometry[8] + geometry[6] + desktopRectangle.Y - anchorOrigin.Y) * scaleY;
        double quartzWidth = desktopRectangle.Width * scaleX, quartzHeight = desktopRectangle.Height * scaleY;
        File.WriteAllText(prefix + "-request.json", String.Format(CultureInfo.InvariantCulture,
            "{{\"x11Rectangle\":[{0},{1},{2},{3}],\"anchorScreenOrigin\":[{4},{5}],\"quartzRectangle\":[{6:R},{7:R},{8:R},{9:R}],\"displayBounds\":[{10:R},{11:R},{12:R},{13:R}]}}\n",
            desktopRectangle.X, desktopRectangle.Y, desktopRectangle.Width, desktopRectangle.Height, anchorOrigin.X, anchorOrigin.Y,
            quartzX, quartzY, quartzWidth, quartzHeight, geometry[13], geometry[14], geometry[15], geometry[16]));
        if (quartzX < geometry[13] - 0.01 || quartzY < geometry[14] - 0.01 ||
            quartzX + quartzWidth > geometry[13] + geometry[15] + 0.01 ||
            quartzY + quartzHeight > geometry[14] + geometry[16] + 0.01)
            throw new InvalidOperationException("Requested actual compositor rectangle is outside the main display; no clipped/offscreen acceptance is permitted.");

        string screenPath = prefix + "-display.png";
        RunCaptureProcess(new ProcessStartInfo("/usr/sbin/screencapture", "-x -D 1 " + Quote(screenPath)),
            prefix + "-capture.stderr.log", 10000);
        if (!File.Exists(screenPath))
            throw new InvalidOperationException("The actual macOS display capture produced no image.");
        using (var screen = new Bitmap(screenPath))
        {
            double pixelsX = screen.Width / geometry[15], pixelsY = screen.Height / geometry[16];
            if (pixelsX <= 0 || Math.Abs(pixelsX - pixelsY) > 0.01)
                throw new InvalidOperationException("The actual screenshot and main display have incompatible pixel scales.");
            int x = (int)Math.Round((quartzX - geometry[13]) * pixelsX);
            int y = (int)Math.Round((quartzY - geometry[14]) * pixelsY);
            int right = (int)Math.Round((quartzX + quartzWidth - geometry[13]) * pixelsX);
            int bottom = (int)Math.Round((quartzY + quartzHeight - geometry[14]) * pixelsY);
            var crop = Rectangle.FromLTRB(x, y, right, bottom);
            if (crop.Width <= 0 || crop.Height <= 0 || !new Rectangle(Point.Empty, screen.Size).Contains(crop))
                throw new InvalidOperationException("Measured actual screenshot crop is outside its pixels.");
            File.WriteAllText(prefix + "-mapping.json", String.Format(CultureInfo.InvariantCulture,
                "{{\"source\":\"full-macos-compositor\",\"anchor\":{0},\"x11Rectangle\":[{1},{2},{3},{4}],\"anchorScreenOrigin\":[{5},{6}],\"quartzRectangle\":[{7:R},{8:R},{9:R},{10:R}],\"xToQuartzScale\":[{11:R},{12:R}],\"displayPixels\":[{13},{14}],\"pixelScale\":[{15:R},{16:R}],\"pixelCrop\":[{17},{18},{19},{20}]}}\n",
                anchor.Handle.ToInt64(), desktopRectangle.X, desktopRectangle.Y, desktopRectangle.Width, desktopRectangle.Height,
                anchorOrigin.X, anchorOrigin.Y, quartzX, quartzY, quartzWidth, quartzHeight, scaleX, scaleY,
                screen.Width, screen.Height, pixelsX, pixelsY, crop.X, crop.Y, crop.Width, crop.Height));
            var output = new Bitmap(desktopRectangle.Width, desktopRectangle.Height, PixelFormat.Format32bppArgb);
            try
            {
                using (Graphics graphics = Graphics.FromImage(output))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    graphics.DrawImage(screen, new Rectangle(Point.Empty, output.Size), crop, GraphicsUnit.Pixel);
                }
                output.Save(prefix + "-crop.png", ImageFormat.Png);
                Console.WriteLine("Actual macOS compositor capture: " + prefix + "-mapping.json");
                return output;
            }
            catch { output.Dispose(); throw; }
        }
    }

    internal static string RunCaptureProcess(ProcessStartInfo start, string errorPath, int timeoutMilliseconds)
    {
        if (timeoutMilliseconds <= 0 || timeoutMilliseconds > 10000)
            throw new ArgumentOutOfRangeException("timeoutMilliseconds", "External capture deadlines must remain between one millisecond and ten seconds.");
        start.UseShellExecute = false;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;
        // Trace the real Mono application, not the external display reader.
        // Its inherited loader diagnostics can otherwise fill redirected pipes.
        start.EnvironmentVariables.Remove("DYLD_PRINT_LIBRARIES");
        using (var capture = Process.Start(start))
        {
            if (capture == null) throw new InvalidOperationException("Cannot start the actual macOS display capture.");
            var watch = Stopwatch.StartNew();
            // Read concurrently with the process. Waiting before draining can
            // deadlock even a successful tool once stderr exceeds its pipe.
            var readError = capture.StandardError.ReadToEndAsync();
            if (!capture.WaitForExit(timeoutMilliseconds))
            {
                capture.Kill();
                capture.WaitForExit(1000);
                readError.Wait(1000);
                File.WriteAllText(errorPath, readError.IsCompleted ? readError.GetAwaiter().GetResult() : "stderr remained open after timed-out capture cleanup");
                throw new TimeoutException("Actual macOS compositor capture exceeded its bounded deadline.");
            }
            int remaining = Math.Max(0, timeoutMilliseconds - (int)watch.ElapsedMilliseconds);
            if (!readError.Wait(remaining))
            {
                File.WriteAllText(errorPath, "stderr remained open after the external capture exited");
                throw new TimeoutException("Actual macOS compositor stderr exceeded its bounded deadline.");
            }
            string error = readError.GetAwaiter().GetResult();
            File.WriteAllText(errorPath, error);
            if (capture.ExitCode != 0)
                throw new InvalidOperationException("Actual macOS compositor capture failed: " + capture.ExitCode + " " + error);
            return error;
        }
    }

    private static string Quote(string value) { return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""; }

    [DllImport("computator_mac_display", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ComputatorMacDesktopGeometry(IntPtr anchor, [Out] double[] data, UIntPtr count, string reportPath);
}
