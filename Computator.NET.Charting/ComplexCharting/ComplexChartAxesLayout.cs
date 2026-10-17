using System;
using System.Drawing;

namespace Computator.NET.Charting.ComplexCharting
{
    // Geometry shared by the screen renderer, exports and portable regression tests.
    internal sealed class ComplexChartAxesLayout
    {
        private ComplexChartAxesLayout(Size size, double xMin, double xMax, double yMin,
            double yMax, float dpi, double relativeArrowSize)
        {
            Size = size;
            var shorter = Math.Min(size.Width, size.Height);
            var dpiScale = float.IsFinite(dpi) && dpi > 0 ? dpi / 96f : 1f;
            Scale = Math.Max(dpiScale, shorter / 480f);
            Margin = Math.Min(Scale, (shorter - 1) / 10f);
            StrokeWidth = Math.Min(1.6f * Scale, Math.Max(0.1f, shorter / 4f));

            var relativeSize = double.IsFinite(relativeArrowSize) && relativeArrowSize >= 0
                ? relativeArrowSize : 0.02;
            ArrowLength = (float)Math.Min(480 * relativeSize * Scale,
                (shorter - 1 - 2 * Margin) / 4);
            ArrowHalfWidth = ArrowLength * 0.45f;
            // The color sampler maps pixels using width/height, not width - 1.
            // Keep a crossing zero in its true position; fitting arrow wings must
            // not move the coordinate axes. The renderer clips wings at the edge.
            XAxisY = ProjectZero(ZeroFraction(-yMax, -yMin), size.Height);
            YAxisX = ProjectZero(ZeroFraction(xMin, xMax), size.Width);
        }

        public Size Size { get; }
        public float Scale { get; }
        public float StrokeWidth { get; }
        public float ArrowLength { get; }
        public float ArrowHalfWidth { get; }
        public float Margin { get; }
        public float XAxisY { get; }
        public float YAxisX { get; }
        public PointF XStart => new PointF(Margin, XAxisY);
        public PointF XEnd => new PointF(Size.Width - 1 - Margin, XAxisY);
        public PointF YStart => new PointF(YAxisX, Size.Height - 1 - Margin);
        public PointF YEnd => new PointF(YAxisX, Margin);
        public PointF[] XArrow => new[]
        {
            XEnd,
            new PointF(XEnd.X - ArrowLength, XAxisY - ArrowHalfWidth),
            new PointF(XEnd.X - ArrowLength, XAxisY + ArrowHalfWidth)
        };
        public PointF[] YArrow => new[]
        {
            YEnd,
            new PointF(YAxisX - ArrowHalfWidth, YEnd.Y + ArrowLength),
            new PointF(YAxisX + ArrowHalfWidth, YEnd.Y + ArrowLength)
        };

        public static ComplexChartAxesLayout Create(Size size, double xMin, double xMax,
            double yMin, double yMax, float dpi, double relativeArrowSize)
        {
            if (size.Width <= 0 || size.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "The axis target must have a positive size.");
            return new ComplexChartAxesLayout(size, xMin, xMax, yMin, yMax, dpi, relativeArrowSize);
        }

        public void GetLabelBounds(SizeF xLabelSize, SizeF yLabelSize,
            out RectangleF xLabelBounds, out RectangleF yLabelBounds)
        {
            var gap = 4 * Scale;
            var xLabelY = XAxisY + gap;
            if (xLabelY + xLabelSize.Height > Size.Height - 1 - Margin)
                xLabelY = XAxisY - gap - xLabelSize.Height;
            xLabelBounds = Fit(new RectangleF(
                XEnd.X - ArrowLength - gap - xLabelSize.Width, xLabelY,
                xLabelSize.Width, xLabelSize.Height));

            var yLabelX = YAxisX - gap - yLabelSize.Width;
            if (yLabelX < Margin)
                yLabelX = YAxisX + gap;
            yLabelBounds = Fit(new RectangleF(yLabelX, YEnd.Y + ArrowLength + gap,
                yLabelSize.Width, yLabelSize.Height));

            // At an off-center origin the two measured labels can otherwise meet.
            if (xLabelBounds.IntersectsWith(yLabelBounds))
                yLabelBounds = Fit(new RectangleF(yLabelBounds.X, xLabelBounds.Bottom + gap,
                    yLabelBounds.Width, yLabelBounds.Height));
        }

        private RectangleF Fit(RectangleF rectangle)
        {
            var width = Math.Min(Math.Max(0, rectangle.Width), Size.Width - 1 - 2 * Margin);
            var height = Math.Min(Math.Max(0, rectangle.Height), Size.Height - 1 - 2 * Margin);
            return new RectangleF(
                Clamp(rectangle.X, Margin, Size.Width - 1 - Margin - width),
                Clamp(rectangle.Y, Margin, Size.Height - 1 - Margin - height), width, height);
        }

        private static float Clamp(float value, float minimum, float maximum) =>
            Math.Max(minimum, Math.Min(maximum, value));

        private float ProjectZero(double fraction, int length)
        {
            // Retain the existing visible boundary axes for one-sided domains.
            if (fraction <= 0)
                return StrokeWidth / 2;
            if (fraction >= 1)
                return length - StrokeWidth / 2;
            return (float)(fraction * length);
        }

        private static double ZeroFraction(double minimum, double maximum)
        {
            if (!double.IsFinite(minimum) || !double.IsFinite(maximum) || maximum <= minimum)
                return 0.5;
            if (maximum <= 0)
                return 1;
            if (minimum >= 0)
                return 0;
            // Normalize before adding so even two extreme finite endpoints cannot
            // overflow the span. This also preserves very small crossing fractions.
            var scale = Math.Max(-minimum, maximum);
            var negativePart = -minimum / scale;
            return negativePart / (negativePart + maximum / scale);
        }
    }
}
