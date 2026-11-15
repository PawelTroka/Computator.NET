using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ComplexCoordinateAliasesTests
{
    [TestCase("x", 3, 4, 3, 0)]
    [TestCase("y", 3, 4, 4, 0)]
    [TestCase("x+y*i", 3, 4, 3, 4)]
    [TestCase("x+y*i", -2, -5, -2, -5)]
    [TestCase("z+x-y", 3, 4, 2, 4)]
    [TestCase("x²+y²+i", 3, 4, 25, 1)]
    [TestCase("x/y+i", 3, 4, 0.75, 1)]
    [TestCase("x-z.Real+(y-z.Imaginary)*i", -2, 5, 0, 0)]
    public void ComplexExpressionsExposeTheRealAndImaginaryCoordinates(
        string expression, double real, double imaginary, double expectedReal, double expectedImaginary)
    {
        var result = new PortableEngine().EvaluateComplex(expression, new Complex(real, imaginary));
        NUnit.Framework.Assert.That((result - new Complex(expectedReal, expectedImaginary)).Magnitude, Is.LessThan(1e-12));
    }

    [Test]
    public void CustomFunctionsKeepTheirOwnCoordinateParametersAndLocals()
    {
        const string definitions = """
            static complex Pair(real x, real y) { return new complex(x,y); }
            static complex Offset(complex value) {
                real x = value.Real+1;
                real y = value.Imaginary-2;
                return Pair(x,y);
            }
            """;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateComplex("Pair(x,y)+Offset(z)", new Complex(3,4), definitions),
            Is.EqualTo(new Complex(7,6)));
        NUnit.Framework.Assert.That(engine.EvaluateComplex("Pair(x,y)+Offset(z)", new Complex(-2,5), definitions),
            Is.EqualTo(new Complex(-3,8)));
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            engine.EvaluateComplex("Pair(x,y)", new Complex(3,4)));
    }

    [Test]
    public void ExpressionCoordinatesTakePrecedenceOverCustomFieldsAsOnDesktop()
    {
        const string definitions = """
            public static real x = 13;
            public static real y = 7;
            static complex Fields() { return x+y*i; }
            """;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateComplex("x+y*i+Fields()", new Complex(3,4), definitions),
            Is.EqualTo(new Complex(16,11)));
        NUnit.Framework.Assert.That(engine.EvaluateComplex("z.Realʸ", new Complex(3,4), definitions),
            Is.EqualTo(new Complex(81,0)));
    }

    [TestCase("new Func<real,real>(x => x+1).Invoke(5)+z", 9, 4)]
    [TestCase("new Func<real>(() => { var y=7; return y; }).Invoke()+z", 10, 4)]
    public void NestedLambdasKeepTheirOwnCoordinateNames(string expression, double real, double imaginary)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateComplex(expression, new Complex(3,4)),
            Is.EqualTo(new Complex(real,imaginary)));
    }

    [TestCase("x")]
    [TestCase("y")]
    public void CoordinateAliasesPreserveSignedZero(string expression)
    {
        var result = new PortableEngine().EvaluateComplex(expression, new Complex(-0.0,-0.0));
        NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(result.Real), Is.EqualTo(long.MinValue));
    }

    [Test]
    public void AddedCoordinatesDoNotShiftUserDiagnostics()
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateComplex("z+\nmissingCoordinate+x", new Complex(3,4)));
        NUnit.Framework.Assert.That(error!.Diagnostics, Has.Count.EqualTo(1));
        var diagnostic = error.Diagnostics[0];
        NUnit.Framework.Assert.That((diagnostic.Code, diagnostic.File, diagnostic.Line, diagnostic.Column),
            Is.EqualTo(("CS0103", "expression.tsl", 2, 1)));
    }

    [Test]
    public void RealEvaluationStillUsesTheIndependentXAndYArguments()
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("x+2y", -2, 5), Is.EqualTo(8));
    }
}
