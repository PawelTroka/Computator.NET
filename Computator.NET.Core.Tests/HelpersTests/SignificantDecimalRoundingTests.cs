using System;
using System.Globalization;
using Computator.NET.Core.Helpers;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.HelpersTests
{
    [TestFixture]
    public class SignificantDecimalRoundingTests
    {
        [TestCase("0", "0", 2)]
        [TestCase("1234.5", "1230", 3)]
        [TestCase("-1234.5", "-1230", 3)]
        [TestCase("2.45", "2.4", 2)]
        [TestCase("9.995", "10", 3)]
        [TestCase("0.00012345", "0.00012", 2)]
        [TestCase("0.0000000000000000000000000001", "0.0000000000000000000000000001", 2)]
        [TestCase("10000000000000000000000000000", "10000000000000000000000000000", 2)]
        [TestCase("79228162514264337593543950335", "79000000000000000000000000000", 2)]
        [TestCase("-79228162514264337593543950335", "-79000000000000000000000000000", 2)]
        public void RoundToSignificantDigitsHandlesDecimalRangeWithoutIntermediateOverflow(
            string input, string expected, int digits)
        {
            var value = decimal.Parse(input, NumberStyles.Float, CultureInfo.InvariantCulture);
            var rounded = value.RoundToSignificantDigits(digits);
            Assert.That(rounded, Is.EqualTo(decimal.Parse(expected, NumberStyles.Float, CultureInfo.InvariantCulture)));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(29)]
        public void RejectsInvalidSignificantDigitCounts(int digits) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => 1m.RoundToSignificantDigits(digits));
    }
}
