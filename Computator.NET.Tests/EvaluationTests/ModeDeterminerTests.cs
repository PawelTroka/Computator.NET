using Computator.NET.Compilation;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;
using NUnit.Framework;

namespace Computator.NET.Tests.EvaluationTests
{
    [TestFixture]
    public class ModeDeterminerTests
    {
        private readonly ModeDeterminer _modeDeterminer = new ModeDeterminer(new TslCompiler());

        [Test]
        public void AloneImaginaryUnit_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("i"));
        }

        [Test]
        public void expressionWithRealExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, _modeDeterminer.DetermineMode("2¹⁰³²¹³ॱ³²³²³²"));
        }

        [Test]
        public void expressionWithRealAndComplexComponentsAfterNumberWithoutDot_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("2sin(2x)+2z"));
        }

        [Test]
        public void expressionWithXVariableAndExponents_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, _modeDeterminer.DetermineMode("(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))"));
            //(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))
        }

        [Test]
        public void expressionWithXVariableInExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, _modeDeterminer.DetermineMode("x²+cos(x²˙ˣ⁺¹ॱ¹⁺ᶜᵒˢ⁽ˣ⁾)+2/3"));
        }

        [Test]
        public void ImaginaryUnitInSimpleExpression_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("2/i"));
        }


        [Test]
        public void implicitRealFunction_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, _modeDeterminer.DetermineMode("x²+y²=5")); //x²+y²+z²=52²
        }

        [Test]
        public void implicitXYZFunction_shouldReturnFxy()
        {
            Assert.AreEqual(CalculationsMode.Fxy, _modeDeterminer.DetermineMode("x²+y²+z²=52²"));
        }


        [Test]
        public void RealExpressionWithImaginaryUnitInExponent_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("(cos(1.0))ⁱ"));
        }

        [Test]
        public void RealNumberWithImaginaryUnitInExponent_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("2ⁱ"));
        }

        [Test]
        public void RealNumberWithRealNumbersInExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, _modeDeterminer.DetermineMode("2¹²+12-6¹²"));
        }

        [Test]
        public void VariableXPlusNumber_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, _modeDeterminer.DetermineMode("2+x"));
        }

        [Test]
        public void VariableYPlusNumber_shouldReturnFxy() //
        {
            Assert.AreEqual(CalculationsMode.Fxy, _modeDeterminer.DetermineMode("2+y"));
        }

        [Test]
        public void VariableZAndImaginaryUnitInSimpleExpression_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("z+z²+1-1+i"));
        }

        [Test]
        public void VariableZPlusNumber_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("2+z"));
        }

        [Test]
        public void XYZ_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, _modeDeterminer.DetermineMode("z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾"));
            //(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))
        }
    }
}