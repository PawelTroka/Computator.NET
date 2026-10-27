using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;

namespace Computator.NET.Charting.Chart3D.UI
{
    // Mono uses its existing libgdiplus/System.Drawing stack. No WPF, browser,
    // GPU driver or extra native library is required.
    internal sealed class MonoSurfaceGrid
    {
        internal readonly double[] X;
        internal readonly double[] Y;
        internal readonly double[] Z; // row-major; NaN means a missing vertex
        internal readonly int FiniteCount;

        internal int Size => X.Length;

        internal MonoSurfaceGrid(double[] x, double[] y, double[] z, int finiteCount)
        {
            X = x;
            Y = y;
            Z = z;
            FiniteCount = finiteCount;
        }
    }

    internal static class MonoSurfaceSampler
    {
        internal const int MaximumGridSize = 97;

        internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        internal static bool ValidBounds(double xMin, double xMax, double yMin, double yMax) =>
            IsFinite(xMin) && IsFinite(xMax) && IsFinite(yMin) && IsFinite(yMax) &&
            xMin < xMax && yMin < yMax && IsFinite(xMax - xMin) && IsFinite(yMax - yMin);

        internal static MonoSurfaceGrid Sample(Function function, double xMin, double xMax,
            double yMin, double yMax, int gridSize)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (function.FunctionType != FunctionType.Real3D)
                throw new NotSupportedException("Mono 3D requires an explicit real f(x,y) surface; implicit surfaces are not supported.");
            if (!ValidBounds(xMin, xMax, yMin, yMax))
                throw new ArgumentException("3D axes require finite, increasing x and y bounds.");
            if (gridSize < 9 || gridSize > MaximumGridSize)
                throw new ArgumentOutOfRangeException(nameof(gridSize));

            var x = new double[gridSize];
            var y = new double[gridSize];
            var z = new double[checked(gridSize * gridSize)];
            for (var i = 0; i < gridSize; i++)
            {
                var ratio = i / (double)(gridSize - 1);
                x[i] = i == 0 ? xMin : i == gridSize - 1 ? xMax : (1 - ratio) * xMin + ratio * xMax;
                y[i] = i == 0 ? yMin : i == gridSize - 1 ? yMax : (1 - ratio) * yMin + ratio * yMax;
            }

            var finite = 0;
            for (var row = 0; row < gridSize; row++)
                for (var column = 0; column < gridSize; column++)
                {
                    double height;
                    try { height = function.Evaluate(x[column], y[row]); }
                    catch (ArithmeticException) { height = double.NaN; }
                    catch (CalculationException error) when (error.InnerException is ArithmeticException)
                    { height = double.NaN; }
                    z[row * gridSize + column] = IsFinite(height) ? height : double.NaN;
                    if (IsFinite(height)) finite++;
                }
            return new MonoSurfaceGrid(x, y, z, finite);
        }
    }

    /// <summary>
    /// Bounded, depth-sorted, orthographic software 3D. Both live painting and
    /// exports invoke this same renderer; sampling is independent of viewport size.
    /// </summary>
    internal static class MonoSceneRenderer
    {
        private struct Vertex
        {
            internal PointF Pixel;
            internal double Depth;
            internal bool Valid;
        }

        private sealed class Primitive
        {
            internal PointF[] Polygon;
            internal PointF Center;
            internal double Depth;
            internal int Shade;
            internal bool Point;
        }

        private struct Projection
        {
            internal double XMid, YMid, ZMid, XSpan, YSpan, ZSpan;
            internal double SinYaw, CosYaw, SinTilt, CosTilt;
            internal double Unit, CenterX, CenterY;
        }

        internal static void Draw(Graphics graphics, Rectangle viewport, Font font,
            IList<MonoSurfaceGrid> surfaces, IList<List<Point3D>> pointSeries, Chart3DMode mode,
            double xMin, double xMax, double yMin, double yMax, bool equalAxes,
            double yaw, double tilt, double zoom, double panX, double panY)
        {
            var state = graphics.Save();
            try
            {
                graphics.SetClip(viewport);
                graphics.Clear(Color.White);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                if (viewport.Width < 45 || viewport.Height < 45) return;
                if (!MonoSurfaceSampler.ValidBounds(xMin, xMax, yMin, yMax))
                {
                    DrawMessage(graphics, font, viewport, "Set finite, increasing x/y bounds.");
                    return;
                }

                var zMin = double.PositiveInfinity;
                var zMax = double.NegativeInfinity;
                var finite = 0;
                foreach (var surface in surfaces)
                    foreach (var z in surface.Z)
                        if (MonoSurfaceSampler.IsFinite(z))
                        {
                            zMin = Math.Min(zMin, z);
                            zMax = Math.Max(zMax, z);
                            finite++;
                        }
                foreach (var series in pointSeries)
                    foreach (var point in series)
                        if (MonoSurfaceSampler.IsFinite(point.x) &&
                            MonoSurfaceSampler.IsFinite(point.y) && MonoSurfaceSampler.IsFinite(point.z))
                        {
                            zMin = Math.Min(zMin, point.z);
                            zMax = Math.Max(zMax, point.z);
                            finite++;
                        }
                if (finite == 0)
                {
                    DrawMessage(graphics, font, viewport,
                        surfaces.Count == 0 && pointSeries.Count == 0
                            ? "Enter f(x,y) to display an interactive 3D chart."
                            : "No finite samples in the selected domain.");
                    return;
                }

                // Constant z surfaces retain a visible plane and readable z ticks.
                if (zMin == zMax)
                {
                    zMin -= 1;
                    zMax += 1;
                }
                var spanZ = zMax - zMin;
                if (!MonoSurfaceSampler.IsFinite(spanZ))
                {
                    DrawMessage(graphics, font, viewport, "Surface z range is too large to display.");
                    return;
                }
                var axisScale = Math.Max(xMax - xMin, Math.Max(yMax - yMin, spanZ));
                var projection = new Projection
                {
                    XMid = xMin / 2 + xMax / 2,
                    YMid = yMin / 2 + yMax / 2,
                    ZMid = zMin / 2 + zMax / 2,
                    XSpan = equalAxes ? axisScale : xMax - xMin,
                    YSpan = equalAxes ? axisScale : yMax - yMin,
                    ZSpan = equalAxes ? axisScale : spanZ,
                    SinYaw = Math.Sin(yaw), CosYaw = Math.Cos(yaw),
                    SinTilt = Math.Sin(tilt), CosTilt = Math.Cos(tilt),
                    Unit = Math.Min(viewport.Width, viewport.Height) * .28 * zoom,
                    CenterX = viewport.Left + viewport.Width * .49 + panX,
                    CenterY = viewport.Top + viewport.Height * .53 + panY
                };

                var primitives = new List<Primitive>();
                foreach (var surface in surfaces)
                {
                    var size = surface.Size;
                    var vertices = new Vertex[surface.Z.Length];
                    for (var row = 0; row < size; row++)
                        for (var column = 0; column < size; column++)
                        {
                            var index = row * size + column;
                            var z = surface.Z[index];
                            if (MonoSurfaceSampler.IsFinite(z))
                                vertices[index] = Project(surface.X[column], surface.Y[row], z, projection);
                        }

                    if (mode == Chart3DMode.Surface)
                    {
                        for (var row = 0; row < size - 1; row++)
                            for (var column = 0; column < size - 1; column++)
                            {
                                var a = row * size + column;
                                var b = a + 1;
                                var c = a + size;
                                var d = c + 1;
                                AddTriangle(primitives, vertices, surface.Z, a, b, c, zMin, spanZ);
                                AddTriangle(primitives, vertices, surface.Z, b, d, c, zMin, spanZ);
                            }
                    }
                    else
                        for (var i = 0; i < vertices.Length; i++)
                            if (vertices[i].Valid)
                                primitives.Add(new Primitive
                                {
                                    Point = true, Center = vertices[i].Pixel, Depth = vertices[i].Depth,
                                    Shade = ColorIndex(surface.Z[i], zMin, spanZ)
                                });
                }

                foreach (var series in pointSeries)
                    foreach (var point in series)
                    {
                        if (!MonoSurfaceSampler.IsFinite(point.x) ||
                            !MonoSurfaceSampler.IsFinite(point.y) || !MonoSurfaceSampler.IsFinite(point.z))
                            continue;
                        var vertex = Project(point.x, point.y, point.z, projection);
                        if (vertex.Valid)
                            primitives.Add(new Primitive
                            {
                                Point = true, Center = vertex.Pixel, Depth = vertex.Depth,
                                Shade = ColorIndex(point.z, zMin, spanZ)
                            });
                    }

                // A sorted painter's algorithm avoids external OpenGL dependencies.
                // Orthographic projection and bounded meshes keep allocations finite.
                primitives.Sort((left, right) => left.Depth.CompareTo(right.Depth));
                var palette = CreatePalette();
                try
                {
                    using (var wire = new Pen(Color.FromArgb(48, 31, 41, 55), .7f))
                        foreach (var primitive in primitives)
                        {
                            if (primitive.Point)
                            {
                                const float radius = 3.5f;
                                graphics.FillEllipse(palette[primitive.Shade],
                                    primitive.Center.X - radius, primitive.Center.Y - radius,
                                    2 * radius, 2 * radius);
                            }
                            else
                            {
                                graphics.FillPolygon(palette[primitive.Shade], primitive.Polygon);
                                if (primitives.Count <= 12000)
                                    graphics.DrawPolygon(wire, primitive.Polygon);
                            }
                        }
                }
                finally
                {
                    foreach (var brush in palette) brush.Dispose();
                }

                DrawAxes(graphics, font, projection, xMin, xMax, yMin, yMax, zMin, zMax);
                using (var note = new SolidBrush(Color.FromArgb(78, 91, 112)))
                    graphics.DrawString("Drag: rotate   Wheel: zoom   Right drag: pan   Home: reset",
                        font, note, viewport.Left + 8, viewport.Bottom - font.Height - 8);
            }
            finally { graphics.Restore(state); }
        }

        private static void DrawMessage(Graphics graphics, Font font, Rectangle bounds, string message)
        {
            using (var brush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                graphics.DrawString(message, font, brush, bounds, format);
        }

        private static Vertex Project(double x, double y, double z, Projection view)
        {
            var nx = 2 * (x - view.XMid) / view.XSpan;
            var ny = 2 * (y - view.YMid) / view.YSpan;
            var nz = 2 * (z - view.ZMid) / view.ZSpan;
            var rx = view.CosYaw * nx - view.SinYaw * ny;
            var ry = view.SinYaw * nx + view.CosYaw * ny;
            var px = view.CenterX + view.Unit * rx;
            var py = view.CenterY + view.Unit * (view.SinTilt * ry - view.CosTilt * nz);
            var depth = view.CosTilt * ry + view.SinTilt * nz;
            var valid = MonoSurfaceSampler.IsFinite(px) && MonoSurfaceSampler.IsFinite(py) &&
                MonoSurfaceSampler.IsFinite(depth) && Math.Abs(px) < 10000000 && Math.Abs(py) < 10000000;
            return valid
                ? new Vertex { Pixel = new PointF((float)px, (float)py), Depth = depth, Valid = true }
                : default(Vertex);
        }

        private static void AddTriangle(List<Primitive> primitives, Vertex[] vertices,
            double[] heights, int a, int b, int c, double zMin, double zSpan)
        {
            if (!vertices[a].Valid || !vertices[b].Valid || !vertices[c].Valid) return;
            primitives.Add(new Primitive
            {
                Polygon = new[] { vertices[a].Pixel, vertices[b].Pixel, vertices[c].Pixel },
                Depth = (vertices[a].Depth + vertices[b].Depth + vertices[c].Depth) / 3,
                Shade = ColorIndex(heights[a] / 3 + heights[b] / 3 + heights[c] / 3, zMin, zSpan)
            });
        }

        private static int ColorIndex(double z, double min, double span)
        {
            var relative = (z - min) / span;
            if (double.IsNaN(relative)) return 0;
            return (int)(Math.Max(0, Math.Min(1, relative)) * 63);
        }

        private static Brush[] CreatePalette()
        {
            var stops = new[]
            {
                Color.FromArgb(30, 58, 138), Color.FromArgb(14, 116, 144),
                Color.FromArgb(34, 197, 94), Color.FromArgb(250, 204, 21),
                Color.FromArgb(239, 68, 68)
            };
            var palette = new Brush[64];
            for (var i = 0; i < palette.Length; i++)
            {
                var t = i / 63.0 * (stops.Length - 1);
                var segment = Math.Min(stops.Length - 2, (int)t);
                var ratio = t - segment;
                var from = stops[segment];
                var to = stops[segment + 1];
                palette[i] = new SolidBrush(Color.FromArgb(
                    (int)(from.R + ratio * (to.R - from.R)),
                    (int)(from.G + ratio * (to.G - from.G)),
                    (int)(from.B + ratio * (to.B - from.B))));
            }
            return palette;
        }

        private static void DrawAxes(Graphics graphics, Font font, Projection view,
            double xMin, double xMax, double yMin, double yMax, double zMin, double zMax)
        {
            using (var axisPen = new Pen(Color.FromArgb(71, 85, 105), 1.7f))
            using (var label = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                var origin = Project(xMin, yMin, zMin, view);
                DrawAxis(graphics, font, axisPen, label, origin,
                    t => Project(xMin + (xMax - xMin) * t, yMin, zMin, view), "X", xMin, xMax);
                DrawAxis(graphics, font, axisPen, label, origin,
                    t => Project(xMin, yMin + (yMax - yMin) * t, zMin, view), "Y", yMin, yMax);
                DrawAxis(graphics, font, axisPen, label, origin,
                    t => Project(xMin, yMin, zMin + (zMax - zMin) * t, view), "Z", zMin, zMax);
            }
        }

        private static void DrawAxis(Graphics graphics, Font font, Pen pen, Brush brush,
            Vertex origin, Func<double, Vertex> endpoint, string title, double min, double max)
        {
            var end = endpoint(1);
            if (!origin.Valid || !end.Valid) return;
            graphics.DrawLine(pen, origin.Pixel, end.Pixel);
            for (var tick = 0; tick <= 2; tick++)
            {
                var t = tick / 2.0;
                var position = endpoint(t);
                if (!position.Valid) continue;
                graphics.DrawLine(pen, position.Pixel.X - 2.5f, position.Pixel.Y - 2.5f,
                    position.Pixel.X + 2.5f, position.Pixel.Y + 2.5f);
                graphics.DrawString((min + (max - min) * t).ToString("G4", CultureInfo.InvariantCulture),
                    font, brush, position.Pixel.X + 5, position.Pixel.Y - font.Height);
            }
            graphics.DrawString(title, font, brush, end.Pixel.X + 7, end.Pixel.Y + 5);
        }
    }
}
