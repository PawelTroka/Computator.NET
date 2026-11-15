using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class PortableSincSignTests
{
    [TestCase(0.0, 1.0)]
    [TestCase(1.5707963267948966, 0.6366197723675814)]
    [TestCase(-1.5707963267948966, 0.6366197723675814)]
    [TestCase(1e-10, 1.0)]
    public void RealSincMatchesDesktopForZeroAndBothSigns(double x, double expected)
    {
        var engine = new PortableEngine();
        Assert.That(engine.EvaluateReal("sinc(x)", x), Is.EqualTo(expected).Within(1e-12));
    }

    [Test]
    public void ComplexSincHandlesZeroAndNonRealArguments()
    {
        var engine = new PortableEngine();
        Assert.That(engine.EvaluateComplex("sinc(z)", Complex.Zero), Is.EqualTo(Complex.One));

        var imaginary = engine.EvaluateComplex("sinc(z)", Complex.ImaginaryOne);
        Assert.That((imaginary - new Complex(Math.Sinh(1), 0)).Magnitude, Is.LessThan(1e-12));

        var argument = new Complex(1, -0.75);
        var actual = engine.EvaluateComplex("sinc(z)", argument);
        Assert.That((actual - Complex.Sin(argument) / argument).Magnitude, Is.LessThan(1e-12));
    }

    [TestCase(-123, -1)]
    [TestCase(-0.5, -1)]
    [TestCase(0, 0)]
    [TestCase(0.5, 1)]
    [TestCase(123, 1)]
    public void SignMatchesDesktopForFiniteInputs(double x, double expected) =>
        Assert.That(new PortableEngine().EvaluateReal("sgn(x)", x), Is.EqualTo(expected));

    [Test]
    public void SignHandlesNaNInfinityAndNegativeZero()
    {
        var engine = new PortableEngine();
        Assert.That(double.IsNaN(engine.EvaluateReal("sgn(x)", double.NaN)), Is.True);
        Assert.That(engine.EvaluateReal("sgn(x)", double.PositiveInfinity), Is.EqualTo(1));
        Assert.That(engine.EvaluateReal("sgn(x)", double.NegativeInfinity), Is.EqualTo(-1));
        Assert.That(engine.EvaluateReal("sgn(x)", -0.0), Is.EqualTo(0));
    }

    [Test]
    public void ScriptsAndCustomFunctionsUseTheSameAdapters()
    {
        var engine = new PortableEngine();
        Assert.That(engine.ExecuteScript("write(sinc(0)); write(sgn(-3));"), Is.EqualTo("1-1"));
        Assert.That(engine.EvaluateReal("Probe(0)",
            customFunctions: "static real Probe(real u) { return sinc(u) + sgn(u); }"), Is.EqualTo(1));
        Assert.That(engine.EvaluateComplex("Probe(z)", Complex.Zero,
            "static complex Probe(complex u) { return sinc(u); }"), Is.EqualTo(Complex.One));
    }

    [Test]
    public void NumericalIntegrationCanSampleSincAtZero()
    {
        var actual = new PortableEngine().Integrate("sinc(x)", -1, 1, steps: 1000);
        Assert.That(double.IsFinite(actual), Is.True);
        Assert.That(actual, Is.EqualTo(1.892166140734366).Within(1e-7));
    }
}
