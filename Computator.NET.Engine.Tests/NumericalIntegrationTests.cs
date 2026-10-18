using System.Globalization;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class NumericalIntegrationTests(bool generatedSource)
{
    private string SolverSource => generatedSource ? Derivative.ToCode + FunctionRoot.ToCode : string.Empty;

    [TestCase(-10.0, 10.0, 0.0, 6.0)]
    [TestCase(10.0, 12.0, 4.75, 10.75)]
    public void NamedSolversComposeInATslExpression(double a, double b, double shift, double expected)
    {
        // A cubic's third derivative is six. The exactly representable 0.125
        // spacing avoids cancellation in the five-point polynomial stencil,
        // but resetting inner levels to the default destroys that result.
        // The root depends on that derivative, and exercises the secant guesses
        // in both a symmetric interval and a translated interval.
        var expression = $$"""
            FunctionRoot.findRoot("secant Newton Raphson method",
                t => t - Derivative.derivative("centered five-point method",
                    u => u³, 2.0, 3, 0.125) - {{Literal(shift)}},
                {{Literal(a)}}, {{Literal(b)}}, 1e-10, 100)
            """;

        // Execute the same caller through the TSL transformer against either
        // the host types or both embedded classes in one generated program.
        var actual = new PortableEngine().EvaluateReal(expression, customFunctions: SolverSource);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(1e-9));
    }

    [Test]
    public void NarrowIntervalRejectsTheNestedTslFunctionBeforeEvaluatingIt()
    {
        var expression = """
            FunctionRoot.findRoot("secant Newton Raphson method",
                t => Derivative.derivative("centered five-point method",
                    u => throw new System.InvalidOperationException("The function must not be sampled."),
                    t, 3, 0.125),
                1.0, System.Math.BitIncrement(System.Math.BitIncrement(1.0)), 1e-10, 100)
            """;

        var actual = new PortableEngine().EvaluateReal(expression, customFunctions: SolverSource);

        NUnit.Framework.Assert.That(actual, Is.NaN);
    }

    private static string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
