using System.Numerics;
using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class SymbolicFunctionPowerTests
{
    [TestCase("cosᶻ(z)", "pow(cos(z),z)")]
    [TestCase("cos⁻ᶻ(z)", "pow(cos(z),-z)")]
    [TestCase("cos⁽ᶻ⁺¹⁾(z)", "pow(cos(z),(z+1))")]
    [TestCase("cos²˙ᶻ(z)", "pow(cos(z),2*z)")]
    [TestCase("cosᶜᵒˢ⁽ᶻ⁾(z)", "pow(cos(z),cos(z))")]
    [TestCase("cosᶻ(z)²", "pow(pow(cos(z),z),2)")]
    [TestCase("cosʸ(sinˣ(x))", "pow(cos(pow(sin(x),x)),y)")]
    [TestCase("2cosʸ(x)", "2*pow(cos(x),y)")]
    [TestCase("cosʸ(x)x", "pow(cos(x),y)*x")]
    [TestCase("System.Math.Cos⁽ʸ⁾(x)", "pow(System.Math.Cos(x),(y))")]
    [TestCase("Probe⁽ʸ⁾()", "pow(Probe(),(y))")]
    [TestCase("Probe⁽ʸ⁺¹⁾(x,y)", "pow(Probe(x,y),(y+1))")]
    [TestCase("Probe⁽⁻ʸ⁾(x)", "pow(Probe(x),(-y))")]
    [TestCase("ψⁿ⁽ˣ⁾(x)", "pow(ψⁿ(x),(x))")]
    [TestCase("sinhᶻ(z)", "pow(sinh(z),z)")]
    [TestCase("arccosᶻ(z)", "pow(arccos(z),z)")]
    [TestCase("return(cosʸ(x))²;", "return pow((pow(cos(x),y)),2);")]
    [TestCase("cos /* comment */ ʸ (x)", "pow(cos(x),y)")]
    public void SymbolicPrefixesUseTheSharedTslPipeline(string source, string expected) =>
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source), Is.EqualTo(expected));

    [TestCase("ψⁿ(x);", "ψⁿ(x);")]
    [TestCase("Probeˣ(x)", "Probeˣ(x)")]
    [TestCase("Probeʸ(x)", "Probeʸ(x)")]
    [TestCase("Probeᶻ(x)", "Probeᶻ(x)")]
    [TestCase("@cosᶻ(z)", "@cosᶻ(z)")]
    [TestCase("Helpers.cosᶻ(z)", "Helpers.cosᶻ(z)")]
    [TestCase("Helpers . cosᶻ(z)", "Helpers . cosᶻ(z)")]
    [TestCase("mycosᶻ(z)", "mycosᶻ(z)")]
    [TestCase("System.Math.Cosᶻ(z)", "System.Math.Cosᶻ(z)")]
    [TestCase("GetHelpers().cosᶻ(z)", "GetHelpers().cosᶻ(z)")]
    [TestCase("GetHelpers(). cosᶻ(z)", "GetHelpers(). cosᶻ(z)")]
    [TestCase("Helpers<real>.cosᶻ(z)", "Helpers<real>.cosᶻ(z)")]
    [TestCase("global::cosᶻ(z)", "global::cosᶻ(z)")]
    [TestCase("Probeᶻ(cosʸ(x))", "Probeᶻ(pow(cos(x),y))")]
    [TestCase("ψⁿ(sinˣ(x))²", "pow(ψⁿ(pow(sin(x),x)),2)")]
    [TestCase("ψⁿ(x)²", "pow(ψⁿ(x),2)")]
    [TestCase("Probeᶻ²(x)", "pow(Probeᶻ(x),2)")]
    [TestCase("@cosᶻ²(z)", "pow(@cosᶻ(z),2)")]
    [TestCase("Helpers.cosᶻ²(z)", "pow(Helpers.cosᶻ(z),2)")]
    [TestCase("@cosᶻ⁽ˣ⁾(z)", "pow(@cosᶻ(z),(x))")]
    [TestCase("record cosᶻ(double value);", "record cosᶻ(double value);")]
    [TestCase("class cosᶻ { public cosᶻ(double value) {} }", "class cosᶻ { public cosᶻ(double value) {} }")]
    [TestCase("new cosᶻ(3).value", "new cosᶻ(3).value")]
    [TestCase("new cosᶻ(3).value + cosᶻ(z)", "new cosᶻ(3).value + pow(cos(z),z)")]
    [TestCase("record Derived() : cosᶻ(3);", "record Derived() : cosᶻ(3);")]
    [TestCase("[cosᶻ(3)] real Probe() => 4;", "[cosᶻ(3)] real Probe() => 4;")]
    public void ExistingCallableNamesArePreserved(string source, string expected) =>
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source), Is.EqualTo(expected));

    [Test]
    public void DeeplyNestedPreservedCallsDoNotHideAnInnerPower()
    {
        var opening = string.Concat(Enumerable.Repeat("Probeᶻ(", 128));
        var closing = new string(')', 128);
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(opening + "cosʸ(x)" + closing),
            Is.EqualTo(opening + "pow(cos(x),y)" + closing));
    }

    [TestCase(0.4, 1.5)]
    [TestCase(-0.7, -2.25)]
    public void RealSymbolicPowersEvaluateWithTheRequestedExponent(double x, double y)
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("cosʸ(x)", x, y),
            Is.EqualTo(Math.Pow(Math.Cos(x), y)).Within(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("cos⁻ʸ(x)", x, y),
            Is.EqualTo(Math.Pow(Math.Cos(x), -y)).Within(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("cos⁽ʸ⁺¹⁾(x)", x, y),
            Is.EqualTo(Math.Pow(Math.Cos(x), y + 1)).Within(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("System.Math.Cos⁽ʸ⁾(x)", x, y),
            Is.EqualTo(Math.Pow(Math.Cos(x), y)).Within(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("2sqrtʸ(x²)x", x, y),
            Is.EqualTo(2 * Math.Pow(Math.Abs(x), y) * x).Within(1e-12));
    }

    [TestCase(0.7, 1.1)]
    [TestCase(2.2, -0.4)]
    [TestCase(0, 0)]
    public void ReportedComplexPowersExecuteThroughTheSharedCompiler(double real, double imaginary)
    {
        var z = new Complex(real, imaginary);
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That((engine.EvaluateComplex("cosᶻ(z)", z) - Complex.Pow(Complex.Cos(z), z)).Magnitude,
            Is.LessThanOrEqualTo(1e-12));
        NUnit.Framework.Assert.That((engine.EvaluateComplex("cos⁻ᶻ(z)", z) - Complex.Pow(Complex.Cos(z), -z)).Magnitude,
            Is.LessThanOrEqualTo(1e-12));
        NUnit.Framework.Assert.That((engine.EvaluateComplex("cos⁽ᶻ⁺¹⁾(z)", z) - Complex.Pow(Complex.Cos(z), z + 1)).Magnitude,
            Is.LessThanOrEqualTo(1e-12));
    }

    [Test]
    public void NestedPrefixAndPostfixPowersEvaluateInOrder()
    {
        const double x = 0.4, y = 1.5;
        var expected = Math.Pow(Math.Pow(Math.Sin(Math.Pow(Math.Cos(x), x)), y), 2);
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("sinʸ(cosˣ(x))²", x, y),
            Is.EqualTo(expected).Within(1e-12));
    }

    [TestCase("Probeˣ(2)", 3)]
    [TestCase("Probeʸ(2)", 4)]
    [TestCase("Probeᶻ(2)", 5)]
    [TestCase("ψⁿ(2)", 6)]
    [TestCase("@cosᶻ(2)", 7)]
    [TestCase("Helpers.cosᶻ(2)", 8)]
    [TestCase("Helpers . cosᶻ(2)", 8)]
    [TestCase("Probeᶻ²(2)", 25)]
    [TestCase("@cosᶻ²(2)", 49)]
    [TestCase("Helpers.cosᶻ²(2)", 64)]
    [TestCase("ψⁿ⁽ʸ⁾(2)", 36)]
    [TestCase("Helpers.cosᶻ⁽ʸ⁾(2)", 64)]
    public void CustomAndQualifiedSuperscriptNamesStillExecute(string expression, double expected)
    {
        const string definitions = """
            static real Probeˣ(real x) => x + 1;
            static real Probeʸ(real x) => x + 2;
            static real Probeᶻ(real x) => x + 3;
            static real ψⁿ(real x) => x + 4;
            static real @cosᶻ(real x) => x + 5;
            static class Helpers { public static real cosᶻ(real x) { return x + 6; } }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, y: 2, customFunctions: definitions),
            Is.EqualTo(expected).Within(1e-12));
    }

    [Test]
    public void ACustomCallAndItsExponentExpressionExecuteOnce()
    {
        const string definitions = """
            static int calls;
            static real Next() { calls++; return 3; }
            static real Exponent() { calls++; return 2; }
            static int Count() => calls;
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Next⁽ᴱˣᵖᵒⁿᵉⁿᵗ⁽⁾⁾()); write(Count());", definitions),
            Is.EqualTo("92"));
    }

    [TestCase("real cosᶻ(real x) => x + 2; write(cosᶻ(3));")]
    [TestCase("Func<double,double> cosᶻ = x => x + 2; write(cosᶻ(3));")]
    [TestCase("real Invoke(Func<double,double> cosᶻ) => cosᶻ(3); write(Invoke(x => x + 2));")]
    public void NamesDeclaredInTheSameScriptTakePrecedenceOverShorthand(string script) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("5"));

    [Test]
    public void ConstructingASuperscriptNamedRecordDoesNotChangeTheFunctionPowerBesideIt()
    {
        const string definitions = "record cosᶻ(real value);";
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("new cosᶻ(3).value", customFunctions: definitions), Is.EqualTo(3));
        var z = new Complex(0.7, 1.1);
        var actual = engine.EvaluateComplex("new cosᶻ(3).value + cosᶻ(z)", z, definitions);
        NUnit.Framework.Assert.That((actual - (3 + Complex.Pow(Complex.Cos(z), z))).Magnitude, Is.LessThanOrEqualTo(1e-12));
    }

    [Test]
    public void AttributeConstructorNamesArePreservedInASeparateScriptFragment()
    {
        const string definitions = "class cosᶻ : Attribute { public cosᶻ(int value) {} }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("[cosᶻ(3)] real Probe() => 4; write(Probe());", definitions),
            Is.EqualTo("4"));
    }

    [TestCase("""write("cosᶻ(z), Probe⁽ʸ⁾(x), ψⁿ(x)");""", "cosᶻ(z), Probe⁽ʸ⁾(x), ψⁿ(x)")]
    [TestCase("write(\"\"\"cosᶻ(z), Probe⁽ʸ⁾(x)\"\"\");", "cosᶻ(z), Probe⁽ʸ⁾(x)")]
    [TestCase("""var y=2; write($"cosʸ(x) = {cosʸ(0)}");""", "cosʸ(x) = 1")]
    [TestCase("/* cosᶻ(z), ()² */ var y=2; write(cosʸ(0)); // cosᶻ(z)\n", "1")]
    public void LiteralAndCommentProtectionStillSurroundsThePowerHandler(string script, string expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));
}
