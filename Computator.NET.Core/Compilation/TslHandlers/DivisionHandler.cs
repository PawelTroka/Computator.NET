using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class DivisionHandler : ITslHandler
    {
        public string Replace(string input)
        {
            var ret = DivisionByIntegerRegex.Replace(input, @"/$1$2.0$3");

            ret = PromoteParenthesizedDivisors(ret);

            return ret;
        }

        private static string PromoteParenthesizedDivisors(string source)
        {
            var candidates = new HashSet<int>();
            foreach (Match match in DivisionByParenthesisStartRegex.Matches(source))
                candidates.Add(match.Index + match.Length - 1);
            if (candidates.Count == 0) return source;

            // A balanced-regex replacement consumes the complete outer divisor,
            // hiding any divisions inside it. Match all parentheses once and
            // insert around original spans, including nested and sibling ones.
            // Keep this iterative: accepted source can contain deep nesting.
            var openings = new Stack<int>();
            var starts = new HashSet<int>();
            var ends = new HashSet<int>();
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index] == '(') openings.Push(index);
                else if (source[index] == ')' && openings.Count != 0)
                {
                    var opening = openings.Pop();
                    if (candidates.Contains(opening) && index > opening + 1)
                    {
                        starts.Add(opening);
                        ends.Add(index);
                    }
                }
            }
            if (starts.Count == 0) return source;

            var result = new StringBuilder(source.Length);
            for (var index = 0; index < source.Length; index++)
            {
                if (starts.Contains(index)) result.Append("(1.0*(");
                result.Append(source[index]);
                if (ends.Contains(index)) result.Append("))");
            }
            return result.ToString();
        }
        // A sign does not change the numerical division rule. Keep it outside
        // the promoted operand, and distinguish unary signs from ++ and --,
        // whose operand must remain assignable and execute exactly once.
        private const string UnarySigns = @"(?:(?:\+(?!\+)|-(?!-))\s*)*";

        private static readonly Regex DivisionByIntegerRegex =
            new Regex(@"[\/]\s*(" + UnarySigns + @")(" + Groups.DecimalDigits + @")([^\.\d_]|$)", RegexOptions.Compiled);

        private static readonly Regex DivisionByParenthesisStartRegex =
            new Regex(@"[\/]\s*" + UnarySigns + @"\(", RegexOptions.Compiled);
    }
}
