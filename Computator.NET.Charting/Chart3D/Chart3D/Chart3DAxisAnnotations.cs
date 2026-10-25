#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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
        public double Depth { get; init; }
        public double StrokeWidth => IsMajor ? 1.4 : 0.8;
    }

    internal sealed class RenderedAxisSegment
    {
        public int Axis { get; init; }
        public Point Start { get; init; }
        public Point End { get; init; }
        public double StartDepth { get; init; }
        public double EndDepth { get; init; }
        public bool IsArrow { get; init; }
        public double StrokeWidth => 1.5;
    }

    internal sealed class RenderedAxisLabel
    {
        public int Axis { get; init; }
        public double? Value { get; init; }
        public string Text { get; init; }
        public Point Position { get; init; }
        public Rect Bounds { get; init; }
        public double Depth { get; init; }
        public bool IsAxisName => !Value.HasValue;
        internal Geometry Geometry { get; init; }
        internal FormattedText TextLayout { get; init; }
        internal Point TextOrigin { get; init; }
    }

    // WPF uses logical device-independent pixels here. Only the projection moves
    // the anchors; the numerical text stays upright and retains its readable size.
    internal sealed class Chart3DAxisAnnotations : FrameworkElement
    {
        private const double LabelGap = 4;
        private readonly List<RenderedAxisTick> _ticks = new List<RenderedAxisTick>();
        private readonly List<RenderedAxisLabel> _labels = new List<RenderedAxisLabel>();
        private readonly List<RenderedAxisSegment> _axisSegments = new List<RenderedAxisSegment>();
        private DrawingGroup _drawing;
        private ProjectedMeshOcclusion _occlusion;
        private MeshGeometry3D _cachedGeometry;
        private Matrix3D _cachedMatrix;
        private Size _cachedSize;
        private double _cachedNear, _cachedFar;
        private bool _cachedBackFaces, _geometryChanged = true;
        private DepthPoint[] _projectedPositions = Array.Empty<DepthPoint>();

        internal Chart3DAxisAnnotations()
        {
            // Grayscale text also renders correctly over an alpha canvas and in
            // bitmap exports, where ClearType's subpixel assumptions do not hold.
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.Grayscale);
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
        }

        internal Action DpiChanged { get; set; }
        internal IReadOnlyList<RenderedAxisTick> TickMarks => _ticks;
        internal IReadOnlyList<RenderedAxisLabel> Labels => _labels;
        internal IReadOnlyList<RenderedAxisSegment> AxisSegments => _axisSegments;

        internal void Update(IReadOnlyList<Chart3DAxis> axes, TransformMatrix transform,
            Viewport3D viewport, Size size, string[] names, Point[] namePositions,
            bool[] activeNames, bool activeLabels, bool activeAxes, Point offset,
            FontFamily family, double fontSize, FontStyle style, FontWeight weight,
            Color labelColor, Color axesColor, MeshGeometry3D geometry = null,
            double? outputPixelsPerDip = null, bool occludeBackFaces = true)
        {
            _ticks.Clear();
            _labels.Clear();
            _axisSegments.Clear();
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
            var pixelsPerDip = outputPixelsPerDip ?? VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var effectiveFontSize = double.IsFinite(fontSize) && fontSize > 0 ? fontSize : 22;
            var tickFontSize = Math.Max(9, Math.Min(18, effectiveFontSize * 0.6));
            UpdateOcclusion(geometry, transform, viewport, size, occludeBackFaces);
            var allProjections = new List<ProjectedAxis>();
            if (axes != null && transform != null && viewport != null)
            {
                foreach (var axis in axes)
                {
                    var start = Project(axis.Start, transform, size);
                    var end = Project(axis.End, transform, size);
                    if (Finite(start) && Finite(end) && double.IsFinite(axis.Minimum) && double.IsFinite(axis.Maximum))
                        allProjections.Add(new ProjectedAxis(axis, start, end, size, transform));
                }
            }
            var projections = activeAxes ? allProjections : new List<ProjectedAxis>();
            foreach (var projected in projections)
            {
                AddAxisSegment(projected, projected.Start3D, projected.Tip3D, false);
                if (projected.ArrowBounds.IsEmpty) continue;
                AddAxisSegment(projected, projected.FirstWing, projected.Tip3D, true);
                AddAxisSegment(projected, projected.SecondWing, projected.Tip3D, true);
            }

            // Reserve actual name ink first so tick labels cannot collide with a
            // configured axis name, even when two projected axes meet near an edge.
            if (activeLabels)
            {
                for (var index = 0; index < 3; index++)
                {
                    if (!activeNames[index] || string.IsNullOrWhiteSpace(names[index]) || !Finite(namePositions[index]))
                        continue;
                    var projected = allProjections.FirstOrDefault(axis => axis.Axis.Index == index);
                    var normal = projected?.Normal ?? new Vector(0, 1);
                    var depth = projected?.Tip3D.Z ?? double.NaN;
                    if (projected != null && !Visible(projected.Tip3D)) continue;
                    var sizeForName = effectiveFontSize;
                    var minimumNameSize = Math.Min(effectiveFontSize, tickFontSize);
                    for (var attempt = 0; attempt < 8; attempt++)
                    {
                        var textLayout = TextLayout(names[index], typeface, sizeForName, labelBrush, pixelsPerDip);
                        if (TryPlace(index, null, names[index], textLayout, namePositions[index], normal,
                            safe, projections, offset, depth, nameDirection: projected?.Tangent))
                            break;
                        // A tiny surviving fragment is not a usable axis name.
                        // Keep it at least as large as the numerical text, unless
                        // the user explicitly configured a smaller name size.
                        if (sizeForName <= minimumNameSize) break;
                        sizeForName = Math.Max(minimumNameSize, sizeForName * 0.8);
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
                    if (safe.Contains(axis.Start) && Visible(axis.Start3D) && activeLabels && activeNames[axis.Axis.Index])
                    {
                        if (axis.Axis.Minimum == 0 && _labels.Any(label => label.Value == 0 &&
                            (label.Position - axis.Start).Length < 8)) continue;
                        var text = AxisTickGenerator.Format(axis.Axis.Minimum, 0, CultureInfo.CurrentCulture);
                        var textLayout = TextLayout(text, typeface, tickFontSize, labelBrush, pixelsPerDip);
                        TryPlace(axis.Axis.Index, axis.Axis.Minimum, text, textLayout, axis.Start, axis.Normal,
                            safe, projections, null, axis.Start3D.Z, axis.ArrowBounds.IsEmpty ? 0 :
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
                    var ink = TextLayout(tick.Label, typeface, tickFontSize, labelBrush, pixelsPerDip)
                        .BuildGeometry(new Point()).Bounds;
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
                var axisPen = new Pen(new SolidColorBrush(axesColor), 1.5);
                foreach (var segment in _axisSegments)
                    drawing.DrawLine(axisPen, segment.Start, segment.End);
                foreach (var tick in _ticks)
                {
                    var color = tick.IsMajor ? axesColor :
                        Color.FromArgb((byte)(axesColor.A * 0.65), axesColor.R, axesColor.G, axesColor.B);
                    var pen = new Pen(new SolidColorBrush(color), tick.StrokeWidth);
                    drawing.DrawLine(pen, tick.Start, tick.End);
                }
                var labelOutline = new Pen(new SolidColorBrush(Contrast(labelColor)), 1.0)
                {
                    LineJoin = PenLineJoin.Round
                };
                foreach (var label in _labels)
                {
                    // A restrained contrast edge keeps custom colors readable.
                    // Native text rendering retains font hinting at small sizes;
                    // the geometry is used only for bounds and this light halo.
                    drawing.DrawGeometry(null, labelOutline, label.Geometry);
                    drawing.DrawText(label.TextLayout, label.TextOrigin);
                }
                drawing.Pop();
            }
            InvalidateVisual();

            void AddTick(ProjectedAxis axis, double value, bool major, string text)
            {
                // Use the production world transform for each marker; do not
                // approximate it using data array indices or another camera.
                var anchor = Project(axis.Axis.At(value), transform, size);
                var position = new Point(anchor.X, anchor.Y);
                if (!Finite(position) || !safe.Contains(position)) return;
                if (_ticks.Any(tick => (tick.Position - position).Length < 1 && (tick.IsMajor || !major)))
                    return;
                var halfLength = major ? 4.0 : 2.0;
                var start = position - axis.Normal * halfLength;
                var end = position + axis.Normal * halfLength;
                if (projections.Any(projected => !projected.ArrowBounds.IsEmpty &&
                    ClipSegment(start, end, projected.ArrowBounds, out _, out _))) return;
                if (_labels.Any(label => ClipSegment(start, end, label.Bounds, out _, out _))) return;
                var fragments = _occlusion.VisibleSegments(
                    new DepthPoint(start.X, start.Y, anchor.Z),
                    new DepthPoint(end.X, end.Y, anchor.Z), (major ? 1.4 : 0.8) / 2 + 0.5 / pixelsPerDip);
                foreach (var fragment in fragments)
                {
                    if ((fragment.End - fragment.Start) * (end - start).Length < 0.5) continue;
                    _ticks.Add(new RenderedAxisTick
                    {
                        Axis = axis.Axis.Index, Value = value, IsMajor = major, Position = position,
                        Start = start + (end - start) * fragment.Start,
                        End = start + (end - start) * fragment.End, Depth = anchor.Z
                    });
                }
                if (fragments.Count == 0 || !Visible(anchor) || !major || !activeLabels || !activeNames[axis.Axis.Index]) return;
                if (value == 0 && _labels.Any(label => label.Value == 0 && (label.Position - position).Length < 8))
                    return;
                var textLayout = TextLayout(text, typeface, tickFontSize, labelBrush, pixelsPerDip);
                TryPlace(axis.Axis.Index, value, text, textLayout, position, axis.Normal,
                    safe, projections, null, anchor.Z);
            }

            void AddAxisSegment(ProjectedAxis axis, DepthPoint start, DepthPoint end, bool arrow)
            {
                foreach (var fragment in _occlusion.VisibleSegments(start, end, 0.75 + 0.5 / pixelsPerDip))
                {
                    var first = Interpolate(start, end, fragment.Start);
                    var last = Interpolate(start, end, fragment.End);
                    var from = new Point(first.X, first.Y);
                    var to = new Point(last.X, last.Y);
                    if ((to - from).Length < 0.5) continue;
                    _axisSegments.Add(new RenderedAxisSegment
                    {
                        Axis = axis.Axis.Index, Start = from, End = to,
                        StartDepth = first.Z, EndDepth = last.Z, IsArrow = arrow
                    });
                }
            }
        }

        internal void Clear()
        {
            _ticks.Clear();
            _labels.Clear();
            _axisSegments.Clear();
            _drawing = null;
            if (_cachedGeometry != null && !_cachedGeometry.IsFrozen) _cachedGeometry.Changed -= GeometryChanged;
            _cachedGeometry = null;
            _occlusion = null;
            _projectedPositions = Array.Empty<DepthPoint>();
            _geometryChanged = true;
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

        private bool TryPlace(int axis, double? value, string text, FormattedText textLayout,
            Point anchor, Vector normal, Rect safe, List<ProjectedAxis> axes, Point? nameOffset,
            double depth, double extraClearance = 0, Vector? nameDirection = null)
        {
            var geometry = textLayout.BuildGeometry(new Point());
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
            if (nameDirection.HasValue)
            {
                // At a surface silhouette, a readable name often fits just
                // beyond the arrow even when both perpendicular sides overlap
                // foreground geometry. Preserve the configured offset's priority.
                var direction = nameDirection.Value;
                var distance = LabelGap + 1.5 +
                    (Math.Abs(direction.X) * width + Math.Abs(direction.Y) * height) / 2;
                var center = anchor + direction * distance;
                candidates.Add(new Rect(center.X - width / 2, center.Y - height / 2, width, height));
            }
            foreach (var candidate in candidates)
            {
                if (!safe.Contains(candidate)) continue;
                var occupied = candidate;
                occupied.Inflate(LabelGap, LabelGap);
                if (_labels.Any(label => label.Bounds.IntersectsWith(occupied))) continue;
                if (_axisSegments.Any(segment => ClipSegment(segment.Start, segment.End, candidate, out _, out _))) continue;
                if (axes.Any(projected => projected.ArrowBounds.IntersectsWith(candidate))) continue;
                if (_ticks.Any(tick => ClipSegment(tick.Start, tick.End, candidate, out _, out _))) continue;
                // Labels are billboards at the anchor's depth. Test their entire
                // footprint, including the contrast edge, before accepting a
                // placement; testing the anchor alone leaves text over surfaces.
                if (_occlusion != null && double.IsFinite(depth) &&
                    !_occlusion.IsRectangleVisible(candidate.Left, candidate.Top,
                        candidate.Width, candidate.Height, depth)) continue;
                var moved = geometry.Clone();
                var origin = new Point(candidate.Left + 1 - ink.Left, candidate.Top + 1 - ink.Top);
                moved.Transform = new TranslateTransform(origin.X, origin.Y);
                _labels.Add(new RenderedAxisLabel
                {
                    Axis = axis, Value = value, Text = text, Position = anchor,
                    Bounds = candidate, Geometry = moved, Depth = depth,
                    TextLayout = textLayout, TextOrigin = origin
                });
                return true;
            }
            return false;
        }

        private static FormattedText TextLayout(string text, Typeface typeface, double size, Brush brush, double pixelsPerDip)
        {
            return new FormattedText(text ?? string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                typeface, size, brush, null, TextFormattingMode.Display, pixelsPerDip);
        }

        private static Color Contrast(Color color)
        {
            var dark = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B < 128;
            var component = dark ? (byte)255 : (byte)0;
            return Color.FromArgb((byte)(color.A * 0.45), component, component, component);
        }

        private void GeometryChanged(object sender, EventArgs args) => _geometryChanged = true;

        private void UpdateOcclusion(MeshGeometry3D geometry, TransformMatrix transform,
            Viewport3D viewport, Size size, bool backFaces)
        {
            if (transform == null || viewport == null)
            {
                if (_cachedGeometry != null && !_cachedGeometry.IsFrozen) _cachedGeometry.Changed -= GeometryChanged;
                _cachedGeometry = null;
                _geometryChanged = true;
                _occlusion = null;
                return;
            }

            // This is the same fixed orthographic camera used by
            // TransformMatrix.VertexToScreenPt. Model rotation, pan and zoom are
            // all in m_totalMatrix; larger transformed Z is nearer the camera.
            var camera = viewport.Camera as OrthographicCamera;
            var near = camera == null ? 1 : camera.Position.Z - camera.NearPlaneDistance;
            var far = camera == null ? -8 : camera.Position.Z - camera.FarPlaneDistance;
            var matrix = transform.m_totalMatrix;
            if (_occlusion != null && !_geometryChanged && ReferenceEquals(geometry, _cachedGeometry) &&
                _cachedMatrix == matrix && _cachedSize == size && _cachedNear == near &&
                _cachedFar == far && _cachedBackFaces == backFaces) return;

            var reuseIndex = _occlusion != null && _cachedSize == size &&
                _cachedNear == near && _cachedFar == far;

            if (!ReferenceEquals(geometry, _cachedGeometry))
            {
                if (_cachedGeometry != null && !_cachedGeometry.IsFrozen) _cachedGeometry.Changed -= GeometryChanged;
                _cachedGeometry = geometry;
                if (_cachedGeometry != null && !_cachedGeometry.IsFrozen) _cachedGeometry.Changed += GeometryChanged;
            }
            _cachedMatrix = matrix;
            _cachedSize = size;
            _cachedNear = near;
            _cachedFar = far;
            _cachedBackFaces = backFaces;
            _geometryChanged = false;
            // Reuse triangle/tile storage during rotation instead of allocating
            // a maximum-quality mesh index on every mouse-move event.
            if (reuseIndex) _occlusion.ClearTriangles();
            else _occlusion = new ProjectedMeshOcclusion(size.Width, size.Height, near, far,
                geometry?.TriangleIndices.Count / 3 ?? 0);
            if (geometry == null) return;

            // Project each actual mesh vertex once. The tile index bounds the
            // subsequent annotation queries even for maximum-quality surfaces.
            if (_projectedPositions.Length != geometry.Positions.Count)
                _projectedPositions = new DepthPoint[geometry.Positions.Count];
            var positions = _projectedPositions;
            for (var index = 0; index < positions.Length; index++)
                positions[index] = Project(geometry.Positions[index], transform, size);
            var indices = geometry.TriangleIndices;
            for (var index = 0; index + 2 < indices.Count; index += 3)
            {
                var a = indices[index];
                var b = indices[index + 1];
                var c = indices[index + 2];
                if ((uint)a >= positions.Length || (uint)b >= positions.Length || (uint)c >= positions.Length)
                    continue;
                var first = positions[a];
                var second = positions[b];
                var third = positions[c];
                // Surface BackMaterial is opaque. Scatter meshes have only
                // front faces, whose projected screen winding is clockwise
                // because screen Y increases downwards.
                if (!backFaces && (second.X - first.X) * (third.Y - first.Y) -
                    (second.Y - first.Y) * (third.X - first.X) >= 0) continue;
                _occlusion.AddTriangle(first, second, third);
            }
        }

        private bool Visible(DepthPoint point) => _occlusion == null || _occlusion.IsVisible(point);

        private static DepthPoint Project(Point3D point, TransformMatrix transform, Size size)
        {
            var projected = transform.m_totalMatrix.Transform(point);
            return new DepthPoint(size.Width / 2 + projected.X * size.Width / 2,
                size.Height / 2 - projected.Y * size.Width / 2, projected.Z);
        }

        private static DepthPoint Interpolate(DepthPoint start, DepthPoint end, double amount) =>
            new DepthPoint(start.X + (end.X - start.X) * amount,
                start.Y + (end.Y - start.Y) * amount, start.Z + (end.Z - start.Z) * amount);

        private static bool Finite(DepthPoint point) =>
            double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z);

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
            public ProjectedAxis(Chart3DAxis axis, DepthPoint start, DepthPoint end, Size size,
                TransformMatrix transform)
            {
                Axis = axis;
                Start3D = start;
                End3D = end;
                Start = new Point(start.X, start.Y);
                End = new Point(end.X, end.Y);
                Tip3D = Project(axis.LabelAnchor, transform, size);
                var direction = End - Start;
                Length = direction.Length;
                Tangent = Length > 0 ? direction / Length : new Vector(1, 0);
                Normal = new Vector(-Tangent.Y, Tangent.X);
                var outward = new Vector((Start.X + End.X - size.Width) / 2,
                    (Start.Y + End.Y - size.Height) / 2);
                if (Vector.Multiply(Normal, outward) < 0) Normal = -Normal;
                ArrowBounds = Rect.Empty;
                if (Length >= 16 && axis.Minimum != axis.Maximum && Finite(Tip3D))
                {
                    // Small screen-space chevrons avoid faceted cones and keep
                    // the same visual weight when the plot is zoomed or exported.
                    var tip = new Point(Tip3D.X, Tip3D.Y);
                    var first = tip - Tangent * 9 + Normal * 3.2;
                    var second = tip - Tangent * 9 - Normal * 3.2;
                    var depth = Tip3D.Z - 9 / Length * (end.Z - start.Z);
                    FirstWing = new DepthPoint(first.X, first.Y, depth);
                    SecondWing = new DepthPoint(second.X, second.Y, depth);
                    var arrow = new Rect(tip, first);
                    arrow.Union(second);
                    arrow.Inflate(1.5, 1.5);
                    ArrowBounds = arrow;
                }
            }
            public Chart3DAxis Axis { get; }
            public DepthPoint Start3D { get; }
            public DepthPoint End3D { get; }
            public DepthPoint Tip3D { get; }
            public DepthPoint FirstWing { get; }
            public DepthPoint SecondWing { get; }
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
