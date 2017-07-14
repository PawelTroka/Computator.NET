using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class RefOutHandler : ITslHandler
    {
        private static readonly Regex readOutRegex = new Regex(@"(read\s*\(\s*)&", RegexOptions.Compiled);

        private static readonly Regex refRegex = new Regex(@"([\(,\s])(&)([α-ωΑ-Ωa-zA-Z_])", RegexOptions.Compiled);
        public string Replace(string code)
        {
            var result = readOutRegex.Replace(code, "$1 out ");
            result = refRegex.Replace(result, @"$1 ref $3");
            return result;
        }
    }
}