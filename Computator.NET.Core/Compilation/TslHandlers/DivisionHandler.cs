using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class DivisionHandler : ITslHandler
    {
        public string Replace(string input)
        {
            var ret = DivisionByIntegerRegex.Replace(input, @"/$1$2.0$3");

            ret = DivisionByParenthesisRegex.Replace(ret, @"/$1(1.0*($2))");

            return ret;
        }
        // A sign does not change the numerical division rule. Keep it outside
        // the promoted operand, and distinguish unary signs from ++ and --,
        // whose operand must remain assignable and execute exactly once.
        private const string UnarySigns = @"(?:(?:\+(?!\+)|-(?!-))\s*)*";

        private static readonly Regex DivisionByIntegerRegex =
            new Regex(@"[\/]\s*(" + UnarySigns + @")(" + Groups.DecimalDigits + @")([^\.\d_]|$)", RegexOptions.Compiled);

        private static readonly Regex DivisionByParenthesisRegex =
            new Regex(@"[\/]\s*(" + UnarySigns + @")(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))", RegexOptions.Compiled);
    }
}
