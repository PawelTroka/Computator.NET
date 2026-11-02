using System;
using System.Drawing;
using System.Linq;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.DataTypes.Charts;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    public class MonoSurfaceRenderingTests
    {
        [TestCase(1e308)]
        [TestCase(-1e308)]
        [TestCase(double.MaxValue)]
        [TestCase(double.MinValue)]
        public void LargeFiniteConstantSurfacesRetainVisibleColoredGeometry(double height)
        {
            var coordinates = new[] { -2.0, 0.0, 2.0 };
            var surface = new MonoSurfaceGrid(coordinates, coordinates,
                Enumerable.Repeat(height, 9).ToArray(), 9);
            using var bitmap = new Bitmap(640, 440);
            using var graphics = Graphics.FromImage(bitmap);
            using var font = new Font(FontFamily.GenericSansSerif, 10);
            foreach (var equalAxes in new[] { false, true })
            {
                MonoSceneRenderer.Draw(graphics, new Rectangle(0, 0, 640, 440), font,
                    new[] { surface }, Array.Empty<System.Collections.Generic.List<Point3D>>(),
                    Chart3DMode.Surface, -2, 2, -2, 2, equalAxes, .76, .55, 1, 0, 0);
                var colored = 0;
                for (var y = 0; y < bitmap.Height; y += 2)
                    for (var x = 0; x < bitmap.Width; x += 2)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        if (Math.Max(pixel.R, Math.Max(pixel.G, pixel.B)) -
                            Math.Min(pixel.R, Math.Min(pixel.G, pixel.B)) > 60) colored++;
                    }
                NUnit.Framework.Assert.That(colored, Is.GreaterThan(500),
                    "A finite constant must render a colored plane rather than disappear through a zero z span.");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RejectedPointsStillCountTowardTheInputLimit(bool nullPoints)
        {
            var read = 0;
            System.Collections.Generic.IEnumerable<Point3D> Input()
            {
                for (var index = 0; index < 20002; index++)
                {
                    read++;
                    yield return nullPoints ? null : new Point3D(double.NaN, 0, 0);
                }
            }
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => MonoPointSeries.CopyBounded(Input()));
            NUnit.Framework.Assert.That(read, Is.EqualTo(20001), "Stop immediately after the first out-of-budget input.");
        }

        [Test]
        public void MaximumSizedValidInputIsCopiedWithoutAliasingItsPoints()
        {
            var point = new Point3D(1, 2, 3);
            var copy = MonoPointSeries.CopyBounded(Enumerable.Repeat(point, 20000));
            NUnit.Framework.Assert.That(copy.Count, Is.EqualTo(20000));
            NUnit.Framework.Assert.That(copy[0], Is.Not.SameAs(point));
            NUnit.Framework.Assert.That(copy[0].z, Is.EqualTo(3));
        }
    }
}
