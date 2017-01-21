using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public class AbsHandler : ITslHandler
    {

        private static readonly Regex absSuperscriptRegex =
    new Regex($@"{SpecialSymbols.ModulusSuperscript}([^{SpecialSymbols.ModulusSuperscript}]*[^\|{SpecialSymbols.OperatorsSuperscript}\s]+\s*){SpecialSymbols.ModulusSuperscript}(\s*)($|(?:[^{SpecialSymbols.SuperscriptAlphabet}\s]+?))",
        RegexOptions.Compiled);

        private static readonly Regex absRegex =
            new Regex($@"\|([^\|]*[^\|\+\-\*{SpecialSymbols.DotSymbol}\=\s]+\s*)\|(\s*)($|(?:[^α-ωΑ-Ωa-z0-9A-Z_\s]+?))",
                RegexOptions.Compiled);

        public string Replace(string code)
        {
            var result = code;

            while (absSuperscriptRegex.IsMatch(result))
                result = absSuperscriptRegex.Replace(result, @"ᵃᵇˢ⁽$1⁾$2$3");

            while (absRegex.IsMatch(result))
                result = absRegex.Replace(result, @"abs($1)$2$3");

            /*
            while (absRegex.IsMatch(result))
            {
                result = absRegex.Replace(result, match =>
                {
                    var ret = match.Result("abs($1)$2$3") + (match.Value.EndsWith("||") ? "|" : string.Empty);//hack for |x||y| case which should be multiplying
                    return ret;
                });
            }*/


            return result;
        }
    }
}