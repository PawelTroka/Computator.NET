using System.Linq;
using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation
{
    public class MultiplyingHandler : ITslHandler
    {
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
            new Regex($@"((?:(?!for\b|foreach\b|while\b|using\b|if\b)\b{Groups.Identifier})|^)(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))\s*(\(|{Groups.IdentifierOrNumber})", RegexOptions.Compiled|RegexOptions.Multiline)
        };
        


    }
}