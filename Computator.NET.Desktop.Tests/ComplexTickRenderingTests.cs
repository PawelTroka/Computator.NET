#nullable disable

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Computator.NET.Charting.ComplexCharting;
using NUnit.Framework;
#if WINDOWS
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Functions;
#endif

namespace Computator.NET.Desktop.Tests
{
    // Linked into Engine.Tests as well: these assertions exercise production
    // layout without GDI, WinForms, or a second tick-generation implementation.
    [TestFixture]
    public class ComplexTickGeometryTests
    {
        [TestCase(-2, 2, -3, 3, 800, 600)]
        [TestCase(1, 9, 2, 6, 800, 600)]
        [TestCase(-9, -1, -6, -2, 800, 600)]
        [TestCase(-3e-8, 3e-8, -2e7, 2e7, 1200, 400)]
        [TestCase(-7, 3, -2, 8, 400, 1000)]
        public void TickValuesFollowTheActualSamplingCoordinates(double xMin, double xMax,
            double yMin, double yMax, int width, int height)
        {
            var axes = Create(width, height, 96, xMin, xMax, yMin, yMax,
                new PointF(37, 51), arrows: false);
            NUnit.Framework.Assert.That(axes.Ticks.MajorTicks, Is.Not.Empty);
            NUnit.Framework.Assert.That(axes.Ticks.MinorTicks, Is.Not.Empty);
            foreach (var mark in axes.Ticks.MajorTicks.Concat(axes.Ticks.MinorTicks))
            {
                var min = mark.HorizontalAxis ? xMin : yMin;
                var max = mark.HorizontalAxis ? xMax : yMax;
                var expectedFraction = (mark.Tick.Value - min) / (max - min);
                NUnit.Framework.Assert.That(mark.Tick.Fraction,
                    Is.EqualTo(expectedFraction).Within(1e-12));
                var expected = mark.HorizontalAxis
                    ? axes.Bounds.Left + (float)(expectedFraction * width)
                    : axes.Bounds.Top + (float)((1 - expectedFraction) * height);
                expected = mark.HorizontalAxis
                    ? Math.Clamp(expected, axes.Bounds.Left, axes.Bounds.Right - 1)
                    : Math.Clamp(expected, axes.Bounds.Top, axes.Bounds.Bottom - 1);
                NUnit.Framework.Assert.That(mark.HorizontalAxis ? mark.Start.X : mark.Start.Y,
                    Is.EqualTo(expected).Within(0.001), mark.Tick.Label);
                NUnit.Framework.Assert.That(mark.HorizontalAxis ? mark.End.X : mark.End.Y,
                    Is.EqualTo(expected).Within(0.001), mark.Tick.Label);
                AssertInside(axes.Bounds, mark.Start);
                AssertInside(axes.Bounds, mark.End);
            }
        }

        [Test]
        public void InteriorZeroUsesWidthAndHeightRatherThanLastPixelScaling()
        {
            var axes = Create(200, 150, 96, -1, 3, -3, 1, arrows: false);
            var xZero = axes.Ticks.MajorTicks.Single(mark => mark.HorizontalAxis && mark.Tick.Value == 0);
            var yZero = axes.Ticks.MajorTicks.Single(mark => !mark.HorizontalAxis && mark.Tick.Value == 0);
            NUnit.Framework.Assert.That(xZero.Start.X, Is.EqualTo(50));
            NUnit.Framework.Assert.That(yZero.Start.Y, Is.EqualTo(37.5));
            NUnit.Framework.Assert.That(xZero.Start.X, Is.EqualTo(axes.Origin.X));
            NUnit.Framework.Assert.That(yZero.Start.Y, Is.EqualTo(axes.Origin.Y));
        }

        [TestCase(-2, 2, -2, 2, 800, 600)]
        [TestCase(1, 3, 1, 3, 800, 600)]
        [TestCase(-3, -1, -3, -1, 800, 600)]
        [TestCase(-0.001, 2, -2, 0.001, 320, 240)]
        [TestCase(-2, 0.001, -0.001, 2, 320, 240)]
        [TestCase(-2, 0, -2, 0, 800, 600)]
        [TestCase(-3e-8, 3e-8, -2e7, 2e7, 1200, 400)]
        [TestCase(1e15, 1e15 + 1, 1e-300, 9e-300, 800, 600)]
        public void MeasuredNumbersStayClearOfEachOtherAndCoordinateNames(double xMin,
            double xMax, double yMin, double yMax, int width, int height)
        {
            var axes = Create(width, height, 96, xMin, xMax, yMin, yMax);
            AssertClearLabels(axes);
            NUnit.Framework.Assert.That(VisibleLabels(axes), Is.Not.Empty);
            foreach (var group in VisibleLabels(axes).GroupBy(mark => mark.HorizontalAxis))
                NUnit.Framework.Assert.That(group.Select(mark => mark.Tick.Label), Is.Unique,
                    "Different coordinates on one axis must remain distinguishable");
            NUnit.Framework.Assert.That(VisibleLabels(axes).Count(mark => mark.Tick.Value == 0),
                Is.LessThanOrEqualTo(1));
        }

        [TestCase(-2, 2, -2, 2)]
        [TestCase(-2, 0, -2, 0)]
        [TestCase(0, 2, 0, 2)]
        [TestCase(-2, 0, 0, 2)]
        [TestCase(0, 2, -2, 0)]
        public void CrossingAndCornerOriginsHaveOneReadableZero(double xMin, double xMax,
            double yMin, double yMax)
        {
            var axes = Create(800, 600, 96, xMin, xMax, yMin, yMax);
            AssertClearLabels(axes);
            var zeros = VisibleLabels(axes).Where(mark => mark.Tick.Value == 0).ToArray();
            NUnit.Framework.Assert.That(zeros, Has.Length.EqualTo(1),
                "Arrowheads and coordinate names must not erase the corner's zero value");
            NUnit.Framework.Assert.That(zeros[0].Tick.Label, Is.EqualTo("0"));
        }

        [Test]
        public void IntermediateMarksAreShorterAndRemainUnlabelled()
        {
            var axes = Create(800, 600, 96, -2, 2, -2, 2);
            NUnit.Framework.Assert.That(axes.Ticks.MinorTicks, Is.Not.Empty);
            NUnit.Framework.Assert.That(axes.Ticks.MinorHalfLength,
                Is.LessThan(axes.Ticks.MajorHalfLength));
            foreach (var mark in axes.Ticks.MajorTicks)
                NUnit.Framework.Assert.That(Length(mark),
                    Is.EqualTo(2 * axes.Ticks.MajorHalfLength).Within(0.001));
            foreach (var mark in axes.Ticks.MinorTicks)
            {
                NUnit.Framework.Assert.That(Length(mark),
                    Is.EqualTo(2 * axes.Ticks.MinorHalfLength).Within(0.001));
                NUnit.Framework.Assert.That(mark.LabelBounds.IsEmpty, Is.True);
                NUnit.Framework.Assert.That(axes.Ticks.MajorTicks.Any(major =>
                    major.HorizontalAxis == mark.HorizontalAxis && major.Tick.Value == mark.Tick.Value),
                    Is.False, "A subdivision must not redraw a major tick");
            }
        }

        [Test]
        public void NonSquareChartsChooseDensityForEachAxisIndependently()
        {
            var wide = Create(1200, 400, 96, -10, 10, -10, 10);
            var tall = Create(400, 1200, 96, -10, 10, -10, 10);
            NUnit.Framework.Assert.That(wide.Ticks.MajorTicks.Count(mark => mark.HorizontalAxis),
                Is.GreaterThan(tall.Ticks.MajorTicks.Count(mark => mark.HorizontalAxis)));
            NUnit.Framework.Assert.That(tall.Ticks.MajorTicks.Count(mark => !mark.HorizontalAxis),
                Is.GreaterThan(wide.Ticks.MajorTicks.Count(mark => !mark.HorizontalAxis)));
            AssertClearLabels(wide);
            AssertClearLabels(tall);
        }

        [TestCase(144, 1200, 900)]
        [TestCase(192, 1600, 1200)]
        public void EquivalentLogicalChartsKeepIntervalsAndScaleTickDimensionsOnce(int dpi,
            int width, int height)
        {
            var baseline = Create(800, 600, 96, -2, 2, -2, 2);
            var scaled = Create(width, height, dpi, -2, 2, -2, 2);
            var ratio = dpi / 96f;
            NUnit.Framework.Assert.That(scaled.Ticks.MajorTicks.Select(Key),
                Is.EqualTo(baseline.Ticks.MajorTicks.Select(Key)));
            NUnit.Framework.Assert.That(scaled.Ticks.MinorTicks.Select(Key),
                Is.EqualTo(baseline.Ticks.MinorTicks.Select(Key)));
            NUnit.Framework.Assert.That(scaled.Ticks.MajorHalfLength / ratio,
                Is.EqualTo(baseline.Ticks.MajorHalfLength).Within(0.001));
            NUnit.Framework.Assert.That(scaled.Ticks.MinorHalfLength / ratio,
                Is.EqualTo(baseline.Ticks.MinorHalfLength).Within(0.001));
            NUnit.Framework.Assert.That(scaled.Ticks.LabelGap / ratio,
                Is.EqualTo(baseline.Ticks.LabelGap).Within(0.001));
            AssertClearLabels(scaled);
        }

        [Test]
        public void ZoomReplacesIntervalsWithValuesFromTheNewVisibleRange()
        {
            var normal = Create(800, 600, 96, -2, 2, -2, 2);
            var zoomed = Create(800, 600, 96, -0.002, 0.002, -0.004, 0.004);
            NUnit.Framework.Assert.That(Spacing(zoomed, true), Is.LessThan(Spacing(normal, true)));
            NUnit.Framework.Assert.That(Spacing(zoomed, false), Is.LessThan(Spacing(normal, false)));
            NUnit.Framework.Assert.That(zoomed.Ticks.MajorTicks.Where(mark => mark.HorizontalAxis)
                .All(mark => mark.Tick.Value >= -0.002 && mark.Tick.Value <= 0.002), Is.True);
            NUnit.Framework.Assert.That(zoomed.Ticks.MajorTicks.Where(mark => !mark.HorizontalAxis)
                .All(mark => mark.Tick.Value >= -0.004 && mark.Tick.Value <= 0.004), Is.True);
            AssertClearLabels(zoomed);
        }

        [Test]
        public void ScientificAndExtremeFiniteDomainsProduceFiniteVisibleMarkings()
        {
            foreach (var extent in new[] { 1e-300, 1e-9, 1e12, double.MaxValue })
            {
                var axes = Create(800, 600, 96, -extent, extent, -extent, extent);
                var labels = VisibleLabels(axes).ToArray();
                NUnit.Framework.Assert.That(labels, Is.Not.Empty, extent.ToString("G17"));
                NUnit.Framework.Assert.That(labels.Any(mark => mark.Tick.Value != 0 &&
                    mark.Tick.Label.IndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);
                foreach (var mark in axes.Ticks.MajorTicks.Concat(axes.Ticks.MinorTicks))
                {
                    NUnit.Framework.Assert.That(double.IsFinite(mark.Tick.Value), Is.True);
                    NUnit.Framework.Assert.That(double.IsFinite(mark.Tick.Fraction), Is.True);
                    AssertInside(axes.Bounds, mark.Start);
                    AssertInside(axes.Bounds, mark.End);
                }
                AssertClearLabels(axes);
            }
        }

        [Test]
        public void AnOffsetTargetMovesNumbersAndMarksTogether()
        {
            var baseline = Create(800, 600, 96, -2, 2, -2, 2);
            var shifted = Create(800, 600, 96, -2, 2, -2, 2, new PointF(37, 51));
            NUnit.Framework.Assert.That(shifted.Ticks.MajorTicks.Select(Key),
                Is.EqualTo(baseline.Ticks.MajorTicks.Select(Key)));
            foreach (var pair in baseline.Ticks.MajorTicks.Zip(shifted.Ticks.MajorTicks))
            {
                NUnit.Framework.Assert.That(pair.Second.Start.X - pair.First.Start.X,
                    Is.EqualTo(37).Within(0.001));
                NUnit.Framework.Assert.That(pair.Second.Start.Y - pair.First.Start.Y,
                    Is.EqualTo(51).Within(0.001));
                NUnit.Framework.Assert.That(pair.Second.LabelBounds.IsEmpty,
                    Is.EqualTo(pair.First.LabelBounds.IsEmpty));
                if (!pair.First.LabelBounds.IsEmpty)
                {
                    NUnit.Framework.Assert.That(pair.Second.LabelBounds.X - pair.First.LabelBounds.X,
                        Is.EqualTo(37).Within(0.001));
                    NUnit.Framework.Assert.That(pair.Second.LabelBounds.Y - pair.First.LabelBounds.Y,
                        Is.EqualTo(51).Within(0.001));
                }
            }
        }

        internal static void AssertClearLabels(ComplexAxesLayout axes)
        {
            var labels = VisibleLabels(axes).ToArray();
            for (var index = 0; index < labels.Length; index++)
            {
                var current = labels[index];
                ComplexAxesGeometryTests.AssertContained(axes.SafeBounds, current.LabelBounds);
                NUnit.Framework.Assert.That(current.LabelBounds.IntersectsWith(axes.XLabelBounds),
                    Is.False, current.Tick.Label + " covers Re(z)");
                NUnit.Framework.Assert.That(current.LabelBounds.IntersectsWith(axes.YLabelBounds),
                    Is.False, current.Tick.Label + " covers Im(z)");
                for (var other = index + 1; other < labels.Length; other++)
                    NUnit.Framework.Assert.That(current.LabelBounds.IntersectsWith(labels[other].LabelBounds),
                        Is.False, current.Tick.Label + " overlaps " + labels[other].Tick.Label);
            }
        }

        internal static System.Collections.Generic.IEnumerable<ComplexTicksLayout.Mark> VisibleLabels(
            ComplexAxesLayout axes) => axes.Ticks.MajorTicks.Where(mark => !mark.LabelBounds.IsEmpty);

        private static ComplexAxesLayout Create(int width, int height, float dpi,
            double xMin, double xMax, double yMin, double yMax, PointF offset = default, bool arrows = true)
        {
            var axes = ComplexAxesLayout.Create(new RectangleF(offset.X, offset.Y, width, height),
                dpi, dpi, xMin, xMax, yMin, yMax, arrows ? 0.02 : 0);
            NUnit.Framework.Assert.That(axes, Is.Not.Null);
            var scale = axes.DpiScale * axes.FontScale;
            axes.PlaceLabels(new SizeF(50 * scale, 20 * scale), new SizeF(50 * scale, 20 * scale));
            axes.Ticks = ComplexTicksLayout.Create(axes, xMin, xMax, yMin, yMax,
                text => new SizeF(text.Length * 7 * scale, 14 * scale), CultureInfo.InvariantCulture);
            return axes;
        }

        private static (bool horizontal, double value) Key(ComplexTicksLayout.Mark mark) =>
            (mark.HorizontalAxis, mark.Tick.Value);

        private static float Length(ComplexTicksLayout.Mark mark) =>
            Math.Abs(mark.End.X - mark.Start.X) + Math.Abs(mark.End.Y - mark.Start.Y);

        private static double Spacing(ComplexAxesLayout axes, bool horizontal)
        {
            var values = axes.Ticks.MajorTicks.Where(mark => mark.HorizontalAxis == horizontal)
                .Select(mark => mark.Tick.Value).OrderBy(value => value).ToArray();
            NUnit.Framework.Assert.That(values.Length, Is.GreaterThan(1));
            return values[1] - values[0];
        }

        private static void AssertInside(RectangleF bounds, PointF point)
        {
            NUnit.Framework.Assert.That(float.IsFinite(point.X) && float.IsFinite(point.Y), Is.True);
            NUnit.Framework.Assert.That(point.X, Is.InRange(bounds.Left, bounds.Right - 1));
            NUnit.Framework.Assert.That(point.Y, Is.InRange(bounds.Top, bounds.Bottom - 1));
        }
    }

#if WINDOWS
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ComplexTickRenderingTests
    {
        [TestCase(320, 240, 96)]
        [TestCase(800, 600, 96)]
        [TestCase(1200, 400, 96)]
        [TestCase(400, 1000, 96)]
        [TestCase(1200, 900, 144)]
        [TestCase(1600, 1200, 192)]
        [TestCase(320, 240, 192)]
        public void EveryPlacedNumberContainsActualGdiGlyphInk(int width, int height, int dpi)
        {
            using var bitmap = NewBitmap(width, height, dpi);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            var axes = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, width, height),
                -2, 2, -2, 2, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            var labels = ComplexTickGeometryTests.VisibleLabels(axes).ToArray();
            NUnit.Framework.Assert.That(labels.Count(mark => mark.HorizontalAxis), Is.GreaterThan(0));
            NUnit.Framework.Assert.That(labels.Count(mark => !mark.HorizontalAxis), Is.GreaterThan(0));
            ComplexTickGeometryTests.AssertClearLabels(axes);
            foreach (var mark in labels)
                NUnit.Framework.Assert.That(CountBlueInk(bitmap, mark.LabelBounds), Is.GreaterThan(2),
                    "No actual numeric glyphs at " + mark.Tick.Label);
        }

        [TestCase(-2, 0, -2, 0)]
        [TestCase(0, 2, 0, 2)]
        [TestCase(-3e-8, 3e-8, -2e7, 2e7)]
        public void BoundaryAndScientificNumbersArePaintedInsideTheirMeasuredBounds(double xMin,
            double xMax, double yMin, double yMax)
        {
            using var bitmap = NewBitmap(800, 600, 96);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            var axes = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, 800, 600),
                xMin, xMax, yMin, yMax, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            ComplexTickGeometryTests.AssertClearLabels(axes);
            var labels = ComplexTickGeometryTests.VisibleLabels(axes).ToArray();
            NUnit.Framework.Assert.That(labels.Count(mark => mark.Tick.Value == 0), Is.EqualTo(1));
            foreach (var mark in labels)
                NUnit.Framework.Assert.That(CountBlueInk(bitmap, mark.LabelBounds), Is.GreaterThan(2),
                    mark.Tick.Label);
        }

        [TestCase(144, 1200, 900)]
        [TestCase(192, 1600, 1200)]
        public void PointFontNumericGlyphsReceiveExactlyOneDpiScale(int dpi, int width, int height)
        {
            using var baselineBitmap = NewBitmap(800, 600, 96);
            using var highDpiBitmap = NewBitmap(width, height, dpi);
            using var baselineGraphics = Graphics.FromImage(baselineBitmap);
            using var highDpiGraphics = Graphics.FromImage(highDpiBitmap);
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            var baseline = ComplexAxesRenderer.Draw(baselineGraphics, new Rectangle(0, 0, 800, 600),
                -2, 2, -2, 2, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            var scaled = ComplexAxesRenderer.Draw(highDpiGraphics, new Rectangle(0, 0, width, height),
                -2, 2, -2, 2, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            var compared = 0;
            foreach (var original in ComplexTickGeometryTests.VisibleLabels(baseline))
            {
                var match = ComplexTickGeometryTests.VisibleLabels(scaled).SingleOrDefault(mark =>
                    mark.HorizontalAxis == original.HorizontalAxis && mark.Tick.Value == original.Tick.Value);
                if (match == null) continue;
                NUnit.Framework.Assert.That(match.LabelBounds.Width / (dpi / 96f),
                    Is.EqualTo(original.LabelBounds.Width).Within(1));
                NUnit.Framework.Assert.That(match.LabelBounds.Height / (dpi / 96f),
                    Is.EqualTo(original.LabelBounds.Height).Within(1));
                compared++;
            }
            NUnit.Framework.Assert.That(compared, Is.GreaterThanOrEqualTo(4));
            NUnit.Framework.Assert.That(font.SizeInPoints, Is.EqualTo(13));
        }

        [Test]
        public void RetainNumericalAxesOnActualDomainColoringAndTheOrdinaryFunctionReference()
        {
            var repository = FindRepositoryRoot();
            var destination = Path.Combine(repository, "TestResults", "chart-axis-rendering", "complex");
            Directory.CreateDirectory(destination);
            var cases = new[]
            {
                new VisualCase("signed-96", 800, 600, 96),
                new VisualCase("positive-only", 800, 600, 96, 1, 3, 1, 3),
                new VisualCase("negative-only", 800, 600, 96, -3, -1, -3, -1),
                new VisualCase("zero-at-corner", 800, 600, 96, -4, 0, -4, 0),
                new VisualCase("wide-96", 1200, 400, 96),
                new VisualCase("tall-96", 400, 1000, 96),
                new VisualCase("small-192", 320, 240, 192),
                new VisualCase("medium-144", 1200, 900, 144),
                new VisualCase("medium-192", 1600, 1200, 192),
                new VisualCase("scientific-mixed-scales", 800, 600, 96,
                    -3e-8, 3e-8, -2e7, 2e7, 1e-8, 1e7),
                new VisualCase("zoomed", 800, 600, 96, -0.2, 0.2, -0.15, 0.15)
            };
            var renderedCases = new List<object>();
            var imagePaths = new List<string>();
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            foreach (var item in cases)
            {
                using var chart = new ComplexChart
                    { Size = new Size(80, 60), Dock = DockStyle.None, Quality = 25, ShouldDrawAxes = false };
                chart.SetChartAreaValues(item.XMin, item.XMax, item.YMin, item.YMax);
                Func<Complex, Complex> function = z =>
                {
                    var w = new Complex(z.Real / item.XScale, z.Imaginary / item.YScale);
                    return w * w - 1;
                };
                chart.AddFunction(new Function(function, FunctionType.Complex));
                WaitForRedraw(chart);
                using var plot = chart.GetImage(item.Width, item.Height);
                using var rendered = NewBitmap(item.Width, item.Height, item.Dpi);
                ComplexAxesLayout axes;
                using (var graphics = Graphics.FromImage(rendered))
                {
                    graphics.DrawImage(plot, new Rectangle(0, 0, item.Width, item.Height));
                    axes = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, item.Width, item.Height),
                        item.XMin, item.XMax, item.YMin, item.YMax, 0.02,
                        "Re(z)", "Im(z)", font, Color.Black, Color.Black);
                }
                ComplexTickGeometryTests.AssertClearLabels(axes);
                NUnit.Framework.Assert.That(ComplexTickGeometryTests.VisibleLabels(axes), Is.Not.Empty, item.Name);
                var filename = item.Name + ".png";
                var path = Path.Combine(destination, filename);
                rendered.Save(path, ImageFormat.Png);
                imagePaths.Add(filename);
                TestContext.AddTestAttachment(path, item.Name + ": production sampling, domain coloring and axes");
                renderedCases.Add(new
                {
                    scenario = item,
                    labels = ComplexTickGeometryTests.VisibleLabels(axes).Select(mark => new
                    {
                        axis = mark.HorizontalAxis ? "Re(z)" : "Im(z)",
                        value = mark.Tick.Value, text = mark.Tick.Label,
                        x = mark.LabelBounds.X, y = mark.LabelBounds.Y,
                        width = mark.LabelBounds.Width, height = mark.LabelBounds.Height
                    }).ToArray(),
                    majorTicks = axes.Ticks.MajorTicks.Count,
                    minorTicks = axes.Ticks.MinorTicks.Count
                });
            }

            const string referenceFilename = "fx-reference.png";
            RetainOrdinaryFunctionReference(Path.Combine(destination, referenceFilename));
            imagePaths.Add(referenceFilename);
            var manifest = Path.Combine(destination, "manifest.json");
            File.WriteAllText(manifest, JsonSerializer.Serialize(new
            {
                issue = 221,
                renderer = "Windows System.Drawing/GDI+; production ComplexChart.GetImage and ComplexAxesRenderer",
                testedCommit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local working tree",
                function = "w² − 1; w = Re(z)/XScale + i Im(z)/YScale (scales are 1 except the mixed-scale case)",
                reference = "Unchanged production Chart2D rendering of sin(x), including its automatic major/minor axes",
                fontFamily = font.FontFamily.Name, fontSizePoints = font.SizeInPoints,
                culture = CultureInfo.CurrentCulture.Name,
                dpiScope = "96/144/192 bitmap DPI; no claim of a physical monitor-switch test",
                cases = renderedCases,
                sources = new[]
                {
                    "Computator.NET.Charting/Axes/AxisTickGenerator.cs",
                    "Computator.NET.Charting/ComplexCharting/ComplexAxesLayout.cs",
                    "Computator.NET.Charting/ComplexCharting/ComplexTicksLayout.cs",
                    "Computator.NET.Charting/ComplexCharting/ComplexAxesRenderer.cs",
                    "Computator.NET.Charting/ComplexCharting/ComplexChart.cs",
                    "Computator.NET.Charting/RealCharting/Chart2D.cs",
                    "Computator.NET.Desktop.Tests/ComplexTickRenderingTests.cs"
                }.Select(path => new { path, sha256 = HashFile(Path.Combine(repository, path)) }).ToArray(),
                images = imagePaths.Select(path => new
                    { path, sha256 = HashFile(Path.Combine(destination, path)) }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddTestAttachment(manifest, "Numerical-axis scenarios, exact sources and generated-image provenance");
        }

        private static void RetainOrdinaryFunctionReference(string path)
        {
            using var chart = new Chart2D { Size = new Size(800, 600), Dock = DockStyle.None };
            chart.SetChartAreaValues(-4, 4, -2, 2);
            chart.AddFunction(new Function(new Func<double, double>(Math.Sin), FunctionType.Real2D)
                { Name = "sin(x)" });
            foreach (var axis in new[] { chart.ChartAreas[0].AxisX, chart.ChartAreas[0].AxisY })
            {
                NUnit.Framework.Assert.That(axis.LabelStyle.Enabled, Is.True);
                NUnit.Framework.Assert.That(axis.MajorTickMark.Enabled, Is.True);
                NUnit.Framework.Assert.That(axis.MajorTickMark.TickMarkStyle, Is.EqualTo(TickMarkStyle.AcrossAxis));
                NUnit.Framework.Assert.That(axis.MinorTickMark.Enabled, Is.True);
                NUnit.Framework.Assert.That(axis.MinorGrid.Enabled, Is.True);
                NUnit.Framework.Assert.That(axis.Crossing, Is.Zero);
            }
            chart.CreateControl();
            chart.SaveImage(path, ChartImageFormat.Png);
            using var image = new Bitmap(path);
            NUnit.Framework.Assert.That(image.Size, Is.EqualTo(new Size(800, 600)));
            NUnit.Framework.Assert.That(chart.Series.Single().Points.Count, Is.GreaterThan(10));
            TestContext.AddTestAttachment(path, "Existing f(x) numerical-axis reference: production Chart2D, sin(x)");
        }

        private static Bitmap NewBitmap(int width, int height, float dpi)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(dpi, dpi);
            return bitmap;
        }

        private static int CountBlueInk(Bitmap bitmap, RectangleF bounds)
        {
            var rectangle = Rectangle.Intersect(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                Rectangle.FromLTRB((int)Math.Floor(bounds.Left), (int)Math.Floor(bounds.Top),
                    (int)Math.Ceiling(bounds.Right), (int)Math.Ceiling(bounds.Bottom)));
            var count = 0;
            for (var y = rectangle.Top; y < rectangle.Bottom; y++)
                for (var x = rectangle.Left; x < rectangle.Right; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.B > color.R + 40 && color.B > color.G + 40) count++;
                }
            return count;
        }

        private static void WaitForRedraw(ComplexChart chart)
        {
            var field = typeof(ComplexChart).GetField("_redrawing", BindingFlags.Instance | BindingFlags.NonPublic);
            var timer = Stopwatch.StartNew();
            while ((bool)field.GetValue(chart) && timer.Elapsed < TimeSpan.FromSeconds(15))
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
            NUnit.Framework.Assert.That((bool)field.GetValue(chart), Is.False,
                "The production complex sampling did not complete");
        }

        private static string HashFile(string path) =>
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

        private static string FindRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                 directory != null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Computator.NET.slnx"))) return directory.FullName;
            throw new DirectoryNotFoundException("Cannot locate the repository for numerical-axis visual evidence");
        }

        private sealed class VisualCase
        {
            public VisualCase(string name, int width, int height, int dpi,
                double xMin = -2, double xMax = 2, double yMin = -2, double yMax = 2,
                double xScale = 1, double yScale = 1)
            {
                Name = name; Width = width; Height = height; Dpi = dpi;
                XMin = xMin; XMax = xMax; YMin = yMin; YMax = yMax; XScale = xScale; YScale = yScale;
            }
            public string Name { get; }
            public int Width { get; }
            public int Height { get; }
            public int Dpi { get; }
            public double XMin { get; }
            public double XMax { get; }
            public double YMin { get; }
            public double YMax { get; }
            public double XScale { get; }
            public double YScale { get; }
        }
    }
#endif
}
