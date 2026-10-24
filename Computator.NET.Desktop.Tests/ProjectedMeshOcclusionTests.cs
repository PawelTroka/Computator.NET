#if !__MonoCS__
using System;
using System.Collections.Generic;
using Computator.NET.Charting.Axes;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    // Runs in the portable Engine.Tests and the native desktop suite. Expected
    // crossings are analytical; no WPF renderer or screenshot is the oracle.
    [TestFixture]
    public class ProjectedMeshOcclusionTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TriangleCoverageUsesActualGeometryInBothWindings(bool reverse)
        {
            var scene = Scene();
            var a = P(20, 20, 0.5); var b = P(80, 20, 0.5); var c = P(20, 80, 0.5);
            scene.AddTriangle(a, reverse ? c : b, reverse ? b : c);
            NUnit.Framework.Assert.That(scene.IsVisible(P(30, 30, 0)), Is.False);
            NUnit.Framework.Assert.That(scene.IsVisible(P(70, 70, 0)), Is.True,
                "Empty space inside a triangle's bounding box must remain visible");
            NUnit.Framework.Assert.That(scene.IsVisible(P(30, 30, 0.75)), Is.True);
            NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 0)), Is.False, "The shared diagonal is covered");
        }

        [TestCase(-0.5)]
        [TestCase(0)]
        [TestCase(1e-12)]
        public void BehindAndCoplanarSurfacesDoNotHideAnnotations(double surfaceDepth)
        {
            var scene = Scene();
            Square(scene, 20, 20, 80, 80, surfaceDepth);
            NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 0)), Is.True);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(40, 40, 20, 20, 0), Is.True);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 1));
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0), 1), (0, 1));
        }

        [Test]
        public void AFlatForegroundPlaneCutsExactlyTheCoveredLineInterval()
        {
            var scene = Scene();
            Square(scene, 20, 20, 80, 80, 0.5);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 0.2), (0.8, 1));
            Intervals(scene.VisibleSegments(P(100, 50, 0), P(0, 50, 0)), (0, 0.2), (0.8, 1));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void SurfaceAndAnnotationDepthCrossingsSplitInsideATriangle(double strokeRadius)
        {
            var scene = Scene();
            Square(scene, 20, 20, 80, 80, 0.5);
            // Annotation depth is -1 + 2t, so it emerges at t=0.75.
            Intervals(scene.VisibleSegments(P(0, 50, -1), P(100, 50, 1), strokeRadius),
                (0, 0.2), (0.75, 1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SlopedSurfaceHidesOnlyItsCloserHalf(bool reverse)
        {
            var scene = Scene();
            // Independent plane equation z=(x-50)/100.
            Quad(scene, P(20, 20, -0.3), P(80, 20, 0.3), P(80, 80, 0.3), P(20, 80, -0.3), reverse);
            NUnit.Framework.Assert.That(scene.IsVisible(P(40, 50, 0)), Is.True);
            NUnit.Framework.Assert.That(scene.IsVisible(P(60, 50, 0)), Is.False);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 0.5), (0.8, 1));
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0), 1), (0, 0.5), (0.8, 1));
        }

        [Test]
        public void DisjointSurfacesAndTheirOverlappingBoundsDoNotFillHoles()
        {
            var scene = Scene();
            Square(scene, 20, 20, 40, 80, 0.5);
            Square(scene, 60, 20, 80, 80, 0.5);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)),
                (0, 0.2), (0.4, 0.6), (0.8, 1));
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(45, 40, 10, 20, 0), Is.True);
        }

        [Test]
        public void OverlappingOccludersAreUnionedWithoutParityHoles()
        {
            var scene = Scene();
            Square(scene, 10, 20, 60, 80, 0.4);
            Square(scene, 40, 20, 90, 80, 0.6);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 0.1), (0.9, 1));
            NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 0)), Is.False);
        }

        [Test]
        public void LabelCoverageFindsATriangleWhollyInsideTheInkBounds()
        {
            var scene = Scene();
            scene.AddTriangle(P(45, 45, 0.5), P(46, 45, 0.5), P(45, 46, 0.5));
            NUnit.Framework.Assert.That(scene.IsVisible(P(40, 40, 0)), Is.True);
            NUnit.Framework.Assert.That(scene.IsVisible(P(60, 60, 0)), Is.True);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(40, 40, 20, 20, 0), Is.False,
                "Checking just the anchor or rectangle corners would miss this foreground triangle");
        }

        [Test]
        public void LabelCoverageUsesTheForegroundPortionOfASlopedTriangle()
        {
            var scene = Scene();
            Quad(scene, P(20, 20, -0.3), P(80, 20, 0.3), P(80, 80, 0.3), P(20, 80, -0.3));
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(30, 40, 10, 20, 0), Is.True);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(45, 40, 10, 20, 0), Is.False);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(45, 40, 10, 20, 0.6), Is.True);
        }

        [Test]
        public void SubpixelForegroundSliversAreNotMissedBySampling()
        {
            var scene = Scene();
            Square(scene, 50.0001, 20, 50.0002, 80, 0.5);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 0.500001), (0.500002, 1));
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(40, 40, 20, 20, 0), Is.False);
        }

        [Test]
        public void StrokeFootprintIsOccludedEvenWhenItsCenterlineIsClear()
        {
            var scene = Scene();
            Square(scene, 20, 50.4, 80, 70, 0.5);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 1));
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0), 0.25), (0, 1));
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0), 0.5), (0, 0.2), (0.8, 1));
        }

        [Test]
        public void StrokeDepthIsComparedAcrossTheWholeRibbon()
        {
            var scene = Scene();
            // The centerline lies on z=0, but its upper screen half is covered.
            Quad(scene, P(20, 49, -1), P(80, 49, -1), P(80, 51, 1), P(20, 51, 1));
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 1));
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0), 0.5), (0, 0.2), (0.8, 1));
        }

        [TestCase(1.0)]
        [TestCase(1.5)]
        [TestCase(2.0)]
        [TestCase(4.0)]
        public void ScaledDpiAndQuarterTurnPreserveGeometricVisibility(double scale)
        {
            var scene = new ProjectedMeshOcclusion(100 * scale, 100 * scale, 1, -8);
            // Rotate the preceding stroke-footprint case by 90 degrees and scale.
            Square(scene, 30 * scale, 20 * scale, 49.6 * scale, 80 * scale, 0.5);
            Intervals(scene.VisibleSegments(P(50 * scale, 0, 0), P(50 * scale, 100 * scale, 0), 0.5 * scale),
                (0, 0.2), (0.8, 1));
        }

        [Test]
        public void CameraNearPlaneClipsCrossingTrianglesRatherThanDroppingThem()
        {
            var scene = Scene();
            scene.AddTriangle(P(10, 10, -1), P(90, 10, 3), P(10, 90, -1));
            NUnit.Framework.Assert.That(scene.IsVisible(P(30, 20, -0.5)), Is.False,
                "The in-frustum part of a triangle still occludes");
            NUnit.Framework.Assert.That(scene.IsVisible(P(70, 20, -0.5)), Is.True,
                "The part in front of the near plane is absent from the rendered surface");
            Intervals(scene.VisibleSegments(P(0, 20, -0.5), P(100, 20, -0.5)), (0, 0.2), (0.5, 1));
        }

        [Test]
        public void CameraFarPlaneAndViewportClipTrianglesGeometrically()
        {
            var scene = Scene();
            scene.AddTriangle(P(-50, 0, -10), P(150, 0, -6), P(-50, 100, -10));
            NUnit.Framework.Assert.That(scene.IsVisible(P(20, 10, -8)), Is.True);
            NUnit.Framework.Assert.That(scene.IsVisible(P(70, 10, -8)), Is.False);
            NUnit.Framework.Assert.That(scene.TriangleCount, Is.GreaterThan(0));
        }

        [TestCase(2)]
        [TestCase(-9)]
        public void FullyClippedSurfacesDoNotOcclude(double depth)
        {
            var scene = Scene();
            Square(scene, 20, 20, 80, 80, depth);
            NUnit.Framework.Assert.That(scene.TriangleCount, Is.Zero);
            NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 0)), Is.True);
        }

        [Test]
        public void AnnotationLinesAreClippedToTheCameraAndViewportToo()
        {
            var scene = Scene();
            Intervals(scene.VisibleSegments(P(-50, 50, 0), P(150, 50, 0)), (0.25, 0.75));
            Intervals(scene.VisibleSegments(P(0, 50, -10), P(100, 50, 2)), (1.0 / 6, 11.0 / 12));
            NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 2)), Is.False);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(40, 40, 20, 20, -9), Is.False);
        }

        [Test]
        public void InvalidAndDegenerateTrianglesCannotInventOcclusion()
        {
            var scene = Scene();
            scene.AddTriangle(P(0, 0, double.NaN), P(100, 0, 0), P(0, 100, 0));
            scene.AddTriangle(P(0, 0, 0.5), P(50, 50, 0.5), P(100, 100, 0.5));
            scene.AddTriangle(P(double.PositiveInfinity, 0, 0), P(100, 0, 0), P(0, 100, 0));
            NUnit.Framework.Assert.That(scene.TriangleCount, Is.Zero);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 1));
        }

        [Test]
        public void InvalidOrCollapsedQueriesHaveNoVisibleGeometry()
        {
            var scene = Scene();
            NUnit.Framework.Assert.That(scene.IsVisible(P(double.NaN, 0, 0)), Is.False);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(40, 40, -1, 20, 0), Is.False);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(90, 40, 20, 20, 0), Is.False);
            NUnit.Framework.Assert.That(scene.VisibleSegments(P(50, 50, 0), P(50, 50, 1)), Is.Empty);
            NUnit.Framework.Assert.That(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0), -1), Is.Empty);
            NUnit.Framework.Assert.That(scene.VisibleSegments(P(0, 50, double.NaN), P(100, 50, 0)), Is.Empty);
        }

        [Test]
        public void AddingTrianglesAfterQueriesInvalidatesNoSpatialCoverage()
        {
            var scene = Scene();
            NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 0)), Is.True);
            Square(scene, 20, 20, 80, 80, 0.5);
            for (var i = 0; i < 10; i++)
                NUnit.Framework.Assert.That(scene.IsVisible(P(50, 50, 0)), Is.False);
        }

        [Test]
        public void ClearingAReusedIndexRemovesEveryOldOccluderBeforeAddingNewGeometry()
        {
            var scene = new ProjectedMeshOcclusion(100, 100, 1, -8, 4);
            Square(scene, 20, 20, 40, 80, 0.5);
            NUnit.Framework.Assert.That(scene.IsVisible(P(30, 50, 0)), Is.False);
            scene.ClearTriangles();
            NUnit.Framework.Assert.That(scene.TriangleCount, Is.Zero);
            NUnit.Framework.Assert.That(scene.IsVisible(P(30, 50, 0)), Is.True);
            NUnit.Framework.Assert.That(scene.IsRectangleVisible(20, 40, 20, 20, 0), Is.True);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 1));
            Square(scene, 60, 20, 80, 80, 0.5);
            NUnit.Framework.Assert.That(scene.IsVisible(P(30, 50, 0)), Is.True);
            NUnit.Framework.Assert.That(scene.IsVisible(P(70, 50, 0)), Is.False);
            Intervals(scene.VisibleSegments(P(0, 50, 0), P(100, 50, 0)), (0, 0.6), (0.8, 1));
        }

        [Test]
        public void MaximumSurfaceQualityHasNoTileSeamsOrSharedEdgeHoles()
        {
            var scene = new ProjectedMeshOcclusion(1000, 1000, 1, -8);
            const int divisions = 249; // 250 x 250 vertices at Quality=100.
            for (var y = 0; y < divisions; y++)
                for (var x = 0; x < divisions; x++)
                    Square(scene, x * 1000.0 / divisions, y * 1000.0 / divisions,
                        (x + 1) * 1000.0 / divisions, (y + 1) * 1000.0 / divisions, 0.5);
            NUnit.Framework.Assert.That(scene.TriangleCount, Is.EqualTo(124002));
            foreach (var coordinate in new[] { 1.0, 32, 64, 499.5, 500, 992, 999 })
            {
                NUnit.Framework.Assert.That(scene.IsVisible(P(coordinate, coordinate, 0)), Is.False);
                NUnit.Framework.Assert.That(scene.IsRectangleVisible(coordinate - 0.5, coordinate - 0.5, 1, 1, 0), Is.False);
            }
            NUnit.Framework.Assert.That(scene.VisibleSegments(P(0, 500, 0), P(1000, 500, 0), 0.75), Is.Empty);
            NUnit.Framework.Assert.That(scene.VisibleSegments(P(0, 0, 0), P(1000, 1000, 0)), Is.Empty);
        }

        private static ProjectedMeshOcclusion Scene() => new ProjectedMeshOcclusion(100, 100, 1, -8);
        private static DepthPoint P(double x, double y, double z) => new DepthPoint(x, y, z);
        private static void Square(ProjectedMeshOcclusion scene, double left, double top, double right, double bottom, double depth) =>
            Quad(scene, P(left, top, depth), P(right, top, depth), P(right, bottom, depth), P(left, bottom, depth));
        private static void Quad(ProjectedMeshOcclusion scene, DepthPoint a, DepthPoint b, DepthPoint c, DepthPoint d, bool reverse = false)
        {
            scene.AddTriangle(a, reverse ? c : b, reverse ? b : c);
            scene.AddTriangle(a, reverse ? d : c, reverse ? c : d);
        }
        private static void Intervals(IReadOnlyList<DepthInterval> actual, params (double Start, double End)[] expected)
        {
            NUnit.Framework.Assert.That(actual.Count, Is.EqualTo(expected.Length));
            for (var i = 0; i < actual.Count; i++)
            {
                NUnit.Framework.Assert.That(actual[i].Start, Is.EqualTo(expected[i].Start).Within(2e-8), $"Interval {i} start");
                NUnit.Framework.Assert.That(actual[i].End, Is.EqualTo(expected[i].End).Within(2e-8), $"Interval {i} end");
            }
        }
    }
}
#endif
