using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class LogicalOrTests
{
    [TestCase("true || false || false")]
    [TestCase("false || true || false")]
    [TestCase("false || false || true")]
    [TestCase("false || false || false")]
    [TestCase("(true || false) || false")]
    [TestCase("true || (false || false)")]
    [TestCase("first || second || third || fourth")]
    [TestCase("1 | 2 | 4")]
    public void OrdinaryOperatorsAreNotAbsoluteValueDelimiters(string expression)
    {
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(expression), Is.EqualTo(expression));
    }

    [TestCase("true || false || false", 1.0)]
    [TestCase("false || true || false", 1.0)]
    [TestCase("false || false || true", 1.0)]
    [TestCase("false || false || false", 0.0)]
    [TestCase("false && true || false || true", 1.0)]
    [TestCase("true || false && false || false", 1.0)]
    [TestCase("(x > 0 ? true : false) || y == 3 || false", 1.0)]
    [TestCase("false || x is > 0 || y is 3", 1.0)]
    [TestCase("x == -2 || y == 0 || x+y == 0", 1.0)]
    [TestCase("|x| > 0 || |y| > 0 || |x+y| > 0", 1.0)]
    [TestCase("|x| > 0 || false || false", 1.0)]
    [TestCase("|x| > 0 || false || |y| > 0", 1.0)]
    [TestCase("false || |x| > 0 || false", 1.0)]
    [TestCase("false || false || |x| > 0", 1.0)]
    [TestCase("x² == 4 || y² == 9 || x > 0", 1.0)]
    [TestCase("(x+1)² == 1 || y² == 9 || x > 0", 1.0)]
    [TestCase("x·x == 4 || y·y == 9 || x > 0", 1.0)]
    [TestCase("2x == -4 || 3y == 0 || x > 0", 1.0)]
    public void BooleanChainsExecuteWithMathematicalNotation(string condition, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal($"({condition}) ? 1.0 : 0.0", -2, 3),
            Is.EqualTo(expected));
    }

    [TestCase("||x||", 2.0)]
    [TestCase("|x||y|", 6.0)]
    [TestCase("||-3||", 3.0)]
    [TestCase("2||x||", 4.0)]
    [TestCase("2||x||+3", 7.0)]
    [TestCase("|(x > 0 || y > 0 || x+y > 0) ? -3 : 2|", 3.0)]
    [TestCase("|x|²", 4.0)]
    [TestCase("xꞋˣꞋ", 4.0)]
    public void AbsoluteValueNotationIsPreserved(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, -2, 3), Is.EqualTo(expected));
    }

    [TestCase("+z", 11.0)]
    [TestCase("-z", 1.0)]
    [TestCase("+x", 8.0)]
    [TestCase("-x", 4.0)]
    [TestCase("+(z+1)", 12.0)]
    [TestCase("-abs(z)", 1.0)]
    [TestCase("*z", 30.0)]
    [TestCase("/z", 1.2)]
    [TestCase("%z", 1.0)]
    [TestCase("+z+1", 12.0)]
    [TestCase("*z+1", 31.0)]
    [TestCase("+z-1", 10.0)]
    [TestCase("+z*2", 16.0)]
    [TestCase("+z/2.0", 8.5)]
    [TestCase("+z%2", 7.0)]
    public void VariableCoefficientsAndArithmeticTermsPreserveNestedAbsoluteValues(string suffix, double expected)
    {
        var expression = "x||y||" + suffix;
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(expression),
            Is.EqualTo("x*(abs((abs(y))))" + suffix));
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 2, -3,
            customFunctions: "public static real z = 5;"), Is.EqualTo(expected));
    }

    [TestCase("x||y||·z", 30.0)]
    [TestCase("x||y||ᶻ", 486.0)]
    public void MathematicalOperatorsAfterNestedAbsoluteValuesArePreserved(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 2, -3,
            customFunctions: "public static real z = 5;"), Is.EqualTo(expected));
    }

    [TestCase("abs(x||y||+5) == 11 || false || false", 1.0)]
    [TestCase("false || abs(x||y||+5) == 11 || false", 1.0)]
    [TestCase("false || false || abs(x||y||+5) == 11", 1.0)]
    [TestCase("(x||y||+5) == 11 || false || false", 1.0)]
    [TestCase("false || (x||y||+5) == 11 || false", 1.0)]
    [TestCase("false || false || (x||y||+5) == 11", 1.0)]
    [TestCase("(x||y||*5) == 30 || false || false", 1.0)]
    [TestCase("false || false || abs(x||y||+5) == 0", 0.0)]
    [TestCase("abs(||x||) == 2 || false || false", 1.0)]
    [TestCase("false || abs(||x||) == 2 || false", 1.0)]
    [TestCase("false || false || abs(||x||) == 2", 1.0)]
    [TestCase("(||x||) == 2 || false || false", 1.0)]
    [TestCase("(x||y||) == 6 || false || false", 1.0)]
    [TestCase("false || abs(x||y||+5) == 11 && false || false", 0.0)]
    public void NestedAbsoluteOperandsDoNotChangeEnclosingBooleanChains(string condition, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal($"({condition}) ? 1.0 : 0.0", 2, -3),
            Is.EqualTo(expected));
    }

    [Test]
    public void NestedArithmeticOperandsRetainShortCircuitEvaluationInScriptsAndCustomFunctions()
    {
        const string functions = """
            public static int Calls;
            public static real x = 2, y = -3;
            public static bool Probe(real value) { Calls++; return value == 11; }
            public static bool Chain()
            {
                return Probe(abs(x||y||+5)) || Probe(0) || Probe(0);
            }
            """;
        const string script = """
            write(Probe(abs(x||y||+5)) || Probe(0) || Probe(0)); write(Calls); Calls=0;
            write(Probe(0) || Probe(abs(x||y||+5)) || Probe(0)); write(Calls); Calls=0;
            write(Probe(0) || Probe(0) || Probe(abs(x||y||+5))); write(Calls); Calls=0;
            write(Probe(0) || Probe(0) || Probe(abs(x||y||+4))); write(Calls); Calls=0;
            write(Chain()); write(Calls);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, functions),
            Is.EqualTo("True1True2True3False3True1"));
    }

    [Test]
    public void BooleanVariablesAndSignedComparisonsFormLogicalChains()
    {
        const string script = """
            var first = false;
            var second = false;
            var z = 2.0;
            write(first || second || +z > 1);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("True"));
    }

    [Test]
    public void ScriptsAndCustomFunctionsRetainShortCircuitEvaluation()
    {
        const string functions = """
            public static int Calls;
            public static bool Probe(bool result) { Calls++; return result; }
            public static bool Chain(bool first, bool second, bool third)
            {
                return Probe(first) || Probe(second) || Probe(third);
            }
            """;
        const string script = """
            write(Chain(true, false, false)); write(Calls); Calls=0;
            write(Chain(false, true, false)); write(Calls); Calls=0;
            write(Chain(false, false, true)); write(Calls); Calls=0;
            write(Chain(false, false, false)); write(Calls); Calls=0;
            write(Probe(false) || Probe(true) || Probe(false)); write(Calls);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, functions),
            Is.EqualTo("True1True2True3False3True2"));
    }

    [Test]
    public void LongBooleanChainsWithinTheSourceLimitArePreserved()
    {
        var expression = string.Join(" || ", Enumerable.Repeat("false", 6000));
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(expression), Is.EqualTo(expression));
    }

    [Test]
    public void LogicalOperatorsInLiteralTextAndCommentsArePreserved()
    {
        const string script = """
            // || is syntax only in the executable condition below: |x||y|
            write("true || false || false; |x||y|; ||x||");
            write(true || false /* |x||y| */ || false);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script),
            Is.EqualTo("true || false || false; |x||y|; ||x||True"));
    }
}
