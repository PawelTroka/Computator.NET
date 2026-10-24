#nullable disable

using System;
using System.Drawing;

namespace Computator.NET.Charting.ComplexCharting
{
    // Pixel geometry only: no control, font or GDI handles, and no dependence on the
    // sampled bitmap's resolution. The same layout is used on screen and in exports.
    internal sealed class ComplexAxesLayout
    {
        public RectangleF Bounds { get; private set; }
        public RectangleF SafeBounds { get; private set; }
        public PointF Origin { get; private set; }
        public PointF XStart { get; private set; }
        public PointF XEnd { get; private set; }
        public PointF YStart { get; private set; }
        public PointF YEnd { get; private set; }
        public PointF[] XArrow { get; private set; }
        public PointF[] YArrow { get; private set; }
        public float StrokeWidth { get; private set; }
        public float ArrowLength { get; private set; }
        public float ArrowHalfWidth { get; private set; }
        public float FontScale { get; private set; }
        public float DpiScale { get; private set; }
        public float LabelGap { get; private set; }
        public RectangleF XLabelBounds { get; private set; }
        public RectangleF YLabelBounds { get; private set; }
        public ComplexTicksLayout Ticks { get; internal set; }

        public static ComplexAxesLayout Create(RectangleF bounds, float dpiX, float dpiY,
            double xMin, double xMax, double yMin, double yMax, double arrowRelativeSize)
        {
            if (!IsFinite(bounds.Left) || !IsFinite(bounds.Top) ||
                !IsFinite(bounds.Right) || !IsFinite(bounds.Bottom) ||
                bounds.Width < 4 || bounds.Height < 4 ||
                !IsFinite(xMin) || !IsFinite(xMax) || xMin >= xMax ||
                !IsFinite(yMin) || !IsFinite(yMax) || yMin >= yMax)
                return null;

            var scaleX = ValidDpi(dpiX) / 96f;
            var scaleY = ValidDpi(dpiY) / 96f;
            var logicalShortSide = Math.Min(bounds.Width / scaleX, bounds.Height / scaleY);
            var shortSide = Math.Min(bounds.Width, bounds.Height);
            var layout = new ComplexAxesLayout
            {
                Bounds = bounds,
                DpiScale = scaleY,
                FontScale = Clamp((float)Math.Sqrt(logicalShortSide / 320f), 1f, 1.8f)
            };

            var decorationScale = layout.DpiScale * layout.FontScale;
            layout.StrokeWidth = Math.Min(1.6f * decorationScale, shortSide / 8f);
            layout.LabelGap = Math.Min(4f * decorationScale, shortSide / 12f);
            var padding = Math.Min(4f * decorationScale, shortSide / 8f);
            layout.SafeBounds = RectangleF.FromLTRB(bounds.Left + padding, bounds.Top + padding,
                bounds.Right - padding, bounds.Bottom - padding);

            if (!IsFinite(arrowRelativeSize) || arrowRelativeSize < 0)
                arrowRelativeSize = 0.02;
            // Keep the relative-size setting linear, including values below the
            // default. Only cap heads that cannot fit safely inside this viewport.
            layout.ArrowLength = Math.Min(shortSide / 4f,
                Math.Min(32f * decorationScale, 480f * (float)Math.Min(arrowRelativeSize, 1) * decorationScale));
            layout.ArrowHalfWidth = layout.ArrowLength * 0.4f;

            // Use the existing sample-grid coordinate convention (Width/Height,
            // not Width-1/Height-1). Clamp only offscreen zeros to the edge pixels.
            var x = ProjectZero(ZeroFraction(xMin, xMax), bounds.Left, bounds.Width);
            var y = ProjectZero(1 - ZeroFraction(yMin, yMax), bounds.Top, bounds.Height);
            layout.Origin = new PointF(x, y);
            layout.XStart = new PointF(bounds.Left, y);
            layout.XEnd = new PointF(bounds.Right - 1, y);
            layout.YStart = new PointF(x, bounds.Bottom - 1);
            layout.YEnd = new PointF(x, bounds.Top);

            // At an edge only the base moves inward. The tip and axis stay at the
            // true zero coordinate; the head retains its length and full width.
            var xBaseTop = Clamp(y - layout.ArrowHalfWidth,
                bounds.Top, Math.Max(y, bounds.Bottom - 1) - 2 * layout.ArrowHalfWidth);
            var yBaseLeft = Clamp(x - layout.ArrowHalfWidth,
                bounds.Left, Math.Max(x, bounds.Right - 1) - 2 * layout.ArrowHalfWidth);
            layout.XArrow = new[]
            {
                layout.XEnd,
                new PointF(layout.XEnd.X - layout.ArrowLength, xBaseTop),
                new PointF(layout.XEnd.X - layout.ArrowLength, xBaseTop + 2 * layout.ArrowHalfWidth)
            };
            layout.YArrow = new[]
            {
                layout.YEnd,
                new PointF(yBaseLeft, layout.YEnd.Y + layout.ArrowLength),
                new PointF(yBaseLeft + 2 * layout.ArrowHalfWidth, layout.YEnd.Y + layout.ArrowLength)
            };
            return layout;
        }

        public void PlaceLabels(SizeF xSize, SizeF ySize)
        {
            xSize = FitSize(xSize);
            ySize = FitSize(ySize);
            var clearance = ArrowHalfWidth + LabelGap;
            var yTop = SafeBounds.Top + ArrowLength;
            var xHead = PolygonBounds(XArrow);

            YLabelBounds = Place(ySize,
                new[] { Origin.X - clearance - ySize.Width, Origin.X + clearance,
                    SafeBounds.Left, SafeBounds.Right - ySize.Width },
                new[] { yTop, Origin.Y + clearance, Origin.Y - clearance - ySize.Height,
                    SafeBounds.Bottom - ySize.Height }, RectangleF.Empty);

            XLabelBounds = Place(xSize,
                new[] { SafeBounds.Right - xSize.Width, Origin.X - clearance - xSize.Width,
                    Origin.X + clearance, SafeBounds.Left },
                new[] { xHead.Bottom + LabelGap, xHead.Top - LabelGap - xSize.Height,
                    YLabelBounds.Bottom + LabelGap, YLabelBounds.Top - LabelGap - xSize.Height,
                    SafeBounds.Bottom - xSize.Height, SafeBounds.Top }, YLabelBounds);
        }

        private RectangleF Place(SizeF size, float[] xs, float[] ys, RectangleF otherLabel)
        {
            if (size.Width <= 0 || size.Height <= 0)
                return RectangleF.Empty;

            var lineClearance = StrokeWidth / 2 + LabelGap / 2;
            var horizontal = RectangleF.FromLTRB(Bounds.Left, Origin.Y - lineClearance,
                Bounds.Right, Origin.Y + lineClearance);
            var vertical = RectangleF.FromLTRB(Origin.X - lineClearance, Bounds.Top,
                Origin.X + lineClearance, Bounds.Bottom);
            var xHead = PolygonBounds(XArrow);
            var yHead = PolygonBounds(YArrow);
            xHead.Inflate(LabelGap / 2, LabelGap / 2);
            yHead.Inflate(LabelGap / 2, LabelGap / 2);
            if (!otherLabel.IsEmpty)
                otherLabel.Inflate(LabelGap, LabelGap);

            var best = RectangleF.Empty;
            var bestOverlap = float.MaxValue;
            var bestDistance = float.MaxValue;
            // Try the positive end first, then the opposite side, then positions
            // beside the other axis. Prefer a clear rectangle over a fixed offset.
            foreach (var x in xs)
            foreach (var y in ys)
            {
                var candidate = new RectangleF(
                    Clamp(x, SafeBounds.Left, SafeBounds.Right - size.Width),
                    Clamp(y, SafeBounds.Top, SafeBounds.Bottom - size.Height), size.Width, size.Height);
                var overlap = Overlap(candidate, horizontal) + Overlap(candidate, vertical) +
                    Overlap(candidate, xHead) + Overlap(candidate, yHead) + Overlap(candidate, otherLabel);
                var dx = candidate.Left - xs[0];
                var dy = candidate.Top - ys[0];
                var distance = dx * dx + dy * dy;
                if (overlap < bestOverlap || (overlap == bestOverlap && distance < bestDistance))
                {
                    best = candidate;
                    bestOverlap = overlap;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private SizeF FitSize(SizeF size)
        {
            if (!IsFinite(size.Width) || !IsFinite(size.Height))
                return SizeF.Empty;
            return new SizeF(Clamp(size.Width, 0, SafeBounds.Width),
                Clamp(size.Height, 0, SafeBounds.Height));
        }

        private static RectangleF PolygonBounds(PointF[] points)
        {
            return RectangleF.FromLTRB(Math.Min(points[0].X, Math.Min(points[1].X, points[2].X)),
                Math.Min(points[0].Y, Math.Min(points[1].Y, points[2].Y)),
                Math.Max(points[0].X, Math.Max(points[1].X, points[2].X)),
                Math.Max(points[0].Y, Math.Max(points[1].Y, points[2].Y)));
        }

        private static float Overlap(RectangleF first, RectangleF second)
        {
            var overlap = RectangleF.Intersect(first, second);
            return overlap.Width > 0 && overlap.Height > 0 ? overlap.Width * overlap.Height : 0;
        }

        private static double ZeroFraction(double min, double max)
        {
            if (min >= 0) return 0;
            if (max <= 0) return 1;
            // Avoid overflow for valid opposite-sign endpoints near double.MaxValue.
            var scale = Math.Max(-min, max);
            return (-min / scale) / ((-min / scale) + (max / scale));
        }

        private static float ProjectZero(double fraction, float start, float length)
        {
            if (fraction <= 0) return start;
            if (fraction >= 1) return start + length - 1;
            return start + (float)(fraction * length);
        }

        private static float ValidDpi(float dpi)
        {
            return IsFinite(dpi) && dpi > 0 ? dpi : 96;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
