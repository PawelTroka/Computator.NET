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
        private static readonly Regex ChangeBackEngineeringNotationRegex =
            new Regex(
                @"{{ENGINERING#NOTATION}(\d+\.?\d*)#([Ee][+-]?\d+){ENGINERING#NOTATION}([^\dα-ωΑ-Ωa-zA-Z_.]*){ENGINERING#NOTATION}}",
                RegexOptions.Compiled);

        private static readonly Regex EngineeringNotationRegex =
            new Regex(@"(\d+\.?\d*)([Ee][+-]?\d+)([^\dα-ωΑ-Ωa-zA-Z_.]|$)", RegexOptions.Compiled);


        public string Replace(string input) //OK
        {
            var result = EngineeringNotationRegex.Replace(input,
                @"{{ENGINERING#NOTATION}$1#$2{ENGINERING#NOTATION}$3{ENGINERING#NOTATION}}");

            result = MultiplyingRegexes.Aggregate(result,
                (current, multiplyingRegex) => multiplyingRegex.Replace(current, "$1$2*$3"));

            result = ChangeBackEngineeringNotationRegex.Replace(result, @"$1$2$3");
            return result;
        }



        private static readonly Regex[] MultiplyingRegexes =
        {
            //example: 2x or 2(cos(x)+1)
            new Regex(
                $@"((?:[^α-ωΑ-Ωa-zA-Z_\d\.][^α-ωΑ-Ωa-z0-9A-Z_]*)|^)(\d+\.?\d*)(\(|(?:{Groups.Identifier}))",
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
