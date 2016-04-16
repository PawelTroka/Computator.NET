using Computator.NET.Evaluation;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ModeDeterminerUnitTest
    {
        private readonly ModeDeterminer modeDeterminer = new ModeDeterminer();

        [Test]
        public void AloneImaginaryUnit_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("i"));
        }

        [Test]
        public void ImaginaryUnitInSimpleExpression_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("2/i"));
        }

        [Test]
        public void VariableZAndImaginaryUnitInSimpleExpression_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("z+z²+1-1+i"));
        }

        [Test]
        public void VariableZPlusNumber_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("2+z"));
        }

        [Test]
        public void VariableXPlusNumber_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("2+x"));
        }

        [Test]
        public void VariableYPlusNumber_shouldReturnFxy() //
        {
            Assert.AreEqual(CalculationsMode.Fxy, modeDeterminer.DetermineMode("2+y"));
        }

        [Test]
        public void expressionWithXVariableInExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("x²+cos(x²˙ˣ⁺¹ॱ¹⁺ᶜᵒˢ⁽ˣ⁾)+2/3"));
        }

        [Test]
        public void expressionWithRealExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("2¹⁰³²¹³ॱ³²³²³²"));
            //z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾
        }

        [Test]
        public void XYZ_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾"));
            //(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))
        }

        [Test]
        public void expressionWithXVariableAndExponents_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))"));
            //(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))
        }


        [Test]
        public void implicitRealFunction_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("x²+y²=5")); //x²+y²+z²=52²
        }

        [Test]
        public void implicitXYZFunction_shouldReturnFxy()
        {
            Assert.AreEqual(CalculationsMode.Fxy, modeDeterminer.DetermineMode("x²+y²+z²=52²"));
        }


        [Test]
        public void RealExpressionWithImaginaryUnitInExponent_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("(cos(1.0))ⁱ"));
        }

        [Test]
        public void RealNumberWithImaginaryUnitInExponent_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("2ⁱ"));
        }

        [Test]
        public void RealNumberWithRealNumbersInExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("2¹²+12-6¹²"));
        }
    }
}