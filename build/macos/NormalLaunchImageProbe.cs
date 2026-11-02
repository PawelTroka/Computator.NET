using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

// External test-only analysis of an unmodified full compositor screenshot.
// No WinForms controls, application references, XGetImage or reconstructed UI.
internal static class NormalLaunchImageProbe
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

    private static int Main(string[] args)
    {
        if (args.Length != 7) return 2;
        var report = new Dictionary<string, object> { { "passed", false }, { "source", "full-macos-compositor" } };
        try
        {
            var geometry = Json.Deserialize<double[]>(File.ReadAllText(args[0]));
            int clientX = Int32.Parse(args[1], CultureInfo.InvariantCulture), clientY = Int32.Parse(args[2], CultureInfo.InvariantCulture);
            int width = Int32.Parse(args[3], CultureInfo.InvariantCulture), height = Int32.Parse(args[4], CultureInfo.InvariantCulture);
            if (geometry.Length != 26 || clientX < 0 || clientY < 0 || width < 500 || height < 300 || width > 8192 || height > 8192)
                throw new InvalidOperationException("Expected measured native geometry and a positive main client rectangle.");
            foreach (double field in geometry)
                if (Double.IsNaN(field) || Double.IsInfinity(field)) throw new InvalidOperationException("Non-finite geometry.");
            double outerWidth = geometry[4] + 2 * geometry[6], outerHeight = geometry[5] + 2 * geometry[6];
            if (outerWidth <= 0 || outerHeight <= 0 || geometry[15] <= 0 || geometry[16] <= 0 || geometry[24] != 2)
                throw new InvalidOperationException("The native window/display is not mapped with positive dimensions.");
            double scaleX = geometry[11] / outerWidth, scaleY = geometry[12] / outerHeight;
            if (scaleX < 0.25 || scaleX > 4 || Math.Abs(scaleX - scaleY) > 0.01)
                throw new InvalidOperationException("X11/Quartz frame scales disagree.");
            // The anchor is the actual Mono client XID, not the whole window or
            // the WM frame. Offsets are measured by XTranslateCoordinates.
            double x = geometry[9] + (geometry[7] + geometry[6] + clientX) * scaleX;
            double y = geometry[10] + (geometry[8] + geometry[6] + clientY) * scaleY;
            double right = x + width * scaleX, bottom = y + height * scaleY;
            if (x < geometry[9] - 0.01 || y < geometry[10] - 0.01 ||
                right > geometry[9] + geometry[11] + 0.01 || bottom > geometry[10] + geometry[12] + 0.01)
                throw new InvalidOperationException("Requested client interior extends outside its actual native frame.");
            if (x < geometry[13] - 0.01 || y < geometry[14] - 0.01 ||
                right > geometry[13] + geometry[15] + 0.01 || bottom > geometry[14] + geometry[16] + 0.01)
                throw new InvalidOperationException("The entire actual client rectangle must be visible on the main display.");
            using (var screen = new Bitmap(args[5]))
            {
                double pixelsX = screen.Width / geometry[15], pixelsY = screen.Height / geometry[16];
                if (pixelsX <= 0 || Math.Abs(pixelsX - pixelsY) > 0.01)
                    throw new InvalidOperationException("Screenshot/display pixel scales disagree.");
                var crop = Rectangle.FromLTRB((int)Math.Round((x - geometry[13]) * pixelsX),
                    (int)Math.Round((y - geometry[14]) * pixelsY), (int)Math.Round((right - geometry[13]) * pixelsX),
                    (int)Math.Round((bottom - geometry[14]) * pixelsY));
                if (crop.Width <= 24 || crop.Height <= 24 || !new Rectangle(Point.Empty, screen.Size).Contains(crop))
                    throw new InvalidOperationException("The measured client crop is outside the actual screenshot pixels.");
                report["pixelCrop"] = new[] { crop.X, crop.Y, crop.Width, crop.Height };
                report["xToQuartzScale"] = new[] { scaleX, scaleY };
                report["pixelScale"] = new[] { pixelsX, pixelsY };
                report["clientX11Rectangle"] = new[] { clientX, clientY, width, height };
                using (var client = screen.Clone(crop, PixelFormat.Format32bppArgb))
                {
                    client.Save(args[6] + "-client.png", ImageFormat.Png);
                    int samples = 0, ink = 0, light = 0;
                    var colors = new HashSet<int>();
                    // Ignore edges. A titlebar or painted WM border cannot
                    // satisfy nonblank client acceptance.
                    for (int py = 12; py < client.Height - 12; py += 2)
                        for (int px = 12; px < client.Width - 12; px += 2)
                        {
                            Color value = client.GetPixel(px, py);
                            colors.Add((value.R >> 4) << 8 | (value.G >> 4) << 4 | (value.B >> 4));
                            if (Math.Max(value.R, Math.Max(value.G, value.B)) < 150) ink++;
                            if (value.R > 220 && value.G > 220 && value.B > 220) light++;
                            samples++;
                        }
                    report["samples"] = samples; report["quantizedColors"] = colors.Count;
                    report["darkInk"] = ink; report["lightSurface"] = light;
                    if (colors.Count < 12 || ink < 150 || light < samples / 20)
                        throw new InvalidOperationException("Mapped client has not produced sufficiently varied real UI pixels.");
                    report["passed"] = true;
                }
            }
        }
        catch (Exception error) { report["error"] = error.ToString(); }
        File.WriteAllText(args[6] + "-paint.json", Json.Serialize(report) + Environment.NewLine);
        return (bool)report["passed"] ? 0 : 1;
    }
}
