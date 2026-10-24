using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class AccessPowerTests
{
    [TestCase("System.Math.PI²", 9.869604401089358)]
    [TestCase("System.Math.E⁻¹", 0.36787944117144233)]
    [TestCase("global::System.Math.PI²", 9.869604401089358)]
    [TestCase("new complex(3,4).Magnitude²", 25)]
    [TestCase("new complex(3,4).Real²+new complex(3,4).Imaginary²", 25)]
    [TestCase("-new complex(3,4).Real²", -9)]
    [TestCase("(new complex(3,4).Real²+1)²", 100)]
    [TestCase("new complex(3,4).Real⁽ʸ⁺¹⁾", 81)]
    [TestCase("new complex(3,4).Real²x", 18)]
    public void CompleteMemberOperandsAreRaisedToThePower(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 2, 3),
            Is.EqualTo(expected).Within(1e-12));

    [TestCase("z.Real²+z.Imaginary²", 25, 0)]
    [TestCase("z.Realʸ", 27, 0)]
    [TestCase("z.Real²+z.Imaginary²+z", 28, 4)]
    [TestCase("2z.Real²", 18, 0)]
    public void ComplexInputComponentsKeepTheirReceivers(string expression, double real, double imaginary)
    {
        var result = new PortableEngine().EvaluateComplex(expression, new Complex(3, 4),
            "public static real y = 3;");
        NUnit.Framework.Assert.That((result - new Complex(real, imaginary)).Magnitude, Is.LessThan(1e-12));
    }

    [TestCase("var values=new[]{3.0,4.0}; write(values[0]²+values[1]²);", "25")]
    [TestCase("var values=new[]{3.0,4.0}; write(-values[0]²);", "-9")]
    [TestCase("var values=new[]{3.0,4.0}; write((values[0]²+values[1]²)²);", "625")]
    [TestCase("var values=new[]{3.0,4.0}; write(values[0]⁻²);", "0.1111111111111111")]
    [TestCase("var values=new[]{3.0,4.0}; var y=3.0; write(values[0]ʸ);", "27")]
    [TestCase("var values=new[]{3.0,4.0}; write(values[0]²·values[1]²);", "144")]
    [TestCase("var values=new[]{3.0,4.0}; write(2values[0]²);", "18")]
    [TestCase("var values=new[]{3.0,4.0}; write(values[0]²/2);", "4.5")]
    [TestCase("var values=new[]{3.0,4.0}; write(values[0]²x);", "18")]
    [TestCase("var values=new[]{3.0,4.0}; write(values[^1]²);", "16")]
    [TestCase("var values=new[]{3.0,4.0}; write(values[(int)((x-1)²)]²);", "16")]
    [TestCase("var values=new[]{new[]{3.0,4.0}}; write(values[0][1]²);", "16")]
    [TestCase("var grid=matrix({{3.0,4.0}}); write(grid[0,0]²+grid[0,1]²);", "25")]
    [TestCase("write(new[]{3.0,4.0}[0]²);", "9")]
    [TestCase("write(new complex(3,4).Magnitude²);", "25")]
    [TestCase("var points=new[]{new complex(3,4)}; write(points[0].Real²);", "9")]
    public void ArrayAndMatrixEntriesExecuteInScripts(string source, string expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source, "public static real x = 2;"),
            Is.EqualTo(expected));

    [Test]
    public void IndexedComplexValuesUseTheComplexPowerOverload()
    {
        var result = new PortableEngine().EvaluateComplex("Values[0]²", Complex.Zero,
            "public static complex[] Values = { new complex(1,1) };");
        NUnit.Framework.Assert.That((result - new Complex(0, 2)).Magnitude, Is.LessThan(1e-12));
    }

    [Test]
    public void ReceiverIndexGetterAndExponentAreEvaluatedOnceInOrder()
    {
        const string definitions = """
            public static string Trace = "";
            public class Sample {
                public real this[int index] { get { Trace += "get"; return 3; } }
                public real Value { get { Trace += "value"; return 4; } }
                public real Read() { Trace += "method"; return 5; }
            }
            public static Sample Next() { Trace += "receiver"; return new Sample(); }
            public static int Index() { Trace += "index"; return 0; }
            public static real Exponent() { Trace += "power"; return 2; }
            """;
        const string script = """
            write(Next()[Index()]ᴱˣᵖᵒⁿᵉⁿᵗ⁽⁾); write(Trace); Trace = "";
            write(Next().Valueᴱˣᵖᵒⁿᵉⁿᵗ⁽⁾); write(Trace); Trace = "";
            write(Next().Read()²); write(Trace);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions),
            Is.EqualTo("9receiverindexgetpower16receivervaluepower25receivermethod"));
    }

    [Test]
    public void PowersInsideAnAccessChainAreLoweredBeforeTheOuterPower()
    {
        const string definitions = """
            public static real[] Values = { 3 };
            public static real[] Make(real value) { return new[]{value}; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Make(Values[0]²)[0]²",
            customFunctions: definitions), Is.EqualTo(81));
    }

    [Test]
    public void AnIndexedDelegateIsInvokedOnlyOnce()
    {
        const string definitions = """
            public static int Calls;
            public static Func<real>[] Functions = { () => { Calls++; return 3; } };
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Functions[0]()²); write(Calls);",
            definitions), Is.EqualTo("91"));
    }

    [TestCase("Values[missing]²")]
    [TestCase("Values[0].Missing²")]
    [TestCase("Values[0²")]
    public void InvalidAccessExpressionsStillReportCompilationErrors(string expression)
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateReal(expression,
                customFunctions: "public static real[] Values = { 3 };"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic =>
            diagnostic.File == "expression.tsl" && diagnostic.Line == 1), Is.True);
    }

    [Test]
    public void CustomDefinitionsUseTheSameOperandBoundaries()
    {
        const string definitions = """
            public static real Probe(real[] data) { return data[0]²+data[1]²; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Probe(new[]{3.0,4.0})",
            customFunctions: definitions), Is.EqualTo(25));
    }

    [TestCase("2.5²", 6.25)]
    [TestCase("-3²", -9)]
    [TestCase("(-3)²", 9)]
    [TestCase("(2²)³", 64)]
    [TestCase("System.Math.Cos(x)²", 1)]
    [TestCase("cos²(x)", 1)]
    [TestCase("2cos(x)²", 2)]
    public void ExistingNumericAndFunctionPowersRetainTheirMeaning(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression),
            Is.EqualTo(expected).Within(1e-12));

    [Test]
    public void LiteralMemberAndIndexNotationIsNotExecutable()
    {
        const string source = """write("z.Real² values[0]²"); // z.Real² values[0]²""";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo("z.Real² values[0]²"));
    }
}
