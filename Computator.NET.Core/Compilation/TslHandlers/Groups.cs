using System.Text.RegularExpressions;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public static class Groups
    {
        public static readonly string PowerCatchingGroup =
            $@"(\s*[{SpecialSymbols.SuperscriptsWithoutSpace}][{SpecialSymbols.Superscripts}]*)((?:(?:[^{
                SpecialSymbols.Superscripts}\(])|$))";

        public const string Identifier = @"[α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*";


        public const string IdentifierOrNumber = @"[α-ωΑ-Ωa-z0-9A-Z_]+";

        // Underscores between digits belong to the numeric literal. The atomic
        // decimal group prevents implicit multiplication from backtracking into
        // an exponent or treating a digit separator as an identifier prefix.
        public const string DecimalDigits = @"[0-9](?:[0-9_]*[0-9])?";
        // Preserve TSL's existing Euler/identifier convention: 2e-10e means
        // 2*e - 10*e, and 1e3x means 1*e3x. Only a complete exponent followed by
        // a delimiter is scientific notation; use (1e3)x for that product.
        private const string DecimalExponent = @"(?:(?>[Ee][+-]?" + DecimalDigits + @")(?![\dα-ωΑ-Ωa-zA-Z_.]))?";
        public const string DecimalLiteral = @"(?>" + DecimalDigits + @"(?:\." + DecimalDigits + @")?" + DecimalExponent + @"|\." + DecimalDigits + DecimalExponent + @")";

        private static readonly Regex ScalarCastRegex = new Regex(
            @"\A\(\s*(?:(?<keyword>bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|decimal|char|string|object)|real|complex|integer|natural|nint|nuint)\s*\)\z",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static bool IsScalarCast(string parentheses, string source, int operandStart)
        {
            var type = ScalarCastRegex.Match(parentheses);
            if (!type.Success) return false;
            while (operandStart < source.Length && char.IsWhiteSpace(source[operandStart])) operandStart++;
            if (operandStart == source.Length) return false;
            // Prefix negation needs an operand. A trailing null-forgiving !
            // belongs to a grouped value, as in 1/(real)! or 1/(real)!+1.
            while (source[operandStart] == '!')
            {
                operandStart++;
                while (operandStart < source.Length && char.IsWhiteSpace(source[operandStart])) operandStart++;
                if (operandStart == source.Length) return false;
            }
            var next = source[operandStart];
            if (char.IsLetter(next))
            {
                var end = operandStart + 1;
                while (end < source.Length && (char.IsLetterOrDigit(source[end]) || source[end] == '_')) end++;
                var token = source.Substring(operandStart, end - operandStart);
                if (token == "is" || token == "as" || token == "switch") return false;
            }
            // A type must be followed by a cast operand. A grouped variable
            // named real in 1/(real) or 1/(real)-x still uses ordinary division.
            // C# also treats +/- after an alias differently from a keyword type.
            return char.IsLetterOrDigit(next) || next == '_' || next == '@' || next == '(' || next == '~'
                || (next == '.' && operandStart + 1 < source.Length && char.IsDigit(source[operandStart + 1]))
                || ((next == '+' || next == '-') && type.Groups["keyword"].Success);
        }

        private const string LegalFirstIdentifier = @"α-ωΑ-Ωa-zA-Z_";
    }
}
