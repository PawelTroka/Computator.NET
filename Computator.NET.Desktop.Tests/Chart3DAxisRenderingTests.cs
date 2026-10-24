using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Computator.NET.Charting.Chart3D.Chart3D;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;
using ChartData = Computator.NET.Charting.Chart3D.Chart3D.Chart3D;
using DataPoint = Computator.NET.DataTypes.Charts.Point3D;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using WorldPoint = System.Windows.Media.Media3D.Point3D;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class Chart3DAxisRenderingTests
    {
        [Test]
        public void DefaultScaleColorsAndEqualAxesProduceReadableNumericLabels()
        {
            var chart = DefaultSurface();
            NUnit.Framework.Assert.That(chart.Scale, Is.EqualTo(0.5));
            NUnit.Framework.Assert.That(chart.EqualAxes, Is.True);
            NUnit.Framework.Assert.That(chart.AxisLabels.Color, Is.EqualTo(Colors.Goldenrod));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Count(label => label.Value.HasValue),
                Is.GreaterThanOrEqualTo(3));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks.Any(tick => !tick.IsMajor), Is.True);
            AssertLabelsFit(chart.AxisLabels.Annotations, Viewport(chart).RenderSize);
        }

        [Test]
        public void NumericAxesAndTheirArrowBoundsMatchTheActualMeshVertices()
        {
            var chart = Surface(800, 600);
            var data = (UniformSurfaceChart3D)Data(chart);
            var cones = data.GetMeshes().OfType<Cone3D>().ToArray();
            var axes = data.GetAxes();
            NUnit.Framework.Assert.That(cones, Has.Length.EqualTo(3));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Where(label => label.Value.HasValue)
                .Select(label => label.Axis).Distinct(), Is.EquivalentTo(new[] { 0, 1, 2 }),
                "This rotated surface exposes all three numerical coordinate scales.");
            for (var axis = 0; axis < 3; axis++)
            {
                NUnit.Framework.Assert.That((axes[axis].LabelAnchor - cones[axis].GetLastPoint()).Length,
                    Is.LessThan(1e-10));
                for (var vertex = 0; vertex < 6; vertex++)
                    NUnit.Framework.Assert.That((axes[axis].ArrowVertices[vertex] - cones[axis].GetPoint(vertex)).Length,
                        Is.LessThan(1e-10));
            }
        }

        [TestCase(1e-6, 1e6, true)]
        [TestCase(1e-6, 1e6, false)]
        [TestCase(1e-16, 1e18, true)]
        public void MixedCoordinateScalesKeepAxisDecorationsSmallAndAllThreeScalesReadable(
            double xUnit, double yUnit, bool normalizeCoordinates)
        {
            var chart = MixedSurface(1000, 750, xUnit, yUnit, normalizeCoordinates);
            var viewport = Viewport(chart);
            var axes = Data(chart).GetAxes();
            var cones = ((UniformSurfaceChart3D)Data(chart)).GetMeshes().OfType<Cone3D>().ToArray();
            for (var axis = 0; axis < 3; axis++)
            {
                var bounds = Rect.Empty;
                for (var vertex = 0; vertex < cones[axis].GetVertexNo(); vertex++)
                {
                    var point = chart.m_transformMatrix.VertexToScreenPt(cones[axis].GetPoint(vertex), viewport);
                    NUnit.Framework.Assert.That(double.IsFinite(point.X) && double.IsFinite(point.Y), Is.True);
                    bounds.Union(point);
                }
                NUnit.Framework.Assert.That(Math.Max(bounds.Width, bounds.Height), Is.LessThan(40),
                    "Arrow geometry must use each radial coordinate's scale, not an average world radius.");
                NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels
                    .Count(label => label.Axis == axis && label.Value.HasValue), Is.GreaterThanOrEqualTo(2),
                    "Every visible axis needs its own readable numerical scale.");
                for (var vertex = 0; vertex < 6; vertex++)
                    NUnit.Framework.Assert.That(axes[axis].ArrowVertices[vertex], Is.EqualTo(cones[axis].GetPoint(vertex)));
                NUnit.Framework.Assert.That(axes[axis].LabelAnchor, Is.EqualTo(cones[axis].GetLastPoint()));
            }
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Any(label => label.Axis == 0 &&
                label.Value.HasValue && label.Text.Contains("e-", StringComparison.OrdinalIgnoreCase)), Is.True);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Any(label => label.Axis == 1 &&
                label.Value.HasValue && label.Text.Contains("e", StringComparison.OrdinalIgnoreCase)), Is.True);
            AssertLabelsFit(chart.AxisLabels.Annotations, viewport.RenderSize);
        }

        [TestCase(800, 600)]
        [TestCase(1200, 400)]
        [TestCase(400, 900)]
        public void NumericMarksUseTheExistingWorldProjectionOnRectangularCharts(int width, int height)
        {
            var chart = Surface(width, height);
            var viewport = Viewport(chart);
            var axes = Data(chart).GetAxes();
            var annotations = chart.AxisLabels.Annotations;
            NUnit.Framework.Assert.That(annotations.TickMarks.Count(tick => tick.IsMajor), Is.GreaterThanOrEqualTo(4));
            NUnit.Framework.Assert.That(annotations.TickMarks.Any(tick => !tick.IsMajor), Is.True);
            NUnit.Framework.Assert.That(annotations.Labels.Count(label => label.Value.HasValue), Is.GreaterThanOrEqualTo(3));
            foreach (var tick in annotations.TickMarks)
            {
                var axis = axes[tick.Axis];
                // Independent orthographic viewport formula for the shipped
                // Width=2 camera, including its width-based vertical scale.
                var world = axis.At(tick.Value);
                var projected = chart.m_transformMatrix.m_totalMatrix.Transform(world);
                var expected = new Point(width / 2.0 + projected.X * width / 2,
                    height / 2.0 - projected.Y * width / 2);
                NUnit.Framework.Assert.That((tick.Position - expected).Length, Is.LessThan(0.00001));
                NUnit.Framework.Assert.That(tick.Value, Is.InRange(axis.Minimum, axis.Maximum));
                NUnit.Framework.Assert.That((tick.End - tick.Start).Length,
                    Is.EqualTo(tick.IsMajor ? 8 : 4).Within(0.00001));
                NUnit.Framework.Assert.That(tick.StrokeWidth, Is.EqualTo(tick.IsMajor ? 1.4 : 0.8));
            }
            AssertLabelsFit(annotations, viewport.RenderSize);
        }

        [TestCase(-5, 5, false)]
        [TestCase(1, 9, false)]
        [TestCase(-9, -1, false)]
        [TestCase(1e-16, 5e-16, true)]
        [TestCase(1e18, 5e18, true)]
        public void SignedAndScientificCoordinatesRemainFiniteAndReadable(double minimum, double maximum, bool scientific)
        {
            var chart = Surface(1000, 750, minimum, maximum);
            var numbers = chart.AxisLabels.Annotations.Labels.Where(label => label.Value.HasValue).ToArray();
            NUnit.Framework.Assert.That(numbers.Length, Is.GreaterThanOrEqualTo(4));
            NUnit.Framework.Assert.That(numbers.All(label => double.IsFinite(label.Value.Value)), Is.True);
            NUnit.Framework.Assert.That(numbers.All(label => !string.IsNullOrWhiteSpace(label.Text)), Is.True);
            if (scientific)
                NUnit.Framework.Assert.That(numbers.Any(label => label.Text.Contains("e", StringComparison.OrdinalIgnoreCase)), Is.True);
            if (maximum < 0) NUnit.Framework.Assert.That(numbers.All(label => label.Value < 0), Is.True);
            if (minimum > 0) NUnit.Framework.Assert.That(numbers.All(label => label.Value > 0), Is.True);
            AssertLabelsFit(chart.AxisLabels.Annotations, Viewport(chart).RenderSize);
        }

        [Test]
        public void NumericLabelsRespectTheCurrentDecimalCultureWithoutChangingCoordinates()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                var chart = Surface(1000, 750, -0.4, 0.6);
                var english = chart.AxisLabels.Annotations.TickMarks
                    .Select(tick => (tick.Axis, tick.Value, tick.Position)).ToArray();
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
                chart.RescaleProjectionMatrix();
                var polish = chart.AxisLabels.Annotations.TickMarks
                    .Select(tick => (tick.Axis, tick.Value, tick.Position)).ToArray();
                NUnit.Framework.Assert.That(polish, Is.EqualTo(english));
                NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels
                    .Any(label => label.Value.HasValue && label.Text.Contains(',')), Is.True);
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public void FlatSurfaceHasAFiniteProjectionAndOneConstantCoordinateLabel()
        {
            var chart = Surface(1200, 900, -5, 5, (x, y) => 0);
            var transformed = chart.m_transformMatrix.m_totalMatrix.Transform(new WorldPoint(1, 1, 0));
            NUnit.Framework.Assert.That(double.IsFinite(transformed.X) && double.IsFinite(transformed.Y) &&
                double.IsFinite(transformed.Z), Is.True);
            var constants = chart.AxisLabels.Annotations.Labels
                .Where(label => label.Axis == 2 && label.Value.HasValue).ToArray();
            NUnit.Framework.Assert.That(constants, Has.Length.EqualTo(1));
            NUnit.Framework.Assert.That(constants[0].Value, Is.Zero);
            NUnit.Framework.Assert.That(constants[0].Text, Is.EqualTo("0"));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks.All(tick => tick.Axis != 2), Is.True,
                "A collapsed coordinate must not invent intermediate subdivisions.");
            AssertLabelsFit(chart.AxisLabels.Annotations, Viewport(chart).RenderSize);
        }

        [Test]
        public void ScatterPointsGetTheSameNumericAxesAndClearRemovesTheAnnotations()
        {
            var chart = EmptyChart(900, 700);
            chart.Mode = Chart3DMode.Points;
            chart.Redraw();
            chart.AddPoints(new[]
            {
                new DataPoint(-3, -2, -1), new DataPoint(3, 2, 1),
                new DataPoint(-2, 1, 3), new DataPoint(1, -3, -2)
            });
            Rotate(chart);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks.Any(tick => tick.IsMajor), Is.True);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Any(label => label.Value.HasValue), Is.True);

            chart.AxisLabels.ActiveLabels = false;
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks.Count, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels, Is.Empty);
            chart.AxisLabels.ActiveLabels = true;
            chart.VisibilityAxes = false;
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks, Is.Empty);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Where(label => label.Value.HasValue), Is.Empty);
            chart.VisibilityAxes = true;
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks.Count, Is.GreaterThan(0));
            chart.ClearAll();
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks, Is.Empty);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels, Is.Empty);
        }

        [Test]
        public void ZoomPanAndResizeRegenerateOnlyVisibleNonoverlappingMarkers()
        {
            var chart = Surface(900, 700);
            var before = chart.AxisLabels.Annotations.TickMarks.Where(tick => tick.IsMajor)
                .Select(tick => (tick.Axis, tick.Value, tick.Position)).ToArray();
            for (var index = 0; index < 3; index++)
                chart.OnViewportMouseWheel(chart, new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120));
            var viewport = Viewport(chart);
            chart.m_transformMatrix.OnRBtnDown(new Point());
            chart.m_transformMatrix.OnMouseMove(new Point(45, -25), viewport);
            chart.m_transformMatrix.OnRBtnUp();
            chart.RescaleProjectionMatrix();
            Arrange(chart, 760, 620);
            var after = chart.AxisLabels.Annotations.TickMarks.Where(tick => tick.IsMajor)
                .Select(tick => (tick.Axis, tick.Value, tick.Position)).ToArray();
            NUnit.Framework.Assert.That(after, Is.Not.EqualTo(before));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks.Count, Is.InRange(1, 300));
            foreach (var tick in chart.AxisLabels.Annotations.TickMarks)
                NUnit.Framework.Assert.That(new Rect(0, 0, 760, 620).Contains(tick.Position), Is.True);
            AssertLabelsFit(chart.AxisLabels.Annotations, Viewport(chart).RenderSize);
        }

        [Test]
        public void NumbersPaintActualWpfGlyphsAndScaleOnceWithBitmapDpi()
        {
            var chart = Surface(800, 600);
            var annotation = chart.AxisLabels.Annotations;
            var numeric = annotation.Labels.Where(label => label.Value.HasValue).ToArray();
            NUnit.Framework.Assert.That(numeric.Length, Is.GreaterThanOrEqualTo(3));
            var normal = RenderVisual(annotation, 800, 600, 96);
            var high = RenderVisual(annotation, 1600, 1200, 192);
            foreach (var label in numeric)
            {
                var ordinaryInk = CountOpaque(normal, label.Bounds, 1);
                var scaledInk = CountOpaque(high, label.Bounds, 2);
                NUnit.Framework.Assert.That(ordinaryInk, Is.GreaterThan(5), label.Text);
                NUnit.Framework.Assert.That(CountDark(normal, label.Bounds), Is.GreaterThan(5),
                    "The configured black text must remain visible inside its contrasting halo: " + label.Text);
                NUnit.Framework.Assert.That(scaledInk / (double)ordinaryInk, Is.InRange(2.5, 5.5), label.Text);
            }
        }

        [Test]
        public void RequestedSizeExportsPreserveTheLiveSceneMatrixAndAnnotations()
        {
            var chart = Surface(800, 600);
            var dimensions = chart.RenderSize;
            var matrix = chart.m_transformMatrix.m_totalMatrix;
            var annotations = chart.AxisLabels.Annotations;
            var labels = annotations.Labels.Select(label => (label.Text, label.Bounds)).ToArray();
            var ticks = annotations.TickMarks.Select(tick => (tick.Value, tick.Position)).ToArray();
            var resizes = 0;
            chart.SizeChanged += (sender, args) => resizes++;
            var first = chart.RenderImage(513, 287);
            var second = chart.RenderImage(513, 287);
            using var decoded = chart.GetImage(513, 287);
            NUnit.Framework.Assert.That(first.PixelWidth, Is.EqualTo(513));
            NUnit.Framework.Assert.That(first.PixelHeight, Is.EqualTo(287));
            NUnit.Framework.Assert.That(decoded.Width, Is.EqualTo(513));
            NUnit.Framework.Assert.That(decoded.Height, Is.EqualTo(287));
            NUnit.Framework.Assert.That(Digest(first), Is.EqualTo(Digest(second)));
            NUnit.Framework.Assert.That(CountNonwhite(first), Is.GreaterThan(500),
                "An opaque white background alone must not satisfy the export regression.");
            NUnit.Framework.Assert.That(chart.RenderSize, Is.EqualTo(dimensions));
            NUnit.Framework.Assert.That(resizes, Is.Zero);
            NUnit.Framework.Assert.That(chart.m_transformMatrix.m_totalMatrix, Is.EqualTo(matrix));
            NUnit.Framework.Assert.That(annotations.Labels.Select(label => (label.Text, label.Bounds)).ToArray(), Is.EqualTo(labels));
            NUnit.Framework.Assert.That(annotations.TickMarks.Select(tick => (tick.Value, tick.Position)).ToArray(), Is.EqualTo(ticks));
            NUnit.Framework.Assert.That(() => chart.GetImage(0, 100), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void RequestedSizeExportsContainNumericalInkWithoutAxisNames()
        {
            var chart = Surface(800, 600);
            var labels = chart.AxisLabels;
            var original = (labels.LabelX, labels.LabelY, labels.LabelZ, labels.ActiveLabels);
            try
            {
                labels.LabelX = labels.LabelY = labels.LabelZ = string.Empty;
                NUnit.Framework.Assert.That(labels.Annotations.Labels.All(label => label.Value.HasValue), Is.True);
                var withNumbers = chart.RenderImage(513, 287, 96, out var exportedLabels);
                NUnit.Framework.Assert.That(exportedLabels.Count(label => label.Value.HasValue), Is.GreaterThanOrEqualTo(3));
                NUnit.Framework.Assert.That(exportedLabels.All(label => label.Value.HasValue), Is.True);
                labels.ActiveLabels = false;
                var withoutNumbers = chart.RenderImage(513, 287);
                var before = Pixels(withNumbers);
                var after = Pixels(withoutNumbers);
                var numericalInk = 0;
                for (var offset = 0; offset < before.Length; offset += 4)
                    if (before[offset] < 80 && before[offset + 1] < 80 && before[offset + 2] < 80 &&
                        (after[offset] - before[offset] > 48 || after[offset + 1] - before[offset + 1] > 48 ||
                         after[offset + 2] - before[offset + 2] > 48)) numericalInk++;
                NUnit.Framework.Assert.That(numericalInk, Is.GreaterThan(50),
                    "Requested-size export must paint black numerical glyphs; blue tick marks and blank names cannot satisfy this check.");
            }
            finally
            {
                labels.LabelX = original.LabelX;
                labels.LabelY = original.LabelY;
                labels.LabelZ = original.LabelZ;
                labels.ActiveLabels = original.ActiveLabels;
            }
        }

        [Test]
        public void KeepNativeThreeDimensionalRenderingEvidence()
        {
            var destination = Path.Combine(RepositoryRoot(), "TestResults", "chart-axis-rendering", "wpf-3d");
            Directory.CreateDirectory(destination);
            var cases = new[]
            {
                new VisualCase("default-settings", 800, 600, 96, -5, 5, true),
                new VisualCase("surface", 800, 600, 96, -5, 5),
                new VisualCase("wide", 1200, 400, 96, -5, 5),
                new VisualCase("tall", 400, 900, 96, -5, 5),
                new VisualCase("positive", 800, 600, 96, 1, 9),
                new VisualCase("negative", 800, 600, 96, -9, -1),
                new VisualCase("small-values", 1000, 750, 96, 1e-16, 5e-16),
                new VisualCase("large-values", 1000, 750, 96, 1e18, 5e18),
                new VisualCase("mixed-ranges-normalized", 1000, 750, 96, -2e-6, 2e-6, false, -2e6, 2e6, true),
                new VisualCase("mixed-ranges-average", 1000, 750, 96, -2e-6, 2e-6, false, -2e6, 2e6),
                new VisualCase("dpi-144", 1200, 900, 144, -5, 5),
                new VisualCase("dpi-192", 1600, 1200, 192, -5, 5)
            };
            var images = new List<object>();
            foreach (var item in cases)
            {
                var chart = item.DefaultSettings ? DefaultSurface() : item.YMaximum.HasValue ?
                    MixedSurface(item.Width * 96 / item.Dpi, item.Height * 96 / item.Dpi,
                        item.Maximum / 2, item.YMaximum.Value / 2, item.NormalizeCoordinates) :
                    Surface(item.Width * 96 / item.Dpi, item.Height * 96 / item.Dpi, item.Minimum, item.Maximum);
                var bitmap = chart.RenderImage(item.Width, item.Height, item.Dpi, out var exportedLabels);
                var path = Path.Combine(destination, item.Name + ".png");
                Save(bitmap, path);
                images.Add(new
                {
                    item.Name, item.Width, item.Height, item.Dpi, item.Minimum, item.Maximum, item.DefaultSettings,
                    yMinimum = item.YMinimum ?? item.Minimum, yMaximum = item.YMaximum ?? item.Maximum,
                    function = item.NormalizeCoordinates ? "x / xUnit + y / yUnit" : "(x + y) / 2",
                    xUnit = item.NormalizeCoordinates ? item.Maximum / 2 : (double?)null,
                    yUnit = item.NormalizeCoordinates ? item.YMaximum / 2 : null,
                    chart.Scale, chart.EqualAxes, labelColor = chart.AxisLabels.Color.ToString(),
                    sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                    labels = exportedLabels.Select(label => new
                    {
                        label.Axis, label.Value, label.Text,
                        x = label.Bounds.X, y = label.Bounds.Y, width = label.Bounds.Width, height = label.Bounds.Height
                    }).ToArray()
                });
                TestContext.AddTestAttachment(path, "Production WPF 3D scene and numerical axis annotations");
            }
            var manifest = Path.Combine(destination, "manifest.json");
            File.WriteAllText(manifest, JsonSerializer.Serialize(new
            {
                testedCommit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local working tree",
                renderer = "Production WPF mesh, TransformMatrix and measured glyph geometry",
                dpiScope = "RenderTargetBitmap DPI; does not simulate a physical monitor transition",
                culture = CultureInfo.CurrentCulture.Name,
                fontFamily = "Arial", fontSizeDips = 22, tickFontSizeDips = 13.2,
                sourceHashes = new[]
                {
                    "Computator.NET.Charting/Axes/AxisTickGenerator.cs",
                    "Computator.NET.Charting/Chart3D/Chart3D/Chart3DAxis.cs",
                    "Computator.NET.Charting/Chart3D/Chart3D/Chart3DAxisAnnotations.cs",
                    "Computator.NET.Charting/Chart3D/Chart3D/AxisLabels.cs",
                    "Computator.NET.Charting/Chart3D/Chart3D/Chart3D.cs",
                    "Computator.NET.Charting/Chart3D/Chart3D/TransformMatrix.cs",
                    "Computator.NET.Charting/Chart3D/UI/Chart3DControl.xaml.cs",
                    "Computator.NET.Desktop.Tests/Chart3DAxisRenderingTests.cs"
                }.Select(path => new
                {
                    path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryRoot(), path))))
                }).ToArray(), images
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddTestAttachment(manifest, "Dimensions, DPI, values, label bounds and image hashes");
        }

        private static Chart3DControl Surface(int width, int height, double minimum = -5, double maximum = 5,
            Func<double, double, double> function = null)
        {
            var chart = EmptyChart(width, height);
            chart.SetChartAreaValues(minimum, maximum, minimum, maximum);
            chart.AddFunction(new Function(function ?? ((x, y) => (x + y) / 2), FunctionType.Real3D));
            Rotate(chart);
            return chart;
        }

        private static Chart3DControl DefaultSurface()
        {
            // Keep production Scale=0.5, EqualAxes=true, and Goldenrod labels.
            // Quality changes only the surface sample count, not its axes.
            var chart = new Chart3DControl { Quality = 0, Background = Brushes.White };
            Arrange(chart, 800, 600);
            chart.AddFunction(new Function(new Func<double, double, double>((x, y) => (x + y) / 2), FunctionType.Real3D));
            Rotate(chart);
            return chart;
        }

        private static Chart3DControl MixedSurface(int width, int height, double xUnit, double yUnit, bool normalizeCoordinates)
        {
            var chart = EmptyChart(width, height);
            chart.SetChartAreaValues(-2 * xUnit, 2 * xUnit, -2 * yUnit, 2 * yUnit);
            chart.AddFunction(new Function(new Func<double, double, double>((x, y) =>
                normalizeCoordinates ? x / xUnit + y / yUnit : (x + y) / 2), FunctionType.Real3D));
            Rotate(chart);
            return chart;
        }

        private static Chart3DControl EmptyChart(int width, int height)
        {
            var chart = new Chart3DControl
            {
                Quality = 0, EqualAxes = false,
                Scale = 0.45 * Math.Min(1, height / (double)width), Background = Brushes.White
            };
            chart.AxisLabels.Color = Colors.Black;
            Arrange(chart, width, height);
            return chart;
        }

        private static void Arrange(Chart3DControl chart, int width, int height)
        {
            chart.Width = width;
            chart.Height = height;
            chart.Measure(new Size(width, height));
            chart.Arrange(new Rect(0, 0, width, height));
            chart.UpdateLayout();
            chart.RescaleProjectionMatrix();
            chart.UpdateLayout();
        }

        private static void Rotate(Chart3DControl chart)
        {
            var viewport = Viewport(chart);
            chart.m_transformMatrix.OnLBtnDown(new Point());
            chart.m_transformMatrix.OnMouseMove(new Point(viewport.ActualWidth * 0.2, -viewport.ActualHeight * 0.1), viewport);
            chart.m_transformMatrix.OnLBtnUp();
            chart.RescaleProjectionMatrix();
            chart.UpdateLayout();
        }

        private static Viewport3D Viewport(Chart3DControl chart) => (Viewport3D)chart.FindName("mainViewport");
        private static ChartData Data(Chart3DControl chart) =>
            (ChartData)typeof(Chart3DControl).GetField("m_3dChart", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(chart);

        private static void AssertLabelsFit(Chart3DAxisAnnotations annotations, Size size)
        {
            var bounds = new Rect(0, 0, size.Width, size.Height);
            for (var first = 0; first < annotations.Labels.Count; first++)
            {
                var label = annotations.Labels[first];
                NUnit.Framework.Assert.That(bounds.Contains(label.Bounds), Is.True, label.Text);
                for (var second = first + 1; second < annotations.Labels.Count; second++)
                    NUnit.Framework.Assert.That(label.Bounds.IntersectsWith(annotations.Labels[second].Bounds), Is.False,
                        label.Text + " / " + annotations.Labels[second].Text);
            }
        }

        private static RenderTargetBitmap RenderVisual(Visual visual, int width, int height, double dpi)
        {
            var result = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            result.Render(visual);
            return result;
        }

        private static byte[] Pixels(BitmapSource bitmap)
        {
            var bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(bytes, bitmap.PixelWidth * 4, 0);
            return bytes;
        }

        private static int CountOpaque(BitmapSource bitmap, Rect rectangle, double scale)
        {
            var bytes = Pixels(bitmap);
            var count = 0;
            for (var y = Math.Max(0, (int)Math.Floor(rectangle.Top * scale));
                y < Math.Min(bitmap.PixelHeight, Math.Ceiling(rectangle.Bottom * scale)); y++)
                for (var x = Math.Max(0, (int)Math.Floor(rectangle.Left * scale));
                    x < Math.Min(bitmap.PixelWidth, Math.Ceiling(rectangle.Right * scale)); x++)
                    if (bytes[(y * bitmap.PixelWidth + x) * 4 + 3] > 127) count++;
            return count;
        }

        private static string Digest(BitmapSource bitmap) => Convert.ToHexString(SHA256.HashData(Pixels(bitmap)));

        private static int CountDark(BitmapSource bitmap, Rect rectangle)
        {
            var bytes = Pixels(bitmap);
            var count = 0;
            for (var y = Math.Max(0, (int)Math.Floor(rectangle.Top));
                y < Math.Min(bitmap.PixelHeight, Math.Ceiling(rectangle.Bottom)); y++)
                for (var x = Math.Max(0, (int)Math.Floor(rectangle.Left));
                    x < Math.Min(bitmap.PixelWidth, Math.Ceiling(rectangle.Right)); x++)
                {
                    var offset = (y * bitmap.PixelWidth + x) * 4;
                    if (bytes[offset + 3] > 127 && bytes[offset] < 80 &&
                        bytes[offset + 1] < 80 && bytes[offset + 2] < 80) count++;
                }
            return count;
        }

        private static int CountNonwhite(BitmapSource bitmap)
        {
            var pixels = Pixels(bitmap);
            var count = 0;
            for (var offset = 0; offset < pixels.Length; offset += 4)
                if (pixels[offset + 3] > 127 &&
                    (pixels[offset] < 230 || pixels[offset + 1] < 230 || pixels[offset + 2] < 230)) count++;
            return count;
        }

        private static void Save(BitmapSource bitmap, string path)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }

        private static string RepositoryRoot()
        {
            for (var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                directory != null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Computator.NET.slnx"))) return directory.FullName;
            throw new InvalidOperationException("Repository root was not found for rendering attachments.");
        }

        private sealed record VisualCase(string Name, int Width, int Height, int Dpi, double Minimum, double Maximum,
            bool DefaultSettings = false, double? YMinimum = null, double? YMaximum = null,
            bool NormalizeCoordinates = false);
    }
}
