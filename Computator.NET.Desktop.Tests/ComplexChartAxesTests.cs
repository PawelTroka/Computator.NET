using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class ComplexChartAxesTests
    {
        [Test]
        public void DrawingUsesOffsetBoundsAndRestoresTheCallersGraphicsState()
        {
            using (var fixture = new ChartFixture(new Size(640, 480)))
            using (var bitmap = new Bitmap(740, 550))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.SetClip(new Rectangle(13, 11, 700, 520));
                graphics.TranslateTransform(7, 9);
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                // GDI+ can canonicalize a requested smoothing value; compare its actual state.
                var smoothing = graphics.SmoothingMode;
                var textHint = graphics.TextRenderingHint;
                var transform = graphics.Transform.Elements;
                var clip = graphics.ClipBounds;
                var pageUnit = graphics.PageUnit;
                var pageScale = graphics.PageScale;
                InvokeDrawAxes(fixture.Chart, graphics, new Rectangle(45, 35, 640, 480));
                NUnit.Framework.Assert.Multiple(() =>
                {
                    NUnit.Framework.Assert.That(graphics.SmoothingMode, Is.EqualTo(smoothing));
                    NUnit.Framework.Assert.That(graphics.TextRenderingHint, Is.EqualTo(textHint));
                    NUnit.Framework.Assert.That(graphics.Transform.Elements, Is.EqualTo(transform));
                    NUnit.Framework.Assert.That(graphics.ClipBounds, Is.EqualTo(clip));
                    NUnit.Framework.Assert.That(graphics.PageUnit, Is.EqualTo(pageUnit));
                    NUnit.Framework.Assert.That(graphics.PageScale, Is.EqualTo(pageScale));
                    var pixels = PixelBounds(bitmap, IsInk);
                    NUnit.Framework.Assert.That(pixels.IsEmpty, Is.False);
                    NUnit.Framework.Assert.That(pixels.Left, Is.GreaterThanOrEqualTo(52));
                    NUnit.Framework.Assert.That(pixels.Top, Is.GreaterThanOrEqualTo(44));
                    NUnit.Framework.Assert.That(pixels.Right, Is.LessThanOrEqualTo(692));
                    NUnit.Framework.Assert.That(pixels.Bottom, Is.LessThanOrEqualTo(524));
                });
            }
        }

        [TestCase(GraphicsUnit.Point)]
        [TestCase(GraphicsUnit.Pixel)]
        public void DpiScalesConfiguredFontsOnceAndKeepsEqualPixelTargetsConsistent(GraphicsUnit unit)
        {
            using (var fixture = new ChartFixture(new Size(640, 480)))
            using (var font = new Font("Arial", 18, FontStyle.Bold, unit))
            {
                var original = fixture.Chart.LabelsFont;
                try
                {
                    fixture.Chart.LabelsFont = font;
                    fixture.Chart.LabelsColor = Color.Red;
                    fixture.Chart.AxesColor = Color.Blue;
                    fixture.Chart.YLabel = null;
                    using (var normal = RenderAxes(fixture.Chart, new Size(640, 480), 96))
                    using (var large = RenderAxes(fixture.Chart, new Size(1280, 960), 96))
                    using (var highDpi = RenderAxes(fixture.Chart, new Size(1280, 960), 192))
                    {
                        var normalGlyph = PixelBounds(normal, IsRed);
                        var largeGlyph = PixelBounds(large, IsRed);
                        var highDpiGlyph = PixelBounds(highDpi, IsRed);
                        NUnit.Framework.Assert.Multiple(() =>
                        {
                            NUnit.Framework.Assert.That(normalGlyph.Height, Is.GreaterThan(0));
                            NUnit.Framework.Assert.That(highDpiGlyph.Height,
                                Is.InRange(normalGlyph.Height * 1.7, normalGlyph.Height * 2.3));
                            NUnit.Framework.Assert.That(highDpiGlyph.Height,
                                Is.EqualTo(largeGlyph.Height).Within(2));
                            NUnit.Framework.Assert.That(highDpiGlyph.Width,
                                Is.EqualTo(largeGlyph.Width).Within(3));
                            NUnit.Framework.Assert.That(fixture.Chart.LabelsFont, Is.SameAs(font));
                        });
                    }
                }
                finally { fixture.Chart.LabelsFont = original; }
            }
        }

        [Test]
        public void OversizedLabelsAndNearEdgeArrowsRemainClippedToTheDestination()
        {
            using (var fixture = new ChartFixture(new Size(640, 480)))
            using (var bitmap = new Bitmap(200, 120))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                fixture.Chart.SetChartAreaValues(-0.01, 99.99, -99.99, 0.01);
                fixture.Chart.XLabel = new string('M', 80);
                fixture.Chart.YLabel = new string('W', 80);
                graphics.Clear(Color.White);
                var bounds = new Rectangle(23, 17, 120, 60);
                InvokeDrawAxes(fixture.Chart, graphics, bounds);
                var ink = PixelBounds(bitmap, IsInk);
                NUnit.Framework.Assert.Multiple(() =>
                {
                    NUnit.Framework.Assert.That(ink.IsEmpty, Is.False);
                    NUnit.Framework.Assert.That(ink.Left, Is.GreaterThanOrEqualTo(bounds.Left));
                    NUnit.Framework.Assert.That(ink.Top, Is.GreaterThanOrEqualTo(bounds.Top));
                    NUnit.Framework.Assert.That(ink.Right, Is.LessThanOrEqualTo(bounds.Right));
                    NUnit.Framework.Assert.That(ink.Bottom, Is.LessThanOrEqualTo(bounds.Bottom));
                });
            }
        }

        [Test]
        public void ConfiguredColorsAndFontRemainVisibleAndScaleWithTheTarget()
        {
            using (var fixture = new ChartFixture(new Size(640, 480)))
            {
                fixture.Chart.AxesColor = Color.Blue;
                fixture.Chart.LabelsColor = Color.Red;
                fixture.Chart.XLabel = "Real axis";
                fixture.Chart.YLabel = null;
                using (var small = RenderAxes(fixture.Chart, new Size(640, 480)))
                using (var large = RenderAxes(fixture.Chart, new Size(1280, 960)))
                {
                    var smallLabel = PixelBounds(small, IsRed);
                    var largeLabel = PixelBounds(large, IsRed);
                    NUnit.Framework.Assert.Multiple(() =>
                    {
                        NUnit.Framework.Assert.That(smallLabel.Height, Is.GreaterThan(0));
                        NUnit.Framework.Assert.That(largeLabel.Height,
                            Is.GreaterThanOrEqualTo(smallLabel.Height * 1.6));
                        NUnit.Framework.Assert.That(BluePixelsInColumn(large, 30),
                            Is.GreaterThan(BluePixelsInColumn(small, 30)));
                    });
                }
                using (var largerFont = new Font("Arial", 26, FontStyle.Bold, GraphicsUnit.Point))
                {
                    var original = fixture.Chart.LabelsFont;
                    try
                    {
                        fixture.Chart.LabelsFont = largerFont;
                        using (var bitmap = RenderAxes(fixture.Chart, new Size(640, 480)))
                            NUnit.Framework.Assert.That(PixelBounds(bitmap, IsRed).Height, Is.GreaterThan(20));
                    }
                    finally { fixture.Chart.LabelsFont = original; }
                }
            }
        }

        [TestCase(50)]
        [TestCase(25)]
        [TestCase(5)]
        public void SaveImageUsesTheDisplayedSizeAndDoesNotBurnAxesIntoTheCache(double quality)
        {
            using (var fixture = new ChartFixture(new Size(640, 480), quality))
            {
                var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                    "complex-axes-save-" + Guid.NewGuid().ToString("N") + ".png");
                var cached = Field<Bitmap>(fixture.Chart, "image");
                fixture.Chart.AxesColor = Color.Blue;
                try
                {
                    fixture.Chart.SaveImage(path, ImageFormat.Png);
                    int[] firstPixels;
                    using (var saved = new Bitmap(path))
                    {
                        NUnit.Framework.Assert.That(saved.Size, Is.EqualTo(fixture.Chart.ClientSize));
                        NUnit.Framework.Assert.That(PixelBounds(saved, IsBlue).IsEmpty, Is.False);
                        firstPixels = Pixels(saved);
                    }
                    fixture.Chart.SaveImage(path, ImageFormat.Png);
                    using (var repeated = new Bitmap(path))
                        NUnit.Framework.Assert.That(Pixels(repeated), Is.EqualTo(firstPixels));
                    NUnit.Framework.Assert.That(Field<Bitmap>(fixture.Chart, "image"), Is.SameAs(cached));
                    NUnit.Framework.Assert.That(PixelBounds(cached, IsInk).IsEmpty, Is.True);

                    fixture.Chart.ShouldDrawAxes = false;
                    fixture.Chart.SaveImage(path, ImageFormat.Png);
                    using (var saved = new Bitmap(path))
                        NUnit.Framework.Assert.That(PixelBounds(saved, IsInk).IsEmpty, Is.True);
                }
                finally { if (File.Exists(path)) File.Delete(path); }
            }
        }

        [Test]
        public void GetImageLeavesTheControlSizeAndMouseSampleArraysUnchanged()
        {
            using (var fixture = new ChartFixture(new Size(320, 240), 25))
            {
                fixture.Sample(z => z * z);
                var points = Field<ComplexPoint[,]>(fixture.Chart, "_pointsValues");
                var colors = Field<Color[,]>(fixture.Chart, "_pointsColors");
                var cached = Field<Bitmap>(fixture.Chart, "image");
                var originalSize = fixture.Chart.Size;
                var resizeCount = 0;
                fixture.Chart.Resize += (_, _) => resizeCount++;
                using (var export = fixture.Chart.GetImage(800, 600))
                    NUnit.Framework.Assert.That(export.Size, Is.EqualTo(new Size(800, 600)));
                AssertDisplayUnchanged(fixture.Chart, originalSize, cached, points, colors);
                NUnit.Framework.Assert.That(resizeCount, Is.Zero);
            }
        }

        [Test]
        public void InvalidExportDimensionsAreRejectedWithoutChangingTheDisplayedChart()
        {
            using (var fixture = new ChartFixture(new Size(320, 240), 25))
            {
                fixture.Sample(z => z * z);
                var points = Field<ComplexPoint[,]>(fixture.Chart, "_pointsValues");
                var colors = Field<Color[,]>(fixture.Chart, "_pointsColors");
                var cached = Field<Bitmap>(fixture.Chart, "image");
                var size = fixture.Chart.Size;
                foreach (var requested in new[] { new Size(0, 100), new Size(100, 0), new Size(-1, 100) })
                    NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                        fixture.Chart.GetImage(requested.Width, requested.Height));
                AssertDisplayUnchanged(fixture.Chart, size, cached, points, colors);
            }
        }

        [Test]
        public void FailedExportSamplingLeavesTheDisplayedChartAndSamplesUnchanged()
        {
            using (var fixture = new ChartFixture(new Size(320, 240), 25))
            {
                fixture.Sample(z => z * z);
                var points = Field<ComplexPoint[,]>(fixture.Chart, "_pointsValues");
                var colors = Field<Color[,]>(fixture.Chart, "_pointsColors");
                var cached = Field<Bitmap>(fixture.Chart, "image");
                var size = fixture.Chart.Size;
                SetField(fixture.Chart, "function", new Function(
                    new Func<Complex, Complex>(_ => throw new InvalidOperationException("Export test failure")),
                    FunctionType.Complex));
                NUnit.Framework.Assert.Catch<Exception>(() => fixture.Chart.GetImage(800, 600));
                AssertDisplayUnchanged(fixture.Chart, size, cached, points, colors);
            }
        }

        [Test]
        public void EmptyChartsCanExportIncludingASinglePixelTarget()
        {
            using (var fixture = new ChartFixture(new Size(320, 240)))
            {
                foreach (var size in new[] { new Size(640, 480), new Size(1, 1) })
                using (var exported = fixture.Chart.GetImage(size.Width, size.Height))
                    NUnit.Framework.Assert.That(exported.Size, Is.EqualTo(size));
                NUnit.Framework.Assert.That(Field<ComplexPoint[,]>(fixture.Chart, "_pointsValues"), Is.Null);
            }
        }

        [TestCase("centered", 640, 480, -5, 5, -5, 5)]
        [TestCase("large", 1280, 960, -5, 5, -5, 5)]
        [TestCase("wide-shifted", 960, 360, -1, 9, -9, 1)]
        [TestCase("one-sided", 640, 480, 1, 9, 1, 9)]
        public void AttachRenderedExamplesForVisualReview(string name, int width, int height,
            double xMin, double xMax, double yMin, double yMax)
        {
            using (var fixture = new ChartFixture(new Size(width, height), 25))
            {
                fixture.Chart.SetChartAreaValues(xMin, xMax, yMin, yMax);
                fixture.Sample(z => z * z);
                using (var exported = (Bitmap)fixture.Chart.GetImage(width, height))
                {
                    NUnit.Framework.Assert.That(exported.Size, Is.EqualTo(new Size(width, height)));
                    var colors = new HashSet<int>();
                    for (var x = 5; x < width - 5; x += 10)
                    for (var y = 5; y < height - 5; y += 10)
                        colors.Add(exported.GetPixel(x, y).ToArgb());
                    NUnit.Framework.Assert.That(colors.Count, Is.GreaterThan(64));
                    var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                        "TestResults", "complex-chart-axes");
                    Directory.CreateDirectory(directory);
                    var path = Path.Combine(directory, name + ".png");
                    exported.Save(path, ImageFormat.Png);
                    TestContext.AddTestAttachment(path, "Complex chart axes: " + name);
                }
            }
        }

        private static void AssertDisplayUnchanged(ComplexChart chart, Size size, Bitmap image,
            ComplexPoint[,] points, Color[,] colors)
        {
            NUnit.Framework.Assert.Multiple(() =>
            {
                NUnit.Framework.Assert.That(chart.Size, Is.EqualTo(size));
                NUnit.Framework.Assert.That(Field<Bitmap>(chart, "image"), Is.SameAs(image));
                NUnit.Framework.Assert.That(Field<ComplexPoint[,]>(chart, "_pointsValues"), Is.SameAs(points));
                NUnit.Framework.Assert.That(Field<Color[,]>(chart, "_pointsColors"), Is.SameAs(colors));
            });
        }

        private static Bitmap RenderAxes(ComplexChart chart, Size size, float dpi = 96)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            bitmap.SetResolution(dpi, dpi);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                InvokeDrawAxes(chart, graphics, new Rectangle(Point.Empty, size));
            }
            return bitmap;
        }

        private static void InvokeDrawAxes(ComplexChart chart, Graphics graphics, Rectangle bounds) =>
            typeof(ComplexChart).GetMethod("DrawAxes", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(chart, new object[] { graphics, bounds });

        private static T Field<T>(ComplexChart chart, string name) =>
            (T)typeof(ComplexChart).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(chart);

        private static void SetField(ComplexChart chart, string name, object value) =>
            typeof(ComplexChart).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(chart, value);

        private static bool IsInk(Color color) => color.R < 240 || color.G < 240 || color.B < 240;
        private static bool IsRed(Color color) => color.R > color.G + 40 && color.R > color.B + 40;
        private static bool IsBlue(Color color) => color.B > color.R + 40 && color.B > color.G + 40;

        private static Rectangle PixelBounds(Bitmap bitmap, Func<Color, bool> predicate)
        {
            var left = bitmap.Width;
            var top = bitmap.Height;
            var right = -1;
            var bottom = -1;
            for (var x = 0; x < bitmap.Width; x++)
            for (var y = 0; y < bitmap.Height; y++)
            {
                if (!predicate(bitmap.GetPixel(x, y))) continue;
                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
            return right < left ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        private static int BluePixelsInColumn(Bitmap bitmap, int x) =>
            Enumerable.Range(0, bitmap.Height).Count(y => IsBlue(bitmap.GetPixel(x, y)));

        private static int[] Pixels(Bitmap bitmap) => Enumerable.Range(0, bitmap.Height)
            .SelectMany(y => Enumerable.Range(0, bitmap.Width).Select(x => bitmap.GetPixel(x, y).ToArgb()))
            .ToArray();

        private sealed class ChartFixture : IDisposable
        {
            public ChartFixture(Size size, double quality = 50)
            {
                Chart = new ComplexChart { ClientSize = size };
                Chart.Quality = quality;
                Chart.SetChartAreaValues(-5, 5, -5, 5);
                Chart.LabelsFont.Dispose();
                Chart.LabelsFont = new Font("Arial", 13, FontStyle.Regular, GraphicsUnit.Point);
                var scale = Math.Max(0.1, Math.Min(1, quality / 50));
                var blank = new Bitmap(Math.Max(1, (int)(size.Width * scale)),
                    Math.Max(1, (int)(size.Height * scale)));
                using (var graphics = Graphics.FromImage(blank)) graphics.Clear(Color.White);
                ReplaceImage(blank);
            }

            public ComplexChart Chart { get; }

            public void Sample(Func<Complex, Complex> function)
            {
                SetField(Chart, "function", new Function(function, FunctionType.Complex));
                var method = typeof(ComplexChart).GetMethod("CalculateValuesAndColors",
                    BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                ReplaceImage((Bitmap)method.Invoke(Chart, null));
            }

            private void ReplaceImage(Bitmap replacement)
            {
                var old = Field<Bitmap>(Chart, "image");
                SetField(Chart, "image", replacement);
                old?.Dispose();
            }

            public void Dispose()
            {
                Field<Bitmap>(Chart, "image")?.Dispose();
                Chart.LabelsFont.Dispose();
                Chart.TitleFont.Dispose();
                Chart.Dispose();
            }
        }
    }
}
