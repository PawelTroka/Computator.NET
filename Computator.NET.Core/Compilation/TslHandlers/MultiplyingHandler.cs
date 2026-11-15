using System.Linq;
using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class MultiplyingHandler : ITslHandler
    {
        // A callable may still contain superscript letters until the complete
        // compilation resolves its name. Keep the trailing multiplication in
        // sqrtʸ(x)x intact before that semantic pass runs.
        private const string CallableIdentifier = @"@?[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";
        public string Replace(string input) //OK
        {
            return MultiplyingRegexes.Aggregate(input,
                (current, multiplyingRegex) => multiplyingRegex.Replace(current, match =>
                    // A leading (real)x is a cast, while f(real)x is still
                    // multiplication after a call with an argument named real.
                    // The unescaped return keyword may be adjacent to a cast.
                    (match.Groups[1].Length == 0 || (match.Groups[1].Value == "return"
                        && (match.Index == 0 || current[match.Index - 1] != '@'))) && Groups.IsScalarCast(match.Groups[2].Value,
                        current, match.Groups[2].Index + match.Groups[2].Length)
                        ? match.Value : match.Result("$1$2*$3")));
        }



        private static readonly Regex[] MultiplyingRegexes =
        {
            //example: 2x or 2(cos(x)+1)
            new Regex(
                $@"((?:[^α-ωΑ-Ωa-zA-Z_\d\.][^α-ωΑ-Ωa-z0-9A-Z_]*)|^)({Groups.DecimalLiteral})(\(|(?:{Groups.Identifier}))",
                RegexOptions.Compiled),

            //example: 2²x
            new Regex(
                $@"{Groups.PowerCatchingGroup}()\s*({Groups.Identifier})",
                RegexOptions.Compiled),

            //example: (2+cos(x))x
            // Statement headers are not calls: lock/fixed may precede an
            // unbraced body, and a catch declaration may precede a when filter.
            // An @-escaped keyword still names an ordinary callable.
            new Regex($@"((?:(?<=@)|(?!(?:for|foreach|while|using|if|lock|fixed|catch)\b))\b{CallableIdentifier}|^)(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))\s*(\(|{Groups.IdentifierOrNumber})", RegexOptions.Compiled|RegexOptions.Multiline)
        };
        


    }
}
