using System.Globalization;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class MonteCarloIntegralTests(bool generatedSource)
{
    private string Source => generatedSource ? Integral.ToCode : string.Empty;

    [TestCase(2.0, 0.0, 1.0, 1024, 2.0)]
    [TestCase(2.0, 0.0, 1.0, 1, 2.0)]
    [TestCase(-3.0, -2.0, 5.0, 7, -21.0)]
    [TestCase(-3.0, 5.0, -2.0, 7, 21.0)]
    [TestCase(0.0, -4.0, 6.0, 31, 0.0)]
    [TestCase(4.0, 1.0, 1.25, 2, 1.0)]
    [TestCase(4.0, 2.0, 2.0, 3, 0.0)]
    public void ConstantIntegralsHaveNoSamplingError(double value, double a,
        double b, int samples, double expected)
    {
        // Exercise either the public host type or the exact source fragment
        // embedded in desktop TSL; a constant has no statistical tolerance.
        var actual = new PortableEngine().EvaluateReal(
            $"Integral.monteCarloMethod(t => {Literal(value)}, {Literal(a)}, {Literal(b)}, {samples})",
            customFunctions: Source);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(-2.0, 5.0, 1)]
    [TestCase(-2.0, 5.0, 2)]
    [TestCase(-2.0, 5.0, 7)]
    [TestCase(5.0, -2.0, 1024)]
    public void SamplesStayInsideTheIntervalAndUseTheRequestedCount(double a,
        double b, int samples)
    {
        // Check the actual sampled points, not a probabilistic accuracy bound.
        // The upstream scalar overload sampled far outside the interval and
        // called the integrand only ceil(N/2) times.
        var probe = $$"""
            static real SampleCount() {
                int count = 0;
                real sum = 0;
                real area = Integral.monteCarloMethod(t => {
                    if (t < {{Literal(Math.Min(a, b))}} || t > {{Literal(Math.Max(a, b))}})
                        throw new System.InvalidOperationException("Sample is outside the integration interval.");
                    count++;
                    real value = t*t + 1;
                    sum += value;
                    return value;
                }, {{Literal(a)}}, {{Literal(b)}}, {{samples}});
                real expected = {{Literal(b - a)}} * (sum / {{samples}});
                if (System.Math.Abs(area - expected) > 1e-12)
                    throw new System.InvalidOperationException("Integral is not the signed interval width times the sampled mean.");
                return count;
            }
            """;

        var actual = new PortableEngine().EvaluateReal("SampleCount()",
            customFunctions: Source + probe);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(samples));
    }

    private static string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}

[TestFixture]
public class MonteCarloDispatcherTests
{
    [Test]
    public void NamedDispatcherUsesTheCorrectedEstimator()
    {
        NUnit.Framework.Assert.That(
            Integral.integrate("Monte Carlo method", _ => 2.0, 0.0, 1.0, 1024),
            Is.EqualTo(2.0));
    }

    [Test]
    public void HostRetainsItsDocumentedSampleCountConversion()
    {
        var calls = 0;
        var actual = Integral.monteCarloMethod(_ => { calls++; return 2.0; }, 0, 1, 7.9);

        NUnit.Framework.Assert.That(calls, Is.EqualTo(7));
        NUnit.Framework.Assert.That(actual, Is.EqualTo(2.0));
    }
}
