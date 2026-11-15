using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class SteffensenRootTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void FindsLinearAndNonlinearRootsInHostAndGeneratedTsl(bool embedded)
    {
        double Root(string function, double a, double b)
        {
            if (embedded)
                return new PortableEngine().EvaluateReal(
                    $"FunctionRoot.findRoot(\"Steffensen's method\", (real t) => {function}, {a}, {b}, 1e-12, 50)",
                    customFunctions: FunctionRoot.ToCode);
            return function switch
            {
                "t*t-2" => FunctionRoot.findRoot("Steffensen's method", t => t * t - 2, a, b, 1e-12, 50),
                "t+3" => FunctionRoot.SteffensenMethod(t => t + 3, a, b, 1e-12, 50),
                _ => throw new ArgumentException(nameof(function))
            };
        }

        NUnit.Framework.Assert.That(Root("t*t-2", 1, 2), Is.EqualTo(Math.Sqrt(2)).Within(1e-10));
        NUnit.Framework.Assert.That(Root("t+3", -5, 1), Is.EqualTo(-3).Within(1e-12));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void NonConvergentOrUndefinedStepsReturnNaN(bool embedded)
    {
        double Run(string body, double a, double b, uint iterations)
        {
            if (embedded)
                return new PortableEngine().EvaluateReal(
                    $"FunctionRoot.SteffensenMethod((real t) => {body}, {a}, {b}, 1e-12, {iterations})",
                    customFunctions: FunctionRoot.ToCode);
            return FunctionRoot.SteffensenMethod(t => body == "1" ? 1 : double.NaN, a, b, 1e-12, iterations);
        }

        NUnit.Framework.Assert.That(Run("1", -1, 1, 10), Is.NaN);
        NUnit.Framework.Assert.That(Run("double.NaN", -1, 1, 10), Is.NaN);
        NUnit.Framework.Assert.That(Run("1", 2, 1, 10), Is.NaN);
        NUnit.Framework.Assert.That(Run("1", -1, 1, 0), Is.NaN);
    }

    [Test]
    public void RejectsInvalidToleranceBeforeSampling()
    {
        var sampled = false;
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            FunctionRoot.SteffensenMethod(x => { sampled = true; return x; }, 0, 1, 0, 10));
        NUnit.Framework.Assert.That(sampled, Is.False);
    }
}
