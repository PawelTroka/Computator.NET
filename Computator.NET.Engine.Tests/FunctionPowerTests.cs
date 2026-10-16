using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.TslHandlers;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class FunctionPowerTests
{
    [TestCase("cos²(x)", "pow(cos(x),2)")]
    [TestCase("cos(x)²", "pow(cos(x),2)")]
    [TestCase("cos⁻¹(x)", "pow(cos(x),-1)")]
    [TestCase("sqrt²ᱸ⁵(x)", "pow(sqrt(x),2.5)")]
    [TestCase("System.Math.Cos(x)²", "pow(System.Math.Cos(x),2)")]
    [TestCase("System.Math.Cos²(x)", "pow(System.Math.Cos(x),2)")]
    [TestCase("Probe()²", "pow(Probe(),2)")]
    [TestCase("Probe²()", "pow(Probe(),2)")]
    [TestCase("Probe(1,cos(x))²", "pow(Probe(1,cos(x)),2)")]
    [TestCase("Probe²(1,cos(x))", "pow(Probe(1,cos(x)),2)")]
    [TestCase("cos(sin²(x))²", "pow(cos(pow(sin(x),2)),2)")]
    [TestCase("cos²(x)²", "pow(pow(cos(x),2),2)")]
    [TestCase("(cos(x)²+1)²", "pow((pow(cos(x),2)+1),2)")]
    [TestCase("cos(x)ʸ", "pow(cos(x),y)")]
    [TestCase("cos (x)²", "pow(cos (x),2)")]
    [TestCase("return (x)²;", "return pow((x),2);")]
    [TestCase("return(cos(x)²)²;", "return pow((pow(cos(x),2)),2);")]
    [TestCase("ψⁿ(x);", "ψⁿ(x);")]
    [TestCase("ψⁿ(x)²", "pow(ψⁿ(x),2)")]
    [TestCase("x²+y²", "pow(x,2)+pow(y,2)")]
    [TestCase("(cos(x))²", "pow((cos(x)),2)")]
    public void PowerHandlerPreservesCallBoundaries(string source, string expected) =>
        NUnit.Framework.Assert.That(new PowHandler().Replace(source), Is.EqualTo(expected));

    [TestCase("cos²(0)+sin²(0)", 1)]
    [TestCase("cos(0)²+sin(0)²", 1)]
    [TestCase("2cos²(0)", 2)]
    [TestCase("cos²(0)x", 3)]
    [TestCase("System.Math.Cos²(0)", 1)]
    [TestCase("sqrt⁻²(4)", 0.25)]
    [TestCase("sqrt(4)ʸ", 8)]
    [TestCase("(cos(sin²(0))²+1)²", 4)]
    public void FunctionPowersExecuteInTheSharedEngine(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 3, 3), Is.EqualTo(expected).Within(1e-12));

    [Test]
    public void CustomFunctionArgumentsAreEvaluatedOnlyOnce()
    {
        const string definitions = "static int calls; static real Next() { calls++; return 3; } static real Count() { return calls; }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Next²()); write(Count());", definitions), Is.EqualTo("91"));
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Next()²); write(Count());", definitions), Is.EqualTo("91"));
    }

    [TestCase("return (cos(x)²+1)²;")]
    [TestCase("return(cos(x)²+1)²;")]
    public void AParenthesizedReturnValueRemainsAStatement(string statement)
    {
        var definitions = "static real Probe(real x) { " + statement + " }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Probe(0)", customFunctions: definitions), Is.EqualTo(4).Within(1e-12));
    }

    [Test]
    public void LiteralsAreNotRewrittenAsFunctionPowers()
    {
        const string source = "write(\"cos²(x), cos(x)² and ψⁿ(x)\");";
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source), Is.EqualTo(source));
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo("cos²(x), cos(x)² and ψⁿ(x)"));
    }
}
