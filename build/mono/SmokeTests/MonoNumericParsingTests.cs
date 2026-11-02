using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.Engine;

internal static partial class Program
{
    private sealed class NumericRoslynEvaluator : ExpressionsEvaluator { }

    private static void CorrectlyRoundedNumericInput()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousDirectory = Environment.CurrentDirectory;
        try
        {
            foreach (var culture in new[] { "en-US", "pl-PL" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                foreach (var literal in new[] { "8.88024331", "8.88024331e0", "888024331E-8", "+8.88024331", "-8.88024331" })
                {
                    var expected = literal[0] == '-' ? unchecked((long)0xc021c2af4049f211UL) : 0x4021c2af4049f211L;
                    Check(BitConverter.DoubleToInt64Bits(new ExpressionsEvaluator().Evaluate(literal, "", CalculationsMode.Real).Evaluate(0.0)) == expected,
                        "Fast expression input changed the nearest binary64 value: " + literal + " in " + culture);
                    Check(BitConverter.DoubleToInt64Bits(new NumericRoslynEvaluator().Evaluate(literal, "", CalculationsMode.Real).Evaluate(0.0)) == expected,
                        "The Roslyn expression path disagrees: " + literal);
                    Check(BitConverter.DoubleToInt64Bits(new PortableEngine().EvaluateReal(literal)) == expected, "Portable literal rounding differs.");
                }
                var inputs = new[] { "8.88024331", "  +8.88024331e0  ", "-8.88024331", "888024331·10⁻⁸", "-0", "-0.0", "-0E0", "1.", ".5", "5E-324", "1E-324", "2.2250738585072014E-308", "1.7976931348623157E308",
                    "1.00000000000000011102230246251565404236316680908203125", "1.00000000000000033306690738754696212708950042724609375", "-1E-324", "+1.e2", "8.88024331\0" };
                var expectedBits = new[] { 0x4021c2af4049f211L, 0x4021c2af4049f211L, unchecked((long)0xc021c2af4049f211UL), 0x4021c2af4049f211L,
                    long.MinValue, long.MinValue, long.MinValue, 0x3ff0000000000000L, 0x3fe0000000000000L, 1L, 0L, 0x0010000000000000L, 0x7fefffffffffffffL,
                    0x3ff0000000000000L, 0x3ff0000000000002L, long.MinValue, 0x4059000000000000L, 0x4021c2af4049f211L };
                for (var i = 0; i < inputs.Length; i++)
                    Check(BitConverter.DoubleToInt64Bits(inputs[i].FromMathString()) == expectedBits[i], "Displayed numeric input rounding differs: " + inputs[i]);
                foreach (var invalid in new[] { "1,25", "1,234", "1_234", "0x10", "1.25d", "8.88 extra", "1E", "--1" })
                    Expect<FormatException>(() => invalid.FromMathString());

                var code = "foreach(var input in new string[] {" + string.Join(",", inputs.Select(text => "\"" + text.Replace("\0", "\\0") + "\"")) +
                    "}) { write(System.BitConverter.DoubleToInt64Bits(input.FromMathString()).ToString(\"x16\")); write(\"|\"); }" +
                    "write(System.BitConverter.DoubleToInt64Bits(8.88024331).ToString(\"x16\"));";
                var output = new StringBuilder();
                new ScriptEvaluator().Evaluate(code, "").Evaluate(text => output.Append(text));
                Equal(output.ToString(), string.Concat(expectedBits.Select(bits => unchecked((ulong)bits).ToString("x16") + "|")) + "4021c2af4049f211");
            }

            var catalog = new FunctionsDetailsFileSource().Details;
            var emitted = new NativeCompiler().Compile("public static class NumericConstants {" + MathematicalConstants.ToCode + PhysicalConstants.ToCode + "}")
                .GetType("NumericConstants", true);
            var fields = new[] { typeof(MathematicalConstants), typeof(PhysicalConstants) }.SelectMany(type => new[] { type }.Concat(type.GetNestedTypes()))
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static)).Where(field => field.Name != "ToCode").ToArray();
            Check(fields.Length == 429, "The numeric-input audit must cover every authored constant field.");
            foreach (var field in fields)
            {
                var description = XElement.Parse(catalog[field.Name].Description).Elements("p").Single(p => (string)p.Attribute("class") == "reference-arguments").Value;
                var text = Regex.Match(description, @"Stored application value: (.*?)(?:\. |\.$)").Groups[1].Value;
                var value = field.GetValue(null);
                var generated = emitted.GetField(field.Name).GetValue(null);
                if (value is double)
                {
                    CheckSameNumericBits((double)value, text.FromMathString(), field.Name + " parsed description");
                    CheckSameNumericBits((double)value, (double)generated, field.Name + " generated source");
                }
                else
                {
                    var parts = text.Trim('<', '>', '(', ')').Split(new[] { ';', ',' });
                    var complex = (Complex)value; var generatedComplex = (Complex)generated;
                    CheckSameNumericBits(complex.Real, parts[0].FromMathString(), field.Name + " real");
                    CheckSameNumericBits(complex.Imaginary, parts[1].FromMathString(), field.Name + " imaginary");
                    CheckSameNumericBits(complex.Real, generatedComplex.Real, field.Name + " generated real");
                    CheckSameNumericBits(complex.Imaginary, generatedComplex.Imaginary, field.Name + " generated imaginary");
                }
            }
            Console.WriteLine("  correctly rounded expression/string/script input; signs, zero, boundaries, cultures and all 429 authored/emitted constants passed");
        }
        finally { CultureInfo.CurrentCulture = previousCulture; Environment.CurrentDirectory = previousDirectory; }
    }

    private static void CheckSameNumericBits(double expected, double actual, string context)
    {
        Check((double.IsNaN(expected) && double.IsNaN(actual)) || BitConverter.DoubleToInt64Bits(expected) == BitConverter.DoubleToInt64Bits(actual), context);
    }
}
