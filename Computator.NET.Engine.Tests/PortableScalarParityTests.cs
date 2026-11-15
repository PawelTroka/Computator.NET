using System.Globalization;
using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableScalarParityTests
{
    [TestCase("tg(PI/4)", 1.0)]
    [TestCase("arcsin(0.5)", Math.PI / 6)]
    [TestCase("arccos(0.5)", Math.PI / 3)]
    [TestCase("arctan(1)", Math.PI / 4)]
    [TestCase("arctg(-1)", -Math.PI / 4)]
    [TestCase("sinh(ln(2))", 0.75)]
    [TestCase("cosh(ln(2))", 1.25)]
    [TestCase("tanh(ln(2))", 0.6)]
    [TestCase("tgh(-ln(2))", -0.6)]
    [TestCase("log2(8)", 3.0)]
    [TestCase("log10(1000)", 3.0)]
    [TestCase("log(81,3)", 4.0)]
    [TestCase("Re(new complex(3,4))", 3.0)]
    [TestCase("Im(new complex(3,4))", 4.0)]
    [TestCase("Phase(new complex(0,1))", Math.PI / 2)]
    [TestCase("Magnitude(new complex(3,4))", 5.0)]
    public void DesktopScalarSpellingsExecuteThroughTsl(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression),
            Is.EqualTo(expected).Within(1e-12));
    }

    private static IEnumerable<TestCaseData> ComplexCases()
    {
        // Independent identities at imaginary arguments exercise Complex overload
        // binding, not just real values implicitly converted to Complex on return.
        yield return new TestCaseData("tg(z)", 0.0, 1.0, 0.0, 0.7615941559557649);
        yield return new TestCaseData("arcsin(z)", 0.0, 1.0, 0.0, 0.8813735870195430);
        yield return new TestCaseData("arccos(z)", 0.0, 1.0, Math.PI / 2, -0.8813735870195430);
        yield return new TestCaseData("arctan(z)", 0.0, 0.5, 0.0, 0.5493061443340548);
        yield return new TestCaseData("arctg(z)", 0.0, -0.5, 0.0, -0.5493061443340548);
        yield return new TestCaseData("sinh(z)", 0.0, Math.PI / 2, 0.0, 1.0);
        yield return new TestCaseData("cosh(z)", 0.0, Math.PI, -1.0, 0.0);
        yield return new TestCaseData("tanh(z)", 0.0, Math.PI / 4, 0.0, 1.0);
        yield return new TestCaseData("tgh(z)", 0.0, -Math.PI / 4, 0.0, -1.0);
        yield return new TestCaseData("log2(z)", -4.0, 0.0, 2.0, 4.532360141827194);
        yield return new TestCaseData("log10(z)", -100.0, 0.0, 2.0, 1.3643763538418412);
        yield return new TestCaseData("log(z,2)", -8.0, 0.0, 3.0, 4.532360141827194);
        yield return new TestCaseData("conj(z)", 3.0, 4.0, 3.0, -4.0);
        yield return new TestCaseData("Re(z)+Im(z)*i", 3.0, 4.0, 3.0, 4.0);
        yield return new TestCaseData("Magnitude(z)*exp(i*Phase(z))", 3.0, 4.0, 3.0, 4.0);
    }

    [TestCaseSource(nameof(ComplexCases))]
    public void ComplexOverloadsRetainTheDesktopBranchConvention(string expression,
        double real, double imaginary, double expectedReal, double expectedImaginary)
    {
        var actual = new PortableEngine().EvaluateComplex(expression, new Complex(real, imaginary));
        NUnit.Framework.Assert.That((actual - new Complex(expectedReal, expectedImaginary)).Magnitude,
            Is.LessThanOrEqualTo(1e-12));
    }

    [TestCase("tg")]
    [TestCase("arcsin")]
    [TestCase("arctan")]
    [TestCase("arctg")]
    [TestCase("sinh")]
    [TestCase("tanh")]
    [TestCase("tgh")]
    public void RealOddFunctionsPreserveNegativeZero(string function)
    {
        var negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
        var actual = new PortableEngine().EvaluateReal(function + "(x)", negativeZero);
        NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual), Is.EqualTo(long.MinValue));
    }

    [TestCase("arcsin(2)")]
    [TestCase("arccos(-2)")]
    [TestCase("log2(-1)")]
    [TestCase("log10(-1)")]
    [TestCase("log(8,1)")]
    [TestCase("log(8,-2)")]
    public void InvalidRealDomainsRemainNaN(string expression)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.NaN);
    }

    [TestCase("log2(0)", double.NegativeInfinity)]
    [TestCase("log10(0)", double.NegativeInfinity)]
    [TestCase("log(0,2)", double.NegativeInfinity)]
    [TestCase("arctan(x)", Math.PI / 2)]
    [TestCase("tanh(x)", 1.0)]
    [TestCase("sinh(x)", double.PositiveInfinity)]
    public void RealEndpointsHaveTheirBclValues(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, double.PositiveInfinity),
            Is.EqualTo(expected));
    }

    [Test]
    public void ExplicitComplexInputProvidesComplexContinuation()
    {
        var engine = new PortableEngine();
        // Merely requesting a Complex result must not silently promote a real
        // argument and change overload selection. The argument chooses the domain.
        NUnit.Framework.Assert.That(engine.EvaluateComplex("log2(-4)", Complex.Zero).Real, Is.NaN);
        var continued = engine.EvaluateComplex("log2(new complex(-4,0))", Complex.Zero);
        NUnit.Framework.Assert.That(continued.Real, Is.EqualTo(2).Within(1e-12));
        NUnit.Framework.Assert.That(continued.Imaginary, Is.EqualTo(4.532360141827194).Within(1e-12));
    }

    [Test]
    public void ExistingPortableLogAndRootContractsAreUnchanged()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("log(100)"), Is.EqualTo(2));
        NUnit.Framework.Assert.That(engine.EvaluateComplex("log(z)", new Complex(100, 0)).Real,
            Is.EqualTo(2).Within(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("sqrt(-1)"), Is.NaN);
        NUnit.Framework.Assert.That(engine.EvaluateComplex("sqrt(z)", new Complex(-1, 0)), Is.EqualTo(Complex.ImaginaryOne));
    }

    [TestCase("pl-PL")]
    [TestCase("de-DE")]
    [NonParallelizable]
    public void DefinitionsAndScriptsUseTheSameScalarCatalog(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var result = new PortableEngine().ExecuteScript(
                "write(Probe(new complex(3,4))); write(\" λ \" ); write(log2(8));",
                "static real Probe(complex z) { return Re(conj(z))+Magnitude(z); }");
            NUnit.Framework.Assert.That(result, Is.EqualTo("8 λ 3"));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Test]
    public void NumericalCommandsCanUseTheNewFunctions()
    {
        var engine = new PortableEngine();
        // integral(sinh(x), 0, ln(2)) = cosh(ln(2)) - 1 = 1/4.
        NUnit.Framework.Assert.That(engine.Integrate("sinh(x)", 0, Math.Log(2), 100),
            Is.EqualTo(0.25).Within(1e-10));
        NUnit.Framework.Assert.That(engine.Differentiate("arctan(x)", 0),
            Is.EqualTo(1).Within(1e-6));
        NUnit.Framework.Assert.That(engine.FindRoot("log2(x)-3", 1, 16),
            Is.EqualTo(8).Within(1e-6));
    }

    [TestCase("sinh", 0.5)]
    [TestCase("arcsin", 0.5)]
    [TestCase("log2", 8.0)]
    [TestCase("log10", 100.0)]
    public void DirectAndCompiledCallsShareTheSameOverload(string function, double argument)
    {
        var method = typeof(ScalarFunctions).GetMethod(function, [typeof(double)]);
        NUnit.Framework.Assert.That(method, Is.Not.Null);
        var direct = (double)method!.Invoke(null, [argument])!;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(function + "(x)", argument), Is.EqualTo(direct));
    }
}
