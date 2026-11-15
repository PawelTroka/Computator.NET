using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;
using Computator.NET.Localization;

namespace Computator.NET.Desktop.Tests
{
    // Shared by Windows NUnit and native Mono acceptance. Exercise the real
    // renderers and decode their output, rather than checking filenames alone.
    internal static class ChartExportContract
    {
        internal static readonly string[] RasterExtensions = { "png", "gif", "jpeg", "bmp", "tiff" };
        internal static readonly string[] ChartKinds = { "real", "complex", "surface" };

        internal static void RoundTrip(string kind, string extension)
        {
            var chart = Create(kind);
            var path = Path.Combine(Path.GetTempPath(), "computator-export-" + Guid.NewGuid().ToString("N") + "." + extension);
            try
            {
                var format = ExpectedFormat(extension);
                // GUID-equivalent ImageFormat objects are valid too.
                chart.SaveImage(path, new ImageFormat(format.Guid));
                using (var decoded = Image.FromFile(path))
                {
                    Check(decoded.RawFormat.Equals(format), kind + ": " + extension + " contains a different image format.");
                    Check(decoded.Width == 320 && decoded.Height == 240, "Export changed the chart dimensions.");
                    using (var bitmap = new Bitmap(decoded))
                    {
                        var colors = new System.Collections.Generic.HashSet<int>();
                        for (var y = 0; y < bitmap.Height; y += 3)
                            for (var x = 0; x < bitmap.Width; x += 3)
                                colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 8, "Export contains no visible chart content.");
                    }
                }
            }
            finally { (chart as IDisposable)?.Dispose(); if (File.Exists(path)) File.Delete(path); }
        }

        internal static void UnsupportedFormatsPreserveFiles(string kind)
        {
            var chart = Create(kind);
            var path = Path.Combine(Path.GetTempPath(), "computator-export-protected-" + Guid.NewGuid().ToString("N"));
            var sentinel = new byte[] { 13, 41, 97, 215, 255 };
            try
            {
                var formats = new[] { ImageFormat.Wmf, ImageFormat.Icon, ImageFormat.MemoryBmp, new ImageFormat(Guid.Empty) };
                foreach (var format in formats)
                    RejectWithoutWriting(chart, path, sentinel, format);
#if __MonoCS__
                RejectWithoutWriting(chart, path, sentinel, ImageFormat.Emf);
#else
                if (kind != "real") RejectWithoutWriting(chart, path, sentinel, ImageFormat.Emf);
#endif
                File.WriteAllBytes(path, sentinel);
                Throws<ArgumentNullException>(() => chart.SaveImage(path, null));
                Check(File.ReadAllBytes(path).SequenceEqual(sentinel), "A null format overwrote the destination.");
            }
            finally { (chart as IDisposable)?.Dispose(); if (File.Exists(path)) File.Delete(path); }
        }

        private static void RejectWithoutWriting(IChart chart, string path, byte[] sentinel, ImageFormat format)
        {
            File.WriteAllBytes(path, sentinel);
            Throws<NotSupportedException>(() => chart.SaveImage(path, format));
            Check(File.ReadAllBytes(path).SequenceEqual(sentinel), "Unsupported export overwrote the destination.");
            File.Delete(path);
            Throws<NotSupportedException>(() => chart.SaveImage(path, format));
            Check(!File.Exists(path), "Unsupported export created a misleading or empty file.");
        }

        internal static void LocalizedChoicesAndModeSwitches()
        {
            var previousCulture = Strings.Culture;
            try
            {
                foreach (var language in new[] { "en", "pl", "de", "cs" })
                {
                    Strings.Culture = CultureInfo.GetCultureInfo(language);
                    using (var dialog = new SaveFileDialog())
                    {
                        foreach (var kind in ChartKinds)
                        {
                            var chart = Create(kind);
                            try
                            {
                                dialog.FilterIndex = 6;
                                ChartImageExport.ConfigureDialog(dialog, chart);
                                var patterns = dialog.Filter.Split('|').Where((text, index) => index % 2 == 1).ToArray();
                                var expected = RasterExtensions;
#if !__MonoCS__
                                if (kind == "real") expected = expected.Concat(new[] { "emf" }).ToArray();
#endif
                                Check(patterns.SequenceEqual(expected.Select(extension => "*." + extension)),
                                    language + "/" + kind + ": the picker advertises an unsupported or mislabeled format.");
                                Check(dialog.FilterIndex == (expected.Length == 6 ? 6 : 1), "A stale EMF selection survived a renderer switch.");
                                for (var index = 0; index < expected.Length; index++)
                                    Check(ChartImageExport.FormatFromFilterIndex(chart, index + 1).Equals(ExpectedFormat(expected[index])),
                                        "The selected filename extension and encoder disagree.");
                                dialog.FilterIndex = 3;
                                ChartImageExport.ConfigureDialog(dialog, chart);
                                Check(dialog.FilterIndex == 3, "A supported remembered export choice was lost.");
                                Throws<ArgumentOutOfRangeException>(() => ChartImageExport.FormatFromFilterIndex(chart, 0));
                                Throws<ArgumentOutOfRangeException>(() => ChartImageExport.FormatFromFilterIndex(chart, expected.Length + 1));
                            }
                            finally { (chart as IDisposable)?.Dispose(); }
                        }
                    }
                }
            }
            finally { Strings.Culture = previousCulture; }
        }

        private static IChart Create(string kind)
        {
            if (kind == "real")
            {
                var chart = new Chart2D { Width = 320, Height = 240 };
                chart.SetChartAreaValues(-2, 2, -1, 5);
                chart.AddDataPoints(new[] { 4.0, 1.0, 0.0, 1.0, 4.0 }, new[] { -2.0, -1.0, 0.0, 1.0, 2.0 });
                return chart;
            }
            if (kind == "complex")
            {
                var chart = new ComplexChart { Width = 320, Height = 240, Quality = 30 };
                chart.SetChartAreaValues(-2, 2, -2, 2);
                chart.AddFunction(new Function((Func<Complex, Complex>)(z => z * z), FunctionType.Complex));
                return chart;
            }
            if (kind != "surface") throw new ArgumentException("Unknown chart kind.", nameof(kind));
            var surface = new Chart3DControl { Width = 320, Height = 240, Quality = 20 };
#if !__MonoCS__
            surface.Background = System.Windows.Media.Brushes.White;
            surface.Measure(new System.Windows.Size(320, 240));
            surface.Arrange(new System.Windows.Rect(0, 0, 320, 240));
            surface.UpdateLayout();
#endif
            surface.SetChartAreaValues(-2, 2, -2, 2);
            surface.AddFunction(new Function((Func<double, double, double>)((x, y) => x * x + y * y), FunctionType.Real3D));
            return surface;
        }

        private static ImageFormat ExpectedFormat(string extension)
        {
            switch (extension)
            {
                case "png": return ImageFormat.Png;
                case "gif": return ImageFormat.Gif;
                case "jpeg": return ImageFormat.Jpeg;
                case "bmp": return ImageFormat.Bmp;
                case "tiff": return ImageFormat.Tiff;
                case "emf": return ImageFormat.Emf;
                default: throw new ArgumentException("Unknown image extension.", nameof(extension));
            }
        }

        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new InvalidOperationException("Expected " + typeof(T).Name + ".");
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
