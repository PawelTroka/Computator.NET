using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableOrientedIntegralTests
{
    [TestCase("x", 1.0, 0.0, -0.5)]
    [TestCase("2*x+1", 3.0, -1.0, -12.0)]
    [TestCase("x*x*x", 3.0, -1.0, -20.0)]
    [TestCase("x*x", -1.0, -3.0, -26.0 / 3.0)]
    [TestCase("x*x", -3.0, -1.0, 26.0 / 3.0)]
    [TestCase("x*x+2", 1.5, 1.5, 0.0)]
    [TestCase("x*x+2", 0.0, 0.0, 0.0)]
    public void PortableIntegrationPreservesTheRequestedOrientation(string expression,
        double a, double b, double expected)
    {
        var actual = new PortableEngine().Integrate(expression, a, b, 4);
        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(1e-12));
    }

    [TestCase(1.0, 0.0, -2.0)]
    [TestCase(0.0, 1.0, 2.0)]
    [TestCase(0.5, 0.5, 0.0)]
    public void CustomIntegrandNeverSamplesOutsideTheInterval(double a, double b, double expected)
    {
        const string functions = """
            static real Probe(real x) {
                if (x < 0 || x > 1) throw new System.ArgumentOutOfRangeException(nameof(x));
                return 2*x+1;
            }
            """;
        var actual = new PortableEngine().Integrate("Probe(x)", a, b, 4, functions);
        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(1e-12));
    }

    [TestCase(double.NaN, 0.0)]
    [TestCase(0.0, double.NaN)]
    [TestCase(double.PositiveInfinity, 0.0)]
    [TestCase(0.0, double.NegativeInfinity)]
    public void NonfiniteIntegrationBoundsAreRejectedBeforeCompilation(double a, double b) =>
        NUnit.Framework.Assert.Throws<ArgumentException>(() =>
            new PortableEngine().Integrate("missing()", a, b));

    [TestCase(1.0, 0.0, 0)]
    [TestCase(1.0, 0.0, 3)]
    [TestCase(1.0, 1.0, 1000002)]
    public void IntegrationWorkLimitsStillApplyForReversedAndEqualBounds(double a, double b, int steps) =>
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PortableEngine().Integrate("missing()", a, b, steps));

    [TestCase(1.0, 0.0)]
    [TestCase(1.0, 1.0)]
    public void ValidBoundsDoNotSuppressCompilationDiagnostics(double a, double b) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().Integrate("missing()", a, b, 2));

    [TestCase(1.0, 0.0)]
    [TestCase(1.0, 1.0)]
    public void RootFindingStillRequiresIncreasingBounds(double a, double b) =>
        NUnit.Framework.Assert.Throws<ArgumentException>(() =>
            new PortableEngine().FindRoot("missing()", a, b));
}
