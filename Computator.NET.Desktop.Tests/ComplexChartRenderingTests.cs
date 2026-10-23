using System;
using System.Drawing;
using System.Linq;
using Computator.NET.Charting.ComplexCharting;
using NUnit.Framework;
#if WINDOWS
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.DataTypes.Functions;
#endif

namespace Computator.NET.Desktop.Tests
{
    // The same geometry cases are also linked into the portable test harness.
    // No System.Drawing.Common, Windows controls or alternate layout algorithm
    // are used by this fixture.
    [TestFixture]
    public class ComplexAxesGeometryTests
    {
        [TestCase(-2, 2, -2, 2, 400, 300)]
        [TestCase(-1, 3, -3, 1, 200, 150)]
        [TestCase(1, 3, 1, 3, 0, 599)]
        [TestCase(-3, -1, -3, -1, 799, 0)]
        [TestCase(0, 2, -2, 0, 0, 0)]
        [TestCase(-2, 0, 0, 2, 799, 599)]
        public void OriginKeepsTheWorldToPixelMapping(double xMin, double xMax,
            double yMin, double yMax, float expectedX, float expectedY)
        {
            var layout = Create(800, 600, 96, xMin, xMax, yMin, yMax);
            NUnit.Framework.Assert.That(layout, Is.Not.Null);
            NUnit.Framework.Assert.That(layout.Origin.X, Is.EqualTo(expectedX).Within(0.01));
            NUnit.Framework.Assert.That(layout.Origin.Y, Is.EqualTo(expectedY).Within(0.01));
            NUnit.Framework.Assert.That(layout.XStart.Y, Is.EqualTo(layout.Origin.Y));
            NUnit.Framework.Assert.That(layout.XEnd.Y, Is.EqualTo(layout.Origin.Y));
            NUnit.Framework.Assert.That(layout.YStart.X, Is.EqualTo(layout.Origin.X));
            NUnit.Framework.Assert.That(layout.YEnd.X, Is.EqualTo(layout.Origin.X));
        }

        [TestCase(-2, 2, -2, 2)]
        [TestCase(-0.001, 2, -2, 0.001)]
        [TestCase(-2, 0.001, -0.001, 2)]
        [TestCase(1, 3, 1, 3)]
        [TestCase(-3, -1, -3, -1)]
        public void MovingTheOriginDoesNotDistortArrowDimensions(double xMin, double xMax,
            double yMin, double yMax)
        {
            var layout = Create(800, 600, 96, xMin, xMax, yMin, yMax);
            AssertArrow(layout.XArrow, layout.Bounds, layout.ArrowLength, layout.ArrowHalfWidth * 2);
            AssertArrow(layout.YArrow, layout.Bounds, layout.ArrowHalfWidth * 2, layout.ArrowLength);
        }

        [TestCase(144, 1200, 900)]
        [TestCase(192, 1600, 1200)]
        public void EquivalentLogicalChartsKeepPhysicalProportions(int dpi, int width, int height)
        {
            var reference = Create(800, 600, 96);
            var scaled = Create(width, height, dpi);
            var ratio = dpi / 96f;
            NUnit.Framework.Assert.That(scaled.FontScale, Is.EqualTo(reference.FontScale).Within(0.001));
            NUnit.Framework.Assert.That(scaled.StrokeWidth / ratio,
                Is.EqualTo(reference.StrokeWidth).Within(0.01));
            NUnit.Framework.Assert.That(scaled.ArrowLength / ratio,
                Is.EqualTo(reference.ArrowLength).Within(0.01));
            NUnit.Framework.Assert.That(scaled.ArrowHalfWidth / ratio,
                Is.EqualTo(reference.ArrowHalfWidth).Within(0.01));
            NUnit.Framework.Assert.That(scaled.LabelGap / ratio,
                Is.EqualTo(reference.LabelGap).Within(0.01));
        }

        [Test]
        public void ChartSizeAndAspectRatioProduceConsistentProportions()
        {
            var small = Create(320, 240, 96);
            var medium = Create(800, 600, 96);
            var large = Create(1600, 1200, 96);
            NUnit.Framework.Assert.That(medium.StrokeWidth, Is.GreaterThanOrEqualTo(small.StrokeWidth));
            NUnit.Framework.Assert.That(large.StrokeWidth, Is.GreaterThanOrEqualTo(medium.StrokeWidth));
            NUnit.Framework.Assert.That(medium.FontScale, Is.GreaterThan(small.FontScale));
            NUnit.Framework.Assert.That(large.FontScale, Is.GreaterThanOrEqualTo(medium.FontScale));

            var wide = Create(1200, 300, 96);
            var tall = Create(300, 1200, 96);
            NUnit.Framework.Assert.That(tall.ArrowLength, Is.EqualTo(wide.ArrowLength));
            NUnit.Framework.Assert.That(tall.ArrowHalfWidth, Is.EqualTo(wide.ArrowHalfWidth));
            NUnit.Framework.Assert.That(tall.StrokeWidth, Is.EqualTo(wide.StrokeWidth));
            NUnit.Framework.Assert.That(tall.FontScale, Is.EqualTo(wide.FontScale));
        }

        [TestCase(-2, 2, -2, 2)]
        [TestCase(-0.001, 2, -2, 0.001)]
        [TestCase(-2, 0.001, -0.001, 2)]
        [TestCase(0, 2, 0, 2)]
        [TestCase(-2, 0, -2, 0)]
        [TestCase(1, 3, 1, 3)]
        [TestCase(-3, -1, -3, -1)]
        public void MeasuredLabelsRemainVisibleAndSeparateNearEveryBoundary(double xMin,
            double xMax, double yMin, double yMax)
        {
            var layout = Create(320, 240, 96, xMin, xMax, yMin, yMax);
            layout.PlaceLabels(new SizeF(65, 24), new SizeF(61, 24));
            AssertContained(layout.SafeBounds, layout.XLabelBounds);
            AssertContained(layout.SafeBounds, layout.YLabelBounds);
            NUnit.Framework.Assert.That(layout.XLabelBounds.IntersectsWith(layout.YLabelBounds), Is.False);
        }

        [Test]
        public void BoundsOffsetMovesEveryGeometricElementTogether()
        {
            var reference = Create(800, 600, 96);
            var shifted = ComplexAxesLayout.Create(new RectangleF(37, 51, 800, 600),
                96, 96, -2, 2, -2, 2, 0.02);
            NUnit.Framework.Assert.That(shifted.Origin.X, Is.EqualTo(reference.Origin.X + 37));
            NUnit.Framework.Assert.That(shifted.Origin.Y, Is.EqualTo(reference.Origin.Y + 51));
            for (var index = 0; index < 3; index++)
            {
                NUnit.Framework.Assert.That(shifted.XArrow[index].X,
                    Is.EqualTo(reference.XArrow[index].X + 37).Within(0.01));
                NUnit.Framework.Assert.That(shifted.XArrow[index].Y,
                    Is.EqualTo(reference.XArrow[index].Y + 51).Within(0.01));
                NUnit.Framework.Assert.That(shifted.YArrow[index].X,
                    Is.EqualTo(reference.YArrow[index].X + 37).Within(0.01));
                NUnit.Framework.Assert.That(shifted.YArrow[index].Y,
                    Is.EqualTo(reference.YArrow[index].Y + 51).Within(0.01));
            }
        }

        [TestCase(-2, 0)]
        [TestCase(0, 2)]
        public void BoundaryXAxisLabelStaysBesideItsOwnAxis(double yMin, double yMax)
        {
            var layout = Create(800, 600, 96, -2, 2, yMin, yMax);
            layout.PlaceLabels(new SizeF(65, 24), new SizeF(61, 24));
            var distance = Math.Abs(layout.XLabelBounds.Top + layout.XLabelBounds.Height / 2 - layout.Origin.Y);
            NUnit.Framework.Assert.That(distance, Is.LessThan(100),
                "A boundary label must not jump to the opposite edge merely to clear an arrowhead");
        }

        [Test]
        public void LabelsNearTheTopRightCornerStaySeparateAndNearThatCorner()
        {
            var layout = Create(800, 600, 96, -2, 0.001, -2, 0.001);
            layout.PlaceLabels(new SizeF(65, 24), new SizeF(61, 24));
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.XLabelBounds);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.YLabelBounds);
            NUnit.Framework.Assert.That(layout.XLabelBounds.IntersectsWith(layout.YLabelBounds), Is.False);
            NUnit.Framework.Assert.That(layout.XLabelBounds.Bottom, Is.LessThan(150));
            NUnit.Framework.Assert.That(layout.YLabelBounds.Bottom, Is.LessThan(150));
        }

        [Test]
        public void FiniteExtremeDomainsKeepTheirStableMathematicalMidpoint()
        {
            var layout = Create(800, 600, 96, -double.MaxValue, double.MaxValue,
                -double.MaxValue, double.MaxValue);
            NUnit.Framework.Assert.That(layout, Is.Not.Null);
            NUnit.Framework.Assert.That(layout.Origin, Is.EqualTo(new PointF(400, 300)));
        }

        [TestCase(-5, 5, -5, 5)]
        [TestCase(-1, 99, -99, 1)]
        [TestCase(1, 9, 1, 9)]
        [TestCase(-9, -1, -9, -1)]
        public void RelativeArrowSizesRemainLinearWithoutMovingTheAxes(double xMin, double xMax,
            double yMin, double yMax)
        {
            var layouts = new[] { 0, 0.005, 0.01, 0.02, 0.04 }.Select(relativeSize =>
                ComplexAxesLayout.Create(new RectangleF(0, 0, 640, 480), 96, 96,
                    xMin, xMax, yMin, yMax, relativeSize)).ToArray();
            NUnit.Framework.Assert.That(layouts[0].ArrowLength, Is.Zero);
            for (var index = 2; index < layouts.Length; index++)
                NUnit.Framework.Assert.That(layouts[index].ArrowLength,
                    Is.EqualTo(2 * layouts[index - 1].ArrowLength).Within(0.0001));
            foreach (var layout in layouts)
            {
                NUnit.Framework.Assert.That(layout.Origin, Is.EqualTo(layouts[0].Origin));
                if (layout.ArrowLength > 0)
                {
                    AssertArrow(layout.XArrow, layout.Bounds, layout.ArrowLength, layout.ArrowHalfWidth * 2);
                    AssertArrow(layout.YArrow, layout.Bounds, layout.ArrowHalfWidth * 2, layout.ArrowLength);
                }
            }
        }

        [TestCase(-1, 99, -99, 1, 1, 1)]
        [TestCase(-99, 1, -1, 99, 99, 99)]
        [TestCase(-0.01, 99.99, -99.99, 0.01, 0.01, 0.01)]
        [TestCase(-99.99, 0.01, -0.01, 99.99, 99.99, 99.99)]
        public void AnInRangeZeroInsideTheLastPixelIsNotMovedToThePixelCenter(double xMin,
            double xMax, double yMin, double yMax, double expectedX, double expectedY)
        {
            var layout = Create(100, 100, 96, xMin, xMax, yMin, yMax);
            NUnit.Framework.Assert.That(layout.Origin.X, Is.EqualTo(expectedX).Within(0.0001));
            NUnit.Framework.Assert.That(layout.Origin.Y, Is.EqualTo(expectedY).Within(0.0001));
            AssertArrow(layout.XArrow, layout.Bounds, layout.ArrowLength, layout.ArrowHalfWidth * 2);
            AssertArrow(layout.YArrow, layout.Bounds, layout.ArrowHalfWidth * 2, layout.ArrowLength);
        }

        [TestCase(-1e-200, 1e200, -1e200, 1e-200, 0, 0)]
        [TestCase(-1e200, 1e-200, -1e-200, 1e200, 799, 599)]
        public void ExtremelyAsymmetricFiniteRangesRemainFinite(double xMin, double xMax,
            double yMin, double yMax, float expectedX, float expectedY)
        {
            var layout = Create(800, 600, 96, xMin, xMax, yMin, yMax);
            NUnit.Framework.Assert.That(layout.Origin, Is.EqualTo(new PointF(expectedX, expectedY)));
            AssertArrow(layout.XArrow, layout.Bounds, layout.ArrowLength, layout.ArrowHalfWidth * 2);
            AssertArrow(layout.YArrow, layout.Bounds, layout.ArrowHalfWidth * 2, layout.ArrowLength);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidDpiUsesTheStandardResolutionInsteadOfInvalidGeometry(float dpi)
        {
            var baseline = Create(640, 480, 96);
            var actual = Create(640, 480, dpi);
            NUnit.Framework.Assert.That(actual.DpiScale, Is.EqualTo(baseline.DpiScale));
            NUnit.Framework.Assert.That(actual.FontScale, Is.EqualTo(baseline.FontScale));
            NUnit.Framework.Assert.That(actual.StrokeWidth, Is.EqualTo(baseline.StrokeWidth));
            NUnit.Framework.Assert.That(actual.ArrowLength, Is.EqualTo(baseline.ArrowLength));
        }

        [TestCase(-1)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void InvalidArrowSettingsUseTheDefaultVisibleHead(double relativeSize)
        {
            var baseline = Create(640, 480, 96);
            var actual = ComplexAxesLayout.Create(new RectangleF(0, 0, 640, 480),
                96, 96, -2, 2, -2, 2, relativeSize);
            NUnit.Framework.Assert.That(actual.ArrowLength, Is.EqualTo(baseline.ArrowLength));
            NUnit.Framework.Assert.That(actual.XArrow, Is.EqualTo(baseline.XArrow));
            NUnit.Framework.Assert.That(actual.YArrow, Is.EqualTo(baseline.YArrow));
        }

        [Test]
        public void ExcessiveArrowSettingsStayInsideTheAvailableTarget()
        {
            var layout = ComplexAxesLayout.Create(new RectangleF(0, 0, 120, 60),
                192, 192, 1, 2, -2, -1, 100);
            NUnit.Framework.Assert.That(layout.ArrowLength, Is.GreaterThan(0).And.LessThanOrEqualTo(15));
            AssertArrow(layout.XArrow, layout.Bounds, layout.ArrowLength, layout.ArrowHalfWidth * 2);
            AssertArrow(layout.YArrow, layout.Bounds, layout.ArrowHalfWidth * 2, layout.ArrowLength);
        }

        [TestCase(0, 600, -2, 2, -2, 2)]
        [TestCase(800, 0, -2, 2, -2, 2)]
        [TestCase(800, 600, 0, 0, -2, 2)]
        [TestCase(800, 600, -2, 2, 2, 2)]
        [TestCase(800, 600, 2, -2, -2, 2)]
        [TestCase(800, 600, -2, 2, 2, -2)]
        [TestCase(800, 600, double.NaN, 2, -2, 2)]
        [TestCase(800, 600, -2, double.PositiveInfinity, -2, 2)]
        public void EmptyOrInvalidDomainsDoNotInventAnAxis(int width, int height,
            double xMin, double xMax, double yMin, double yMax)
        {
            NUnit.Framework.Assert.That(Create(width, height, 96, xMin, xMax, yMin, yMax), Is.Null);
        }

        private static ComplexAxesLayout Create(int width, int height, float dpi,
            double xMin = -2, double xMax = 2, double yMin = -2, double yMax = 2)
        {
            return ComplexAxesLayout.Create(new RectangleF(0, 0, width, height),
                dpi, dpi, xMin, xMax, yMin, yMax, 0.02);
        }

        private static void AssertArrow(PointF[] arrow, RectangleF bounds, float width, float height)
        {
            NUnit.Framework.Assert.That(arrow, Has.Length.EqualTo(3));
            foreach (var point in arrow)
            {
                NUnit.Framework.Assert.That(point.X, Is.InRange(bounds.Left, bounds.Right));
                NUnit.Framework.Assert.That(point.Y, Is.InRange(bounds.Top, bounds.Bottom));
            }
            NUnit.Framework.Assert.That(arrow.Max(p => p.X) - arrow.Min(p => p.X),
                Is.EqualTo(width).Within(0.01));
            NUnit.Framework.Assert.That(arrow.Max(p => p.Y) - arrow.Min(p => p.Y),
                Is.EqualTo(height).Within(0.01));
        }

        internal static void AssertContained(RectangleF outer, RectangleF inner)
        {
            NUnit.Framework.Assert.That(inner.Width, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(inner.Height, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(inner.Left, Is.GreaterThanOrEqualTo(outer.Left - 0.01));
            NUnit.Framework.Assert.That(inner.Top, Is.GreaterThanOrEqualTo(outer.Top - 0.01));
            NUnit.Framework.Assert.That(inner.Right, Is.LessThanOrEqualTo(outer.Right + 0.01));
            NUnit.Framework.Assert.That(inner.Bottom, Is.LessThanOrEqualTo(outer.Bottom + 0.01));
        }
    }

#if WINDOWS
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ComplexChartRenderingTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [TestCase(320, 240, 96)]
        [TestCase(800, 600, 96)]
        [TestCase(1600, 1200, 96)]
        [TestCase(1200, 400, 96)]
        [TestCase(400, 1000, 96)]
        [TestCase(1200, 900, 144)]
        [TestCase(1600, 1200, 192)]
        [TestCase(320, 240, 192)]
        public void RealGdiTextFitsMeasuredBoundsAndDrawsBothCoordinateLabels(int width, int height, int dpi)
        {
            using var bitmap = NewBitmap(width, height, dpi);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            using var font = new Font("Cambria", 13, FontStyle.Regular, GraphicsUnit.Point);
            var layout = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, width, height),
                -2, 2, -2, 2, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            NUnit.Framework.Assert.That(layout, Is.Not.Null);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.XLabelBounds);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.YLabelBounds);
            NUnit.Framework.Assert.That(layout.XLabelBounds.IntersectsWith(layout.YLabelBounds), Is.False);
            NUnit.Framework.Assert.That(CountBlueInk(bitmap, layout.XLabelBounds), Is.GreaterThan(10));
            NUnit.Framework.Assert.That(CountBlueInk(bitmap, layout.YLabelBounds), Is.GreaterThan(10));
        }

        [TestCase(144, 1200, 900)]
        [TestCase(192, 1600, 1200)]
        public void PointFontTextGetsExactlyOneDpiScale(int dpi, int width, int height)
        {
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            var baseline = RenderLayout(800, 600, 96, font);
            var scaled = RenderLayout(width, height, dpi, font);
            var ratio = dpi / 96f;
            NUnit.Framework.Assert.That(scaled.XLabelBounds.Width / ratio,
                Is.EqualTo(baseline.XLabelBounds.Width).Within(1));
            NUnit.Framework.Assert.That(scaled.XLabelBounds.Height / ratio,
                Is.EqualTo(baseline.XLabelBounds.Height).Within(1));
        }

        [Test]
        public void AnExplicitPixelFontKeepsItsDevicePixelUnitsAtHigherDpi()
        {
            using var font = new Font("Cambria", 18, FontStyle.Bold, GraphicsUnit.Pixel);
            var baseline = RenderLayout(640, 480, 96, font);
            var highDpi = RenderLayout(1280, 960, 192, font);
            NUnit.Framework.Assert.That(highDpi.XLabelBounds.Width,
                Is.EqualTo(baseline.XLabelBounds.Width).Within(0.5));
            NUnit.Framework.Assert.That(highDpi.XLabelBounds.Height,
                Is.EqualTo(baseline.XLabelBounds.Height).Within(0.5));
            NUnit.Framework.Assert.That(font.Unit, Is.EqualTo(GraphicsUnit.Pixel));
            NUnit.Framework.Assert.That(font.Size, Is.EqualTo(18));
        }

        [TestCase(GraphicsUnit.Point)]
        [TestCase(GraphicsUnit.Pixel)]
        public void UserFontsAndCustomLabelsRemainUsableAndAreNotMutatedOrDisposed(GraphicsUnit unit)
        {
            using var font = new Font("Georgia", 22, FontStyle.Bold | FontStyle.Italic, unit);
            var initialSize = font.Size;
            using var bitmap = NewBitmap(900, 600, 144);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            var layout = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, 900, 600),
                -0.001, 2, -2, 0.001, 0.02, "Real coordinate", "Imaginary coordinate", font,
                Color.Black, Color.Blue);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.XLabelBounds);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.YLabelBounds);
            NUnit.Framework.Assert.That(CountBlueInk(bitmap, layout.XLabelBounds), Is.GreaterThan(10));
            NUnit.Framework.Assert.That(CountBlueInk(bitmap, layout.YLabelBounds), Is.GreaterThan(10));
            NUnit.Framework.Assert.That(font.Size, Is.EqualTo(initialSize));
            NUnit.Framework.Assert.That(font.Unit, Is.EqualTo(unit));
            NUnit.Framework.Assert.That(font.Style, Is.EqualTo(FontStyle.Bold | FontStyle.Italic));
            NUnit.Framework.Assert.That(font.GetHeight(graphics), Is.GreaterThan(0));
        }

        [Test]
        public void OversizedCustomFontFitsBothLabelsWithoutOverlapOnASmallChart()
        {
            using var font = new Font("Cambria", 80, FontStyle.Regular, GraphicsUnit.Point);
            using var bitmap = NewBitmap(240, 160, 96);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            var layout = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, 240, 160),
                -2, 2, -2, 2, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.XLabelBounds);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.YLabelBounds);
            NUnit.Framework.Assert.That(layout.XLabelBounds.IntersectsWith(layout.YLabelBounds), Is.False);
            NUnit.Framework.Assert.That(CountBlueInk(bitmap, layout.XLabelBounds), Is.GreaterThan(10));
            NUnit.Framework.Assert.That(CountBlueInk(bitmap, layout.YLabelBounds), Is.GreaterThan(10));
            NUnit.Framework.Assert.That(font.SizeInPoints, Is.EqualTo(80));
        }

        [Test]
        public void EmptyCoordinateNamesAndDisabledArrowheadsRetainNumericalAxes()
        {
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            using var bitmap = NewBitmap(320, 240, 96);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            var layout = ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, 320, 240),
                -2, 2, -2, 2, 0, string.Empty, null, font, Color.Black, Color.Blue);
            NUnit.Framework.Assert.That(layout.ArrowLength, Is.Zero);
            NUnit.Framework.Assert.That(layout.XLabelBounds.IsEmpty, Is.True);
            NUnit.Framework.Assert.That(layout.YLabelBounds.IsEmpty, Is.True);
            NUnit.Framework.Assert.That(layout.Ticks, Is.Not.Null);
            var numericalLabels = layout.Ticks.MajorTicks.Where(mark => !mark.LabelBounds.IsEmpty).ToArray();
            NUnit.Framework.Assert.That(numericalLabels, Is.Not.Empty,
                "Clearing coordinate names must not also hide coordinate values");
            foreach (var mark in numericalLabels)
                NUnit.Framework.Assert.That(CountBlueInk(bitmap, mark.LabelBounds), Is.GreaterThan(2),
                    mark.Tick.Label);
            NUnit.Framework.Assert.That(bitmap.GetPixel(160, 30).R, Is.LessThan(128));
            NUnit.Framework.Assert.That(bitmap.GetPixel(30, 120).R, Is.LessThan(128));
        }

        [Test]
        public void VeryLongLabelsRemainInsideASmallOffsetTarget()
        {
            using var bitmap = NewBitmap(200, 120, 96);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            var target = new Rectangle(23, 17, 120, 60);
            var layout = ComplexAxesRenderer.Draw(graphics, target, -0.01, 99.99, -99.99, 0.01,
                0.02, new string('M', 80), new string('W', 80), font, Color.Black, Color.Blue);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.XLabelBounds);
            ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.YLabelBounds);
            NUnit.Framework.Assert.That(layout.XLabelBounds.IntersectsWith(layout.YLabelBounds), Is.False);
            for (var y = 0; y < bitmap.Height; y++)
                for (var x = 0; x < bitmap.Width; x++)
                    if (!target.Contains(x, y))
                        NUnit.Framework.Assert.That(bitmap.GetPixel(x, y).ToArgb(),
                            Is.EqualTo(Color.White.ToArgb()), $"Ink outside target at ({x},{y})");
        }

        [Test]
        public void OffsetRenderingStaysInsideTheTargetAndRestoresGraphicsState()
        {
            using var bitmap = NewBitmap(420, 320, 144);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            var target = new Rectangle(37, 41, 340, 240);
            graphics.SetClip(new Rectangle(13, 11, 390, 300));
            graphics.TranslateTransform(7, 9);
            var initialClip = graphics.ClipBounds;
            ComplexAxesRenderer.Draw(graphics, target, -2, 2, -2, 2, 0.02,
                "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            NUnit.Framework.Assert.That(graphics.ClipBounds, Is.EqualTo(initialClip));
            var translatedTarget = new Rectangle(target.X + 7, target.Y + 9, target.Width, target.Height);
            for (var y = 0; y < bitmap.Height; y++)
                for (var x = 0; x < bitmap.Width; x++)
                    if (!translatedTarget.Contains(x, y))
                        NUnit.Framework.Assert.That(bitmap.GetPixel(x, y).ToArgb(),
                            Is.EqualTo(Color.White.ToArgb()), $"Ink outside target at ({x},{y})");

            graphics.ResetTransform();
            graphics.PageUnit = GraphicsUnit.Millimeter;
            graphics.PageScale = 1.25f;
            graphics.TranslateTransform(3, 5);
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            var originalTransform = graphics.Transform.Elements;
            var originalClip = graphics.ClipBounds;
            ComplexAxesRenderer.Draw(graphics, target, -2, 2, -2, 2, 0.02,
                "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
            NUnit.Framework.Assert.That(graphics.PageUnit, Is.EqualTo(GraphicsUnit.Millimeter));
            NUnit.Framework.Assert.That(graphics.PageScale, Is.EqualTo(1.25f));
            NUnit.Framework.Assert.That(graphics.Transform.Elements, Is.EqualTo(originalTransform));
            NUnit.Framework.Assert.That(graphics.ClipBounds, Is.EqualTo(originalClip));
            NUnit.Framework.Assert.That(graphics.SmoothingMode, Is.EqualTo(SmoothingMode.None));
            NUnit.Framework.Assert.That(graphics.TextRenderingHint, Is.EqualTo(TextRenderingHint.SingleBitPerPixel));
        }

        [Test]
        public void GetImageUsesRequestedDimensionsWithoutResizingOrReplacingLiveSamples()
        {
            using var chart = CreateChart(320, 240, 10);
            InitializeFunction(chart, z => z * z - 1);
            var cachedImage = PrivateField(chart, "image");
            var cachedPoints = PrivateField(chart, "_pointsValues");
            var before = ImageDigest((Bitmap)cachedImage);
            var resized = 0;
            chart.Resize += (_, _) => resized++;
            using var result = chart.GetImage(513, 287);
            NUnit.Framework.Assert.That(result.Size, Is.EqualTo(new Size(513, 287)));
            NUnit.Framework.Assert.That(chart.Size, Is.EqualTo(new Size(320, 240)));
            NUnit.Framework.Assert.That(resized, Is.Zero);
            NUnit.Framework.Assert.That(PrivateField(chart, "image"), Is.SameAs(cachedImage));
            NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.SameAs(cachedPoints));
            NUnit.Framework.Assert.That(ImageDigest((Bitmap)cachedImage), Is.EqualTo(before));
        }

        [TestCase(50)]
        [TestCase(25)]
        [TestCase(5)]
        public void SavingAxesDoesNotBurnThemIntoTheCachedPlot(double quality)
        {
            using var chart = CreateChart(320, 240, quality);
            InitializeFunction(chart, z => z * z - 1);
            var cachedImage = (Bitmap)PrivateField(chart, "image");
            var before = ImageDigest(cachedImage);
            var directory = Path.Combine(Path.GetTempPath(), "computator-chart-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                chart.ShouldDrawAxes = false;
                using var originalPlot = (Bitmap)chart.GetImage(chart.ClientSize.Width, chart.ClientSize.Height);
                chart.ShouldDrawAxes = true;
                var withAxes = Path.Combine(directory, "with-axes.png");
                chart.SaveImage(withAxes, ImageFormat.Png);
                NUnit.Framework.Assert.That(ImageDigest(cachedImage), Is.EqualTo(before));
                string firstSave;
                using (var firstSavedImage = new Bitmap(withAxes))
                    firstSave = ImageDigest(firstSavedImage);
                chart.SaveImage(withAxes, ImageFormat.Png);
                using (var repeatedAxes = new Bitmap(withAxes))
                    NUnit.Framework.Assert.That(ImageDigest(repeatedAxes), Is.EqualTo(firstSave));
                using var expectedAxes = (Bitmap)chart.GetImage(chart.ClientSize.Width, chart.ClientSize.Height);

                chart.ShouldDrawAxes = false;
                var withoutAxes = Path.Combine(directory, "without-axes.png");
                chart.SaveImage(withoutAxes, ImageFormat.Png);
                using var axes = new Bitmap(withAxes);
                using var plotOnly = new Bitmap(withoutAxes);
                using var repeatedPlot = (Bitmap)chart.GetImage(chart.ClientSize.Width, chart.ClientSize.Height);
                NUnit.Framework.Assert.That(axes.Size, Is.EqualTo(chart.ClientSize));
                NUnit.Framework.Assert.That(plotOnly.Size, Is.EqualTo(chart.ClientSize));
                NUnit.Framework.Assert.That(ImageDigest(plotOnly), Is.EqualTo(ImageDigest(originalPlot)));
                NUnit.Framework.Assert.That(ImageDigest(repeatedPlot), Is.EqualTo(ImageDigest(originalPlot)));
                NUnit.Framework.Assert.That(ImageDigest(axes), Is.EqualTo(ImageDigest(expectedAxes)));
                NUnit.Framework.Assert.That(ImageDigest(axes), Is.Not.EqualTo(ImageDigest(plotOnly)));
                NUnit.Framework.Assert.That(ImageDigest(cachedImage), Is.EqualTo(before));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestCase(640, 480)]
        [TestCase(1, 1)]
        public void EmptyChartsExportAtTheRequestedSizeWithTheirBackgroundColor(int width, int height)
        {
            using var chart = CreateChart(320, 240, 25);
            chart.BackColor = Color.CornflowerBlue;
            chart.ShouldDrawAxes = false;
            var cachedImage = PrivateField(chart, "image");
            using var exported = (Bitmap)chart.GetImage(width, height);
            NUnit.Framework.Assert.That(exported.Size, Is.EqualTo(new Size(width, height)));
            NUnit.Framework.Assert.That(chart.ClientSize, Is.EqualTo(new Size(320, 240)));
            NUnit.Framework.Assert.That(PrivateField(chart, "image"), Is.SameAs(cachedImage));
            NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.Null);
            var wrongPixel = false;
            for (var y = 0; y < exported.Height && !wrongPixel; y++)
                for (var x = 0; x < exported.Width; x++)
                    if (exported.GetPixel(x, y).ToArgb() != chart.BackColor.ToArgb())
                    {
                        wrongPixel = true;
                        break;
                    }
            NUnit.Framework.Assert.That(wrongPixel, Is.False, "An empty chart must preserve its opaque background");
        }

        [Test]
        public void ExportEvaluatesTheSameCoordinateConventionWithoutChangingLiveTooltipPoints()
        {
            var samples = new ConcurrentBag<Complex>();
            using var chart = CreateChart(100, 80, 50);
            chart.SetChartAreaValues(-2, 4, -3, 5);
            InitializeFunction(chart, z => { samples.Add(z); return z; });
            while (samples.TryTake(out _)) { }
            var livePoints = PrivateField(chart, "_pointsValues");
            chart.ShouldDrawAxes = false;
            using var result = chart.GetImage(33, 17);
            NUnit.Framework.Assert.That(samples.Count, Is.EqualTo(33 * 17));
            NUnit.Framework.Assert.That(samples.Min(z => z.Real), Is.EqualTo(-2));
            NUnit.Framework.Assert.That(samples.Max(z => z.Real), Is.EqualTo(4 - 6d / 33).Within(1e-12));
            NUnit.Framework.Assert.That(samples.Max(z => z.Imaginary), Is.EqualTo(5));
            NUnit.Framework.Assert.That(samples.Min(z => z.Imaginary), Is.EqualTo(-3 + 8d / 17).Within(1e-12));
            NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.SameAs(livePoints));
        }

        [Test]
        public void FailedOrInvalidExportsLeaveTheLiveChartIntact()
        {
            var fail = false;
            using var chart = CreateChart(100, 80, 10);
            InitializeFunction(chart, z => fail ? throw new InvalidOperationException("test evaluation failure") : z);
            var cachedImage = PrivateField(chart, "image");
            var cachedPoints = PrivateField(chart, "_pointsValues");
            var resized = 0;
            chart.Resize += (_, _) => resized++;
            NUnit.Framework.Assert.That(() => chart.GetImage(0, 100), Throws.TypeOf<ArgumentOutOfRangeException>());
            NUnit.Framework.Assert.That(() => chart.GetImage(100, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
            fail = true;
            NUnit.Framework.Assert.That(() => chart.GetImage(33, 17), Throws.Exception);
            NUnit.Framework.Assert.That(chart.Size, Is.EqualTo(new Size(100, 80)));
            NUnit.Framework.Assert.That(resized, Is.Zero);
            NUnit.Framework.Assert.That(PrivateField(chart, "image"), Is.SameAs(cachedImage));
            NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.SameAs(cachedPoints));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NewPlotRequestedDuringRenderingPublishesOnlyTheLatestFunction(bool clearFirst)
        {
            using var chart = CreateChart(100, 80, 5);
            using var started = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            chart.AddFunction(new Function(new Func<Complex, Complex>(z =>
            {
                started.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("The test did not release its blocked function");
                return new Complex(2, 0);
            }), FunctionType.Complex));
            try
            {
                NUnit.Framework.Assert.That(started.Wait(TimeSpan.FromSeconds(5)), Is.True);
                if (clearFirst) chart.ClearAll();
                chart.AddFunction(new Function(new Func<Complex, Complex>(z => new Complex(17, 0)),
                    FunctionType.Complex));
            }
            finally
            {
                release.Set();
                WaitForRedraw(chart);
            }
            var points = (ComplexPoint[,])PrivateField(chart, "_pointsValues");
            NUnit.Framework.Assert.That(points, Is.Not.Null);
            NUnit.Framework.Assert.That(points.Cast<ComplexPoint>().All(point => point.Fz == new Complex(17, 0)),
                Is.True, "A completed obsolete render must not replace the newly requested plot");
        }

        [Test]
        public void ResizeRequestedDuringRenderingUpdatesTheSamplingGrid()
        {
            using var chart = CreateChart(100, 80, 5);
            using var started = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            chart.AddFunction(new Function(new Func<Complex, Complex>(z =>
            {
                started.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("The test did not release its blocked function");
                return z;
            }), FunctionType.Complex));
            try
            {
                NUnit.Framework.Assert.That(started.Wait(TimeSpan.FromSeconds(5)), Is.True);
                chart.ClientSize = new Size(240, 160);
                chart.Quality = 50;
            }
            finally
            {
                release.Set();
                WaitForRedraw(chart);
            }
            var points = (ComplexPoint[,])PrivateField(chart, "_pointsValues");
            NUnit.Framework.Assert.That(points.GetLength(0), Is.EqualTo(240));
            NUnit.Framework.Assert.That(points.GetLength(1), Is.EqualTo(160));
            NUnit.Framework.Assert.That(((Bitmap)PrivateField(chart, "image")).Size, Is.EqualTo(chart.ClientSize));
        }

        [Test]
        public void DisposingAChartReleasesItsCachedBitmapAndTooltipSamples()
        {
            using var chart = CreateChart(100, 80, 5);
            InitializeFunction(chart, z => z);
            var bitmap = (Bitmap)PrivateField(chart, "image");
            chart.Dispose();
            NUnit.Framework.Assert.That(() => bitmap.Width, Throws.TypeOf<ArgumentException>(),
                "The control owns its cached image and must release it when closed");
            NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.Null);
        }

        [Test]
        public void MouseCoordinatesFollowTheVisibleCachedPlotWhileItsReplacementRenders()
        {
            using var chart = CreateChart(100, 80, 5);
            InitializeFunction(chart, z => z);
            using var started = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            chart.AddFunction(new Function(new Func<Complex, Complex>(z =>
            {
                started.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("The test did not release its blocked function");
                return z + 17;
            }), FunctionType.Complex));
            try
            {
                NUnit.Framework.Assert.That(started.Wait(TimeSpan.FromSeconds(5)), Is.True);
                chart.ClientSize = new Size(240, 160);
                chart.Quality = 50;
                typeof(ComplexChart).GetMethod("_MouseClick", PrivateInstance).Invoke(chart,
                    new object[] { chart, new MouseEventArgs(MouseButtons.Left, 1, 120, 80, 0) });
                var tooltip = ((ToolTip)PrivateField(chart, "toolTip")).GetToolTip(chart);
                NUnit.Framework.Assert.That(tooltip, Does.StartWith("f(z) = 0"),
                    "Until the replacement is ready, mouse coordinates must match the visible cached samples");
                NUnit.Framework.Assert.That(tooltip, Does.Contain("z = 0"));
            }
            finally
            {
                release.Set();
                WaitForRedraw(chart);
            }
        }

        [TestCase("replace")]
        [TestCase("clear")]
        [TestCase("dispose")]
        [TestCase("current")]
        public void FailedRenderingReportsOnlyCurrentErrorsAndAllowsTheNextPlot(string action)
        {
            // NUnit wraps its STA synchronization context and records async void
            // exceptions itself. A dedicated UI thread exercises the real application
            // exception route without disposing NUnit's shared marshaling control.
            Exception failure = null;
            var thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(null);
                try { AssertFailedRenderingBehavior(action); }
                catch (Exception exception) { failure = exception; }
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            NUnit.Framework.Assert.That(thread.Join(TimeSpan.FromSeconds(30)), Is.True,
                "The isolated UI thread did not finish");
            if (failure != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }

        private static void AssertFailedRenderingBehavior(string action)
        {
            using var chart = CreateChart(100, 80, 5);
            using var started = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            Exception uiError = null;
            ThreadExceptionEventHandler captureError = (_, args) => uiError = args.Exception;
            Application.ThreadException += captureError;
            try
            {
                chart.AddFunction(new Function(new Func<Complex, Complex>(z =>
                {
                    started.Set();
                    if (!release.Wait(TimeSpan.FromSeconds(10)))
                        throw new TimeoutException("The test did not release its blocked function");
                    throw new InvalidOperationException("test evaluation failure");
                }), FunctionType.Complex));
                NUnit.Framework.Assert.That(started.Wait(TimeSpan.FromSeconds(5)), Is.True);
                if (action == "replace")
                    chart.AddFunction(new Function(new Func<Complex, Complex>(z => new Complex(17, 0)),
                        FunctionType.Complex));
                else if (action == "clear")
                    chart.ClearAll();
                else if (action == "dispose")
                    chart.Dispose();
            }
            finally
            {
                release.Set();
                try
                {
                    WaitForRedraw(chart);
                    // async void posts its exception separately from the await continuation.
                    Application.DoEvents();
                }
                finally
                {
                    Application.ThreadException -= captureError;
                }
            }

            if (action == "current")
            {
                NUnit.Framework.Assert.That(uiError, Is.Not.Null,
                    "A failure in the current plot must remain visible to the application");
                NUnit.Framework.Assert.That(uiError.ToString(), Does.Contain("test evaluation failure"));
                InitializeFunction(chart, z => new Complex(17, 0));
            }
            else
                NUnit.Framework.Assert.That(uiError, Is.Null,
                    "A discarded plot must not report a late error after replacement, clear or disposal");

            var points = (ComplexPoint[,])PrivateField(chart, "_pointsValues");
            if (action == "replace" || action == "current")
            {
                NUnit.Framework.Assert.That(points, Is.Not.Null);
                NUnit.Framework.Assert.That(points.Cast<ComplexPoint>().All(point => point.Fz == new Complex(17, 0)),
                    Is.True, "A failed evaluation must not block the next requested plot");
            }
            else
                NUnit.Framework.Assert.That(points, Is.Null);
        }

        [Test]
        public void ClearingWhileAFunctionIsRenderingDoesNotRepublishItsOldSamples()
        {
            using var chart = CreateChart(100, 80, 5);
            using var started = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            chart.AddFunction(new Function(new Func<Complex, Complex>(z =>
            {
                started.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("The test did not release its blocked function");
                return z;
            }), FunctionType.Complex));
            object clearedImage = null;
            try
            {
                NUnit.Framework.Assert.That(started.Wait(TimeSpan.FromSeconds(5)), Is.True);
                chart.ClearAll();
                clearedImage = PrivateField(chart, "image");
                NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.Null);
            }
            finally
            {
                release.Set();
                WaitForRedraw(chart);
            }
            NUnit.Framework.Assert.That(PrivateField(chart, "image"), Is.SameAs(clearedImage));
            NUnit.Framework.Assert.That(PrivateField(chart, "_pointsValues"), Is.Null);
        }

        [Test]
        public void ProduceBeforeAfterImagesFromTheActualDomainColoringAndGdiRenderers()
        {
            var repository = FindRepositoryRoot();
            var destination = Path.Combine(repository, "TestResults", "complex-chart-rendering");
            Directory.CreateDirectory(destination);
            var cases = new[]
            {
                new VisualCase("small-96", 320, 240, 96),
                new VisualCase("medium-96", 800, 600, 96),
                new VisualCase("large-96", 1600, 1200, 96),
                new VisualCase("wide-96", 1200, 400, 96),
                new VisualCase("tall-96", 400, 1000, 96),
                new VisualCase("medium-144", 1200, 900, 144),
                new VisualCase("medium-192", 1600, 1200, 192),
                new VisualCase("small-192", 320, 240, 192),
                new VisualCase("near-left-top", 800, 600, 96, -0.001, 2, -2, 0.001),
                new VisualCase("near-right-bottom", 800, 600, 96, -2, 0.001, -0.001, 2),
                new VisualCase("near-right-top", 800, 600, 96, -2, 0.001, -2, 0.001),
                new VisualCase("positive-only", 800, 600, 96, 1, 3, 1, 3),
                new VisualCase("negative-only", 800, 600, 96, -3, -1, -3, -1)
            };
            using var font = new Font("Cambria", 13, GraphicsUnit.Point);
            foreach (var item in cases)
            {
                // This is the production complex sampling/color mapping, with axes
                // disabled. Both overlays receive identical plot pixels and DPI.
                using var chart = CreateChart(80, 60, 25);
                chart.SetChartAreaValues(item.XMin, item.XMax, item.YMin, item.YMax);
                chart.ShouldDrawAxes = false;
                InitializeFunction(chart, z => z * z - 1);
                using var plot = chart.GetImage(item.Width, item.Height);
                using var before = NewBitmap(item.Width, item.Height, item.Dpi);
                using var after = NewBitmap(item.Width, item.Height, item.Dpi);
                using (var graphics = Graphics.FromImage(before))
                {
                    graphics.DrawImage(plot, new Rectangle(0, 0, item.Width, item.Height));
                    DrawOriginalAxes(graphics, item, font);
                }
                using (var graphics = Graphics.FromImage(after))
                {
                    graphics.DrawImage(plot, new Rectangle(0, 0, item.Width, item.Height));
                    var layout = ComplexAxesRenderer.Draw(graphics,
                        new Rectangle(0, 0, item.Width, item.Height), item.XMin, item.XMax, item.YMin, item.YMax,
                        0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Black);
                    ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.XLabelBounds);
                    ComplexAxesGeometryTests.AssertContained(layout.SafeBounds, layout.YLabelBounds);
                    NUnit.Framework.Assert.That(layout.XLabelBounds.IntersectsWith(layout.YLabelBounds), Is.False,
                        item.Name);
                }
                var beforePath = Path.Combine(destination, item.Name + "-before.png");
                var afterPath = Path.Combine(destination, item.Name + "-after.png");
                before.Save(beforePath, ImageFormat.Png);
                after.Save(afterPath, ImageFormat.Png);
                TestContext.AddTestAttachment(beforePath, item.Name + " original axes on identical plot pixels");
                TestContext.AddTestAttachment(afterPath, item.Name + " corrected axes on identical plot pixels");
            }
            var manifest = Path.Combine(destination, "manifest.json");
            File.WriteAllText(manifest, JsonSerializer.Serialize(new
            {
                renderer = "Windows System.Drawing/GDI+; actual production ComplexAxesRenderer",
                testedCommit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local working tree",
                baselineCommit = "575315952a219e4355b7f575ce0b165614ef6797",
                baselineSource = "Computator.NET.Charting/ComplexCharting/ComplexChart.cs DrawAxes/GetMiddlePoint",
                function = "z² − 1",
                plot = "Actual production sampling and domain coloring; identical background per before/after pair",
                fontFamily = font.FontFamily.Name,
                fontSizePoints = font.SizeInPoints,
                dpiScope = "Bitmap DPI at 96/144/192; does not simulate physical monitor switching",
                cases,
                sourceHashes = new[]
                {
                    "Computator.NET.Charting/ComplexCharting/ComplexAxesLayout.cs",
                    "Computator.NET.Charting/ComplexCharting/ComplexAxesRenderer.cs",
                    "Computator.NET.Charting/ComplexCharting/ComplexChart.cs"
                }.Select(path => new
                {
                    path,
                    sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(repository, path))))
                }).ToArray(),
                images = cases.SelectMany(item => new[] { item.Name + "-before.png", item.Name + "-after.png" })
                    .Select(path => new
                    {
                        path,
                        sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(destination, path))))
                    }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddTestAttachment(manifest, "Rendering provenance, dimensions and DPI scenarios");
        }

        private static ComplexAxesLayout RenderLayout(int width, int height, float dpi, Font font)
        {
            using var bitmap = NewBitmap(width, height, dpi);
            using var graphics = Graphics.FromImage(bitmap);
            return ComplexAxesRenderer.Draw(graphics, new Rectangle(0, 0, width, height),
                -2, 2, -2, 2, 0.02, "Re(z)", "Im(z)", font, Color.Black, Color.Blue);
        }

        private static ComplexChart CreateChart(int width, int height, double quality)
        {
            var chart = new ComplexChart { Size = new Size(width, height), Dock = DockStyle.None, Quality = quality };
            chart.SetChartAreaValues(-2, 2, -2, 2);
            return chart;
        }

        private static void InitializeFunction(ComplexChart chart, Func<Complex, Complex> function)
        {
            chart.AddFunction(new Function(function, FunctionType.Complex));
            WaitForRedraw(chart);
        }

        private static void WaitForRedraw(ComplexChart chart)
        {
            var timer = Stopwatch.StartNew();
            while ((bool)PrivateField(chart, "_redrawing") && timer.Elapsed < TimeSpan.FromSeconds(15))
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
            NUnit.Framework.Assert.That((bool)PrivateField(chart, "_redrawing"), Is.False,
                "The existing async redraw did not complete");
        }

        private static object PrivateField(ComplexChart chart, string name)
        {
            return typeof(ComplexChart).GetField(name, PrivateInstance).GetValue(chart);
        }

        private static Bitmap NewBitmap(int width, int height, float dpi)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(dpi, dpi);
            return bitmap;
        }

        private static string ImageDigest(Bitmap bitmap)
        {
            // Hash decoded pixels, not PNG metadata/encoder decisions.
            var pixels = new byte[bitmap.Width * bitmap.Height * 4];
            var offset = 0;
            for (var y = 0; y < bitmap.Height; y++)
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    pixels[offset++] = color.A;
                    pixels[offset++] = color.R;
                    pixels[offset++] = color.G;
                    pixels[offset++] = color.B;
                }
            return Convert.ToHexString(SHA256.HashData(pixels));
        }

        private static int CountBlueInk(Bitmap bitmap, RectangleF bounds)
        {
            var rectangle = Rectangle.Intersect(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                Rectangle.Ceiling(bounds));
            var count = 0;
            for (var y = rectangle.Top; y < rectangle.Bottom; y++)
                for (var x = rectangle.Left; x < rectangle.Right; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.B > color.R + 40 && color.B > color.G + 40)
                        count++;
                }
            return count;
        }

        private static string FindRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                 directory != null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Computator.NET.slnx")))
                    return directory.FullName;
            throw new DirectoryNotFoundException("Cannot locate repository for retained chart rendering evidence");
        }

        private sealed class VisualCase
        {
            public VisualCase(string name, int width, int height, int dpi,
                double xMin = -2, double xMax = 2, double yMin = -2, double yMax = 2)
            {
                Name = name; Width = width; Height = height; Dpi = dpi;
                XMin = xMin; XMax = xMax; YMin = yMin; YMax = yMax;
            }
            public string Name { get; }
            public int Width { get; }
            public int Height { get; }
            public int Dpi { get; }
            public double XMin { get; }
            public double XMax { get; }
            public double YMin { get; }
            public double YMax { get; }
        }

        // Test-only reproduction of master 575315952a219e4355b7f575ce0b165614ef6797.
        // Deliberately retain the original fixed offsets and coordinate-dependent
        // arrowheads. This helper produces comparisons; it is not the test oracle.
        private static void DrawOriginalAxes(Graphics graphics, VisualCase item, Font font)
        {
            var x = (int)(Math.Abs(item.XMin) / (item.XMax - item.XMin) * item.Width);
            var y = (int)(Math.Abs(item.YMax) / (item.YMax - item.YMin) * item.Height);
            if (item.XMax <= 0) x = item.Width - 1;
            else if (item.XMin >= 0) x = 0;
            if (item.YMax <= 0) y = 0;
            else if (item.YMin >= 0) y = item.Height - 1;
            var middle = new Point(x, y);
            var xEnd = new Point(item.Width, y);
            var yStart = new Point(x, 0);
            using var pen = new Pen(Color.Black);
            using var brush = new SolidBrush(Color.Black);
            graphics.DrawLine(pen, middle, new Point(x, item.Height));
            graphics.DrawLine(pen, middle, yStart);
            graphics.DrawLine(pen, yStart, new Point((int)(x * 0.98), (int)(item.Height * 0.02)));
            graphics.DrawLine(pen, yStart, new Point((int)(x * 1.02), (int)(item.Height * 0.02)));
            graphics.DrawString("Im(z)", font, brush, x - 60, 15);
            graphics.DrawLine(pen, middle, xEnd);
            graphics.DrawLine(pen, middle, new Point(0, y));
            graphics.DrawLine(pen, xEnd, new Point((int)(item.Width * 0.98), (int)(y * 0.98)));
            graphics.DrawLine(pen, xEnd, new Point((int)(item.Width * 0.98), (int)(y * 1.02)));
            graphics.DrawString("Re(z)", font, brush, item.Width - 60, y + 10);
        }
    }
#endif
}
