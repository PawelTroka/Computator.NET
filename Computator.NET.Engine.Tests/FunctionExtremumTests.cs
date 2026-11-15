using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class FunctionExtremumTests
{
    [TestCase(0.1)]
    [TestCase(0.5)]
    [TestCase(0.9)]
    public void FindsAnInteriorMinimumIncludingNearEitherBoundary(double location)
    {
        var result = FunctionExtremum.minimize(x => (x - location) * (x - location), 0, 1, 1e-10);
        NUnit.Framework.Assert.That(result, Is.EqualTo(location).Within(1e-8));
    }

    [Test]
    public void FindsAnInteriorMaximumAndReturnsItsLocationInsteadOfItsValue()
    {
        var result = FunctionExtremum.maximize(x => 7 - (x - 2) * (x - 2), -3, 5);
        NUnit.Framework.Assert.That(result, Is.EqualTo(2).Within(1e-7));
    }

    [Test]
    public void SolvesNonquadraticAndNonsmoothUnimodalFunctions()
    {
        NUnit.Framework.Assert.That(FunctionExtremum.maximize(Math.Sin, 0, Math.PI),
            Is.EqualTo(Math.PI / 2).Within(1e-7));
        NUnit.Framework.Assert.That(FunctionExtremum.minimize(x => Math.Abs(x - 0.2), -1, 1),
            Is.EqualTo(0.2).Within(1e-7));
    }

    [TestCase(false, 1, -3)]
    [TestCase(false, -1, 5)]
    [TestCase(true, 1, 5)]
    [TestCase(true, -1, -3)]
    public void MonotoneFunctionsReturnTheExactClosedIntervalEndpoint(bool maximum, int slope, double expected)
    {
        var result = maximum
            ? FunctionExtremum.maximize(x => slope * x, -3, 5)
            : FunctionExtremum.minimize(x => slope * x, -3, 5);
        NUnit.Framework.Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void FlatFunctionsChooseTheLowerEndpoint(bool maximum)
    {
        var result = maximum
            ? FunctionExtremum.maximize(x => 7, -3, 5)
            : FunctionExtremum.minimize(x => 7, -3, 5);
        NUnit.Framework.Assert.That(result, Is.EqualTo(-3));
    }

    [Test]
    public void EqualBoundsSampleTheSinglePointOnce()
    {
        var seen = new List<double>();
        NUnit.Framework.Assert.That(FunctionExtremum.minimize(x => { seen.Add(x); return 9; }, 2, 2), Is.EqualTo(2));
        NUnit.Framework.Assert.That(seen, Is.EqualTo(new[] { 2.0 }));
    }

    [Test]
    public void TheSearchNeverExpandsBeyondTheSuppliedBounds()
    {
        var seen = new List<double>();
        var result = FunctionExtremum.minimize(x =>
        {
            seen.Add(x);
            if (x < 0 || x > 1) throw new InvalidOperationException("outside domain");
            return (x - 0.01) * (x - 0.01);
        }, 0, 1);
        NUnit.Framework.Assert.That(result, Is.EqualTo(0.01).Within(1e-7));
        NUnit.Framework.Assert.That(seen, Does.Contain(0).And.Contain(1));
        NUnit.Framework.Assert.That(seen.All(x => x >= 0 && x <= 1), Is.True);
    }

    [TestCase(1e12)]
    [TestCase(-1e12)]
    public void TranslatingANarrowIntervalDoesNotCausePrematureConvergence(double offset)
    {
        var expected = offset + 25;
        var result = FunctionExtremum.minimize(x => (x - expected) * (x - expected), offset, offset + 100);
        NUnit.Framework.Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    [TestCase(1e-200)]
    [TestCase(1e200)]
    public void ScaledIntervalsRetainTheSameUnimodalSolution(double scale)
    {
        var result = FunctionExtremum.minimize(x => (x / scale - 0.25) * (x / scale - 0.25),
            0, scale, scale * 1e-10);
        NUnit.Framework.Assert.That(result / scale, Is.EqualTo(0.25).Within(1e-7));
    }

    [Test]
    public void AnIntervalWithOnlyItsEndpointsStillReturnsTheBetterOne()
    {
        var a = 1.0;
        var b = Math.BitIncrement(a);
        NUnit.Framework.Assert.That(FunctionExtremum.minimize(x => -x, a, b), Is.EqualTo(b));
    }

    [TestCase(double.NaN, 1)]
    [TestCase(0, double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity, 1)]
    [TestCase(2, 1)]
    [TestCase(-double.MaxValue, double.MaxValue)]
    public void InvalidBoundsAreRejectedBeforeSampling(double a, double b)
    {
        var samples = 0;
        NUnit.Framework.Assert.That(() => FunctionExtremum.minimize(x => { samples++; return x; }, a, b),
            Throws.InstanceOf<ArgumentException>());
        NUnit.Framework.Assert.That(samples, Is.Zero);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void InvalidToleranceIsRejectedBeforeSampling(double tolerance)
    {
        var samples = 0;
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            FunctionExtremum.minimize(x => { samples++; return x; }, 0, 1, tolerance));
        NUnit.Framework.Assert.That(samples, Is.Zero);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1000001)]
    public void InvalidBudgetsCannotStartAnUnboundedSearch(int iterations)
    {
        var samples = 0;
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            FunctionExtremum.minimize(x => { samples++; return x; }, 0, 1, iterations: iterations));
        NUnit.Framework.Assert.That(samples, Is.Zero);
    }

    [Test]
    public void NullFunctionIsRejected()
    {
        NUnit.Framework.Assert.Throws<ArgumentNullException>(() => FunctionExtremum.minimize(null!, 0, 1));
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void NonfiniteEndpointAndInteriorSamplesAreRejected(double invalid)
    {
        NUnit.Framework.Assert.Throws<ArithmeticException>(() => FunctionExtremum.minimize(x => invalid, 0, 1));
        NUnit.Framework.Assert.Throws<ArithmeticException>(() =>
            FunctionExtremum.minimize(x => x == 0 || x == 1 ? 1 : invalid, 0, 1));
        NUnit.Framework.Assert.Throws<ArithmeticException>(() => FunctionExtremum.maximize(x => invalid, 0, 1));
    }

    [Test]
    public void CallbackExceptionsRemainVisible()
    {
        var error = new InvalidOperationException("callback failed");
        var actual = NUnit.Framework.Assert.Throws<InvalidOperationException>(() =>
            FunctionExtremum.minimize(x => throw error, 0, 1));
        NUnit.Framework.Assert.That(actual, Is.SameAs(error));
    }

    [Test]
    public void ExhaustedBudgetDoesNotReturnAnUnconvergedCandidate()
    {
        NUnit.Framework.Assert.Throws<InvalidOperationException>(() =>
            FunctionExtremum.minimize(x => (x - 0.25) * (x - 0.25), 0, 1, 1e-12, 1));
    }

    [Test]
    public void UnattainablePrecisionStillHonorsTheIterationBudget()
    {
        var samples = 0;
        NUnit.Framework.Assert.Throws<InvalidOperationException>(() =>
            FunctionExtremum.minimize(x => { samples++; return x; }, 0, 1e300, 1e-300));
        NUnit.Framework.Assert.That(samples, Is.LessThanOrEqualTo(1003));
    }
}
