using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public class AbsHandler : ITslHandler
    {
        private static readonly string AbsSuperscriptPattern =
            $@"{SpecialSymbols.ModulusSuperscript}([^{SpecialSymbols.ModulusSuperscript}]*[^{SpecialSymbols.ModulusSuperscript}{SpecialSymbols.OperatorsSuperscript}\s]+\s*){SpecialSymbols.ModulusSuperscript}(\s*)($|(?:[^{SpecialSymbols.SuperscriptAlphabet}\s]+?))";

        private static readonly string AbsPattern =
            $@"\|([^\|]*[^\|\+\-\*{SpecialSymbols.DotSymbol}\=\s]+\s*)\|(\s*)($|(?:[^α-ωΑ-Ωa-z0-9A-Z_\s]+?))";

        private static readonly Regex AbsSuperscriptWithMultiplyingRegex = new Regex(
            $@"([{SpecialSymbols.SuperscriptAlphabet}]+)\s*{AbsSuperscriptPattern}", RegexOptions.Compiled | RegexOptions.RightToLeft);

        private static readonly Regex AbsWithMultiplyingRegex = new Regex($@"({Groups.IdentifierOrNumber})\s*{AbsPattern}", RegexOptions.Compiled|RegexOptions.RightToLeft);

        private static readonly Regex AbsSuperscriptRegex = new Regex(AbsSuperscriptPattern, RegexOptions.Compiled);

        private static readonly Regex AbsRegex = new Regex(AbsPattern, RegexOptions.Compiled);


        public string Replace(string code)
        {
            var result = code;

            while (AbsSuperscriptRegex.IsMatch(result))
            {
                result = AbsSuperscriptRegex.Replace(result, match =>
                {
                    var match2 = AbsSuperscriptWithMultiplyingRegex.Match(result, 0, match.Index + match.Length);
                    if (match2.Success &&
                        match2.Index < match.Index &&
                        match2.Index + match2.Length - 1 == match.Index + match.Length - 1)
                        return match.Result($@"{SpecialSymbols.DotSymbolSuperscript}⁽ᵃᵇˢ⁽$1⁾⁾$2$3");
                    return match.Result(@"⁽ᵃᵇˢ⁽$1⁾⁾$2$3");
                });
            }

            while (AbsRegex.IsMatch(result))
            {
                result = AbsRegex.Replace(result, match =>
                {
                    var match2 = AbsWithMultiplyingRegex.Match(result, 0, match.Index+match.Length);
                    if (match2.Success &&
                        match2.Index < match.Index &&
                        match2.Index + match2.Length - 1 == match.Index + match.Length - 1)
                        return match.Result("*(abs($1))$2$3");
                    return match.Result("(abs($1))$2$3");
                });
            }

            return result;
        }
    }
}