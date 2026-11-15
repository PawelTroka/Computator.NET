using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class GaussKronrod61Tests
{
    [TestCase(false)]
    [TestCase(true)]
    public void IntegratesSmoothPolynomialsAndTrigonometricFunctions(bool generated)
    {
        var quartic = generated
            ? new PortableEngine().EvaluateReal(
                "Integral.gaussKronrod61Method((real t) => t*t*t*t, 0, 1, 10000)",
                customFunctions: Integral.ToCode)
            : Integral.gaussKronrod61Method(t => t*t*t*t, 0, 1, 10000);
        NUnit.Framework.Assert.That(quartic, Is.EqualTo(0.2).Within(1e-10));

        var sine = generated
            ? new PortableEngine().EvaluateReal(
                "Integral.gaussKronrod61Method((real t) => sin(t), 0, PI, 10000)",
                customFunctions: Integral.ToCode)
            : Integral.gaussKronrod61Method(Math.Sin, 0, Math.PI, 10000);
        NUnit.Framework.Assert.That(sine, Is.EqualTo(2).Within(1e-8));
    }

    [Test]
    public void NamedDispatcherExposesTheSixtyOnePointRule()
    {
        NUnit.Framework.Assert.That(
            Integral.integrate("61-point Gauss–Kronrod method", x => x*x, 0, 1, 10000),
            Is.EqualTo(1.0 / 3).Within(1e-10));
    }

    [Test]
    public void EqualBoundsReturnZeroWithoutSampling()
    {
        var called = false;
        var value = Integral.gaussKronrod61Method(x => { called = true; throw new Exception(); }, 3, 3);
        NUnit.Framework.Assert.That(value, Is.Zero);
        NUnit.Framework.Assert.That(called, Is.False);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(1e15)]
    public void InvalidPrecisionIsRejectedBeforeSampling(double inverseTolerance)
    {
        var called = false;
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            Integral.gaussKronrod61Method(x => { called = true; return x; }, 0, 1, inverseTolerance));
        NUnit.Framework.Assert.That(called, Is.False);
    }

    [Test]
    public void NonFinitePrecisionIsRejected()
    {
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            Integral.gaussKronrod61Method(Math.Sin, 0, 1, double.NaN));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            Integral.gaussKronrod61Method(Math.Sin, 0, 1, double.PositiveInfinity));
    }
}
