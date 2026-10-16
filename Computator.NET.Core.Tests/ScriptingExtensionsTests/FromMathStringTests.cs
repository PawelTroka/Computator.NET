using System;
using System.Globalization;
using System.Text;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ScriptingExtensionsTests
{
    [TestFixture]
    [NonParallelizable]
    public class FromMathStringTests
    {
        [TestCase("0", 0d)]
        [TestCase("-12.5", -12.5)]
        [TestCase("  +1.25E+3  ", 1250d)]
        [TestCase("1.25e-3", 0.00125)]
        [TestCase("10³", 1000d)]
        [TestCase("10⁻³", 0.001)]
        [TestCase("10⁺³", 1000d)]
        [TestCase("10⁰", 1d)]
        [TestCase("2.5·10³", 2500d)]
        [TestCase(" -2.5 · 10⁻³ ", -0.0025)]
        [TestCase("2·3", 6d)]
        [TestCase("2e1·3e-1", 6d)]
        [TestCase("∞", double.PositiveInfinity)]
        [TestCase("+∞", double.PositiveInfinity)]
        [TestCase("-∞", double.NegativeInfinity)]
        [TestCase("NaN", double.NaN)]
        public void ParsesSupportedNumericForms(string text, double expected)
        {
            NUnit.Framework.Assert.That(text.FromMathString(), Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase(" \t\r\n ")]
        [TestCase("1·2·3")]
        [TestCase("1··2")]
        [TestCase("·2")]
        [TestCase("2·")]
        [TestCase("1·2³")]
        [TestCase("2³")]
        [TestCase("10⁻")]
        [TestCase("10⁺")]
        [TestCase("10²3")]
        [TestCase("10²⁻³")]
        [TestCase("10³ॱ⁵")]
        [TestCase("1·10⁻⁻³")]
        [TestCase("1,25")]
        [TestCase("1,234")]
        [TestCase("1E")]
        [TestCase("sin(1)")]
        public void RejectsMalformedOrExpressionInputInsteadOfReturningZero(string text)
        {
            NUnit.Framework.Assert.Throws<FormatException>(() => text.FromMathString());
        }

        [Test]
        public void NullHasAnExplicitArgumentContract()
        {
            NUnit.Framework.Assert.Throws<ArgumentNullException>(() => ScriptingExtensions.FromMathString(null));
        }

        [TestCase("en-US")]
        [TestCase("pl-PL")]
        [TestCase("de-DE")]
        [TestCase("fr-FR")]
        public void BothDisplayNotationsRoundTripBinary64WithoutIntermediateRounding(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousNotation = settings.NumericalOutputNotation;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                var values = new[] { 0d, -0d, 1d, Math.BitIncrement(1d), -12.5,
                    double.Epsilon, -double.Epsilon, Math.BitIncrement(double.Epsilon),
                    1e-320, 2.2250738585072014e-308, 1.232223e-21,
                    1e308, double.MaxValue, double.MinValue,
                    double.PositiveInfinity, double.NegativeInfinity };
                foreach (var notation in new[] { NumericalOutputNotationType.MathematicalNotation,
                    NumericalOutputNotationType.EngineeringNotation })
                {
                    settings.NumericalOutputNotation = notation;
                    foreach (var value in values)
                    {
                        var text = value.ToMathString();
                        NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(text.FromMathString()),
                            Is.EqualTo(BitConverter.DoubleToInt64Bits(value)), text);
                    }
                    NUnit.Framework.Assert.That(double.IsNaN(double.NaN.ToMathString().FromMathString()), Is.True);
                }
                NUnit.Framework.Assert.That("5·10⁻³²⁴".FromMathString(), Is.EqualTo(double.Epsilon));
                NUnit.Framework.Assert.That("1.25·10⁻³".FromMathString(), Is.EqualTo(0.00125));
                NUnit.Framework.Assert.Throws<FormatException>(() => "1,25".FromMathString());
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                settings.NumericalOutputNotation = previousNotation;
            }
        }

        [TestCase("en-US")]
        [TestCase("pl-PL")]
        public void GeneratedScriptsUseTheSameParserAndPreserveLiteralNotation(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousDirectory = Environment.CurrentDirectory;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                var output = new StringBuilder();
                new ScriptEvaluator().Evaluate(@"
                    write(System.BitConverter.DoubleToInt64Bits(""5·10⁻³²⁴"".FromMathString()));
                    write(""|"");
                    write(""10³"".FromMathString());
                    write(""|"");
                    write(double.IsPositiveInfinity(""∞"".FromMathString()));
                    write(""|"");
                    try { write(""1·2·3"".FromMathString()); }
                    catch (System.FormatException) { write(""rejected""); }
                ", "").Evaluate(text => output.Append(text));
                NUnit.Framework.Assert.That(output.ToString(), Is.EqualTo("1|1000|True|rejected"));
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
                CultureInfo.CurrentCulture = previousCulture;
            }
        }
    }
}
