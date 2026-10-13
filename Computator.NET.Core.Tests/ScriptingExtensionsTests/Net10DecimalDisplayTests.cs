using System;
using System.Globalization;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ScriptingExtensionsTests
{
    [TestFixture]
    [NonParallelizable]
    public class Net10DecimalDisplayTests
    {
        [TestCase("0", "0")]
        [TestCase("1.2300", "1.23")]
        [TestCase("0.0001", "0.0001")]
        [TestCase("0.00001", "1E-5")]
        [TestCase("1.232223E-21", "1.232223E-21")]
        [TestCase("-1.232223E-21", "-1.232223E-21")]
        [TestCase("3E23", "3E+23")]
        [TestCase("1E-28", "1E-28")]
        [TestCase("1.2345678901234567890123456789", "1.2345678901234567890123456789")]
        [TestCase("79228162514264337593543950335", "7.9228162514264337593543950335E+28")]
        [TestCase("-79228162514264337593543950335", "-7.9228162514264337593543950335E+28")]
        public void DisplayPreservesTheExactDecimalValue(string input, string expected)
        {
            var value = decimal.Parse(input, NumberStyles.Float, CultureInfo.InvariantCulture);
            var actual = DecimalDisplay.Format(value);
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(decimal.Parse(actual, NumberStyles.Float, CultureInfo.InvariantCulture), Is.EqualTo(value));
        }

        [TestCase("pl-PL")]
        [TestCase("de-DE")]
        public void BothNotationModesAreInvariantAndDoNotExposeBinaryArtifacts(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousNotation = settings.NumericalOutputNotation;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                settings.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
                NUnit.Framework.Assert.That((1.232223e-21m).ToMathString(), Is.EqualTo("1.232223·10⁻²¹"));
                settings.NumericalOutputNotation = NumericalOutputNotationType.EngineeringNotation;
                NUnit.Framework.Assert.That((1.232223e-21m).ToMathString(), Is.EqualTo("1.232223E-21"));
                // The migration must not lower double precision to conceal a decimal bug.
                var value = Math.BitIncrement(1.0);
                NUnit.Framework.Assert.That(double.Parse(value.ToMathString(), CultureInfo.InvariantCulture), Is.EqualTo(value));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                settings.NumericalOutputNotation = previousNotation;
            }
        }
    }
}
