#if !__MonoCS__
#nullable disable
using System;
using System.Collections.Generic;

namespace Computator.NET.Charting.Axes
{
    internal readonly struct DepthPoint
    {
        public DepthPoint(double x, double y, double z) { X = x; Y = y; Z = z; }
        public double X { get; }
        public double Y { get; }
        // Orthographic view depth: larger values are closer to the viewer.
        public double Z { get; }

        internal bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
        internal static DepthPoint Interpolate(DepthPoint a, DepthPoint b, double t) =>
            new DepthPoint((1 - t) * a.X + t * b.X, (1 - t) * a.Y + t * b.Y, (1 - t) * a.Z + t * b.Z);
    }

    internal readonly struct DepthInterval
    {
        public DepthInterval(double start, double end) { Start = start; End = end; }
        public double Start { get; }
        public double End { get; }
    }

    // Visibility against the actual projected mesh, not its rectangular bounds.
    // Orthographic projection makes triangle and annotation depth affine in the
    // screen plane, so their intersections can be clipped without raster samples.
    // Rebuild after a view/mesh change; queries allocate no per-triangle objects.
    internal sealed class ProjectedMeshOcclusion
    {
        private const double DepthEpsilon = 1e-9;
        private readonly double _width, _height, _near, _far, _tileSize;
        private readonly int _columns, _rows;
        private readonly List<int>[] _tiles;
        private readonly List<Triangle> _triangles;
        private int[] _visited = Array.Empty<int>();
        private int _queryStamp;

        public ProjectedMeshOcclusion(double width, double height, double nearDepth, double farDepth, int triangleCapacity = 0)
        {
            if (!double.IsFinite(width) || width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (!double.IsFinite(height) || height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (!double.IsFinite(nearDepth)) throw new ArgumentOutOfRangeException(nameof(nearDepth));
            if (!double.IsFinite(farDepth) || farDepth > nearDepth) throw new ArgumentOutOfRangeException(nameof(farDepth));
            if (triangleCapacity < 0) throw new ArgumentOutOfRangeException(nameof(triangleCapacity));
            _width = width; _height = height; _near = nearDepth; _far = farDepth;
            _triangles = new List<Triangle>(triangleCapacity);
            // Bound index memory even for a very large requested-size export.
            _tileSize = Math.Max(32, Math.Max(width, height) / 128);
            _columns = Math.Max(1, (int)Math.Ceiling(width / _tileSize));
            _rows = Math.Max(1, (int)Math.Ceiling(height / _tileSize));
            _tiles = new List<int>[_columns * _rows];
        }

        internal int TriangleCount => _triangles.Count;

        // Reuse the large mesh and tile buffers while rotating/zooming a view.
        // Only a viewport/camera-plane change requires another spatial index.
        internal void ClearTriangles()
        {
            _triangles.Clear();
            foreach (var tile in _tiles) tile?.Clear();
        }

        public void AddTriangle(DepthPoint a, DepthPoint b, DepthPoint c)
        {
            if (!a.IsFinite || !b.IsFinite || !c.IsFinite) return;
            if (Math.Max(a.X, Math.Max(b.X, c.X)) < 0 || Math.Min(a.X, Math.Min(b.X, c.X)) > _width ||
                Math.Max(a.Y, Math.Max(b.Y, c.Y)) < 0 || Math.Min(a.Y, Math.Min(b.Y, c.Y)) > _height ||
                Math.Max(a.Z, Math.Max(b.Z, c.Z)) < _far || Math.Min(a.Z, Math.Min(b.Z, c.Z)) > _near) return;

            if (InView(a) && InView(b) && InView(c))
            {
                StoreTriangle(a, b, c);
                return;
            }

            // Clipping is necessary: simply discarding a triangle with one
            // vertex outside the camera planes leaves a false visibility hole.
            Span<DepthPoint> polygon = stackalloc DepthPoint[16];
            Span<DepthPoint> scratch = stackalloc DepthPoint[16];
            polygon[0] = a; polygon[1] = b; polygon[2] = c;
            var count = 3;
            count = Clip(polygon, count, scratch, 0, 0, true); Swap(ref polygon, ref scratch);
            count = Clip(polygon, count, scratch, 0, _width, false); Swap(ref polygon, ref scratch);
            count = Clip(polygon, count, scratch, 1, 0, true); Swap(ref polygon, ref scratch);
            count = Clip(polygon, count, scratch, 1, _height, false); Swap(ref polygon, ref scratch);
            count = Clip(polygon, count, scratch, 2, _far, true); Swap(ref polygon, ref scratch);
            count = Clip(polygon, count, scratch, 2, _near, false); Swap(ref polygon, ref scratch);
            for (var i = 1; i + 1 < count; i++) StoreTriangle(polygon[0], polygon[i], polygon[i + 1]);
        }

        public bool IsVisible(DepthPoint point)
        {
            if (!point.IsFinite || !InView(point)) return false;
            var candidates = Candidates(point.X, point.Y, point.X, point.Y);
            while (candidates.Next(out var triangle))
                if (triangle.MaxZ > point.Z + DepthEpsilon && triangle.Contains(point.X, point.Y) &&
                    triangle.DepthAt(point.X, point.Y) > point.Z + DepthEpsilon) return false;
            return true;
        }

        // Labels are kept whole for legibility. Check the entire final ink/halo
        // rectangle at its world anchor's billboard depth, including triangles
        // wholly inside the rectangle (corner sampling would miss those).
        public bool IsRectangleVisible(double left, double top, double width, double height, double depth)
        {
            var right = left + width;
            var bottom = top + height;
            if (!double.IsFinite(left) || !double.IsFinite(top) || !double.IsFinite(right) ||
                !double.IsFinite(bottom) || !double.IsFinite(depth) || width <= 0 || height <= 0 ||
                left < 0 || top < 0 || right > _width || bottom > _height || depth < _far || depth > _near) return false;

            var candidates = Candidates(left, top, right, bottom);
            Span<DepthPoint> polygon = stackalloc DepthPoint[16];
            Span<DepthPoint> scratch = stackalloc DepthPoint[16];
            while (candidates.Next(out var triangle))
            {
                if (triangle.MaxZ <= depth + DepthEpsilon || !triangle.Intersects(left, top, right, bottom)) continue;
                polygon[0] = triangle.A; polygon[1] = triangle.B; polygon[2] = triangle.C;
                var count = 3;
                count = Clip(polygon, count, scratch, 0, left, true); Swap(ref polygon, ref scratch);
                count = Clip(polygon, count, scratch, 0, right, false); Swap(ref polygon, ref scratch);
                count = Clip(polygon, count, scratch, 1, top, true); Swap(ref polygon, ref scratch);
                count = Clip(polygon, count, scratch, 1, bottom, false); Swap(ref polygon, ref scratch);
                count = Clip(polygon, count, scratch, 2, depth + DepthEpsilon, true); Swap(ref polygon, ref scratch);
                if (HasArea(polygon, count)) return false;
            }
            return true;
        }

        // Returned parameters refer to the original uncut segment. A finite
        // stroke is a camera-facing ribbon with the centerline's affine depth.
        // Hide the entire cross-section wherever any of it is occluded, avoiding
        // colored fringes at silhouettes while retaining crisp, flat line caps.
        public IReadOnlyList<DepthInterval> VisibleSegments(DepthPoint start, DepthPoint end, double strokeRadius = 0)
        {
            if (!start.IsFinite || !end.IsFinite || !double.IsFinite(strokeRadius) || strokeRadius < 0)
                return Array.Empty<DepthInterval>();
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var dz = end.Z - start.Z;
            var lengthSquared = dx * dx + dy * dy;
            if (!double.IsFinite(lengthSquared) || lengthSquared <= 1e-24 || !double.IsFinite(dz))
                return Array.Empty<DepthInterval>();
            var first = 0.0;
            var last = 1.0;
            if (!ClipLine(start.X, dx, ref first, ref last) ||
                !ClipLine(_width - start.X, -dx, ref first, ref last) ||
                !ClipLine(start.Y, dy, ref first, ref last) ||
                !ClipLine(_height - start.Y, -dy, ref first, ref last) ||
                !ClipLine(start.Z - _far, dz, ref first, ref last) ||
                !ClipLine(_near - start.Z, -dz, ref first, ref last) || last <= first)
                return Array.Empty<DepthInterval>();

            var clippedStart = DepthPoint.Interpolate(start, end, first);
            var clippedEnd = DepthPoint.Interpolate(start, end, last);
            var minX = Math.Min(clippedStart.X, clippedEnd.X) - strokeRadius;
            var maxX = Math.Max(clippedStart.X, clippedEnd.X) + strokeRadius;
            var minY = Math.Min(clippedStart.Y, clippedEnd.Y) - strokeRadius;
            var maxY = Math.Max(clippedStart.Y, clippedEnd.Y) + strokeRadius;
            var minZ = Math.Min(clippedStart.Z, clippedEnd.Z);
            var candidates = Candidates(minX, minY, maxX, maxY);
            var hidden = new List<DepthInterval>();
            var length = Math.Sqrt(lengthSquared);
            Span<DepthPoint> polygon = stackalloc DepthPoint[16];
            Span<DepthPoint> scratch = stackalloc DepthPoint[16];
            while (candidates.Next(out var triangle))
            {
                if (triangle.MaxZ <= minZ + DepthEpsilon || !triangle.Intersects(minX, minY, maxX, maxY)) continue;
                var hiddenFirst = first;
                var hiddenLast = last;
                if (strokeRadius == 0)
                {
                    if (!triangle.ClipLine(start, dx, dy, ref hiddenFirst, ref hiddenLast) ||
                        !ClipLine(triangle.DepthAt(start.X, start.Y) - start.Z - DepthEpsilon,
                            triangle.DepthX * dx + triangle.DepthY * dy - dz, ref hiddenFirst, ref hiddenLast)) continue;
                }
                else
                {
                    polygon[0] = InRibbon(triangle.A, start, dx, dy, dz, lengthSquared, length);
                    polygon[1] = InRibbon(triangle.B, start, dx, dy, dz, lengthSquared, length);
                    polygon[2] = InRibbon(triangle.C, start, dx, dy, dz, lengthSquared, length);
                    var count = 3;
                    count = Clip(polygon, count, scratch, 0, first, true); Swap(ref polygon, ref scratch);
                    count = Clip(polygon, count, scratch, 0, last, false); Swap(ref polygon, ref scratch);
                    count = Clip(polygon, count, scratch, 1, -strokeRadius, true); Swap(ref polygon, ref scratch);
                    count = Clip(polygon, count, scratch, 1, strokeRadius, false); Swap(ref polygon, ref scratch);
                    count = Clip(polygon, count, scratch, 2, 0, true); Swap(ref polygon, ref scratch);
                    if (!HasArea(polygon, count)) continue;
                    hiddenFirst = hiddenLast = polygon[0].X;
                    for (var i = 1; i < count; i++)
                    {
                        hiddenFirst = Math.Min(hiddenFirst, polygon[i].X);
                        hiddenLast = Math.Max(hiddenLast, polygon[i].X);
                    }
                }
                if (hiddenLast > hiddenFirst) hidden.Add(new DepthInterval(hiddenFirst, hiddenLast));
            }

            hidden.Sort((a, b) => a.Start.CompareTo(b.Start));
            var visible = new List<DepthInterval>();
            var cursor = first;
            // Adjacent mesh triangles can calculate their shared boundary a few
            // ULPs apart. Join those numerical seams, not real subpixel gaps.
            var parameterTolerance = 1e-10 / length + 32 * 2.2204460492503131e-16;
            foreach (var interval in hidden)
            {
                if (interval.Start - cursor > parameterTolerance) visible.Add(new DepthInterval(cursor, interval.Start));
                cursor = Math.Max(cursor, interval.End);
                if (cursor >= last) break;
            }
            if (last - cursor > parameterTolerance) visible.Add(new DepthInterval(cursor, last));
            return visible;
        }

        private static DepthPoint InRibbon(DepthPoint point, DepthPoint start, double dx, double dy,
            double dz, double lengthSquared, double length)
        {
            var x = point.X - start.X;
            var y = point.Y - start.Y;
            var u = (x * dx + y * dy) / lengthSquared;
            return new DepthPoint(u, (dx * y - dy * x) / length, point.Z - start.Z - u * dz - DepthEpsilon);
        }

        private bool InView(DepthPoint point) => point.X >= 0 && point.X <= _width &&
            point.Y >= 0 && point.Y <= _height && point.Z >= _far && point.Z <= _near;

        private void StoreTriangle(DepthPoint a, DepthPoint b, DepthPoint c)
        {
            var triangle = new Triangle(a, b, c);
            if (!triangle.IsValid) return;
            var id = _triangles.Count;
            _triangles.Add(triangle);
            var left = Column(triangle.MinX);
            var right = Column(triangle.MaxX);
            var top = Row(triangle.MinY);
            var bottom = Row(triangle.MaxY);
            for (var y = top; y <= bottom; y++)
                for (var x = left; x <= right; x++)
                {
                    var index = y * _columns + x;
                    (_tiles[index] ??= new List<int>()).Add(id);
                }
        }

        private int Column(double x) => (int)Math.Max(0, Math.Min(_columns - 1, Math.Floor(x / _tileSize)));
        private int Row(double y) => (int)Math.Max(0, Math.Min(_rows - 1, Math.Floor(y / _tileSize)));

        private CandidateEnumerator Candidates(double left, double top, double right, double bottom)
        {
            if (_visited.Length < _triangles.Count) Array.Resize(ref _visited, _triangles.Count);
            if (_queryStamp == int.MaxValue) { Array.Clear(_visited); _queryStamp = 0; }
            return new CandidateEnumerator(this, ++_queryStamp, Column(left), Row(top), Column(right), Row(bottom));
        }

        private struct CandidateEnumerator
        {
            private readonly ProjectedMeshOcclusion _owner;
            private readonly int _stamp, _left, _right, _bottom;
            private int _x, _y, _index;
            internal CandidateEnumerator(ProjectedMeshOcclusion owner, int stamp, int left, int top, int right, int bottom)
            {
                _owner = owner; _stamp = stamp; _left = left; _right = right; _bottom = bottom;
                _x = left; _y = top; _index = 0;
            }
            internal bool Next(out Triangle triangle)
            {
                while (_y <= _bottom)
                {
                    var tile = _owner._tiles[_y * _owner._columns + _x];
                    while (tile != null && _index < tile.Count)
                    {
                        var id = tile[_index++];
                        if (_owner._visited[id] == _stamp) continue;
                        _owner._visited[id] = _stamp;
                        triangle = _owner._triangles[id];
                        return true;
                    }
                    _index = 0;
                    if (++_x > _right) { _x = _left; _y++; }
                }
                triangle = default;
                return false;
            }
        }

        private readonly struct Triangle
        {
            internal readonly DepthPoint A, B, C;
            internal readonly double MinX, MinY, MaxX, MaxY, MaxZ, DepthX, DepthY;
            private readonly double _sign;
            internal readonly bool IsValid;

            internal Triangle(DepthPoint a, DepthPoint b, DepthPoint c)
            {
                A = a; B = b; C = c;
                MinX = Math.Min(a.X, Math.Min(b.X, c.X)); MaxX = Math.Max(a.X, Math.Max(b.X, c.X));
                MinY = Math.Min(a.Y, Math.Min(b.Y, c.Y)); MaxY = Math.Max(a.Y, Math.Max(b.Y, c.Y));
                MaxZ = Math.Max(a.Z, Math.Max(b.Z, c.Z));
                var bx = b.X - a.X; var by = b.Y - a.Y;
                var cx = c.X - a.X; var cy = c.Y - a.Y;
                var area = bx * cy - by * cx;
                _sign = area < 0 ? -1 : 1;
                DepthX = ((b.Z - a.Z) * cy - (c.Z - a.Z) * by) / area;
                DepthY = (bx * (c.Z - a.Z) - cx * (b.Z - a.Z)) / area;
                IsValid = area != 0 && double.IsFinite(area) && double.IsFinite(DepthX) && double.IsFinite(DepthY);
            }

            internal double DepthAt(double x, double y) => A.Z + DepthX * (x - A.X) + DepthY * (y - A.Y);
            internal bool Intersects(double left, double top, double right, double bottom) =>
                MaxX >= left && MinX <= right && MaxY >= top && MinY <= bottom;
            internal bool Contains(double x, double y) => Intersects(x, y, x, y) &&
                Edge(A, B, x, y) >= 0 && Edge(B, C, x, y) >= 0 && Edge(C, A, x, y) >= 0;
            private double Edge(DepthPoint a, DepthPoint b, double x, double y) =>
                _sign * ((b.X - a.X) * (y - a.Y) - (b.Y - a.Y) * (x - a.X));

            internal bool ClipLine(DepthPoint start, double dx, double dy, ref double first, ref double last) =>
                ClipEdge(A, B, start, dx, dy, ref first, ref last) &&
                ClipEdge(B, C, start, dx, dy, ref first, ref last) &&
                ClipEdge(C, A, start, dx, dy, ref first, ref last);
            private bool ClipEdge(DepthPoint a, DepthPoint b, DepthPoint start, double dx, double dy,
                ref double first, ref double last) =>
                ProjectedMeshOcclusion.ClipLine(Edge(a, b, start.X, start.Y),
                    _sign * ((b.X - a.X) * dy - (b.Y - a.Y) * dx), ref first, ref last);
        }

        // Keep the half-plane value + parameter * slope >= 0.
        private static bool ClipLine(double value, double slope, ref double first, ref double last)
        {
            if (slope == 0) return value >= 0;
            var t = -value / slope;
            if (slope > 0) first = Math.Max(first, t);
            else last = Math.Min(last, t);
            return first <= last;
        }

        private static void Swap(ref Span<DepthPoint> a, ref Span<DepthPoint> b)
        {
            var temporary = a; a = b; b = temporary;
        }

        private static int Clip(Span<DepthPoint> input, int count, Span<DepthPoint> output,
            int coordinate, double boundary, bool keepAbove)
        {
            if (count == 0) return 0;
            var outputCount = 0;
            var previous = input[count - 1];
            var previousDistance = Distance(previous, coordinate, boundary, keepAbove);
            for (var i = 0; i < count; i++)
            {
                var current = input[i];
                var currentDistance = Distance(current, coordinate, boundary, keepAbove);
                if ((previousDistance >= 0) != (currentDistance >= 0))
                {
                    var scale = Math.Max(Math.Abs(previousDistance), Math.Abs(currentDistance));
                    var t = (previousDistance / scale) / (previousDistance / scale - currentDistance / scale);
                    output[outputCount++] = DepthPoint.Interpolate(previous, current, t);
                }
                if (currentDistance >= 0) output[outputCount++] = current;
                previous = current;
                previousDistance = currentDistance;
            }
            return outputCount;
        }

        private static double Distance(DepthPoint point, int coordinate, double boundary, bool keepAbove)
        {
            var value = coordinate == 0 ? point.X : coordinate == 1 ? point.Y : point.Z;
            return keepAbove ? value - boundary : boundary - value;
        }

        private static bool HasArea(Span<DepthPoint> polygon, int count)
        {
            if (count < 3) return false;
            var area = 0.0;
            var origin = polygon[0];
            for (var i = 1; i + 1 < count; i++)
                area += (polygon[i].X - origin.X) * (polygon[i + 1].Y - origin.Y) -
                    (polygon[i].Y - origin.Y) * (polygon[i + 1].X - origin.X);
            return Math.Abs(area) > 1e-14;
        }
    }
}
#endif
