using System.Text.RegularExpressions;
using Computator.NET.DataTypes.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class PowHandler : ITslHandler
    {
        // Superscript letters may be legal parts of existing function names
        // (notably ψⁿ). Only numeric literals before arguments are power shorthand;
        // symbolic exponents remain unambiguous after the closing parenthesis.
        private const string Identifier = @"@?[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";
        private const string QualifiedIdentifier = Identifier + @"(?:\s*\.\s*" + Identifier + @")*";
        private const string Parentheses = @"\((?:[^()]|(?<open>\()|(?<-open>\)))*(?(open)(?!))\)";
        private const string SuperscriptDigits = "⁰¹²³⁴⁵⁶⁷⁸⁹";

        private static readonly Regex FunctionWithPrefixPowerRegex = new Regex(
            @"(?<function>" + QualifiedIdentifier + @")\s*(?<power>[⁺⁻]?[" + SuperscriptDigits + @"]+(?:" +
            Regex.Escape(SpecialSymbols.DecimalSeparatorSuperscript) + "[" + SuperscriptDigits + @"]+)?)\s*(?<arguments>" + Parentheses + ")",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Capture a named call together with its arguments. Matching just (x)
        // in cos(x)² would incorrectly produce cospow((x),2).
        private static readonly Regex ExpressionInParenthesesRaisedToAnyPowerRegex = new Regex(
            @"(?:(?<function>" + QualifiedIdentifier + @")(?<gap>\s*))?(" + Parentheses + ")" + Groups.PowerCatchingGroup,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex NumberRaisedToAnyPowerRegex = new Regex(
            $@"(\d+\.?\d*){Groups.PowerCatchingGroup}", RegexOptions.Compiled);

        private static readonly Regex VariableRaisedToAnyPowerRegex = new Regex(
            $@"({Groups.Identifier}){Groups.PowerCatchingGroup}", RegexOptions.Compiled);

        public string Replace(string input)
        {
            var result = input;
            string previous;
            do
            {
                previous = result;
                result = FunctionWithPrefixPowerRegex.Replace(result, ReplacePrefixPower);
                result = ExpressionInParenthesesRaisedToAnyPowerRegex.Replace(result, ReplaceParenthesizedPower);
            } while (result != previous);

            result = NumberRaisedToAnyPowerRegex.Replace(result, ReplaceMatchWithPow);
            return VariableRaisedToAnyPowerRegex.Replace(result, ReplaceMatchWithPow);
        }

        private static string ReplacePrefixPower(Match match)
        {
            var function = match.Groups["function"].Value;
            if (SyntaxFacts.GetKeywordKind(function) != SyntaxKind.None) return match.Value;
            return "pow(" + function + match.Groups["arguments"].Value + "," +
                SpecialSymbols.SuperscriptsToAscii(match.Groups["power"].Value) + ")";
        }

        private static string ReplaceParenthesizedPower(Match match)
        {
            var function = match.Groups["function"].Value;
            var gap = match.Groups["gap"].Value;
            var expression = match.Groups[1].Value;
            var prefix = "";
            // return (x)² is a statement containing a power, not a call to return.
            // An originally adjacent '(' also separated the keyword from its
            // operand; preserve that separation when '(' becomes the word pow.
            if (SyntaxFacts.GetKeywordKind(function) != SyntaxKind.None)
                prefix = function + (gap.Length == 0 ? " " : gap);
            else
                expression = function + gap + expression;
            return prefix + "pow(" + expression + "," +
                SpecialSymbols.SuperscriptsToAscii(match.Groups[2].Value) + ")" + match.Groups[3].Value;
        }

        private static string ReplaceMatchWithPow(Match match)
        {
            return "pow(" + match.Groups[1].Value + "," +
                SpecialSymbols.SuperscriptsToAscii(match.Groups[2].Value) + ")" + match.Groups[3].Value;
        }
    }
}
