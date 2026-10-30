using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class PowHandler : ITslHandler
    {
        // Syntax-only previews reserve bare symbolic prefixes for conventional elementary
        // function spellings below: cosᶻ(z) means pow(cos(z),z). Other identifiers
        // (notably ψⁿ, Probeᶻ and qualified names) keep their existing meaning.
        // Named/qualified identifiers accept a grouped prefix: Probe⁽ʸ⁾(x).
        // An intentional custom name overlapping this shorthand can be escaped
        // as @cosᶻ; declarations in the same TSL fragment are protected as well.
        // Evaluation and automatic routing defer bare names to FunctionPowerResolver,
        // which also supports custom and qualified calls with their actual bindings.
        private static readonly HashSet<string> SymbolicPowerFunctions = new HashSet<string>(StringComparer.Ordinal)
        {
            "sinc", "sin", "cos", "tan", "tg", "ctg", "cot", "sec", "csc", "cosec",
            "sinh", "cosh", "tanh", "tgh", "coth", "ctgh", "sech", "csch", "cosech",
            "arcsin", "arccos", "arctan", "arctg", "arccot", "arcctg", "arcsec", "arccsc", "arccosec",
            "arsinh", "arcosh", "artanh", "artgh", "arcoth", "arctgh", "arcsch", "arcosech", "arsech",
            "ln", "log", "log2", "log10", "sqrt", "root", "pow", "exp"
        };
        private const string Identifier = @"@?[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*";
        private const string QualifiedIdentifier = Identifier + @"(?:\s*\.\s*" + Identifier + @")*";
        private const string Parentheses = @"\((?:[^()]|(?<open>\()|(?<-open>\)))*(?(open)(?!))\)";
        private const string SuperscriptDigits = "⁰¹²³⁴⁵⁶⁷⁸⁹";
        private static readonly char[] SuperscriptCharacters = SpecialSymbols.SuperscriptsWithoutSpace.ToCharArray();
        private static readonly Regex BareCallableNameRegex = new Regex(
            @"\A" + QualifiedIdentifier + @"\s*\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex NumericPowerRegex = new Regex(
            @"\A[⁺⁻]?[" + SuperscriptDigits + @"]+(?:" + Regex.Escape(SpecialSymbols.DecimalSeparatorSuperscript) +
            "[" + SuperscriptDigits + @"]+)?\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex GroupedPowerRegex = new Regex(
            @"\A⁽(?:[^⁽⁾]|(?<openPower>⁽)|(?<-openPower>⁾))*(?(openPower)(?!))⁾\z",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex FunctionWithPrefixPowerRegex = new Regex(
            @"(?<function>" + QualifiedIdentifier + @")\s*(?<power>[" + SpecialSymbols.SuperscriptsWithoutSpace +
            "][" + SpecialSymbols.Superscripts + @"]*)\s*(?<arguments>" + Parentheses + ")",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Capture a named call together with its arguments. Matching just (x)
        // in cos(x)² would incorrectly produce cospow((x),2).
        private static readonly Regex ExpressionInParenthesesRaisedToAnyPowerRegex = new Regex(
            @"(?:(?<function>" + QualifiedIdentifier + @")(?<gap>\s*))?(" + Parentheses + ")" + Groups.PowerCatchingGroup,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Reuse the coefficient grammar so scientific notation, digit separators
        // and leading decimal points form one power operand. Never start inside
        // an identifier: the digits in x1² belong to x1, not a separate number.
        private static readonly Regex NumberRaisedToAnyPowerRegex = new Regex(
            $@"(?<![\p{{L}}\p{{Nl}}\p{{Mn}}\p{{Mc}}\p{{Nd}}\p{{Pc}}\p{{Cf}}.])({Groups.DecimalLiteral}){Groups.PowerCatchingGroup}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex VariableRaisedToAnyPowerRegex = new Regex(
            $@"({Groups.Identifier}){Groups.PowerCatchingGroup}", RegexOptions.Compiled);

        private static readonly Regex PostfixPowerRegex = new Regex(
            Groups.PowerCatchingGroup, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex MemberAccessRegex = new Regex(
            @"\.\s*" + Identifier, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string Replace(string input) => Replace(input, false);

        internal string Replace(string input, bool deferSymbolicFunctionPowers)
        {
            var result = input;
            string previous;
            do
            {
                previous = result;
                result = ReplacePrefixPowers(result, deferSymbolicFunctionPowers);
                result = ReplaceAccessPowers(result);
                result = ExpressionInParenthesesRaisedToAnyPowerRegex.Replace(result, ReplaceParenthesizedPower);
            } while (result != previous);

            result = NumberRaisedToAnyPowerRegex.Replace(result, ReplaceMatchWithPow);
            return VariableRaisedToAnyPowerRegex.Replace(result, ReplaceMatchWithPow);
        }

        private static string ReplaceAccessPowers(string source)
        {
            if (source.IndexOf('[') < 0 && (source.IndexOf('.') < 0 || !MemberAccessRegex.IsMatch(source)))
                return source;
            var powers = PostfixPowerRegex.Matches(source).Cast<Match>().ToArray();
            if (powers.Length == 0) return source;

            // Ask the existing C# parser where complete member/index operands
            // begin. Mask only superscripts, keeping positions and line breaks;
            // the original operand is copied verbatim into the pow call below.
            // A trailing identifier regex would turn z.Real² into z.pow(Real,2)
            // and cannot capture a matrix entry or a receiver such as Next().
            var syntax = source.ToCharArray();
            var operandEnds = new HashSet<int>();
            foreach (var power in powers)
            {
                var end = power.Groups[1].Index;
                while (end > 0 && char.IsWhiteSpace(source[end - 1])) end--;
                operandEnds.Add(end);
                for (var index = power.Groups[1].Index; index < power.Groups[1].Index + power.Groups[1].Length; index++)
                    if (!char.IsWhiteSpace(syntax[index])) syntax[index] = ' ';
            }
            var operands = CSharpSyntaxTree.ParseText(new string(syntax),
                    new CSharpParseOptions(LanguageVersion.CSharp14)).GetRoot()
                .DescendantNodes().OfType<ExpressionSyntax>()
                .Where(expression => operandEnds.Contains(expression.Span.End))
                .Where(expression => expression is MemberAccessExpressionSyntax or ElementAccessExpressionSyntax
                    || (expression is InvocationExpressionSyntax call
                        && call.Expression is MemberAccessExpressionSyntax or ElementAccessExpressionSyntax))
                .GroupBy(expression => expression.Span.End)
                .Select(group => group.OrderBy(expression => expression.Span.Start).First())
                .Where(expression => !expression.DescendantTokens().Any(token => token.IsMissing))
                .ToDictionary(expression => expression.Span.End);

            var changes = new List<TextChange>();
            var previousEnd = 0;
            foreach (var power in powers)
            {
                var exponent = power.Groups[1];
                var end = exponent.Index;
                while (end > 0 && char.IsWhiteSpace(source[end - 1])) end--;
                if (!operands.TryGetValue(end, out var operand) || operand.Span.Start < previousEnd) continue;
                var span = TextSpan.FromBounds(operand.Span.Start, exponent.Index + exponent.Length);
                changes.Add(new TextChange(span, "pow(" + source.Substring(operand.Span.Start, operand.Span.Length) +
                    "," + SpecialSymbols.SuperscriptsToAscii(exponent.Value) + ")"));
                previousEnd = span.End;
            }
            // Nested accesses are rewritten inside out by the existing outer
            // loop. Each operand and exponent appears once, preserving effects.
            return changes.Count == 0 ? source : SourceText.From(source).WithChanges(changes).ToString();
        }

        private static string ReplacePrefixPowers(string source, bool deferSymbolicFunctionPowers)
        {
            var output = new StringBuilder();
            var copied = 0;
            var next = 0;
            while (true)
            {
                var match = FunctionWithPrefixPowerRegex.Match(source, next);
                if (!match.Success) break;
                var replacement = ReplacePrefixPower(source, match, deferSymbolicFunctionPowers);
                if (replacement == null)
                {
                    // Preserve the outer name but keep scanning its arguments:
                    // ψⁿ(...) must not hide an inner cosʸ(x). This is iterative so
                    // nested preserved calls do not recurse on the managed stack.
                    next = match.Groups["arguments"].Index + 1;
                    continue;
                }
                output.Append(source, copied, match.Index - copied).Append(replacement);
                copied = next = match.Index + match.Length;
            }
            return copied == 0 ? source : output.Append(source, copied, source.Length - copied).ToString();
        }

        private static string? ReplacePrefixPower(string source, Match match, bool deferSymbolicFunctionPowers)
        {
            var function = match.Groups["function"].Value;
            var power = match.Groups["power"].Value.Trim();
            if (SyntaxFacts.GetKeywordKind(function) != SyntaxKind.None) return null;
            if (deferSymbolicFunctionPowers && BareCallableNameRegex.IsMatch(
                source.Substring(match.Index, match.Groups["arguments"].Index - match.Index)))
                return null;

            // Superscript letters are identifier characters, so the greedy name
            // match may include part of the exponent (cosᶜᵒˢ⁽ᶻ⁾, for example).
            // Split them off only when the remaining unqualified name is one of
            // the reserved spellings, never from a custom or qualified callable.
            var firstSuperscript = function.IndexOfAny(SuperscriptCharacters);
            var name = firstSuperscript < 0 ? function : function.Substring(0, firstSuperscript);
            if (SymbolicPowerFunctions.Contains(name) && !FollowsMemberAccess(source, match.Index))
            {
                power = function.Substring(name.Length) + power;
                function = name;
            }
            else if (!NumericPowerRegex.IsMatch(power) && !GroupedPowerRegex.IsMatch(power))
                return null;

            if (deferSymbolicFunctionPowers)
            {
                // An explicit prefix already chose its callable boundary. Keep
                // that exact name during later binding, including protected names
                // restored by TslCompiler. This prevents Probeʸ⁺¹(x) becoming
                // (Probe(x)^y)^1 when no callable Probeʸ actually exists.
                var terminalStart = function.LastIndexOf('.') + 1;
                while (char.IsWhiteSpace(function[terminalStart])) terminalStart++;
                if (function[terminalStart] != '@') function = function.Insert(terminalStart, "@");
            }
            return "pow(" + function + match.Groups["arguments"].Value + "," +
                SpecialSymbols.SuperscriptsToAscii(power) + ")";
        }

        internal static bool IsSymbolicPowerIdentifier(string name)
        {
            var firstSuperscript = name.IndexOfAny(SuperscriptCharacters);
            return firstSuperscript > 0 && SymbolicPowerFunctions.Contains(name.Substring(0, firstSuperscript));
        }

        private static bool FollowsMemberAccess(string source, int index)
        {
            while (index > 0 && char.IsWhiteSpace(source[index - 1])) index--;
            return index > 0 && (source[index - 1] == '.' || source[index - 1] == ':');
        }

        private static string ReplaceParenthesizedPower(Match match)
        {
            var function = match.Groups["function"].Value;
            var gap = match.Groups["gap"].Value;
            var expression = match.Groups[1].Value;
            var prefix = "";
            // return (x)² is a statement containing a power, not a call to return.
            // An originally adjacent '(' also separated the keyword from its
            // operand; preserve that separation when '(' becomes the word pow.
            if (SyntaxFacts.GetKeywordKind(function) != SyntaxKind.None)
                prefix = function + (gap.Length == 0 ? " " : gap);
            else
                expression = function + gap + expression;
            return prefix + "pow(" + expression + "," +
                SpecialSymbols.SuperscriptsToAscii(match.Groups[2].Value) + ")" + match.Groups[3].Value;
        }

        private static string ReplaceMatchWithPow(Match match)
        {
            return "pow(" + match.Groups[1].Value + "," +
                SpecialSymbols.SuperscriptsToAscii(match.Groups[2].Value) + ")" + match.Groups[3].Value;
        }
    }
}
