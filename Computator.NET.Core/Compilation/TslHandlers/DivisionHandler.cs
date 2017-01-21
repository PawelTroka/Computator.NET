using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation
{
    public class DivisionHandler : ITslHandler
    {
        public string Replace(string input)
        {
            var ret = DivisionByIntegerRegex.Replace(input, @"/$1.0$2");

            ret = DivisionByParenthesisRegex.Replace(ret, @"/(1.0*($1))");

            return ret;
        }
        private static readonly Regex DivisionByIntegerRegex = new Regex(@"[\/]\s*(\d+)([^\.\d]|$)", RegexOptions.Compiled);

        private static readonly Regex DivisionByParenthesisRegex =
            new Regex(@"[\/]\s*(\((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!))\))", RegexOptions.Compiled);
    }
}