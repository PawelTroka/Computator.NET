#if __MonoCS__
using System;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp;

namespace Computator.NET.Core.Compilation
{
    // Mono 6.x's BCL (and its UTF8 parser) can round decimal input one ULP
    // away from the nearest binary64 value. Keep its invariant input grammar
    // and exceptions, then use the already-shipped compiler's decimal parser.
    internal static class MonoDoubleParser
    {
        internal static bool TryParse(string text, out double value)
        {
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return false;
            return TryRound(text, ref value);
        }

        internal static double Parse(string text)
        {
            var value = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
            if (!TryRound(text, ref value)) throw new FormatException("The input is not a supported invariant floating-point number.");
            return value;
        }

        private static bool TryRound(string text, ref double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return true;
            // NumberStyles.Float accepts signs, surrounding whitespace, and a
            // trailing decimal point. The BCL also permits terminating NULs.
            var number = text.TrimEnd('\0').Trim();
            var negative = number[0] == '-';
            if (negative || number[0] == '+') number = number.Substring(1);
            var exponent = number.IndexOfAny(new[] { 'e', 'E' });
            var mantissaEnd = exponent < 0 ? number.Length : exponent;
            if (number[mantissaEnd - 1] == '.') number = number.Insert(mantissaEnd, "0");
            var literal = number + "d";
            var token = SyntaxFactory.ParseToken(literal);
            // ParseToken may return a valid prefix (e.g. 1 from 1.d). Require
            // one complete double token rather than accepting any suffix.
            if (token.ContainsDiagnostics || token.Text != literal || !(token.Value is double)) return false;
            var rounded = (double)token.Value;
            value = negative ? -rounded : rounded; // Retain the sign of zero.
            return true;
        }
    }
}
#endif
