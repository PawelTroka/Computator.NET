using System.Linq;
using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation
{
    public class AbsHandler : ITslHandler
    {
        private static readonly Regex absRegex =
            new Regex(@"\|((?:[^|]|(?<open>\|)|(?<-open>\|))+(?(open)(?!)))\|",
                RegexOptions.Compiled);

        public string Replace(string code)
        {
            var result = code;

            while (result.Count(c => c == '|') > 0 && result.Count(c => c == '|') % 2 == 0)
                result = absRegex.Replace(result, @"abs($1)");
            return result;
        }
    }
}