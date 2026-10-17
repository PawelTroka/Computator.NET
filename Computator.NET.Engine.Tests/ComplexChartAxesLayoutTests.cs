using System.Drawing;
using Computator.NET.Charting.ComplexCharting;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ComplexChartAxesLayoutTests
{
    [Test]
    public void LargerChartsScaleStrokesArrowsAndLabelsTogether()
    {
        var small = Layout(new Size(640, 480));
        var large = Layout(new Size(1280, 960));
        NUnit.Framework.Assert.Multiple(() =>
        {
            NUnit.Framework.Assert.That(large.Scale, Is.EqualTo(2 * small.Scale));
            NUnit.Framework.Assert.That(large.StrokeWidth, Is.EqualTo(2 * small.StrokeWidth));
            NUnit.Framework.Assert.That(large.ArrowLength, Is.EqualTo(2 * small.ArrowLength));
            NUnit.Framework.Assert.That(large.Margin, Is.EqualTo(2 * small.Margin));
        });
    }

    [TestCase(480, 96, 1, 1.6f)]
    [TestCase(480, 192, 2, 3.2f)]
    [TestCase(960, 96, 2, 3.2f)]
    [TestCase(960, 192, 2, 3.2f)]
    public void PixelFontScaleAccountsForDpiExactlyOnce(int shortSide, float dpi,
        float expectedScale, float expectedStrokeWidth)
    {
        var layout = Layout(new Size(shortSide * 2, shortSide), dpi);
        NUnit.Framework.Assert.Multiple(() =>
        {
            NUnit.Framework.Assert.That(layout.Scale, Is.EqualTo(expectedScale));
            NUnit.Framework.Assert.That(layout.StrokeWidth, Is.EqualTo(expectedStrokeWidth));
        });
    }

    [TestCase(-5, 5, -5, 5)]
    [TestCase(-1, 9, -9, 1)]
    [TestCase(1, 9, 1, 9)]
    [TestCase(-9, -1, -9, -1)]
    [TestCase(1, 9, -9, -1)]
    [TestCase(-9, -1, 1, 9)]
    public void ArrowheadsStayIsotropicWhenAxesMove(double xMin, double xMax,
        double yMin, double yMax)
    {
        var bounds = new Rectangle(0, 0, 900, 480);
        var layout = ComplexChartAxesLayout.Create(bounds.Size, xMin, xMax, yMin, yMax, 96, 0.02);
        NUnit.Framework.Assert.Multiple(() =>
        {
            NUnit.Framework.Assert.That(layout.XArrow[0], Is.EqualTo(layout.XEnd));
            NUnit.Framework.Assert.That(layout.YArrow[0], Is.EqualTo(layout.YEnd));
            NUnit.Framework.Assert.That(layout.XArrow[0].X - layout.XArrow[1].X,
                Is.EqualTo(layout.YArrow[1].Y - layout.YArrow[0].Y).Within(0.0001));
            NUnit.Framework.Assert.That(layout.XArrow[2].Y - layout.XArrow[1].Y,
                Is.EqualTo(layout.YArrow[2].X - layout.YArrow[1].X).Within(0.0001));
            NUnit.Framework.Assert.That(layout.XArrow[1].Y + layout.XArrow[2].Y,
                Is.EqualTo(2 * layout.XAxisY).Within(0.0001));
            NUnit.Framework.Assert.That(layout.YArrow[1].X + layout.YArrow[2].X,
                Is.EqualTo(2 * layout.YAxisX).Within(0.0001));
            AssertInside(bounds, layout.XEnd);
            AssertInside(bounds, layout.YEnd);
        });
    }

    [TestCase(-1, 99, -99, 1, 1, 1)]
    [TestCase(-99, 1, -1, 99, 99, 99)]
    [TestCase(-0.01, 99.99, -99.99, 0.01, 0.01, 0.01)]
    [TestCase(-99.99, 0.01, -0.01, 99.99, 99.99, 99.99)]
    public void CrossingAxesMatchTheSampledZeroEvenWhenArrowWingsReachTheEdge(
        double xMin, double xMax, double yMin, double yMax, double expectedX, double expectedY)
    {
        // Sampling uses min + pixel * (max - min) / width, so the zero's
        // destination coordinate is a fraction of width, not width - 1.
        var layout = ComplexChartAxesLayout.Create(new Size(100, 100),
            xMin, xMax, yMin, yMax, 96, 0.02);
        NUnit.Framework.Assert.Multiple(() =>
        {
            NUnit.Framework.Assert.That(layout.YAxisX, Is.EqualTo(expectedX).Within(0.0001));
            NUnit.Framework.Assert.That(layout.XAxisY, Is.EqualTo(expectedY).Within(0.0001));
        });
    }

    [Test]
    public void ArrowSizeChangesDoNotMoveTheMathematicalAxes()
    {
        var layouts = new[] { 0, 0.005, 0.01, 0.02, 0.04 }.Select(relativeSize =>
            ComplexChartAxesLayout.Create(new Size(640, 480), -1, 99, -99, 1, 96, relativeSize)).ToArray();
        NUnit.Framework.Assert.Multiple(() =>
        {
            NUnit.Framework.Assert.That(layouts[0].ArrowLength, Is.Zero);
            NUnit.Framework.Assert.That(layouts[2].ArrowLength, Is.EqualTo(2 * layouts[1].ArrowLength));
            NUnit.Framework.Assert.That(layouts[3].ArrowLength, Is.EqualTo(2 * layouts[2].ArrowLength));
            NUnit.Framework.Assert.That(layouts[4].ArrowLength, Is.EqualTo(2 * layouts[3].ArrowLength));
            foreach (var layout in layouts)
            {
                NUnit.Framework.Assert.That(layout.YAxisX, Is.EqualTo(6.4f).Within(0.0001));
                NUnit.Framework.Assert.That(layout.XAxisY, Is.EqualTo(4.8f).Within(0.0001));
            }
        });
    }

    [TestCase(-5, 5, -5, 5)]
    [TestCase(1, 9, 1, 9)]
    [TestCase(-9, -1, -9, -1)]
    [TestCase(-1, 99, -99, 1)]
    public void MeasuredLabelsFitWithinTargetsAndMoveAwayFromEdges(double xMin, double xMax,
        double yMin, double yMax)
    {
        var bounds = new Rectangle(0, 0, 640, 480);
        var size = new SizeF(135, 36);
        var layout = ComplexChartAxesLayout.Create(bounds.Size, xMin, xMax, yMin, yMax, 96, 0.02);
        layout.GetLabelBounds(size, size, out var xBounds, out var yBounds);
        foreach (var label in new[] { xBounds, yBounds })
        {
            AssertInside(bounds, label.Location);
            AssertInside(bounds, new PointF(label.Right, label.Bottom));
        }
        NUnit.Framework.Assert.That(xBounds.IntersectsWith(yBounds), Is.False);
    }

    [Test]
    public void HugeFiniteRangesKeepZeroInTheCenterWithoutOverflowingTheirSpan()
    {
        var layout = ComplexChartAxesLayout.Create(new Size(640, 480),
            -double.MaxValue, double.MaxValue, -double.MaxValue, double.MaxValue, 96, 0.02);
        NUnit.Framework.Assert.That(new PointF(layout.YAxisX, layout.XAxisY), Is.EqualTo(new PointF(320, 240)));
    }

    [TestCase(double.NaN, 1)]
    [TestCase(0, double.PositiveInfinity)]
    [TestCase(3, 3)]
    [TestCase(4, -4)]
    public void InvalidRangesUseFiniteCenteredAxes(double minimum, double maximum)
    {
        var layout = ComplexChartAxesLayout.Create(new Size(640, 480),
            minimum, maximum, minimum, maximum, 96, 0.02);
        NUnit.Framework.Assert.That(new PointF(layout.YAxisX, layout.XAxisY), Is.EqualTo(new PointF(320, 240)));
    }

    [TestCase(0, 480)]
    [TestCase(640, 0)]
    [TestCase(-1, 480)]
    public void EmptyOrNegativeTargetsAreRejected(int width, int height)
    {
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            Layout(new Size(width, height)));
    }

    [Test]
    public void TinyTargetsAndInvalidPresentationValuesKeepFiniteBoundedGeometry()
    {
        var bounds = new Rectangle(0, 0, 1, 1);
        foreach (var dpi in new[] { 96f, 192f, 0f, float.NaN })
        foreach (var arrowSize in new[] { 0.02, 0, -1, double.NaN, double.PositiveInfinity, 100 })
        {
            var layout = ComplexChartAxesLayout.Create(bounds.Size, 1, 2, -2, -1, dpi, arrowSize);
            foreach (var point in layout.XArrow.Concat(layout.YArrow))
                AssertInside(bounds, point);
            NUnit.Framework.Assert.That(layout.StrokeWidth, Is.GreaterThan(0).And.LessThanOrEqualTo(0.25f));
        }
    }

    private static ComplexChartAxesLayout Layout(Size size, float dpi = 96) =>
        ComplexChartAxesLayout.Create(size, -5, 5, -5, 5, dpi, 0.02);

    private static void AssertInside(Rectangle bounds, PointF point)
    {
        NUnit.Framework.Assert.That(point.X, Is.InRange((float)bounds.Left, (float)bounds.Right));
        NUnit.Framework.Assert.That(point.Y, Is.InRange((float)bounds.Top, (float)bounds.Bottom));
    }
}
