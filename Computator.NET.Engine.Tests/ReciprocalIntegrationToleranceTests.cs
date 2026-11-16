using System;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ReciprocalIntegrationToleranceTests
{
    private static readonly Func<Func<double, double>, double, double, double, double>[] Methods =
    [
        Integral.nonAdaptiveGaussKronrodMethod,
        Integral.doubleExponentialTransformation
    ];

    [Test]
    public void HostRejectsInvalidInverseTolerancesBeforeCallingIntegrand()
    {
        foreach (var method in Methods)
            foreach (var invalid in new[] { 0d, -1d, double.NaN, double.PositiveInfinity,
                         double.NegativeInfinity, double.Epsilon })
            {
                var samples = 0;
                NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                    method(x => { samples++; return x; }, 0, 1, invalid),
                    $"Method {method.Method.Name} accepted inverse tolerance {invalid}.");
                NUnit.Framework.Assert.That(samples, Is.Zero,
                    $"Method {method.Method.Name} evaluated its integrand before validating N.");
            }
    }

    [TestCase("nonAdaptiveGaussKronrodMethod", 0)]
    [TestCase("nonAdaptiveGaussKronrodMethod", -1)]
    [TestCase("doubleExponentialTransformation", 0)]
    [TestCase("doubleExponentialTransformation", -1)]
    public void EmittedTslRejectsNonpositiveInverseTolerances(string method, int budget)
    {
        var expression = $"Integral.{method}(t => t, 0, 1, {budget})";
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PortableEngine().EvaluateReal(expression, customFunctions: Integral.ToCode));
    }

    [Test]
    public void PositiveInverseTolerancesRetainExistingIntegrationResults()
    {
        foreach (var method in Methods)
        {
            var integral = method(_ => 2.0, 0, 1, 10000);
            NUnit.Framework.Assert.That(integral, Is.EqualTo(2.0).Within(1e-3), method.Method.Name);
        }
    }
}
