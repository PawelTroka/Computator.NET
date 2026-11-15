using System;
using System.Linq;
using Computator.NET.Charting.RealCharting;
using NUnit.Framework;
#if WINDOWS
using System.Collections.Generic;
using System.Threading;
using Computator.NET.DataTypes.Functions;
#endif

namespace Computator.NET.Desktop.Tests
{
    // Source-linked into Engine.Tests so the production sampler runs on every OS.
    [TestFixture]
    public class RealChartSamplingTests
    {
        [TestCase(1e16, 1e16 + 2)]
        [TestCase(-1e16, -1e16 + 2)]
        [TestCase(1, 1.0000000000000002)]
        [TestCase(0, double.Epsilon)]
        public void AdjacentRepresentableBoundsFinishAtBothEndpoints(double minimum, double maximum)
        {
            var values = ChartSamplingRange.Create(minimum, maximum, 3571);
            NUnit.Framework.Assert.That(values, Is.EqualTo(new[] { minimum, maximum }));
        }

        [TestCase(-10, 10, 83)]
        [TestCase(-10, 10, 3571)]
        [TestCase(-10, 10, 47497)]
        [TestCase(-10, 10, 152.76)]
        [TestCase(1e16, 1e16 + 1024, 3571)]
        [TestCase(-1e16, -1e16 + 1024, 3571)]
        [TestCase(-1e27, 1e27, 3571)]
        [TestCase(0, 1e-320, 3571)]
        public void SamplesAreOrderedUniqueBoundedAndIncludeTheEndpoints(double minimum,
            double maximum, double intervals)
        {
            var values = ChartSamplingRange.Create(minimum, maximum, intervals);
            NUnit.Framework.Assert.That(values.Length, Is.InRange(2, (int)Math.Ceiling(intervals) + 1));
            NUnit.Framework.Assert.That(values[0], Is.EqualTo(minimum));
            NUnit.Framework.Assert.That(values[values.Length - 1], Is.EqualTo(maximum));
            for (var index = 1; index < values.Length; index++)
            {
                NUnit.Framework.Assert.That(values[index], Is.GreaterThan(values[index - 1]));
                NUnit.Framework.Assert.That(values[index], Is.InRange(minimum, maximum));
            }
        }

        [Test]
        public void OrdinaryRangesRetainTheirResolutionAndUniformSpacing()
        {
            var values = ChartSamplingRange.Create(-2, 2, 100);
            NUnit.Framework.Assert.That(values, Has.Length.EqualTo(101));
            NUnit.Framework.Assert.That(values[50], Is.EqualTo(0));
            foreach (var spacing in values.Skip(1).Zip(values, (next, previous) => next - previous))
                NUnit.Framework.Assert.That(spacing, Is.EqualTo(0.04).Within(1e-15));
        }

        [TestCase(double.NaN, 1, 83)]
        [TestCase(0, double.NaN, 83)]
        [TestCase(double.NegativeInfinity, 1, 83)]
        [TestCase(0, double.PositiveInfinity, 83)]
        [TestCase(-double.MaxValue, double.MaxValue, 83)]
        [TestCase(1, 1, 83)]
        [TestCase(1, 0, 83)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 1, -1)]
        [TestCase(0, 1, double.NaN)]
        [TestCase(0, 1, double.PositiveInfinity)]
        [TestCase(0, 1, 50001)]
        [TestCase(0, 1, double.MaxValue)]
        public void InvalidBoundsOrResolutionProduceNoSamples(double minimum, double maximum, double intervals)
        {
            NUnit.Framework.Assert.That(ChartSamplingRange.Create(minimum, maximum, intervals), Is.Empty);
        }
    }

#if WINDOWS
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class RealChartSamplingWorkflowTests
    {
        [TestCase(1e16)]
        [TestCase(-1e16)]
        public void AddingAndRefreshingAnExplicitFunctionCannotRepeatAnUnrepresentableStep(double minimum)
        {
            using var chart = new Chart2D { Visible = false, Quality = 0 };
            chart.SetChartAreaValues(minimum, minimum + 2, -1, 1);
            var sampled = new HashSet<double>();
            var function = new Function(new Func<double, double>(x =>
            {
                // On the old implementation the second evaluation fails promptly,
                // rather than leaving an infinite sampling loop in the test runner.
                if (!sampled.Add(x)) throw new InvalidOperationException("The chart sampled the same x twice.");
                return x - minimum;
            }), false) { Name = "narrow explicit" };

            chart.AddFunction(function);
            AssertExplicitSamples(chart, sampled, minimum);
            sampled.Clear();
            chart.Visible = true;
            AssertExplicitSamples(chart, sampled, minimum);
            sampled.Clear();
            chart.Quality = 50;
            AssertExplicitSamples(chart, sampled, minimum);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void AddingAndRefreshingAnImplicitFunctionCannotStallOnEitherAxis(bool narrowX, bool narrowY)
        {
            using var chart = new Chart2D { Visible = false, Quality = 0 };
            var xMin = narrowX ? 1e16 : -1;
            var yMin = narrowY ? -1e16 : -1;
            var xMax = narrowX ? xMin + 2 : 1;
            var yMax = narrowY ? yMin + 2 : 1;
            chart.SetChartAreaValues(xMin, xMax, yMin, yMax);
            var sampled = new HashSet<Tuple<double, double>>();
            var function = new Function(new Func<double, double, double>((x, y) =>
            {
                if (!sampled.Add(Tuple.Create(x, y)))
                    throw new InvalidOperationException("The chart sampled the same coordinate pair twice.");
                return 0;
            }), true) { Name = "narrow implicit" };
            var expected = (narrowX ? 2 : 84) * (narrowY ? 2 : 84);

            chart.AddFunction(function);
            AssertImplicitSamples(chart, sampled, expected, xMin, xMax, yMin, yMax);
            sampled.Clear();
            chart.Visible = true;
            AssertImplicitSamples(chart, sampled, expected, xMin, xMax, yMin, yMax);
        }

        private static void AssertExplicitSamples(Chart2D chart, HashSet<double> sampled, double minimum)
        {
            NUnit.Framework.Assert.That(sampled, Is.EquivalentTo(new[] { minimum, minimum + 2 }));
            NUnit.Framework.Assert.That(chart.Series.Single().Points.Select(point => point.XValue),
                Is.EqualTo(new[] { minimum, minimum + 2 }));
        }

        private static void AssertImplicitSamples(Chart2D chart, HashSet<Tuple<double, double>> sampled,
            int expected, double xMin, double xMax, double yMin, double yMax)
        {
            NUnit.Framework.Assert.That(sampled, Has.Count.EqualTo(expected));
            NUnit.Framework.Assert.That(sampled, Does.Contain(Tuple.Create(xMin, yMin)));
            NUnit.Framework.Assert.That(sampled, Does.Contain(Tuple.Create(xMax, yMax)));
            NUnit.Framework.Assert.That(chart.Series.Single().Points, Has.Count.EqualTo(expected));
        }
    }
#endif
}
