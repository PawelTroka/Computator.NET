using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Tests.ScriptingExtensionsTests
{
    [TestFixture]
    class ToMathStringTests
    {
        [TestCase(double.NaN, "NaN")]
        [TestCase(double.NegativeInfinity, "-" + SpecialSymbols.Infinity)]
        [TestCase(double.PositiveInfinity, SpecialSymbols.Infinity)]
        [TestCase(3.41241241241241, "3.41241241241241")]
        [TestCase(3e123, "3" + SpecialSymbols.DotSymbolString + "10¹²³")]
        [TestCase(-3.2223e-123, "-3.2223" + SpecialSymbols.DotSymbolString + "10⁻¹²³")]
        [TestCase(123.2223e-123, "1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹")]
        [TestCase(-123.2223e-123, "-1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹")]
        [TestCase(123.2223e123, "1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵")]
        [TestCase(-123.2223e123, "-1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵")]
        public void DoubleToMathString(double value, string result)
        {
            Assert.AreEqual(result, value.ToMathString());
        }


        [TestCase(3.41241241241241, "3.41241241241241")]
        [TestCase(3e23, "3" + SpecialSymbols.DotSymbolString + "10²³")]
        [TestCase(-3.2223e-23, "-3.2223" + SpecialSymbols.DotSymbolString + "10⁻²³")]
        [TestCase(123.2223e-23, "1.232223" + SpecialSymbols.DotSymbolString + "10⁻²¹")]
        [TestCase(-123.2223e-23, "-1.232223" + SpecialSymbols.DotSymbolString + "10⁻²¹")]
        [TestCase(123.2223e23, "1.232223" + SpecialSymbols.DotSymbolString + "10²⁵")]
        [TestCase(-123.2223e23, "-1.232223" + SpecialSymbols.DotSymbolString + "10²⁵")]
        public void DecimalToMathString(decimal value, string result)
        {
            Assert.AreEqual(result, value.ToMathString());
        }


        [TestCase(double.NaN, "NaN")]
        [TestCase(double.NegativeInfinity, "-" + SpecialSymbols.Infinity)]
        [TestCase(double.PositiveInfinity, SpecialSymbols.Infinity)]
        [TestCase(3.41241241241241, "3.41241241241241")]
        [TestCase(-3.41241241241241, "-3.41241241241241")]
        [TestCase(3e123, "3" + SpecialSymbols.DotSymbolString + "10¹²³")]
        [TestCase(-3.2223e-123, "-3.2223" + SpecialSymbols.DotSymbolString + "10⁻¹²³")]
        [TestCase(123.2223e-123, "1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹")]
        [TestCase(-123.2223e-123, "-1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹")]
        [TestCase(123.2223e123, "1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵")]
        [TestCase(-123.2223e123, "-1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵")]
        public void ComplexFromDoubleToMathString(double value, string result)
        {
            Assert.AreEqual(result, ((Complex)value).ToMathString());
        }





        [TestCase(double.NaN, double.NaN, "NaN" + "+NaN" + SpecialSymbols.DotSymbolString + "i")]

        [TestCase(double.NegativeInfinity, double.NaN, "-" + SpecialSymbols.Infinity + "+NaN" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(double.NaN, double.NegativeInfinity, "NaN-" + SpecialSymbols.Infinity + SpecialSymbols.DotSymbolString + "i")]


        [TestCase(double.PositiveInfinity, double.NaN, SpecialSymbols.Infinity + "+NaN" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(double.NaN, double.PositiveInfinity, "NaN+" + SpecialSymbols.Infinity + SpecialSymbols.DotSymbolString + "i")]




        [TestCase(double.NegativeInfinity, double.NegativeInfinity, "-" + SpecialSymbols.Infinity + "-" + SpecialSymbols.Infinity + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity, SpecialSymbols.Infinity + "+" + SpecialSymbols.Infinity + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(double.PositiveInfinity, double.NegativeInfinity, SpecialSymbols.Infinity + "-" + SpecialSymbols.Infinity + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, "-" + SpecialSymbols.Infinity + "+" + SpecialSymbols.Infinity + SpecialSymbols.DotSymbolString + "i")]




        [TestCase(3.41241241241241, 321.142, "3.41241241241241" + "+321.142" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(-3.41241241241241, 321.142, "-3.41241241241241" + "+321.142" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(3.41241241241241, -321.142, "3.41241241241241" + "-321.142" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(-3.41241241241241, -321.142, "-3.41241241241241" + "-321.142" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(0, -321.142, "-321.142" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(3.41241241241241, 0, "3.41241241241241")]

        [TestCase(0, 3e123, "3" + SpecialSymbols.DotSymbolString + "10¹²³" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(0, -3.2223e-123, "-3.2223" + SpecialSymbols.DotSymbolString + "10⁻¹²³" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(0, 123.2223e-123, "1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(0, -123.2223e-123, "-1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(0, 123.2223e123, "1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(0, -123.2223e123, "-1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵" + SpecialSymbols.DotSymbolString + "i")]
        [TestCase(3e123, 0, "3" + SpecialSymbols.DotSymbolString + "10¹²³")]
        [TestCase(-3.2223e-123, 0, "-3.2223" + SpecialSymbols.DotSymbolString + "10⁻¹²³")]
        [TestCase(123.2223e-123, 0, "1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹")]
        [TestCase(-123.2223e-123, 0, "-1.232223" + SpecialSymbols.DotSymbolString + "10⁻¹²¹")]
        [TestCase(123.2223e123, 0, "1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵")]
        [TestCase(-123.2223e123, 0, "-1.232223" + SpecialSymbols.DotSymbolString + "10¹²⁵")]
        public void ComplexToMathString(double re, double im, string result)
        {
            Assert.AreEqual(result, (new Complex(re, im)).ToMathString());
        }
    }
}
