#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Computator.NET.Charting.Axes;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    internal sealed class RenderedAxisTick
    {
        public int Axis { get; init; }
        public double Value { get; init; }
        public bool IsMajor { get; init; }
        public Point Position { get; init; }
        public Point Start { get; init; }
        public Point End { get; init; }
        public double StrokeWidth => IsMajor ? 1.4 : 0.8;
    }

    internal sealed class RenderedAxisLabel
    {
        public int Axis { get; init; }
        public double? Value { get; init; }
        public string Text { get; init; }
        public Point Position { get; init; }
        public Rect Bounds { get; init; }
        public bool IsAxisName => !Value.HasValue;
        internal Geometry Geometry { get; init; }
    }

    // WPF uses logical device-independent pixels here. Only the projection moves
    // the anchors; the numerical text stays upright and retains its readable size.
    internal sealed class Chart3DAxisAnnotations : FrameworkElement
    {
        private const double LabelGap = 4;
        private readonly List<RenderedAxisTick> _ticks = new List<RenderedAxisTick>();
        private readonly List<RenderedAxisLabel> _labels = new List<RenderedAxisLabel>();
        private DrawingGroup _drawing;

        internal Action DpiChanged { get; set; }
        internal IReadOnlyList<RenderedAxisTick> TickMarks => _ticks;
        internal IReadOnlyList<RenderedAxisLabel> Labels => _labels;

        internal void Update(IReadOnlyList<Chart3DAxis> axes, TransformMatrix transform,
            Viewport3D viewport, Size size, string[] names, Point[] namePositions,
            bool[] activeNames, bool activeLabels, bool activeAxes, Point offset,
            FontFamily family, double fontSize, FontStyle style, FontWeight weight,
            Color labelColor, Color axesColor)
        {
            _ticks.Clear();
            _labels.Clear();
            _drawing = new DrawingGroup();
            if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) ||
                size.Width < 16 || size.Height < 16)
            {
                InvalidateVisual();
                return;
            }

            Width = size.Width;
            Height = size.Height;
            var safe = new Rect(3, 3, Math.Max(1, size.Width - 6), Math.Max(1, size.Height - 6));
            var typeface = new Typeface(family ?? new FontFamily("Arial"), style, weight, FontStretches.Normal);
            var labelBrush = new SolidColorBrush(labelColor);
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var effectiveFontSize = double.IsFinite(fontSize) && fontSize > 0 ? fontSize : 22;
            var tickFontSize = Math.Max(9, Math.Min(18, effectiveFontSize * 0.6));
            var projections = new List<ProjectedAxis>();
            if (activeAxes && axes != null && transform != null && viewport != null)
            {
                foreach (var axis in axes)
                {
                    var start = transform.VertexToScreenPt(axis.Start, viewport);
                    var end = transform.VertexToScreenPt(axis.End, viewport);
                    if (Finite(start) && Finite(end) && double.IsFinite(axis.Minimum) && double.IsFinite(axis.Maximum))
                        projections.Add(new ProjectedAxis(axis, start, end, size, transform, viewport));
                }
            }

            // Reserve actual name ink first so tick labels cannot collide with a
            // configured axis name, even when two projected axes meet near an edge.
            if (activeLabels)
            {
                for (var index = 0; index < 3; index++)
                {
                    if (!activeNames[index] || string.IsNullOrWhiteSpace(names[index]) || !Finite(namePositions[index]))
                        continue;
                    var projected = projections.FirstOrDefault(axis => axis.Axis.Index == index);
                    var normal = projected?.Normal ?? new Vector(0, 1);
                    var sizeForName = effectiveFontSize;
                    for (var attempt = 0; attempt < 8; attempt++)
                    {
                        var geometry = TextGeometry(names[index], typeface, sizeForName, labelBrush, pixelsPerDip);
                        if (TryPlace(index, null, names[index], geometry, namePositions[index], normal,
                            safe, projections, offset))
                            break;
                        sizeForName *= 0.8;
                    }
                }
            }

            foreach (var axis in projections)
            {
                // A collapsed coordinate (for example z=0 on a flat surface) has
                // one meaningful value. A noncollapsed axis viewed end-on has no
                // distinct screen positions, so it cannot carry a numeric scale.
                if (axis.Axis.Minimum == axis.Axis.Maximum)
                {
                    if (safe.Contains(axis.Start) && activeLabels && activeNames[axis.Axis.Index])
                    {
                        if (axis.Axis.Minimum == 0 && _labels.Any(label => label.Value == 0 &&
                            (label.Position - axis.Start).Length < 8)) continue;
                        var text = AxisTickGenerator.Format(axis.Axis.Minimum, 0, CultureInfo.CurrentCulture);
                        var geometry = TextGeometry(text, typeface, tickFontSize, labelBrush, pixelsPerDip);
                        TryPlace(axis.Axis.Index, axis.Axis.Minimum, text, geometry, axis.Start, axis.Normal,
                            safe, projections, null, axis.ArrowBounds.IsEmpty ? 0 :
                            Math.Max(axis.ArrowBounds.Width, axis.ArrowBounds.Height));
                    }
                    continue;
                }
                if (axis.Length < 16 || !ClipSegment(axis.Start, axis.End, safe, out var first, out var last))
                    continue;
                var visibleLength = axis.Length * (last - first);
                if (visibleLength < 16) continue;
                var range = axis.Axis.Maximum - axis.Axis.Minimum;
                var minimum = axis.Axis.Minimum + first * range;
                var maximum = axis.Axis.Minimum + last * range;
                var ticks = AxisTickGenerator.Create(minimum, maximum, visibleLength, 64, CultureInfo.CurrentCulture);
                // Include the actual formatted text's extent along the projected
                // axis, rather than assuming all numerical labels have one width.
                var spacing = 64.0;
                foreach (var tick in ticks.MajorTicks)
                {
                    var ink = TextGeometry(tick.Label, typeface, tickFontSize, labelBrush, pixelsPerDip).Bounds;
                    spacing = Math.Max(spacing,
                        Math.Abs(axis.Tangent.X) * ink.Width + Math.Abs(axis.Tangent.Y) * ink.Height + 24);
                }
                if (spacing > 65)
                    ticks = AxisTickGenerator.Create(minimum, maximum, visibleLength, spacing, CultureInfo.CurrentCulture);
                foreach (var tick in ticks.MinorTicks) AddTick(axis, tick.Value, false, null);
                // Prefer a zero at a shared origin, then place the other values.
                foreach (var tick in ticks.MajorTicks.OrderBy(tick => tick.Value == 0 ? 0 : 1).ThenBy(tick => tick.Value))
                    AddTick(axis, tick.Value, true, tick.Label);
            }

            using (var drawing = _drawing.Open())
            {
                drawing.PushClip(new RectangleGeometry(new Rect(new Point(), size)));
                foreach (var tick in _ticks)
                {
                    var color = tick.IsMajor ? axesColor :
                        Color.FromArgb((byte)(axesColor.A * 0.65), axesColor.R, axesColor.G, axesColor.B);
                    var halo = new Pen(new SolidColorBrush(Contrast(color)), tick.StrokeWidth + 1.2);
                    var pen = new Pen(new SolidColorBrush(color), tick.StrokeWidth);
                    drawing.DrawLine(halo, tick.Start, tick.End);
                    drawing.DrawLine(pen, tick.Start, tick.End);
                }
                var labelOutline = new Pen(new SolidColorBrush(Contrast(labelColor)), 1.4)
                {
                    LineJoin = PenLineJoin.Round
                };
                foreach (var label in _labels)
                    drawing.DrawGeometry(labelBrush, labelOutline, label.Geometry);
                drawing.Pop();
            }
            InvalidateVisual();

            void AddTick(ProjectedAxis axis, double value, bool major, string text)
            {
                // Use the production world transform for each marker; do not
                // approximate it using data array indices or another camera.
                var position = transform.VertexToScreenPt(axis.Axis.At(value), viewport);
                if (!Finite(position) || !safe.Contains(position)) return;
                if (_ticks.Any(tick => (tick.Position - position).Length < 1 && (tick.IsMajor || !major)))
                    return;
                var halfLength = major ? 4.0 : 2.0;
                var start = position - axis.Normal * halfLength;
                var end = position + axis.Normal * halfLength;
                if (projections.Any(projected => !projected.ArrowBounds.IsEmpty &&
                    ClipSegment(start, end, projected.ArrowBounds, out _, out _))) return;
                if (_labels.Any(label => ClipSegment(start, end, label.Bounds, out _, out _))) return;
                _ticks.Add(new RenderedAxisTick
                {
                    Axis = axis.Axis.Index, Value = value, IsMajor = major, Position = position,
                    Start = start, End = end
                });
                if (!major || !activeLabels || !activeNames[axis.Axis.Index]) return;
                if (value == 0 && _labels.Any(label => label.Value == 0 && (label.Position - position).Length < 8))
                    return;
                var geometry = TextGeometry(text, typeface, tickFontSize, labelBrush, pixelsPerDip);
                TryPlace(axis.Axis.Index, value, text, geometry, position, axis.Normal,
                    safe, projections, null);
            }
        }

        internal void Clear()
        {
            _ticks.Clear();
            _labels.Clear();
            _drawing = null;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (_drawing != null) drawingContext.DrawDrawing(_drawing);
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            DpiChanged?.Invoke();
        }

        private bool TryPlace(int axis, double? value, string text, Geometry geometry,
            Point anchor, Vector normal, Rect safe, List<ProjectedAxis> axes, Point? nameOffset,
            double extraClearance = 0)
        {
            var ink = geometry.Bounds;
            if (ink.IsEmpty || ink.Width <= 0 || ink.Height <= 0) return false;
            // Halo and a little whitespace belong to the collision rectangle too.
            var width = ink.Width + 2;
            var height = ink.Height + 2;
            if (width > safe.Width || height > safe.Height) return false;
            var candidates = new List<Rect>();
            if (nameOffset.HasValue)
            {
                var offset = nameOffset.Value;
                candidates.Add(new Rect(anchor.X + offset.X, anchor.Y + offset.Y, width, height));
            }
            var clearance = 4 + LabelGap + extraClearance +
                (Math.Abs(normal.X) * width + Math.Abs(normal.Y) * height) / 2;
            foreach (var side in new[] { 1, -1 })
            {
                var center = anchor + normal * (clearance * side);
                candidates.Add(new Rect(center.X - width / 2, center.Y - height / 2, width, height));
            }
            foreach (var candidate in candidates)
            {
                if (!safe.Contains(candidate)) continue;
                var occupied = candidate;
                occupied.Inflate(LabelGap, LabelGap);
                if (_labels.Any(label => label.Bounds.IntersectsWith(occupied))) continue;
                if (axes.Any(projected => projected.Length > 2 &&
                    ClipSegment(projected.Start, projected.End, candidate, out _, out _))) continue;
                if (axes.Any(projected => projected.ArrowBounds.IntersectsWith(candidate))) continue;
                if (_ticks.Any(tick => ClipSegment(tick.Start, tick.End, candidate, out _, out _))) continue;
                var moved = geometry.Clone();
                moved.Transform = new TranslateTransform(candidate.Left + 1 - ink.Left, candidate.Top + 1 - ink.Top);
                _labels.Add(new RenderedAxisLabel
                {
                    Axis = axis, Value = value, Text = text, Position = anchor,
                    Bounds = candidate, Geometry = moved
                });
                return true;
            }
            return false;
        }

        private static Geometry TextGeometry(string text, Typeface typeface, double size, Brush brush, double pixelsPerDip)
        {
            return new FormattedText(text ?? string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                typeface, size, brush, pixelsPerDip).BuildGeometry(new Point());
        }

        private static Color Contrast(Color color)
        {
            var dark = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B < 128;
            var component = dark ? (byte)255 : (byte)0;
            return Color.FromArgb((byte)(color.A * 0.8), component, component, component);
        }

        private static bool Finite(Point point) => double.IsFinite(point.X) && double.IsFinite(point.Y);

        private static bool ClipSegment(Point start, Point end, Rect bounds, out double first, out double last)
        {
            first = 0;
            last = 1;
            var delta = end - start;
            return ClipSide(-delta.X, start.X - bounds.Left, ref first, ref last) &&
                ClipSide(delta.X, bounds.Right - start.X, ref first, ref last) &&
                ClipSide(-delta.Y, start.Y - bounds.Top, ref first, ref last) &&
                ClipSide(delta.Y, bounds.Bottom - start.Y, ref first, ref last);
        }

        private static bool ClipSide(double denominator, double numerator, ref double first, ref double last)
        {
            if (denominator == 0) return numerator >= 0;
            var ratio = numerator / denominator;
            if (denominator < 0)
            {
                if (ratio > last) return false;
                first = Math.Max(first, ratio);
            }
            else
            {
                if (ratio < first) return false;
                last = Math.Min(last, ratio);
            }
            return true;
        }

        private sealed class ProjectedAxis
        {
            public ProjectedAxis(Chart3DAxis axis, Point start, Point end, Size size,
                TransformMatrix transform, Viewport3D viewport)
            {
                Axis = axis;
                Start = start;
                End = end;
                var direction = end - start;
                Length = direction.Length;
                Tangent = Length > 0 ? direction / Length : new Vector(1, 0);
                Normal = new Vector(-Tangent.Y, Tangent.X);
                var outward = new Vector((start.X + end.X - size.Width) / 2,
                    (start.Y + end.Y - size.Height) / 2);
                if (Vector.Multiply(Normal, outward) < 0) Normal = -Normal;
                var arrow = Rect.Empty;
                foreach (var vertex in axis.ArrowVertices)
                {
                    var point = transform.VertexToScreenPt(vertex, viewport);
                    if (Finite(point)) arrow.Union(point);
                }
                if (!arrow.IsEmpty) arrow.Inflate(2, 2);
                ArrowBounds = arrow;
            }
            public Chart3DAxis Axis { get; }
            public Point Start { get; }
            public Point End { get; }
            public Vector Tangent { get; }
            public Vector Normal { get; }
            public double Length { get; }
            public Rect ArrowBounds { get; }
        }
    }
}
#endif
