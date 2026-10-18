using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Computator.NET.Charting.Axes;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    // Linked into Engine.Tests as well: these regressions exercise the shared
    // numeric implementation without a Windows control, font or graphics handle.
    [TestFixture]
    public class AxisTickGeneratorTests
    {
        [TestCase(-7.0, 9.0)]
        [TestCase(1.0, 9.0)]
        [TestCase(-9.0, -1.0)]
        [TestCase(-0.75, 0.75)]
        [TestCase(-1e-200, 3e-200)]
        [TestCase(1e200, 3e200)]
        [TestCase(-3e200, -1e200)]
        [TestCase(1e12, 1e12 + 0.01)]
        [TestCase(-1e12 - 0.01, -1e12)]
        public void VisibleTicksHaveOrderedFiniteCoordinatesAndDistinctReadableLabels(double minimum, double maximum)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, 800);
            AssertValid(ticks, minimum, maximum);
            NUnit.Framework.Assert.That(ticks.MajorTicks.Count, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(ticks.MajorTicks.Select(tick => tick.Label), Is.Unique);
            foreach (var tick in ticks.MajorTicks)
            {
                var displayed = double.Parse(tick.Label, NumberStyles.Float, CultureInfo.InvariantCulture);
                NUnit.Framework.Assert.That(double.IsFinite(displayed), Is.True);
                NUnit.Framework.Assert.That(Math.Abs(displayed - tick.Value) / ticks.MajorStep,
                    Is.LessThan(1e-8), "Label precision must describe its tick, including large offsets");
                if (tick.Value != 0)
                    NUnit.Framework.Assert.That(displayed, Is.Not.Zero, "A small nonzero coordinate must not become zero");
            }
        }

        [TestCase(-7.0, 9.0)]
        [TestCase(-0.003, 0.007)]
        [TestCase(-1e200, 3e200)]
        public void ZeroAppearsOnceAtItsMathematicalPosition(double minimum, double maximum)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, 800);
            var zero = ticks.MajorTicks.Single(tick => tick.Value == 0);
            NUnit.Framework.Assert.That(zero.Label, Is.EqualTo("0"));
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(zero.Value), Is.Zero);
            NUnit.Framework.Assert.That(zero.Fraction, Is.EqualTo(-minimum / (maximum - minimum)).Within(1e-14));
            NUnit.Framework.Assert.That(ticks.MinorTicks.Any(tick => tick.Value == 0), Is.False);
        }

        [TestCase(1.0, 9.0)]
        [TestCase(-9.0, -1.0)]
        [TestCase(1e-200, 9e-200)]
        [TestCase(1e-200, 2.0)]
        [TestCase(-2.0, -1e-200)]
        public void AnOffscreenZeroIsNotInventedAtTheVisibleBoundary(double minimum, double maximum)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, 800);
            NUnit.Framework.Assert.That(ticks.MajorTicks.Any(tick => tick.Label == "0"), Is.False);
            AssertValid(ticks, minimum, maximum);
        }

        [TestCase(double.Epsilon, 2.0)]
        [TestCase(-2.0, -double.Epsilon)]
        public void SubnormalBoundsDoNotTurnTheOffscreenZeroIntoAnArbitraryTick(double minimum, double maximum)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, 800);
            var nearZeroBoundary = minimum > 0 ? minimum : maximum;
            NUnit.Framework.Assert.That(ticks.MajorTicks, Is.Not.Empty);
            NUnit.Framework.Assert.That(ticks.MajorTicks.Concat(ticks.MinorTicks)
                .Any(tick => tick.Value == 0 || tick.Value == nearZeroBoundary), Is.False,
                "A zero grid candidate must not become an unrelated subnormal boundary value");
            AssertValid(ticks, minimum, maximum);
        }

        [Test]
        public void DecimalEndpointsAreNotLostToFloatingPointMultiplication()
        {
            var ticks = AxisTickGenerator.Create(-0.3, 0.3, 384);
            NUnit.Framework.Assert.That(ticks.MajorTicks.First().Value, Is.EqualTo(-0.3));
            NUnit.Framework.Assert.That(ticks.MajorTicks.Last().Value, Is.EqualTo(0.3));
            NUnit.Framework.Assert.That(ticks.MajorTicks.First().Fraction, Is.Zero);
            NUnit.Framework.Assert.That(ticks.MajorTicks.Last().Fraction, Is.EqualTo(1));
            NUnit.Framework.Assert.That(ticks.MajorTicks.Select(tick => tick.Label),
                Does.Not.Contain("0.30000000000000004"));
        }

        [Test]
        public void MinorTicksFillMajorIntervalsWithoutDuplicatingThem()
        {
            var ticks = AxisTickGenerator.Create(-5, 5, 640);
            NUnit.Framework.Assert.That(ticks.MinorTicks.Count, Is.GreaterThan(ticks.MajorTicks.Count));
            NUnit.Framework.Assert.That(ticks.MinorTicks.All(tick => tick.Label == string.Empty), Is.True);
            NUnit.Framework.Assert.That(ticks.MinorTicks.Select(tick => tick.Value)
                .Intersect(ticks.MajorTicks.Select(tick => tick.Value)), Is.Empty);
            var all = ticks.MajorTicks.Concat(ticks.MinorTicks).OrderBy(tick => tick.Fraction).ToArray();
            for (var index = 1; index < all.Length; index++)
                NUnit.Framework.Assert.That((all[index].Fraction - all[index - 1].Fraction) * 640,
                    Is.GreaterThan(10), "Subdivisions must remain visually separate");
        }

        [TestCase(1.25)]
        [TestCase(1.5)]
        [TestCase(2.0)]
        [TestCase(3.0)]
        public void ScalingViewportAndLabelSpacingTogetherPreservesEveryTick(double scale)
        {
            var normal = AxisTickGenerator.Create(-7, 9, 640, 64);
            var scaled = AxisTickGenerator.Create(-7, 9, 640 * scale, 64 * scale);
            NUnit.Framework.Assert.That(scaled.MajorStep, Is.EqualTo(normal.MajorStep));
            NUnit.Framework.Assert.That(scaled.MajorTicks.Select(tick => (tick.Value, tick.Fraction, tick.Label)),
                Is.EqualTo(normal.MajorTicks.Select(tick => (tick.Value, tick.Fraction, tick.Label))));
            NUnit.Framework.Assert.That(scaled.MinorTicks.Select(tick => (tick.Value, tick.Fraction)),
                Is.EqualTo(normal.MinorTicks.Select(tick => (tick.Value, tick.Fraction))));
        }

        [Test]
        public void VisibleSizeAndZoomControlDensityIndependentlyOnEachAxis()
        {
            var shortAxis = AxisTickGenerator.Create(-10, 10, 240);
            var longAxis = AxisTickGenerator.Create(-10, 10, 1200);
            var zoomed = AxisTickGenerator.Create(-1, 1, 1200);
            NUnit.Framework.Assert.That(longAxis.MajorStep, Is.LessThan(shortAxis.MajorStep));
            NUnit.Framework.Assert.That(longAxis.MajorTicks.Count, Is.GreaterThan(shortAxis.MajorTicks.Count));
            NUnit.Framework.Assert.That(zoomed.MajorStep, Is.LessThan(longAxis.MajorStep));
            NUnit.Framework.Assert.That(zoomed.MajorTicks.Count,
                Is.InRange(longAxis.MajorTicks.Count - 2, longAxis.MajorTicks.Count + 2));
        }

        [TestCase(-1e-12, 1e-12)]
        [TestCase(1e12, 5e12)]
        public void ExtremeMagnitudesUseCompactScientificNotation(double minimum, double maximum)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, 800);
            foreach (var tick in ticks.MajorTicks.Where(tick => tick.Value != 0))
            {
                NUnit.Framework.Assert.That(tick.Label, Does.Contain("e"));
                NUnit.Framework.Assert.That(tick.Label.Length, Is.LessThan(16));
                NUnit.Framework.Assert.That(tick.Label, Does.Not.Contain("e+0").And.Not.Contain("e-0"));
            }
        }

        [Test]
        public void LocaleChangesDecimalSeparatorsWithoutChangingValuesOrPositions()
        {
            var invariant = AxisTickGenerator.Create(-0.75, 0.75, 384);
            var polish = AxisTickGenerator.Create(-0.75, 0.75, 384, 64, CultureInfo.GetCultureInfo("pl-PL"));
            NUnit.Framework.Assert.That(polish.MajorTicks.Select(tick => tick.Value),
                Is.EqualTo(invariant.MajorTicks.Select(tick => tick.Value)));
            NUnit.Framework.Assert.That(polish.MajorTicks.Select(tick => tick.Fraction),
                Is.EqualTo(invariant.MajorTicks.Select(tick => tick.Fraction)));
            NUnit.Framework.Assert.That(polish.MajorTicks.Any(tick => tick.Label.Contains(',')), Is.True);
            foreach (var tick in polish.MajorTicks)
                NUnit.Framework.Assert.That(double.Parse(tick.Label, CultureInfo.GetCultureInfo("pl-PL")),
                    Is.EqualTo(tick.Value).Within(1e-14));
            NUnit.Framework.Assert.That(AxisTickGenerator.Format(2.5e-20, 5e-21,
                CultureInfo.GetCultureInfo("pl-PL")), Is.EqualTo("2,5e-20"));
        }

        [TestCase(0.25, 0.25, "0.25")]
        [TestCase(-0.75, 0.25, "-0.75")]
        [TestCase(0.0001, 0.00001, "0.0001")]
        [TestCase(1e-10, 2e-11, "1e-10")]
        [TestCase(2.5e12, 5e11, "2.5e+12")]
        [TestCase(1.234567e100, 0, "1.23457e+100")]
        [TestCase(0.0, 0.1, "0")]
        [TestCase(-0.0, 0.1, "0")]
        public void FormatterKeepsStepPrecisionAndOmitsUnhelpfulZeros(double value, double step, string expected)
        {
            NUnit.Framework.Assert.That(AxisTickGenerator.Format(value, step), Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(ExtremeDomains))]
        public void FiniteExtremeAndAdjacentDoubleDomainsTerminateWithValidTicks(double minimum, double maximum)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, 800);
            AssertValid(ticks, minimum, maximum);
            NUnit.Framework.Assert.That(ticks.MajorTicks.Count, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(ticks.MajorTicks.Select(tick => tick.Label), Is.Unique);
            foreach (var tick in ticks.MajorTicks)
                NUnit.Framework.Assert.That(double.IsFinite(double.Parse(tick.Label,
                    NumberStyles.Float, CultureInfo.InvariantCulture)), Is.True);
            if (minimum == -double.MaxValue && maximum == double.MaxValue)
                NUnit.Framework.Assert.That(ticks.MajorTicks.Single(tick => tick.Value == 0).Fraction,
                    Is.EqualTo(0.5));
        }

        private static IEnumerable<object[]> ExtremeDomains()
        {
            yield return new object[] { -double.MaxValue, double.MaxValue };
            yield return new object[] { double.MaxValue / 2, double.MaxValue };
            yield return new object[] { -double.MaxValue, -double.MaxValue / 2 };
            yield return new object[] { Math.BitDecrement(double.MaxValue), double.MaxValue };
            yield return new object[] { 1.0, Math.BitIncrement(1.0) };
            yield return new object[] { 1e15, 1e15 + 1 };
            yield return new object[] { -1e15 - 1, -1e15 };
            yield return new object[] { 0.0, double.Epsilon };
            yield return new object[] { -double.Epsilon, double.Epsilon };
            yield return new object[] { -1e-320, 1e-320 };
            yield return new object[] { -1e-200, 1e200 };
        }

        [Test]
        public void ExcessiveTargetSizeStillProducesABoundedTickCollection()
        {
            var ticks = AxisTickGenerator.Create(-1e100, 1e100, double.MaxValue, double.Epsilon);
            AssertValid(ticks, -1e100, 1e100);
            NUnit.Framework.Assert.That(ticks.MajorTicks.Count + ticks.MinorTicks.Count, Is.LessThan(800));
        }

        [TestCase(0, 0, 800, 64)]
        [TestCase(2, -2, 800, 64)]
        [TestCase(double.NaN, 2, 800, 64)]
        [TestCase(-2, double.PositiveInfinity, 800, 64)]
        [TestCase(double.NegativeInfinity, 2, 800, 64)]
        [TestCase(-2, 2, double.NaN, 64)]
        [TestCase(-2, 2, double.PositiveInfinity, 64)]
        [TestCase(-2, 2, 0, 64)]
        [TestCase(-2, 2, 31, 64)]
        [TestCase(-2, 2, 800, 0)]
        [TestCase(-2, 2, 800, -1)]
        [TestCase(-2, 2, 800, double.NaN)]
        public void InvalidOrTooShortDomainsReturnAnEmptyLayout(double minimum, double maximum,
            double length, double spacing)
        {
            var ticks = AxisTickGenerator.Create(minimum, maximum, length, spacing);
            NUnit.Framework.Assert.That(ticks.MajorTicks, Is.Empty);
            NUnit.Framework.Assert.That(ticks.MinorTicks, Is.Empty);
            NUnit.Framework.Assert.That(ticks.MajorStep, Is.Zero);
        }

        private static void AssertValid(AxisTickSet ticks, double minimum, double maximum)
        {
            NUnit.Framework.Assert.That(double.IsFinite(ticks.MajorStep), Is.True);
            NUnit.Framework.Assert.That(ticks.MajorStep, Is.GreaterThan(0));
            foreach (var collection in new[] { ticks.MajorTicks, ticks.MinorTicks })
            {
                var previous = double.NegativeInfinity;
                var previousFraction = -1.0;
                foreach (var tick in collection)
                {
                    NUnit.Framework.Assert.That(double.IsFinite(tick.Value) && double.IsFinite(tick.Fraction), Is.True);
                    NUnit.Framework.Assert.That(tick.Value, Is.InRange(minimum, maximum));
                    NUnit.Framework.Assert.That(tick.Fraction, Is.InRange(0, 1));
                    NUnit.Framework.Assert.That(tick.Value, Is.GreaterThan(previous));
                    NUnit.Framework.Assert.That(tick.Fraction, Is.GreaterThan(previousFraction));
                    previous = tick.Value;
                    previousFraction = tick.Fraction;
                }
            }
        }
    }
}
