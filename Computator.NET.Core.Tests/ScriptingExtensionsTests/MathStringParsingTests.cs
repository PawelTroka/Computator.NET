using System;
using System.Globalization;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ScriptingExtensionsTests
{
    [TestFixture]
    [NonParallelizable]
    public class MathStringParsingTests
    {
        [TestCase("0", 0d)]
        [TestCase("-2.5", -2.5)]
        [TestCase(".5", 0.5)]
        [TestCase("+1.25e+2", 125d)]
        [TestCase("1E-3", 0.001)]
        [TestCase(" 2.5 · 10³ ", 2500d)]
        [TestCase("10³", 1000d)]
        [TestCase("10⁻³", 0.001)]
        [TestCase("10⁺³", 1000d)]
        [TestCase("-10³", -1000d)]
        [TestCase("+10³", 1000d)]
        [TestCase("2·-10³", -2000d)]
        [TestCase("-2·-10³", 2000d)]
        [TestCase("+2·-10³", -2000d)]
        [TestCase("2·3", 6d)]
        [TestCase("2·10", 20d)]
        [TestCase("1e2·3", 300d)]
        [TestCase("2·1e2", 200d)]
        [TestCase("5·10⁻³²⁴", double.Epsilon)]
        [TestCase("-5·10⁻³²⁴", -double.Epsilon)]
        [TestCase("1.7976931348623157·10³⁰⁸", double.MaxValue)]
        [TestCase("0·10⁹⁹⁹", 0d)]
        [TestCase("0.0001·10³¹²", 1e308)]
        [TestCase("∞", double.PositiveInfinity)]
        [TestCase("+∞", double.PositiveInfinity)]
        [TestCase("-∞", double.NegativeInfinity)]
        [TestCase("Infinity", double.PositiveInfinity)]
        [TestCase("-Infinity", double.NegativeInfinity)]
        public void RecognizedNotationHasTheExpectedValue(string text, double expected)
        {
            Assert.That(text.FromMathString(), Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase(" \r\n")]
        [TestCase(".")]
        [TestCase("-")]
        [TestCase("1,5")]
        [TestCase("1,000")]
        [TestCase("1·2·3")]
        [TestCase("·10²")]
        [TestCase("2·")]
        [TestCase("2·2³")]
        [TestCase("2³")]
        [TestCase("2·10⁻")]
        [TestCase("10⁺")]
        [TestCase("10²3")]
        [TestCase("10³.0")]
        [TestCase("2·10²⁻")]
        [TestCase("10⁻⁻²")]
        [TestCase("1E")]
        [TestCase("1E2E3")]
        [TestCase("abc")]
        [TestCase("2*3")]
        [TestCase("2·¹")]
        public void MalformedNumbersAreRejectedInsteadOfInventingAValue(string text)
        {
            Assert.Throws<FormatException>(() => text.FromMathString());
        }

        [Test]
        public void NullHasAnArgumentError()
        {
            Assert.Throws<ArgumentNullException>(() => ScriptingExtensions.FromMathString(null));
        }

        [Test]
        public void NaNRemainsNaN()
        {
            Assert.That(double.IsNaN("NaN".FromMathString()), Is.True);
        }

        [TestCase("en-US", NumericalOutputNotationType.MathematicalNotation)]
        [TestCase("pl-PL", NumericalOutputNotationType.MathematicalNotation)]
        [TestCase("de-DE", NumericalOutputNotationType.MathematicalNotation)]
        [TestCase("en-US", NumericalOutputNotationType.EngineeringNotation)]
        [TestCase("pl-PL", NumericalOutputNotationType.EngineeringNotation)]
        [TestCase("de-DE", NumericalOutputNotationType.EngineeringNotation)]
        public void DisplayRoundTripsTheExactBinary64Bits(string culture, NumericalOutputNotationType notation)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousNotation = settings.NumericalOutputNotation;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                settings.NumericalOutputNotation = notation;
                var values = new[] { 0d, BitConverter.Int64BitsToDouble(long.MinValue), double.Epsilon,
                    -double.Epsilon, double.MaxValue, double.MinValue, 1e-308, -1e-308, 1e200, -1e200,
                    Math.BitIncrement(1d), double.PositiveInfinity, double.NegativeInfinity };
                foreach (var value in values)
                    AssertRoundTrip(value);

                // Reproducible values from across the binary64 exponent range.
                var random = new Random(105);
                var bytes = new byte[sizeof(double)];
                for (var index = 0; index < 256; index++)
                {
                    random.NextBytes(bytes);
                    var value = BitConverter.ToDouble(bytes, 0);
                    if (!double.IsNaN(value))
                        AssertRoundTrip(value);
                }
                Assert.Throws<FormatException>(() => "1,5".FromMathString());
                Assert.That("1.5".FromMathString(), Is.EqualTo(1.5));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                settings.NumericalOutputNotation = previousNotation;
            }
        }

        private static void AssertRoundTrip(double value)
        {
            var display = value.ToMathString();
            Assert.That(BitConverter.DoubleToInt64Bits(display.FromMathString()),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(value)), display);
        }
    }
}
