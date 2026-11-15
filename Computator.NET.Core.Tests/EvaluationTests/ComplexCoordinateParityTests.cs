using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.Engine;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture("default")]
    [TestFixture("precompiled")]
    [TestFixture("generated")]
    public class ComplexCoordinateParityTests
    {
        private readonly string _mode;
        public ComplexCoordinateParityTests(string mode) { _mode = mode; }

        private class CompiledEvaluator : ExpressionsEvaluator
        {
            protected override bool UseDirectExpressions => false;
        }
        private sealed class GeneratedEvaluator : CompiledEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }
        private ExpressionsEvaluator Evaluator() => _mode == "default" ? new ExpressionsEvaluator()
            : _mode == "precompiled" ? new CompiledEvaluator() : new GeneratedEvaluator();

        [TestCase("x+y*i", 3, 4, 3, 4)]
        [TestCase("z+x-y", -2, 5, -9, 5)]
        [TestCase("x²+y²+i", 3, 4, 25, 1)]
        [TestCase("new System.Func<real,real>(x => x+1).Invoke(5)+z", 3, 4, 9, 4)]
        [TestCase("new System.Func<real>(() => { var y=7; return y; }).Invoke()+z", 3, 4, 10, 4)]
        public void PortableCoordinatesMatchEachDesktopCompilationPath(
            string source, double real, double imaginary, double expectedReal, double expectedImaginary)
        {
            var z = new Complex(real, imaginary);
            var expected = new Complex(expectedReal, expectedImaginary);
            var desktop = Evaluator().Evaluate(source, "", CalculationsMode.Complex).Evaluate(z);
            var portable = new PortableEngine().EvaluateComplex(source, z);
            NUnit.Framework.Assert.That((desktop - expected).Magnitude, Is.LessThan(1e-12));
            NUnit.Framework.Assert.That((portable - expected).Magnitude, Is.LessThan(1e-12));
        }

        [Test]
        public void CustomFunctionsMayDeclareTheirOwnCoordinates()
        {
            const string source = "Pair(x,y)+Offset(z)";
            const string definitions = @"
                static complex Pair(real x, real y) { return new complex(x,y); }
                static complex Offset(complex value) {
                    real x = value.Real+1;
                    real y = value.Imaginary-2;
                    return Pair(x,y);
                }";
            var z = new Complex(3, 4);
            var desktop = Evaluator().Evaluate(source, definitions, CalculationsMode.Complex).Evaluate(z);
            var portable = new PortableEngine().EvaluateComplex(source, z, definitions);
            NUnit.Framework.Assert.That(desktop, Is.EqualTo(new Complex(7, 6)));
            NUnit.Framework.Assert.That(portable, Is.EqualTo(desktop));
        }

        [Test]
        public void ExpressionCoordinatesTakePrecedenceOverCustomFields()
        {
            const string definitions = @"
                public static real x = 13;
                public static real y = 7;
                static complex Fields() { return x+y*i; }";
            var z = new Complex(3, 4);
            foreach (var example in new[] {
                (Source: "x+y*i+Fields()", Expected: new Complex(16,11)),
                (Source: "z.Realʸ", Expected: new Complex(81,0))
            })
            {
                var desktop = Evaluator().Evaluate(example.Source, definitions, CalculationsMode.Complex).Evaluate(z);
                var portable = new PortableEngine().EvaluateComplex(example.Source, z, definitions);
                NUnit.Framework.Assert.That(desktop, Is.EqualTo(example.Expected));
                NUnit.Framework.Assert.That(portable, Is.EqualTo(desktop));
            }
        }
    }
}
