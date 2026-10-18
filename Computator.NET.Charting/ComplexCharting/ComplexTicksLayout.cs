#nullable disable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Computator.NET.Charting.Axes;

namespace Computator.NET.Charting.ComplexCharting
{
    // Uses the same Width/Height sample mapping as ComplexAxesLayout. Text is
    // measured by the renderer; this class owns only testable pixel geometry.
    internal sealed class ComplexTicksLayout
    {
        internal sealed class Mark
        {
            public AxisTick Tick { get; set; }
            public bool HorizontalAxis { get; set; }
            public PointF Start { get; set; }
            public PointF End { get; set; }
            public RectangleF LabelBounds { get; set; }
            public bool DrawMark { get; set; } = true;
        }

        public IReadOnlyList<Mark> MajorTicks { get; private set; }
        public IReadOnlyList<Mark> MinorTicks { get; private set; }
        public float MajorHalfLength { get; private set; }
        public float MinorHalfLength { get; private set; }
        public float LabelGap { get; private set; }

        public static ComplexTicksLayout Create(ComplexAxesLayout axes,
            double xMin, double xMax, double yMin, double yMax,
            Func<string, SizeF> measureLabel, CultureInfo culture)
        {
            var scale = axes.DpiScale * axes.FontScale;
            var layout = new ComplexTicksLayout
            {
                MajorHalfLength = 4f * scale,
                MinorHalfLength = 2f * scale,
                LabelGap = 3f * scale
            };
            var xTicks = Generate(xMin, xMax, axes.Bounds.Width, 64 * scale,
                true, measureLabel, culture, scale);
            var yTicks = Generate(yMin, yMax, axes.Bounds.Height, 48 * scale,
                false, measureLabel, culture, scale);
            var major = new List<Mark>();
            var minor = new List<Mark>();
            layout.AddMarks(major, axes, xTicks.MajorTicks, true, layout.MajorHalfLength, true);
            layout.AddMarks(major, axes, yTicks.MajorTicks, false, layout.MajorHalfLength, true);
            layout.AddMarks(minor, axes, xTicks.MinorTicks, true, layout.MinorHalfLength, false);
            layout.AddMarks(minor, axes, yTicks.MinorTicks, false, layout.MinorHalfLength, false);

            var occupied = new List<RectangleF>();
            AddOccupied(occupied, axes.XLabelBounds, layout.LabelGap);
            AddOccupied(occupied, axes.YLabelBounds, layout.LabelGap);
            AddOccupied(occupied, PolygonBounds(axes.XArrow), layout.LabelGap);
            AddOccupied(occupied, PolygonBounds(axes.YArrow), layout.LabelGap);
            var lineGap = axes.StrokeWidth / 2 + layout.LabelGap / 2;
            occupied.Add(RectangleF.FromLTRB(axes.Bounds.Left, axes.Origin.Y - lineGap,
                axes.Bounds.Right, axes.Origin.Y + lineGap));
            occupied.Add(RectangleF.FromLTRB(axes.Origin.X - lineGap, axes.Bounds.Top,
                axes.Origin.X + lineGap, axes.Bounds.Bottom));

            // A single zero belongs to the crossing. Give it space before other
            // numbers, then share the occupied rectangles across both axes.
            var zeroPlaced = false;
            foreach (var mark in major.OrderBy(mark => mark.Tick.Value == 0 ? 0 : 1))
            {
                if (mark.Tick.Value == 0 && zeroPlaced)
                    continue;
                var size = measureLabel(mark.Tick.Label);
                if (!ValidSize(size) || size.Width > axes.SafeBounds.Width ||
                    size.Height > axes.SafeBounds.Height)
                    continue;

                var crossingZero = mark.Tick.Value == 0 && xMin <= 0 && xMax >= 0 &&
                    yMin <= 0 && yMax >= 0;
                foreach (var candidate in layout.Candidates(mark, axes, size, crossingZero))
                {
                    if (!Contains(axes.SafeBounds, candidate) ||
                        occupied.Any(rect => rect.IntersectsWith(candidate)))
                        continue;
                    mark.LabelBounds = candidate;
                    AddOccupied(occupied, candidate, layout.LabelGap);
                    zeroPlaced |= mark.Tick.Value == 0;
                    break;
                }
            }
            layout.MajorTicks = major.AsReadOnly();
            layout.MinorTicks = minor.AsReadOnly();
            return layout;
        }

        private static AxisTickSet Generate(double min, double max, double length, double spacing,
            bool horizontal, Func<string, SizeF> measureLabel, CultureInfo culture, float scale)
        {
            var ticks = AxisTickGenerator.Create(min, max, length, spacing, culture);
            // Recalculate intervals using real glyph widths/heights, rather than
            // shrinking text to accommodate long scientific-notation labels.
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var measured = ticks.MajorTicks.Select(tick => measureLabel(tick.Label))
                    .Where(ValidSize).Select(size => horizontal ? size.Width : size.Height)
                    .DefaultIfEmpty(0).Max();
                var required = measured + 12 * scale;
                if (required <= spacing)
                    break;
                spacing = required;
                ticks = AxisTickGenerator.Create(min, max, length, spacing, culture);
            }
            return ticks;
        }

        private void AddMarks(List<Mark> destination, ComplexAxesLayout axes,
            IReadOnlyList<AxisTick> ticks, bool horizontal, float halfLength, bool major)
        {
            foreach (var tick in ticks)
            {
                var position = horizontal
                    ? axes.Bounds.Left + (float)(tick.Fraction * axes.Bounds.Width)
                    : axes.Bounds.Top + (float)((1 - tick.Fraction) * axes.Bounds.Height);
                position = horizontal
                    ? Clamp(position, axes.Bounds.Left, axes.Bounds.Right - 1)
                    : Clamp(position, axes.Bounds.Top, axes.Bounds.Bottom - 1);
                var mark = new Mark
                {
                    Tick = tick,
                    HorizontalAxis = horizontal,
                    Start = horizontal ? new PointF(position, Math.Max(axes.Bounds.Top, axes.Origin.Y - halfLength))
                        : new PointF(Math.Max(axes.Bounds.Left, axes.Origin.X - halfLength), position),
                    End = horizontal ? new PointF(position, Math.Min(axes.Bounds.Bottom - 1, axes.Origin.Y + halfLength))
                        : new PointF(Math.Min(axes.Bounds.Right - 1, axes.Origin.X + halfLength), position)
                };
                var ink = RectangleF.FromLTRB(Math.Min(mark.Start.X, mark.End.X), Math.Min(mark.Start.Y, mark.End.Y),
                    Math.Max(mark.Start.X, mark.End.X), Math.Max(mark.Start.Y, mark.End.Y));
                ink.Inflate(axes.StrokeWidth, axes.StrokeWidth);
                if (axes.ArrowLength > 0 && (ink.IntersectsWith(PolygonBounds(axes.XArrow)) ||
                    ink.IntersectsWith(PolygonBounds(axes.YArrow))))
                {
                    // The arrow already marks its endpoint. Keep the numerical
                    // candidate, including zero at a boundary crossing.
                    if (!major)
                        continue;
                    mark.DrawMark = false;
                }
                destination.Add(mark);
            }
        }

        private IEnumerable<RectangleF> Candidates(Mark mark, ComplexAxesLayout axes,
            SizeF size, bool crossingZero)
        {
            var clearance = MajorHalfLength + LabelGap;
            if (crossingZero)
            {
                // At a boundary the zero may share a corner with arrowheads and
                // coordinate names. Include positions beyond their actual ink,
                // preferring the nearest clear one; never move the axis or tick.
                var headClearance = Math.Max(clearance, axes.ArrowLength + 2 * LabelGap);
                var xs = new List<float>();
                var ys = new List<float>();
                foreach (var offset in new[] { clearance, headClearance })
                {
                    xs.Add(axes.Origin.X - offset - size.Width);
                    xs.Add(axes.Origin.X + offset);
                    ys.Add(axes.Origin.Y + offset);
                    ys.Add(axes.Origin.Y - offset - size.Height);
                }
                foreach (var name in new[] { axes.XLabelBounds, axes.YLabelBounds })
                {
                    if (name.IsEmpty) continue;
                    xs.Add(name.Left - 2 * LabelGap - size.Width);
                    xs.Add(name.Right + 2 * LabelGap);
                    ys.Add(name.Top - 2 * LabelGap - size.Height);
                    ys.Add(name.Bottom + 2 * LabelGap);
                }
                foreach (var candidate in xs.SelectMany(x => ys.Select(y =>
                    new RectangleF(x, y, size.Width, size.Height))).OrderBy(rect =>
                    Math.Pow(rect.Left + rect.Width / 2 - axes.Origin.X, 2) +
                    Math.Pow(rect.Top + rect.Height / 2 - axes.Origin.Y, 2)))
                    yield return candidate;
            }
            else if (mark.HorizontalAxis)
            {
                var x = Clamp(mark.Start.X - size.Width / 2,
                    axes.SafeBounds.Left, axes.SafeBounds.Right - size.Width);
                yield return new RectangleF(x, axes.Origin.Y + clearance, size.Width, size.Height);
                yield return new RectangleF(x, axes.Origin.Y - clearance - size.Height, size.Width, size.Height);
            }
            else
            {
                var y = Clamp(mark.Start.Y - size.Height / 2,
                    axes.SafeBounds.Top, axes.SafeBounds.Bottom - size.Height);
                yield return new RectangleF(axes.Origin.X - clearance - size.Width, y, size.Width, size.Height);
                yield return new RectangleF(axes.Origin.X + clearance, y, size.Width, size.Height);
            }
        }

        private static void AddOccupied(List<RectangleF> occupied, RectangleF rectangle, float gap)
        {
            if (rectangle.IsEmpty)
                return;
            rectangle.Inflate(gap, gap);
            occupied.Add(rectangle);
        }

        private static RectangleF PolygonBounds(PointF[] points)
        {
            return RectangleF.FromLTRB(points.Min(point => point.X), points.Min(point => point.Y),
                points.Max(point => point.X), points.Max(point => point.Y));
        }

        private static bool Contains(RectangleF outer, RectangleF inner)
        {
            return inner.Left >= outer.Left && inner.Top >= outer.Top &&
                inner.Right <= outer.Right && inner.Bottom <= outer.Bottom;
        }

        private static bool ValidSize(SizeF size)
        {
            return float.IsFinite(size.Width) && float.IsFinite(size.Height) &&
                size.Width > 0 && size.Height > 0;
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
