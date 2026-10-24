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

        private const string LegalFirstIdentifier = @"α-ωΑ-Ωa-zA-Z_";
    }
}