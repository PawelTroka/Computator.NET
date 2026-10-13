using System.Globalization;
using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.DataTypes.Text;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableEngineTests
{
    [TestCase("2x+3", 7, 0, 17)]
    [TestCase("6/3", 0, 0, 2)]
    [TestCase("x²+y²", 3, 4, 25)]
    [TestCase("2·cos(0)", 0, 0, 2)]
    [TestCase("sqrt(9)+|x|", -2, 0, 5)]
    [TestCase("sin(PI/2)", 0, 0, 1)]
    public void ExistingNotationExecutesWithoutWindows(string expression, double x, double y, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, x, y), Is.EqualTo(expected).Within(1e-12));

    [Test]
    public void ComplexArithmeticAndCustomDefinitionsWork()
    {
        var engine = new PortableEngine();
        var complex = engine.EvaluateComplex("z²", new Complex(1, 1));
        // NUnit does not apply a double tolerance to Complex objects. Compare the
        // numerical error norm, retaining the original absolute tolerance.
        NUnit.Framework.Assert.That((complex - new Complex(0, 2)).Magnitude, Is.LessThanOrEqualTo(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probe(2)", customFunctions: "static real Probe(real x) { return x/2; }"), Is.EqualTo(1));
    }

    [Test]
    public void ScriptUsesTheSameTslTransformerAndDefinitions()
    {
        var result = new PortableEngine().ExecuteScript("var f(real x)=x²; write(f(3)); write(Probe(1));", "static real Probe(real x) { return x+1; }");
        NUnit.Framework.Assert.That(result, Is.EqualTo("92"));
    }

    [Test]
    public void InvalidCodeReportsTheUserDocumentAndRecovers()
    {
        var engine = new PortableEngine();
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() => engine.EvaluateReal("missing(1)"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.File == "expression.tsl" && diagnostic.Line == 1), Is.True);
        error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() => engine.EvaluateReal("Probe(1)", customFunctions: "static real Probe(real x) { return missing; }"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.File == "custom-functions.tslf"), Is.True);
        NUnit.Framework.Assert.That(engine.EvaluateReal("2+2"), Is.EqualTo(4));
    }

    [Test]
    public void RequestsNeverReusePreviousGeneratedDefinitions()
    {
        var engine = new PortableEngine();
        Parallel.For(0, 12, number => NUnit.Framework.Assert.That(engine.EvaluateReal("Probe(0)",
            customFunctions: "static real Probe(real x) { return " + number + "; }"), Is.EqualTo(number)));
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => engine.EvaluateReal("Probe(0)"));
    }

    [Test]
    public void ExtractedLanguageTypesComeFromThePortableAssembly()
    {
        NUnit.Framework.Assert.That(typeof(TslCompiler).Assembly, Is.SameAs(typeof(PortableEngine).Assembly));
        NUnit.Framework.Assert.That(typeof(SpecialSymbols).Assembly, Is.SameAs(typeof(PortableEngine).Assembly));
        var references = typeof(PortableEngine).Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name ?? "");
        NUnit.Framework.Assert.That(references.Any(name => name.Contains("Windows.Forms") || name.Contains("PresentationFramework") || name.Contains("DataVisualization") || name == "Computator.NET.DataTypes"), Is.False);
        NUnit.Framework.Assert.That(Environment.Version.Major, Is.EqualTo(10));
    }

    [TestCase("pl-PL")]
    [TestCase("de-DE")]
    [NonParallelizable]
    public void ExecutionDoesNotDependOnUiCulture(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture); NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("1.5/2"), Is.EqualTo(0.75)); }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Test]
    public void SourceAndOutputLimitsAreEnforced()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.Throws<ArgumentException>(() => engine.EvaluateReal(new string('1', 65537)));
        NUnit.Framework.Assert.Throws<InvalidOperationException>(() => engine.ExecuteScript("write(new string('a',65537));"));
    }
}
