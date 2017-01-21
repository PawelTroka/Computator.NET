using System.Text.RegularExpressions;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Compilation
{
    public class PowHandler : ITslHandler
    {
        private static readonly Regex ExpressionInParenthesesRaisedToAnyPowerRegex =
            new Regex(@"(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))" + Groups.PowerCatchingGroup,
                RegexOptions.Compiled);

        private static readonly Regex NumberRaisedToAnyPowerRegex = new Regex($@"(\d+\.?\d*){Groups.PowerCatchingGroup}",
            RegexOptions.Compiled);

        private static readonly Regex VariableRaisedToAnyPowerRegex =
            new Regex(
                $@"({Groups.Identifier}){Groups.PowerCatchingGroup}", RegexOptions.Compiled);


        public string Replace(string input) //OK
        {
            var nativeCompilerCompatiblePowerNotation = @"pow($1,$2)$3";

            var result = input;

            while (ExpressionInParenthesesRaisedToAnyPowerRegex.IsMatch(result))
                result = ExpressionInParenthesesRaisedToAnyPowerRegex.Replace(result,
                    //http://stackoverflow.com/questions/7898310/using-regex-to-balance-match-parenthesis
                    nativeCompilerCompatiblePowerNotation);

            result = NumberRaisedToAnyPowerRegex.Replace(result, nativeCompilerCompatiblePowerNotation);

            result = VariableRaisedToAnyPowerRegex.Replace(result, nativeCompilerCompatiblePowerNotation);

            result = SpecialSymbols.SuperscriptsToAscii(result);

            return result;
        }
    }
}