using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class RefOutHandler : ITslHandler
    {
        // Only the complete read identifier uses out; a suffix in spread or
        // a Unicode identifier still follows the ordinary ref convention.
        private static readonly Regex readOutRegex = new Regex(
            @"(?<![\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}])(read\s*\(\s*)&", RegexOptions.Compiled);

        // Reference shorthand starts an argument, possibly a named one. A
        // whitespace-only prefix also matches binary expressions such as a &b
        // and would incorrectly turn the AND operator into a ref modifier.
        private static readonly Regex refRegex = new Regex(@"([\(,:]\s*)(&)([α-ωΑ-Ωa-zA-Z_])", RegexOptions.Compiled);
        public string Replace(string code)
        {
            var result = readOutRegex.Replace(code, "$1 out ");
            result = refRegex.Replace(result, @"$1 ref $3");
            return result;
        }
    }
}
