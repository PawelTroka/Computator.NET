using System.Globalization;
using System.Text.Json;
using Computator.NET.PortableWeb;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableCatalogTests
{
    private static JsonElement[] Items(bool scripting) =>
        JsonSerializer.SerializeToElement(PortableCatalog.Items(scripting)).EnumerateArray().ToArray();

    private static string Text(JsonElement item) => item.GetProperty("text").GetString()!;
    private static string Caption(JsonElement item) => item.GetProperty("menuText").GetString()!;

    [TestCase("Euclid.GreatestCommonDivisor", "(12,18)", 6)]
    [TestCase("Polynomial.Evaluate", "(2,1,-3,2)", 3)]
    [TestCase("Polynomial.Zero", ".Evaluate(2)", 0)]
    [TestCase("Polynomial.Add", "(new Polynomial(1,2),new Polynomial(3,-1)).Evaluate(2)", 6)]
    [TestCase("DifferentialEquation.rungeKutta4", "((t,u) => 2*t,3.0,0.0,2.0,21)[20]", 7)]
    [TestCase("Integral.simpsonMethod", "(t => t*t,0,1,100)", 1.0 / 3)]
    [TestCase("Derivative.derivative", "(t => t*t,3)", 6)]
    [TestCase("FunctionRoot.BrentMethod", "(t => t*t-2,0,2)", 1.4142135623730951)]
    public void BothCataloguesExposeCallableScientificNames(string name, string arguments, double expected)
    {
        foreach (var scripting in new[] { false, true })
            NUnit.Framework.Assert.That(Items(scripting).Select(Text), Does.Contain(name),
                scripting ? "scripting catalogue" : "expression catalogue");

        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal(name + arguments), Is.EqualTo(expected).Within(1e-8));
        var output = engine.ExecuteScript("write(" + name + arguments + ");");
        NUnit.Framework.Assert.That(double.Parse(output, CultureInfo.InvariantCulture), Is.EqualTo(expected).Within(1e-8));
    }

    [Test]
    public void PolynomialConstructorCompletesTheTypeAfterNew()
    {
        var constructors = Items(true).Where(entry => Text(entry) == "Polynomial").ToArray();
        NUnit.Framework.Assert.That(constructors.Select(Caption), Does.Contain("new Polynomial(int n)")
            .And.Contain("new Polynomial(real coefficient)"));
        var item = constructors.Single(entry => Caption(entry).Contains("params"));
        NUnit.Framework.Assert.That(Caption(item), Is.EqualTo("new Polynomial(params real[] coefficients)"));
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("new " + Text(item) + "(1,-3,2).Evaluate(2)"), Is.EqualTo(3));
        NUnit.Framework.Assert.That(engine.EvaluateReal("new " + Text(item) + "(3).Evaluate(2)"), Is.Zero);
        NUnit.Framework.Assert.That(engine.EvaluateReal("new " + Text(item) + "(3.0).Evaluate(2)"), Is.EqualTo(3));
    }

    [Test]
    public void OverloadCaptionsPreserveActualIntegerWidthsAndReferenceArguments()
    {
        var items = Items(true);
        var powers = items.Where(item => Text(item) == "Euclid.PowerOfTwo").Select(Caption).ToArray();
        NUnit.Framework.Assert.That(powers, Has.Length.EqualTo(2));
        NUnit.Framework.Assert.That(powers, Does.Contain("int Euclid.PowerOfTwo(int exponent)"));
        NUnit.Framework.Assert.That(powers, Does.Contain("integer Euclid.PowerOfTwo(integer exponent)"));
        var extended = items.Where(item => Text(item) == "Euclid.ExtendedGreatestCommonDivisor").Select(Caption);
        NUnit.Framework.Assert.That(extended.Any(caption => caption.Contains("out integer x, out integer y")), Is.True);
        var division = items.Single(item => Text(item) == "Polynomial.DivideRemainder");
        NUnit.Framework.Assert.That(Caption(division), Is.EqualTo("(Polynomial, Polynomial) Polynomial.DivideRemainder(Polynomial a, Polynomial b)"));
    }

    [TestCase("pl-PL")]
    [TestCase("de-DE")]
    [NonParallelizable]
    public void OptionalArgumentCaptionsRemainInvariantAndDescribeDefaults(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var items = Items(false);
            NUnit.Framework.Assert.That(Caption(items.Single(item => Text(item) == "FunctionRoot.BrentMethod")),
                Does.Contain("real errorMax = 1E-08, uint N = 10000"));
            NUnit.Framework.Assert.That(items.Where(item => Text(item) == "DifferentialEquation.rungeKutta4").Select(Caption),
                Has.All.Contains("int samples = 101"));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Test]
    public void ExistingFunctionsRemainUnqualifiedAndWriteRemainsScriptOnly()
    {
        var expressions = Items(false).Select(Text).ToArray();
        var scripts = Items(true).Select(Text).ToArray();
        NUnit.Framework.Assert.That(expressions, Does.Contain("sin").And.Contain("matrix").And.Contain("PI"));
        NUnit.Framework.Assert.That(expressions, Does.Not.Contain("write"));
        NUnit.Framework.Assert.That(scripts, Does.Contain("write"));
        NUnit.Framework.Assert.That(expressions.Any(name => name.Contains(".op_") || name.Contains(".get_") || name.Contains("ToCode")), Is.False);
    }

    [Test]
    public void GenericSignatureHelpIsEncodedAsTextForTheBrowser()
    {
        var item = Items(false).Single(entry => Text(entry) == "Integral.simpsonMethod");
        NUnit.Framework.Assert.That(Caption(item), Does.Contain("Func<real, real>"));
        var description = item.GetProperty("details").GetProperty("description").GetString();
        NUnit.Framework.Assert.That(description, Does.Contain("Func&lt;real, real&gt;").And.Not.Contains("<real"));
    }
}
