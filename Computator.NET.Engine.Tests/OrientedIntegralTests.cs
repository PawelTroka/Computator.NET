using System.Globalization;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class OrientedIntegralTests
{
    private static readonly string[] Methods =
    [
        nameof(Integral.rectangleMethod),
        nameof(Integral.trapezoidalMethod),
        nameof(Integral.simpsonMethod)
    ];

    public static IEnumerable<TestCaseData> Cases()
    {
        foreach (var generated in new[] { false, true })
        {
            foreach (var method in Methods)
            {
                yield return Case(method, "2*t+1", 3, -1, 4, -12, generated);
                yield return Case(method, "2*t+1", -1, 3, 4, 12, generated);
                yield return Case(method, "2*t+1", 1.5, 1.5, 4, 0, generated);
                // An integrand with a restricted domain detects nodes outside
                // the interval, even if an implementation only negates its sum.
                yield return Case(method, "Guarded(t)", 1, 0, 4, -2, generated);
            }

            // These are the rule-specific sums on six equal subintervals,
            // independently calculated for t^2 on [2, -1]. Only Simpson is exact.
            yield return Case(nameof(Integral.rectangleMethod), "t*t", 2, -1, 6, -47.0 / 16, generated);
            yield return Case(nameof(Integral.trapezoidalMethod), "t*t", 2, -1, 6, -25.0 / 8, generated);
            yield return Case(nameof(Integral.simpsonMethod), "t*t", 2, -1, 6, -3, generated);
            // Preserve the existing adjustment of an odd count to the next even count.
            yield return Case(nameof(Integral.simpsonMethod), "t*t*t", 3, -1, 3, -20, generated);
        }
    }

    private static TestCaseData Case(string method, string integrand, double a, double b,
        int steps, double expected, bool generated) =>
        new TestCaseData(method, integrand, a, b, steps, expected, generated)
            .SetName($"OrientedIntegral_{method}_{(generated ? "Generated" : "Host")}_{integrand}_{a}_{b}_{steps}");

    [TestCaseSource(nameof(Cases))]
    public void QuadratureUsesTheOrientedInterval(string method, string integrand,
        double a, double b, int steps, double expected, bool generated)
    {
        double actual;
        if (generated)
        {
            // Embed the exact shipping ToCode fragment as a nested class, just
            // as the Windows evaluator does. It shadows the imported host type;
            // calling the normal portable Integral alone would miss a stale copy.
            var expression = $"Integral.{method}((real t) => {integrand}, " +
                $"{a.ToString("R", CultureInfo.InvariantCulture)}, {b.ToString("R", CultureInfo.InvariantCulture)}, {steps})";
            actual = new PortableEngine().EvaluateReal(expression,
                customFunctions: Integral.ToCode + @"
                    static real Guarded(real t) {
                        if (t < 0 || t > 1) throw new System.ArgumentOutOfRangeException(nameof(t));
                        return 2*t+1;
                    }");
        }
        else
        {
            Func<double, double> function = integrand switch
            {
                "2*t+1" => t => 2*t+1,
                "t*t" => t => t*t,
                "t*t*t" => t => t*t*t,
                "Guarded(t)" => t => t < 0 || t > 1
                    ? throw new ArgumentOutOfRangeException(nameof(t)) : 2*t+1,
                _ => throw new ArgumentException(nameof(integrand))
            };
            actual = method switch
            {
                nameof(Integral.rectangleMethod) => Integral.rectangleMethod(function, a, b, steps),
                nameof(Integral.trapezoidalMethod) => Integral.trapezoidalMethod(function, a, b, steps),
                nameof(Integral.simpsonMethod) => Integral.simpsonMethod(function, a, b, steps),
                _ => throw new ArgumentException(nameof(method))
            };
        }

        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(1e-12));
    }
}
