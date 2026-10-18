using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class SymbolicFunctionPowerContextTests
{
    [TestCase("cosʸ(x)", 5)]
    [TestCase("cosʸ(x)²", 25)]
    [TestCase("cosʸ⁽ʸ⁾(x)", 25)]
    [TestCase("@cosʸ(x)", 5)]
    public void SeparateCustomFunctionDeclarationsTakePrecedence(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, x: 3, y: 2,
            customFunctions: "static real cosʸ(real x) => x+2;"), Is.EqualTo(expected));

    [Test]
    public void SeparateComplexCustomFunctionRetainsItsMeaning()
    {
        var z = new Complex(0.7, -0.4);
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateComplex("cosᶻ(z)", z,
            "static complex cosᶻ(complex z) => z+2;"), Is.EqualTo(z + 2));
    }

    [Test]
    public void ScriptsSeeNamesFromTheCustomFunctionsEditor() =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(cosʸ(3));",
            "static real cosʸ(real x) => x+2;"), Is.EqualTo("5"));

    [Test]
    public void GroupingCanExplicitlyRequestTheBuiltInFunctionPower()
    {
        const double x = 0.4;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("cos⁽ʸ⁾(x)", x: x, y: 2,
            customFunctions: "static real cosʸ(real x) => x+2;"),
            Is.EqualTo(Math.Pow(Math.Cos(x), 2)).Within(1e-12));
    }

    [TestCase("static real cosʸ() => 7;")]
    [TestCase("static real cosʸ = 7;")]
    public void AnInvalidCallToAnExistingNameDoesNotSilentlyCallCosine(string definitions) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateReal("cosʸ(x)", x: 3, y: 2, customFunctions: definitions));

    [TestCase("static real Helper() { real cosʸ(real x) => x+2; return cosʸ(3); }")]
    [TestCase("static real Helper(real cosʸ) => cosʸ;")]
    [TestCase("class Holder { public static real cosʸ(real x) => x+2; }")]
    [TestCase("record cosʸ(real value);")]
    [TestCase("static string text = \"static real cosʸ(real x) => x+2;\";")]
    [TestCase("/* static real cosʸ(real x) => x+2; */")]
    public void NamesOutsideTheSharedMemberScopeDoNotDisableShorthand(string definitions) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("cosʸ(x)", x: 0.4, y: 2,
            customFunctions: definitions), Is.EqualTo(Math.Pow(Math.Cos(0.4), 2)).Within(1e-12));

    [Test]
    public void ADeclaredDelegateRemainsCallableAcrossFragments() =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(cosʸ(3));",
            "static Func<double,double> cosʸ = x => x+2;"), Is.EqualTo("5"));

    [Test]
    public void CustomNamesDoNotLeakToTheNextEvaluation()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("cosʸ(x)", x: 3, y: 2,
            customFunctions: "static real cosʸ(real x) => x+2;"), Is.EqualTo(5));
        NUnit.Framework.Assert.That(engine.EvaluateReal("cosʸ(x)", x: 0.4, y: 2),
            Is.EqualTo(Math.Pow(Math.Cos(0.4), 2)).Within(1e-12));
    }
}
