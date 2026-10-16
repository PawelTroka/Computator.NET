using System;
using System.Globalization;
using System.Text;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Core.Evaluation
{
    /// <summary>Parses invariant numbers in the application's display notation.</summary>
    /// <remarks>
    /// Accepts decimal/E notation, signed powers of ten with superscript integer
    /// exponents, and one middle-dot product. This is not an expression evaluator.
    /// Decimal points are dots; thousands separators are deliberately rejected.
    /// Scientific display notation is converted before binary64 rounding so even
    /// subnormal values emitted by ToMathString can round-trip.
    /// </remarks>
    public static class MathStringParser
    {
        private const string SuperscriptDigits = "⁰¹²³⁴⁵⁶⁷⁸⁹";

        public static double Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            text = text.Trim();
            var separator = text.IndexOf(SpecialSymbols.DotSymbol);
            if (separator < 0)
                return ParseScalar(text);

            if (text.IndexOf(SpecialSymbols.DotSymbol, separator + 1) >= 0)
                throw new FormatException("A numeric value may contain at most one multiplication separator.");

            var left = text.Substring(0, separator).Trim();
            var right = text.Substring(separator + 1).Trim();
            if (left.Length == 0 || right.Length == 0)
                throw new FormatException("Both factors of a numeric value are required.");

            if (TryPowerOfTen(right, out var negative, out var exponent) && IsDecimalMantissa(left))
            {
                // Parsing 1E-324 first produces zero. Parse 5E-324 as one
                // number instead; the same rule avoids intermediate overflow.
                var mantissa = negative ? Negate(left) : left;
                return ParseInvariant(mantissa + "E" + exponent);
            }

            // Preserve ordinary two-factor products (including E notation).
            // These are two explicit binary64 operands, not a decimal expression.
            return ParseScalar(left) * ParseScalar(right);
        }

        private static double ParseScalar(string text)
        {
            if (TryPowerOfTen(text, out var negative, out var exponent))
                return ParseInvariant((negative ? "-1E" : "1E") + exponent);
            return ParseInvariant(text);
        }

        private static double ParseInvariant(string text)
        {
            if (text == SpecialSymbols.Infinity || text == "+" + SpecialSymbols.Infinity)
                return double.PositiveInfinity;
            if (text == "-" + SpecialSymbols.Infinity)
                return double.NegativeInfinity;
            return double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static bool TryPowerOfTen(string text, out bool negative, out string exponent)
        {
            negative = text.StartsWith("-", StringComparison.Ordinal);
            exponent = null;
            var start = text.StartsWith("+", StringComparison.Ordinal) || negative ? 1 : 0;
            if (text.Length <= start + 2 || text[start] != '1' || text[start + 1] != '0')
                return false;

            var position = start + 2;
            var result = new StringBuilder();
            if (text[position] == '⁺' || text[position] == '⁻')
            {
                result.Append(text[position] == '⁺' ? '+' : '-');
                position++;
            }
            if (position == text.Length)
                return false;

            for (; position < text.Length; position++)
            {
                var digit = SuperscriptDigits.IndexOf(text[position]);
                if (digit < 0)
                    return false;
                result.Append((char)('0' + digit));
            }
            exponent = result.ToString();
            return true;
        }

        private static bool IsDecimalMantissa(string text)
        {
            var position = text.Length > 0 && (text[0] == '+' || text[0] == '-') ? 1 : 0;
            var hasDigit = false;
            var hasDot = false;
            for (; position < text.Length; position++)
            {
                var character = text[position];
                if (character >= '0' && character <= '9')
                    hasDigit = true;
                else if (character == '.' && !hasDot)
                    hasDot = true;
                else
                    return false;
            }
            return hasDigit;
        }

        private static string Negate(string mantissa)
        {
            if (mantissa[0] == '-')
                return mantissa.Substring(1);
            if (mantissa[0] == '+')
                return "-" + mantissa.Substring(1);
            return "-" + mantissa;
        }
    }
}
