using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public static class Groups
    {
        public static readonly string PowerCatchingGroup =
            $@"(\s*[{SpecialSymbols.SuperscriptsWithoutSpace}][{SpecialSymbols.Superscripts}]*)((?:(?:[^{
                SpecialSymbols.Superscripts}\(])|$))";

        public const string Identifier = @"[α-ωΑ-Ωa-zA-Z_][α-ωΑ-Ωa-z0-9A-Z_]*";


        public const string IdentifierOrNumber = @"[α-ωΑ-Ωa-z0-9A-Z_]+";

        private const string LegalFirstIdentifier = @"α-ωΑ-Ωa-zA-Z_";
    }
}