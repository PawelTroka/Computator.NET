using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class MathStringInputTests
    {
        [TestCase("10³", 1000d)]
        [TestCase("2·3", 6d)]
        [TestCase("1e2·3", 300d)]
        [TestCase("5·10⁻³²⁴", double.Epsilon)]
        [TestCase("1.7976931348623157·10³⁰⁸", double.MaxValue)]
        [TestCase("-∞", double.NegativeInfinity)]
        public void GeneratedScriptsUseTheSameParser(string input, double expected)
        {
            var code = "write(System.BitConverter.DoubleToInt64Bits(" + Literal(input) +
                ".FromMathString()).ToString(System.Globalization.CultureInfo.InvariantCulture));";
            Assert.That(Run(code), Is.EqualTo(BitConverter.DoubleToInt64Bits(expected).ToString(CultureInfo.InvariantCulture)));
        }

        [TestCase("1·2·3")]
        [TestCase("2·2³")]
        [TestCase("10⁻")]
        [TestCase("1,5")]
        public void GeneratedScriptsRejectTheSameMalformedInput(string input)
        {
            var code = "try { var value = " + Literal(input) +
                ".FromMathString(); write(\"unexpected\"); } catch (System.FormatException) { write(\"format\"); }";
            Assert.That(Run(code), Is.EqualTo("format"));
        }

        [TestCase(NumericalOutputNotationType.MathematicalNotation)]
        [TestCase(NumericalOutputNotationType.EngineeringNotation)]
        public void GeneratedFormattingAndParsingRoundTripExtremeValues(NumericalOutputNotationType notation)
        {
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previous = settings.NumericalOutputNotation;
            try
            {
                settings.NumericalOutputNotation = notation;
                var code = @"
                    var tiny = System.Double.Epsilon;
                    var large = System.Double.MaxValue;
                    if (tiny.ToMathString().FromMathString() != tiny) throw new System.InvalidOperationException(""tiny"");
                    if (large.ToMathString().FromMathString() != large) throw new System.InvalidOperationException(""large"");
                    write(""roundtrip"");";
                Assert.That(Run(code), Is.EqualTo("roundtrip"));
            }
            finally { settings.NumericalOutputNotation = previous; }
        }

        [TestCase("1·2·3")]
        [TestCase("2·2³")]
        [TestCase("1,5")]
        public void InvalidNumericControlInputPreservesThePreviousValue(string input)
        {
            using var control = new ScientificNumericUpDown { UpDownMode = ScientificNumericUpDown.UpDownBehavior.Addition, Value = 17m };
            control.Text = input;
            Validate(control);
            Assert.That(control.Value, Is.EqualTo(17m));
            control.Text = "10³";
            Validate(control);
            Assert.That(control.Value, Is.EqualTo(1000m));
        }

        private static void Validate(ScientificNumericUpDown control)
        {
            typeof(ScientificNumericUpDown).GetMethod("ValidateEditText", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(control, null);
        }

        private static string Literal(string input) => "@\"" + input.Replace("\"", "\"\"") + "\"";

        private static string Run(string code)
        {
            GSLInitializer.Initialize();
            var output = new StringBuilder();
            new ScriptEvaluator().Evaluate(code, "").Evaluate(text => output.Append(text));
            return output.ToString();
        }
    }
}
