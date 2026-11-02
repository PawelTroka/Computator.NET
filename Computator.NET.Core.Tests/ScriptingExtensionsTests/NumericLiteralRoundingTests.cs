using System;
using System.Globalization;
using System.Text;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ScriptingExtensionsTests
{
    [TestFixture, NonParallelizable]
    public class NumericLiteralRoundingTests
    {
        [TestCase("8.88024331", 0x4021c2af4049f211L)]
        [TestCase("8.88024331e0", 0x4021c2af4049f211L)]
        [TestCase("888024331E-8", 0x4021c2af4049f211L)]
        [TestCase("-8.88024331", unchecked((long)0xc021c2af4049f211UL))]
        [TestCase("-0.0", long.MinValue)]
        [TestCase("5E-324", 1L)]
        [TestCase("1E-324", 0L)]
        [TestCase("1.7976931348623157E308", 0x7fefffffffffffffL)]
        public void FastExpressionsMatchTheCorrectlyRoundedCompiledLiteral(string input, long expected)
        {
            var function = new ExpressionsEvaluator().Evaluate(input, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(function.Evaluate(0.0)), Is.EqualTo(expected));
        }

        [TestCase("8.88024331", 0x4021c2af4049f211L)]
        [TestCase("  +8.88024331e0  ", 0x4021c2af4049f211L)]
        [TestCase("888024331·10⁻⁸", 0x4021c2af4049f211L)]
        [TestCase("-0", long.MinValue)]
        [TestCase("-0E0", long.MinValue)]
        [TestCase("1.", 0x3ff0000000000000L)]
        [TestCase(".5", 0x3fe0000000000000L)]
        [TestCase("2.2250738585072014E-308", 0x0010000000000000L)]
        public void NumericTextPreservesAcceptedFormsAndBinary64Bits(string input, long expected)
        {
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(input.FromMathString()), Is.EqualTo(expected));
        }

        [TestCase("en-US")]
        [TestCase("pl-PL")]
        public void GeneratedCallsUseInvariantParsingWithoutAcceptingGroupingOrLiteralSuffixes(string culture)
        {
            var previousCulture = CultureInfo.CurrentCulture; var previousDirectory = Environment.CurrentDirectory;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                foreach (var text in new[] { "1,25", "1,234", "1_234", "0x10", "1.25d", "1E", "--1" })
                    NUnit.Framework.Assert.Throws<FormatException>(() => text.FromMathString(), text);
                var output = new StringBuilder();
                new ScriptEvaluator().Evaluate("write(System.BitConverter.DoubleToInt64Bits(\"8.88024331\".FromMathString()).ToString(\"x16\"));", "")
                    .Evaluate(text => output.Append(text));
                NUnit.Framework.Assert.That(output.ToString(), Is.EqualTo("4021c2af4049f211"));
            }
            finally { CultureInfo.CurrentCulture = previousCulture; Environment.CurrentDirectory = previousDirectory; }
        }
    }
}
