using System.Globalization;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class DerivativeStepTests
{
    private static readonly string[] Methods =
    [
        nameof(Derivative.finiteDifferenceFormula),
        nameof(Derivative.stableFiniteDifferenceFormula),
        nameof(Derivative.twoPointfiniteDifferenceFormula),
        nameof(Derivative.centeredFivePointMethod)
    ];

    [Test]
    public void ThirdDerivativeOfCubicUsesRequestedStep(
        [ValueSource(nameof(Methods))] string method, [Values] bool generated)
    {
        // The stable forward formula takes sqrt(eps); use the same 0.001
        // relative spacing in all four schemes. Forward differences have
        // truncation error, so comparison is to the analytic derivative with
        // an absolute error budget rather than bitwise equality to another rule.
        var eps = method == nameof(Derivative.stableFiniteDifferenceFormula) ? 1e-6 : 1e-3;
        var actual = generated
            ? new PortableEngine().EvaluateReal(
                $"Derivative.{method}(t => t*t*t, 2.0, 3, {Literal(eps)})",
                customFunctions: Derivative.ToCode)
            : Evaluate(method, t => t * t * t, 3, eps);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(6.0).Within(0.02));
    }

    [Test]
    public void CallerStepReachesEveryLevelOfTheSamplingStencil(
        [ValueSource(nameof(Methods))] string method,
        [Values(2u, 3u)] uint order, [Values] bool generated)
    {
        // Choose exactly representable steps giving the same rightmost
        // displacement, x/4, in every rule. After two/three levels from x=2,
        // the requested stencil reaches 2*(5/4)^2 and 2*(5/4)^3 respectively.
        // Inspecting where the user's callable is evaluated catches a reset to
        // the tiny default even when it happens to give an accurate derivative.
        var eps = method switch
        {
            nameof(Derivative.stableFiniteDifferenceFormula) => 0.0625,
            nameof(Derivative.centeredFivePointMethod) => 0.125,
            _ => 0.25
        };
        double largestSample = double.NegativeInfinity;
        if (generated)
        {
            var probe = $$"""
                public static double LargestSample = double.NegativeInfinity;
                public static double Sample(double value)
                {
                    LargestSample = System.Math.Max(LargestSample, value);
                    return value*value;
                }
                public static double Probe()
                {
                    Derivative.{{method}}(Sample, 2.0, {{order}}, {{Literal(eps)}});
                    return LargestSample;
                }
                """;
            largestSample = new PortableEngine().EvaluateReal("Probe()",
                customFunctions: Derivative.ToCode + probe);
        }
        else
        {
            Evaluate(method, value =>
            {
                largestSample = Math.Max(largestSample, value);
                return value * value;
            }, order, eps);
        }

        NUnit.Framework.Assert.That(largestSample, Is.EqualTo(order == 2 ? 3.125 : 3.90625));
    }

    [Test]
    public void ZeroAndFirstOrderRetainExistingBehavior(
        [ValueSource(nameof(Methods))] string method, [Values] bool generated)
    {
        var eps = method == nameof(Derivative.stableFiniteDifferenceFormula) ? 0.0625 : 0.25;
        var expectedFirst = method is nameof(Derivative.finiteDifferenceFormula)
            or nameof(Derivative.stableFiniteDifferenceFormula) ? 4.5 : 4.0;
        foreach (var order in new[] { 0u, 1u })
        {
            var actual = generated
                ? new PortableEngine().EvaluateReal(
                    $"Derivative.{method}(t => t*t, 2.0, {order}, {Literal(eps)})",
                    customFunctions: Derivative.ToCode)
                : Evaluate(method, t => t * t, order, eps);
            NUnit.Framework.Assert.That(actual, Is.EqualTo(order == 0 ? 4.0 : expectedFirst).Within(1e-12));
        }
    }

    private static string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static double Evaluate(string method, Func<double, double> function, uint order, double eps) =>
        method switch
        {
            nameof(Derivative.finiteDifferenceFormula) => Derivative.finiteDifferenceFormula(function, 2, order, eps),
            nameof(Derivative.stableFiniteDifferenceFormula) => Derivative.stableFiniteDifferenceFormula(function, 2, order, eps),
            nameof(Derivative.twoPointfiniteDifferenceFormula) => Derivative.twoPointfiniteDifferenceFormula(function, 2, order, eps),
            nameof(Derivative.centeredFivePointMethod) => Derivative.centeredFivePointMethod(function, 2, order, eps),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };
}
