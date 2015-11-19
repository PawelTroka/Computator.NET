using Computator.NET.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ModeDeterminerUnitTest
    {
        private ModeDeterminer modeDeterminer = new ModeDeterminer();

        [TestMethod]
        public void AloneImaginaryUnit_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("i"));
        }

        [TestMethod]
        public void ImaginaryUnitInSimpleExpression_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("2/i"));
        }

        [TestMethod]
        public void VariableZAndImaginaryUnitInSimpleExpression_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("z+z²+1-1+i"));
        }

        [TestMethod]
        public void VariableZPlusNumber_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("2+z"));
        }
        [TestMethod]
        public void VariableXPlusNumber_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("2+x"));
        }
        [TestMethod]
        public void VariableYPlusNumber_shouldReturnFxy()//
        {
            Assert.AreEqual(CalculationsMode.Fxy, modeDeterminer.DetermineMode("2+y"));
        }

        [TestMethod]
        public void expressionWithXVariableInExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("x²+cos(x²˙ˣ⁺¹ॱ¹⁺ᶜᵒˢ⁽ˣ⁾)+2/3"));
        }

        [TestMethod]
        public void expressionWithRealExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("2¹⁰³²¹³ॱ³²³²³²"));//z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾
        }

        [TestMethod]
        public void XYZ_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("z·x·y+yˣ˙ᶻ˙ʸ⁺¹¹˙ˣ⁺ᶜᵒˢ⁽ˣ˸ʸ⁾"));//(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))
        }

        [TestMethod]
        public void expressionWithXVariableAndExponents_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))"));//(10²·x)/(10-6·x²+(25-x²)²+10·(25-x²))
        }


        [TestMethod]
        public void implicitRealFunction_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("x²+y²=5"));//x²+y²+z²=52²
        }

        [TestMethod]
        public void implicitXYZFunction_shouldReturnFxy()
        {
            Assert.AreEqual(CalculationsMode.Fxy, modeDeterminer.DetermineMode("x²+y²+z²=52²"));
        }


        [TestMethod]
        public void RealExpressionWithImaginaryUnitInExponent_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("(cos(1.0))ⁱ"));
        }

        [TestMethod]
        public void RealNumberWithImaginaryUnitInExponent_shouldReturnComplex()
        {
            Assert.AreEqual(CalculationsMode.Complex, modeDeterminer.DetermineMode("2ⁱ"));
        }

        [TestMethod]
        public void RealNumberWithRealNumbersInExponent_shouldReturnReal()
        {
            Assert.AreEqual(CalculationsMode.Real, modeDeterminer.DetermineMode("2¹²+12-6¹²"));
        }
    }
}