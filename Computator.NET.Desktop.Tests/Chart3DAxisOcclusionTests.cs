using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Computator.NET.Charting.Chart3D.Chart3D;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;
using ChartData = Computator.NET.Charting.Chart3D.Chart3D.Chart3D;
using DataPoint = Computator.NET.DataTypes.Charts.Point3D;
using WorldPoint = System.Windows.Media.Media3D.Point3D;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class Chart3DAxisOcclusionTests
    {
        [Test]
        public void ForegroundSurfaceHidesActualAxisTickAndTextInkButBackgroundDoesNot()
        {
            var scene = SyntheticScene();
            scene.Reload(null);
            var unobscured = Pixels(Render(scene.Labels.Annotations, 640, 480));
            NUnit.Framework.Assert.That(unobscured.Any(value => value != 0), Is.True);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.AxisSegments, Is.Not.Empty);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.TickMarks, Is.Not.Empty);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.Labels, Is.Not.Empty);

            // This exact full-screen plane is in front of every annotation.
            // Unlike a screenshot-only assertion, an empty alpha plane proves
            // the hidden marks were not just recoloured to match the surface.
            scene.Reload(ScreenRectangle(new Rect(0, 0, 640, 480), 0.7, 640, 480));
            NUnit.Framework.Assert.That(scene.Labels.Annotations.AxisSegments, Is.Empty);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.TickMarks, Is.Empty);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.Labels, Is.Empty);
            NUnit.Framework.Assert.That(Pixels(Render(scene.Labels.Annotations, 640, 480)),
                Is.All.EqualTo((byte)0));

            // The same projected footprint behind the axes must not hide them.
            scene.Reload(ScreenRectangle(new Rect(0, 0, 640, 480), -0.8, 640, 480));
            NUnit.Framework.Assert.That(Pixels(Render(scene.Labels.Annotations, 640, 480)),
                Is.EqualTo(unobscured));
        }

        [Test]
        public void AForegroundPatchClipsTheTickEndEvenWhenItsAnchorIsUnobscured()
        {
            var scene = SyntheticScene();
            scene.Reload(null);
            var tick = scene.Labels.Annotations.TickMarks.Where(item => item.Axis == 0 && item.IsMajor)
                .OrderBy(item => Math.Abs(item.Value)).First();
            var bottom = Math.Max(tick.Start.Y, tick.End.Y);
            var patch = new Rect(tick.Position.X - 3, bottom - 1.5, 6, 4);
            NUnit.Framework.Assert.That(patch.Contains(tick.Position), Is.False);
            var occluder = ScreenRectangle(patch, 0.6, 640, 480);
            scene.Reload(occluder);

            var fragments = scene.Labels.Annotations.TickMarks
                .Where(item => item.Axis == tick.Axis && item.Value == tick.Value).ToArray();
            NUnit.Framework.Assert.That(fragments, Is.Not.Empty,
                "The unobscured part of the tick should remain visible.");
            NUnit.Framework.Assert.That(fragments.Sum(item => (item.End - item.Start).Length),
                Is.LessThan((tick.End - tick.Start).Length - 1));
            AssertVisible(scene.Labels.Annotations, Triangles(occluder, Matrix3D.Identity), 640, 480);
        }

        [Test]
        public void ALabelCannotBleedThroughASurfaceThatMissesItsAnchor()
        {
            var scene = SyntheticScene();
            scene.Reload(null);
            var label = scene.Labels.Annotations.Labels.First(item => item.Value.HasValue);
            var patch = label.Bounds;
            patch.Inflate(-Math.Min(1, patch.Width / 4), -Math.Min(1, patch.Height / 4));
            NUnit.Framework.Assert.That(patch.Contains(label.Position), Is.False);
            var occluder = ScreenRectangle(patch, 0.6, 640, 480);
            scene.Reload(occluder);

            var replacements = scene.Labels.Annotations.Labels
                .Where(item => item.Axis == label.Axis && item.Value == label.Value).ToArray();
            NUnit.Framework.Assert.That(replacements.All(item => !item.Bounds.IntersectsWith(patch)), Is.True,
                "A hidden label may be omitted or placed on its other side, but its whole footprint must be visible.");
            AssertVisible(scene.Labels.Annotations, Triangles(occluder, Matrix3D.Identity), 640, 480);
        }

        [Test]
        public void MutatingTheSameMeshInvalidatesTheCachedDepthGeometry()
        {
            var scene = SyntheticScene();
            scene.Reload(null);
            var unobscured = Pixels(Render(scene.Labels.Annotations, 640, 480));
            var mesh = ScreenRectangle(new Rect(0, 0, 640, 480), 0.7, 640, 480);
            scene.Reload(mesh);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.AxisSegments, Is.Empty);
            for (var index = 0; index < mesh.Positions.Count; index++)
            {
                var point = mesh.Positions[index];
                mesh.Positions[index] = new WorldPoint(point.X, point.Y, -0.8);
            }
            scene.Reload(mesh);
            NUnit.Framework.Assert.That(Pixels(Render(scene.Labels.Annotations, 640, 480)),
                Is.EqualTo(unobscured), "Changing the existing mesh must rebuild its depth information.");
        }

        [Test]
        public void FrozenMeshesSupportDepthTestingClearAndReloadWithoutChangeSubscriptions()
        {
            var scene = SyntheticScene();
            var mesh = ScreenRectangle(new Rect(0, 0, 640, 480), 0.7, 640, 480);
            mesh.Freeze();
            NUnit.Framework.Assert.That(() => scene.Reload(mesh), Throws.Nothing);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.AxisSegments, Is.Empty);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.Labels, Is.Empty);
            NUnit.Framework.Assert.That(() => scene.Labels.Remove(), Throws.Nothing);
            NUnit.Framework.Assert.That(() => scene.Reload(mesh), Throws.Nothing);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.AxisSegments, Is.Empty);
            NUnit.Framework.Assert.That(() => scene.Reload(null), Throws.Nothing);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.AxisSegments, Is.Not.Empty);
            NUnit.Framework.Assert.That(scene.Labels.Annotations.Labels, Is.Not.Empty);
        }

        [TestCase(1.2)]
        [TestCase(-9)]
        public void SurfacesBeyondTheCamerasClipPlanesDoNotHideAnnotations(double depth)
        {
            var scene = SyntheticScene();
            scene.Reload(null);
            var unobscured = Pixels(Render(scene.Labels.Annotations, 640, 480));
            scene.Reload(ScreenRectangle(new Rect(0, 0, 640, 480), depth, 640, 480));
            NUnit.Framework.Assert.That(Pixels(Render(scene.Labels.Annotations, 640, 480)),
                Is.EqualTo(unobscured));
        }

        [TestCase("plane")]
        [TestCase("saddle")]
        [TestCase("waves")]
        [TestCase("flat")]
        public void RotatingZoomingAndPanningRecomputesVisibilityAgainstActualSurfaceTriangles(string shape)
        {
            var chart = Surface(shape);
            var initiallyVisible = chart.AxisLabels.Annotations.AxisSegments
                .Select(segment => (segment.Start, segment.End)).ToArray();
            var triangles = Triangles(SceneMesh(chart), chart.m_transformMatrix.m_totalMatrix);
            AssertVisible(chart.AxisLabels.Annotations, triangles, 800, 600);
            foreach (var rotation in new[] { (Yaw: -60.0, Pitch: 50.0), (Yaw: 155.0, Pitch: -30.0) })
            {
                RotateBy(chart, rotation.Yaw, rotation.Pitch);
                chart.OnViewportMouseWheel(chart, new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120));
                var viewport = Viewport(chart);
                chart.m_transformMatrix.OnRBtnDown(new Point());
                chart.m_transformMatrix.OnMouseMove(new Point(19, -13), viewport);
                chart.m_transformMatrix.OnRBtnUp();
                chart.RescaleProjectionMatrix();
                chart.UpdateLayout();
                triangles = Triangles(SceneMesh(chart), chart.m_transformMatrix.m_totalMatrix);
                AssertVisible(chart.AxisLabels.Annotations, triangles, 800, 600);
                if (shape == "waves" && rotation.Yaw == -60)
                {
                    // This camera looks past the axes into the actual wave
                    // mesh. Prove it exercises hidden geometry, rather than
                    // merely confirming an unobscured scene stays visible.
                    var fullLength = Data(chart).GetAxes().Sum(axis =>
                        (chart.m_transformMatrix.VertexToScreenPt(axis.LabelAnchor, viewport) -
                         chart.m_transformMatrix.VertexToScreenPt(axis.Start, viewport)).Length);
                    var visibleLength = chart.AxisLabels.Annotations.AxisSegments
                        .Where(segment => !segment.IsArrow).Sum(segment => (segment.End - segment.Start).Length);
                    NUnit.Framework.Assert.That(visibleLength, Is.GreaterThan(0).And.LessThan(fullLength - 10));
                    NUnit.Framework.Assert.That(CountSurfaceHiddenAxisAnchors(chart, triangles), Is.GreaterThan(0),
                        "The independent ray oracle must find genuine surface-hidden anchors in this view.");
                }
            }
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.AxisSegments, Is.Not.Empty,
                "Depth testing must retain the portions that are still exposed.");
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.AxisSegments
                .Select(segment => (segment.Start, segment.End)).ToArray(), Is.Not.EqualTo(initiallyVisible));
        }

        [Test]
        public void SceneMeshContainsOnlyPlottedSurfaceAndNoFacetedAxisCylinders()
        {
            var chart = Surface("waves");
            var data = (UniformSurfaceChart3D)Data(chart);
            var surface = data.GetMeshes(false).Cast<Mesh3D>().Single();
            var mesh = SceneMesh(chart);
            NUnit.Framework.Assert.That(mesh.Positions.Count, Is.EqualTo(surface.GetVertexNo()));
            NUnit.Framework.Assert.That(mesh.TriangleIndices.Count, Is.EqualTo(surface.GetTriangleNo() * 3));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.AxisSegments.Count, Is.GreaterThan(3));
        }

        [Test]
        public void PartlyOccludedAxisNamesRemainReadableOrAreOmittedAsWholeLabels()
        {
            // This is the exact native evidence view that previously shrank y
            // to a 4.192 x 5.363 DIP box beside the foreground plane.
            var chart = EmptyChart();
            chart.Scale = 0.3375;
            chart.AxisLabels.Color = Colors.Black;
            chart.SetChartAreaValues(-5, 5, -5, 5);
            chart.AddFunction(new Function(new Func<double, double, double>((x, y) => (x + y) / 2),
                FunctionType.Real3D));
            RotateBy(chart, 36, -18);
            RotateBy(chart, -72, 36);
            var names = chart.AxisLabels.Annotations.Labels.Where(label => label.IsAxisName).ToArray();
            NUnit.Framework.Assert.That(names.Select(label => label.Text), Does.Contain("x").And.Contain("z"),
                "Fully exposed axis names must remain available.");
            var pixels = Pixels(Render(chart.AxisLabels.Annotations, 800, 600));
            foreach (var label in names)
            {
                NUnit.Framework.Assert.That(label.Bounds.Height, Is.GreaterThanOrEqualTo(9),
                    "A name must not shrink into an unreadable remnant: " + label.Text);
                var ink = 0;
                for (var y = Math.Max(0, (int)Math.Floor(label.Bounds.Top));
                    y < Math.Min(600, Math.Ceiling(label.Bounds.Bottom)); y++)
                    for (var x = Math.Max(0, (int)Math.Floor(label.Bounds.Left));
                        x < Math.Min(800, Math.Ceiling(label.Bounds.Right)); x++)
                    {
                        var offset = (y * 800 + x) * 4;
                        if (pixels[offset + 3] > 127 && pixels[offset] < 100 &&
                            pixels[offset + 1] < 100 && pixels[offset + 2] < 100) ink++;
                    }
                NUnit.Framework.Assert.That(ink, Is.GreaterThan(5),
                    "Each surviving name must paint readable native text: " + label.Text);
            }
        }

        [TestCase(96)]
        [TestCase(144)]
        [TestCase(192)]
        public void ExportedLabelsRespectSurfaceDepthAtEachBitmapDpi(int dpi)
        {
            var chart = Surface("saddle");
            RotateBy(chart, 140, 55);
            var liveMatrix = chart.m_transformMatrix.m_totalMatrix;
            var liveLabels = chart.AxisLabels.Annotations.Labels.Select(item => (item.Text, item.Bounds)).ToArray();
            var triangles = Triangles(SceneMesh(chart), liveMatrix);
            var bitmap = chart.RenderImage(8 * dpi, 6 * dpi, dpi, out var exportedLabels);
            NUnit.Framework.Assert.That(bitmap.PixelWidth, Is.EqualTo(8 * dpi));
            NUnit.Framework.Assert.That(bitmap.PixelHeight, Is.EqualTo(6 * dpi));
            NUnit.Framework.Assert.That(exportedLabels, Is.Not.Empty);
            foreach (var label in exportedLabels)
                AssertLabelVisible(label, triangles, 768, 576);
            NUnit.Framework.Assert.That(chart.m_transformMatrix.m_totalMatrix, Is.EqualTo(liveMatrix));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels
                .Select(item => (item.Text, item.Bounds)).ToArray(), Is.EqualTo(liveLabels));
        }

        [TestCase("png")]
        [TestCase("bmp")]
        public void LosslessSavedImagesMatchTheDepthAwareExportWithoutChangingTheLiveScene(string extension)
        {
            var chart = Surface("waves");
            RotateBy(chart, 120, 50);
            var matrix = chart.m_transformMatrix.m_totalMatrix;
            var dimensions = chart.RenderSize;
            var segments = chart.AxisLabels.Annotations.AxisSegments
                .Select(item => (item.Start, item.End, item.StartDepth, item.EndDepth)).ToArray();
            var expected = chart.RenderImage(800, 600);
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "issue255-lossless-" + extension + "." + extension);
            try
            {
                chart.SaveImage(path, extension == "png" ? System.Drawing.Imaging.ImageFormat.Png :
                    System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapSource decoded;
                using (var stream = File.OpenRead(path))
                    decoded = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad);
                NUnit.Framework.Assert.That(decoded.PixelWidth, Is.EqualTo(800));
                NUnit.Framework.Assert.That(decoded.PixelHeight, Is.EqualTo(600));
                NUnit.Framework.Assert.That(Pixels(decoded), Is.EqualTo(Pixels(expected)));
                NUnit.Framework.Assert.That(chart.m_transformMatrix.m_totalMatrix, Is.EqualTo(matrix));
                NUnit.Framework.Assert.That(chart.RenderSize, Is.EqualTo(dimensions));
                NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.AxisSegments
                    .Select(item => (item.Start, item.End, item.StartDepth, item.EndDepth)).ToArray(), Is.EqualTo(segments));
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Test]
        public void ScatterAndCustomPresentationSettingsKeepWorkingWithDepthAwareAnnotations()
        {
            var chart = EmptyChart();
            chart.Mode = Chart3DMode.Points;
            chart.Redraw();
            chart.AddPoints(new[]
            {
                new DataPoint(-3, -2, -1), new DataPoint(3, 2, 1),
                new DataPoint(-2, 1, 3), new DataPoint(1, -3, -2)
            });
            RotateBy(chart, 36, -18);
            chart.AxisLabels.SetProporties(new FontFamily("Georgia"), 24, Colors.Crimson,
                FontStyles.Italic, FontWeights.Bold);
            chart.AxisLabels.LabelX = "distance";
            // Setting presentation properties invalidates the live visual.
            // Complete that normal WPF layout/render pass before sampling it.
            chart.UpdateLayout();
            NUnit.Framework.Assert.That(chart.AxisLabels.Color, Is.EqualTo(Colors.Crimson));
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Any(item => item.Text == "distance"), Is.True);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks, Is.Not.Empty);
            var pixels = Pixels(Render(chart.AxisLabels.Annotations, 800, 600));
            var redInk = 0;
            for (var index = 0; index < pixels.Length; index += 4)
                if (pixels[index + 3] > 127 && pixels[index + 2] > pixels[index + 1] + 40 &&
                    pixels[index + 2] > pixels[index] + 40) redInk++;
            NUnit.Framework.Assert.That(redInk, Is.GreaterThan(20), "Native text rendering must preserve the user's colour.");
            chart.AxisLabels.ActiveXLabel = false;
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels.Any(item => item.Axis == 0), Is.False);
            chart.AxisLabels.ActiveLabels = false;
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.Labels, Is.Empty);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.AxisSegments, Is.Not.Empty);
            chart.VisibilityAxes = false;
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.TickMarks, Is.Empty);
            NUnit.Framework.Assert.That(chart.AxisLabels.Annotations.AxisSegments, Is.Empty);
        }

        // Independent camera-space Moller-Trumbore ray intersections. These
        // assertions do not call the production clipping/depth helper, so they
        // catch wrong depth sign, omitted transforms and stale scene geometry.
        private static void AssertVisible(Chart3DAxisAnnotations annotations, Triangle[] triangles,
            double width, double height)
        {
            foreach (var segment in annotations.AxisSegments)
                for (var sample = 1; sample < 10; sample++)
                {
                    var fraction = sample / 10.0;
                    var position = segment.Start + (segment.End - segment.Start) * fraction;
                    var depth = segment.StartDepth + (segment.EndDepth - segment.StartDepth) * fraction;
                    AssertPointVisible(position, depth, triangles, width, height, "axis segment");
                }
            foreach (var tick in annotations.TickMarks)
                for (var sample = 1; sample < 10; sample++)
                    AssertPointVisible(tick.Start + (tick.End - tick.Start) * (sample / 10.0),
                        tick.Depth, triangles, width, height, "tick " + tick.Value);
            foreach (var label in annotations.Labels)
                AssertLabelVisible(label, triangles, width, height);
        }

        private static void AssertLabelVisible(RenderedAxisLabel label, Triangle[] triangles,
            double width, double height)
        {
            AssertPointVisible(label.Position, label.Depth, triangles, width, height, "anchor " + label.Text);
            foreach (var x in new[] { 0.05, 0.5, 0.95 })
                foreach (var y in new[] { 0.05, 0.5, 0.95 })
                    AssertPointVisible(new Point(label.Bounds.Left + label.Bounds.Width * x,
                        label.Bounds.Top + label.Bounds.Height * y), label.Depth, triangles,
                        width, height, "label " + label.Text);
        }

        private static void AssertPointVisible(Point screen, double depth, Triangle[] triangles,
            double width, double height, string description)
        {
            NUnit.Framework.Assert.That(double.IsFinite(depth), Is.True, description);
            NUnit.Framework.Assert.That(depth, Is.InRange(-8.0000001, 1.0000001), description);
            var rayOrigin = new WorldPoint((screen.X - width / 2) * 2 / width,
                (height / 2 - screen.Y) * 2 / width, 2);
            foreach (var triangle in triangles)
            {
                var hitDepth = RayDepth(rayOrigin, triangle);
                if (hitDepth.HasValue && hitDepth >= -8 && hitDepth <= 1)
                    NUnit.Framework.Assert.That(hitDepth.Value, Is.LessThanOrEqualTo(depth + 1e-7),
                        description + " at " + screen + " must not appear through a closer triangle.");
            }
        }

        private static int CountSurfaceHiddenAxisAnchors(Chart3DControl chart, Triangle[] triangles)
        {
            var count = 0;
            var viewport = Viewport(chart);
            var rectangle = new Rect(0, 0, viewport.ActualWidth, viewport.ActualHeight);
            foreach (var axis in Data(chart).GetAxes())
                for (var sample = 1; sample < 20; sample++)
                {
                    var world = axis.At(axis.Minimum + (axis.Maximum - axis.Minimum) * sample / 20.0);
                    var transformed = chart.m_transformMatrix.m_totalMatrix.Transform(world);
                    var screen = chart.m_transformMatrix.VertexToScreenPt(world, viewport);
                    if (!rectangle.Contains(screen) || transformed.Z < -8 || transformed.Z > 1) continue;
                    var origin = new WorldPoint(transformed.X, transformed.Y, 2);
                    if (triangles.Any(triangle =>
                    {
                        var hitDepth = RayDepth(origin, triangle);
                        return hitDepth.HasValue && hitDepth >= -8 && hitDepth <= 1 &&
                            hitDepth > transformed.Z + 1e-7;
                    })) count++;
                }
            return count;
        }

        private static double? RayDepth(WorldPoint origin, Triangle triangle)
        {
            var direction = new Vector3D(0, 0, -1);
            var edge1 = triangle.B - triangle.A;
            var edge2 = triangle.C - triangle.A;
            var cross = Vector3D.CrossProduct(direction, edge2);
            var determinant = Vector3D.DotProduct(edge1, cross);
            if (!double.IsFinite(determinant) || Math.Abs(determinant) < 1e-12) return null;
            var distance = origin - triangle.A;
            var u = Vector3D.DotProduct(distance, cross) / determinant;
            if (u < -1e-10 || u > 1 + 1e-10) return null;
            var secondCross = Vector3D.CrossProduct(distance, edge1);
            var v = Vector3D.DotProduct(direction, secondCross) / determinant;
            if (v < -1e-10 || u + v > 1 + 1e-10) return null;
            var alongRay = Vector3D.DotProduct(edge2, secondCross) / determinant;
            return origin.Z - alongRay;
        }

        private static Triangle[] Triangles(MeshGeometry3D mesh, Matrix3D matrix)
        {
            var positions = mesh.Positions.Select(matrix.Transform).ToArray();
            var result = new List<Triangle>();
            for (var index = 0; index + 2 < mesh.TriangleIndices.Count; index += 3)
                result.Add(new Triangle(positions[mesh.TriangleIndices[index]],
                    positions[mesh.TriangleIndices[index + 1]], positions[mesh.TriangleIndices[index + 2]]));
            return result.ToArray();
        }

        private static MeshGeometry3D ScreenRectangle(Rect rectangle, double depth, double width, double height)
        {
            WorldPoint PointAt(double x, double y) =>
                new WorldPoint((x - width / 2) * 2 / width, (height / 2 - y) * 2 / width, depth);
            return new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    PointAt(rectangle.Left, rectangle.Top), PointAt(rectangle.Right, rectangle.Top),
                    PointAt(rectangle.Right, rectangle.Bottom), PointAt(rectangle.Left, rectangle.Bottom)
                },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
            };
        }

        private static AnnotationScene SyntheticScene()
        {
            var viewport = new Viewport3D
            {
                Camera = new OrthographicCamera(new WorldPoint(0, 0, 2), new Vector3D(0, 0, -1),
                    new Vector3D(0, 1, 0), 2) { NearPlaneDistance = 1, FarPlaneDistance = 10 }
            };
            var canvas = new Canvas();
            var host = new Grid { Width = 640, Height = 480 };
            host.Children.Add(viewport);
            host.Children.Add(canvas);
            host.Measure(new Size(640, 480));
            host.Arrange(new Rect(0, 0, 640, 480));
            host.UpdateLayout();
            var radii = new Vector3D(0.002, 0.002, 0.002);
            var axes = new[]
            {
                new Chart3DAxis(0, -0.65, 0.65, new WorldPoint(-0.65, -0.4, 0),
                    new WorldPoint(0.65, -0.4, 0), radii),
                new Chart3DAxis(1, -0.4, 0.6, new WorldPoint(-0.65, -0.4, 0),
                    new WorldPoint(-0.65, 0.6, 0), radii),
                new Chart3DAxis(2, -0.3, 0.3, new WorldPoint(-0.65, -0.4, -0.3),
                    new WorldPoint(-0.65, -0.4, 0.3), radii)
            };
            var labels = new AxisLabels(canvas) { ActiveYLabel = false, ActiveZLabel = false };
            return new AnnotationScene(host, viewport, labels,
                new TransformMatrix { m_totalMatrix = Matrix3D.Identity }, axes);
        }

        private static Chart3DControl Surface(string shape)
        {
            var chart = EmptyChart();
            chart.SetChartAreaValues(-3, 3, -3, 3);
            Func<double, double, double> function = shape switch
            {
                "saddle" => (x, y) => (x * x - y * y) / 3,
                "waves" => (x, y) => Math.Sin(x) * Math.Cos(y),
                "flat" => (x, y) => 0,
                _ => (x, y) => (x + y) / 2
            };
            chart.AddFunction(new Function(function, FunctionType.Real3D));
            RotateBy(chart, 36, -18);
            return chart;
        }

        private static Chart3DControl EmptyChart()
        {
            var chart = new Chart3DControl
            {
                Width = 800, Height = 600, Quality = 0, EqualAxes = false, Scale = 0.32,
                Background = Brushes.White
            };
            chart.Measure(new Size(800, 600));
            chart.Arrange(new Rect(0, 0, 800, 600));
            chart.UpdateLayout();
            return chart;
        }

        private static void RotateBy(Chart3DControl chart, double yaw, double pitch)
        {
            var viewport = Viewport(chart);
            chart.m_transformMatrix.OnLBtnDown(new Point());
            chart.m_transformMatrix.OnMouseMove(new Point(viewport.ActualWidth * yaw / 180,
                viewport.ActualHeight * pitch / 180), viewport);
            chart.m_transformMatrix.OnLBtnUp();
            chart.RescaleProjectionMatrix();
            chart.UpdateLayout();
        }

        private static Viewport3D Viewport(Chart3DControl chart) => (Viewport3D)chart.FindName("mainViewport");
        private static ChartData Data(Chart3DControl chart) =>
            (ChartData)typeof(Chart3DControl).GetField("m_3dChart", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(chart);
        private static MeshGeometry3D SceneMesh(Chart3DControl chart) =>
            Viewport(chart).Children.OfType<ModelVisual3D>().Select(visual => visual.Content)
                .OfType<GeometryModel3D>().Select(model => model.Geometry).OfType<MeshGeometry3D>().Single();

        private static RenderTargetBitmap Render(Visual visual, int width, int height)
        {
            var result = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            result.Render(visual);
            return result;
        }

        private static byte[] Pixels(BitmapSource bitmap)
        {
            if (bitmap.Format != PixelFormats.Pbgra32)
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Pbgra32, null, 0);
            var result = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(result, bitmap.PixelWidth * 4, 0);
            return result;
        }

        private readonly record struct Triangle(WorldPoint A, WorldPoint B, WorldPoint C);
        private sealed record AnnotationScene(Grid Host, Viewport3D Viewport, AxisLabels Labels,
            TransformMatrix Transform, IReadOnlyList<Chart3DAxis> Axes)
        {
            public void Reload(MeshGeometry3D geometry)
            {
                Labels.Reload(Axes, Transform, Viewport, true, Colors.DarkSlateGray, geometry);
                Host.UpdateLayout();
            }
        }
    }
}
