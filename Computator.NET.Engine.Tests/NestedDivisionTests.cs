using System.Numerics;
using Computator.NET.Core.Compilation.TslHandlers;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class NestedDivisionTests
{
    [TestCase("1/(1/(2))", 2)]
    [TestCase("6/(3/(2))", 4)]
    [TestCase("6/((1+2)/(1+1))", 4)]
    [TestCase("1/(1/(1/(1+1)))", 0.5)]
    [TestCase("1/((1/(2)))", 2)]
    [TestCase("1/-(1/-(2))", 2)]
    [TestCase("1/+(1/-(2))", -2)]
    [TestCase("1/ - - (1/ - - (2))", 2)]
    [TestCase("1/(1/(2)+1/(2))", 1)]
    [TestCase("1/(1/(2))+1/(1/(4))", 6)]
    [TestCase("1/(1/(2))/(1/(4))", 8)]
    [TestCase("1/(2/(3+1))", 2)]
    [TestCase("1/(1/(x+1))", 4)]
    [TestCase("1/(1/(2²))", 4)]
    [TestCase("1/(1/(1_000))", 1000)]
    [TestCase("1/(1/(2e3))", 2000)]
    [TestCase("1/(1/(0))", 0)]
    [TestCase("1/(1/-(0))", 0)]
    public void EveryGroupedDenominatorUsesMathematicalDivision(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 3),
            Is.EqualTo(expected).Within(1e-13));
    }

    [TestCase("var n=2; write(1/(1/(n++))); write(\":\"); write(n);", "2:3")]
    [TestCase("var n=3; write(1/(1/(--n))); write(\":\"); write(n);", "2:2")]
    [TestCase("var n=1; write(1/(1/(++n))); write(\":\"); write(n);", "2:2")]
    [TestCase("var n=2; write(1/(1/n));", "Infinity")]
    [TestCase("var n=3; write(1/(1/--(n))); write(\":\"); write(n);", "Infinity:2")]
    [TestCase("var n=1; write(1/(1/++(n))); write(\":\"); write(n);", "Infinity:2")]
    public void GroupingPreservesSideEffectsAndBareIntegerDivisorSemantics(string script, string expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));
    }

    [Test]
    public void CustomFunctionsAndBothOperandsExecuteOnceInTheirOriginalOrder()
    {
        const string functions = """
            static string Calls="";
            static int Next(int n) { Calls += n; return n; }
            static real Probe() { return Next(6)/(Next(3)/(Next(2))); }
            """;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.ExecuteScript("write(Probe()); write(\":\"); write(Calls);", functions),
            Is.EqualTo("4:632"));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probe()", customFunctions: functions), Is.EqualTo(4));
    }

    [Test]
    public void ComplexEvaluationPromotesNestedRealDenominators()
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateComplex("z/(1/(2))", new Complex(1, 2)),
            Is.EqualTo(new Complex(2, 4)));
    }

    [Test]
    public void CommentsAndLiteralTextRemainSeparateFromDivisionSyntax()
    {
        const string script = """
            write("1/(1/(2)) /* unmatched ( */");
            write(":");
            // This unmatched ) must not affect the executable parentheses.
            write(1 / /* ) */ (1 / /* ( */ (2)));
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script),
            Is.EqualTo("1/(1/(2)) /* unmatched ( */:2"));
    }

    [TestCase("1/()")]
    [TestCase("1/((2)")]
    [TestCase(")1/(2")]
    public void IncompleteOrEmptyDenominatorsAreNotWrapped(string source)
    {
        NUnit.Framework.Assert.That(new DivisionHandler().Replace(source), Is.EqualTo(source));
    }

    [Test]
    public void DeeplyNestedDenominatorsDoNotRequireRecursiveRewriting()
    {
        // Below the public 65,536-character input limit. This deliberately
        // exercises translation alone, not the C# compiler's nesting limit.
        const int depth = 12000;
        var source = string.Concat(Enumerable.Repeat("1/(", depth)) + "2" + new string(')', depth);
        var expected = string.Concat(Enumerable.Repeat("1/(1.0*((", depth)) + "2" + new string(')', depth * 3);
        NUnit.Framework.Assert.That(new DivisionHandler().Replace(source), Is.EqualTo(expected));
    }
}
