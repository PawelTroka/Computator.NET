using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class TslTutorialExamplesTests
{
    [Test]
    public void FirstCalculationExamplesHaveTheDocumentedValues()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("2x+3", 7), Is.EqualTo(17));
        NUnit.Framework.Assert.That(engine.EvaluateReal("sin(PI/2)+x²", 3), Is.EqualTo(10).Within(1e-12));

        var value = engine.EvaluateComplex("z²", new Complex(1, 1));
        NUnit.Framework.Assert.That((value - new Complex(0, 2)).Magnitude, Is.LessThan(1e-12));
    }

    [Test]
    public void ScriptAndLocalFunctionExamplesStayExecutable()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.ExecuteScript(
            "real width = 3; real height = 4; var diagonalSquared = width*width + height*height; write(diagonalSquared);"),
            Is.EqualTo("25"));
        NUnit.Framework.Assert.That(engine.ExecuteScript("var f(real x) = x*x+1; write(f(3));"),
            Is.EqualTo("10"));
    }

    [Test]
    public void ImportedCustomFunctionIsUsableByExpressionsAndScripts()
    {
        const string definitions = "static real OffsetSquare(real x) { return x*x+1; }";
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("OffsetSquare(3)", customFunctions: definitions),
            Is.EqualTo(10));
        NUnit.Framework.Assert.That(engine.ExecuteScript("write(OffsetSquare(3));", definitions),
            Is.EqualTo("10"));
    }

    [Test]
    public void MatrixAndNumericalExamplesStayExecutable()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.ExecuteScript("var a = matrix({{1,2},{3,4}}); write(det(a));"),
            Is.EqualTo("-2"));
        NUnit.Framework.Assert.That(engine.Integrate("x*x", 0, 3, steps: 100), Is.EqualTo(9).Within(1e-10));
        NUnit.Framework.Assert.That(engine.Differentiate("x*x", 3), Is.EqualTo(6).Within(1e-5));
        NUnit.Framework.Assert.That(engine.FindRoot("x*x-2", 0, 2), Is.EqualTo(Math.Sqrt(2)).Within(1e-6));
        NUnit.Framework.Assert.That(engine.FindRoot("x*x-9", 0, 5), Is.EqualTo(3).Within(1e-6));
    }
}
