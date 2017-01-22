using System.Text.RegularExpressions;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public class PowHandler : ITslHandler
    {
        private static readonly Regex ExpressionInParenthesesRaisedToAnyPowerRegex =
            new Regex(@"(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))" + Groups.PowerCatchingGroup,
                RegexOptions.Compiled);

        private static readonly Regex NumberRaisedToAnyPowerRegex = new Regex(
            $@"(\d+\.?\d*){Groups.PowerCatchingGroup}",
            RegexOptions.Compiled);

        private static readonly Regex VariableRaisedToAnyPowerRegex =
            new Regex(
                $@"({Groups.Identifier}){Groups.PowerCatchingGroup}", RegexOptions.Compiled);


        private const string NativeCompilerCompatiblePowerNotation = @"pow($1,$2)$3";

        public string Replace(string input) //OK
        {
            var result = input;

            while (ExpressionInParenthesesRaisedToAnyPowerRegex.IsMatch(result))
                result = ExpressionInParenthesesRaisedToAnyPowerRegex.Replace(result,
                    //http://stackoverflow.com/questions/7898310/using-regex-to-balance-match-parenthesis
                    ReplaceMatchWithPow);

            result = NumberRaisedToAnyPowerRegex.Replace(result, ReplaceMatchWithPow);

            result = VariableRaisedToAnyPowerRegex.Replace(result, ReplaceMatchWithPow);

            //result = SpecialSymbols.SuperscriptsToAscii(result);

            return result;
        }

        private static string ReplaceMatchWithPow(Match match)
        {
            return match.Result(NativeCompilerCompatiblePowerNotation.Replace("$2",
                SpecialSymbols.SuperscriptsToAscii(match.Groups[2].Value)));
        }
    }
}