using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Computator.NET.DataTypes.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    public class AbsHandler : ITslHandler
    {
        private static readonly string AbsSuperscriptPattern =
            $@"{SpecialSymbols.ModulusSuperscript}([^{SpecialSymbols.ModulusSuperscript}]*[^{SpecialSymbols.ModulusSuperscript}{SpecialSymbols.OperatorsSuperscript}\s]+\s*){SpecialSymbols.ModulusSuperscript}(\s*)($|(?:[^{SpecialSymbols.SuperscriptAlphabet}\s]+?))";

        private static readonly string AbsPattern =
            $@"\|([^\|]*[^\|\+\-\*{SpecialSymbols.DotSymbol}\=\s]+\s*)\|(\s*)($|(?:[^α-ωΑ-Ωa-z0-9A-Z_\s]+?))";

        private static readonly Regex AbsSuperscriptWithMultiplyingRegex = new Regex(
            $@"([{SpecialSymbols.SuperscriptAlphabet}]+)\s*{AbsSuperscriptPattern}", RegexOptions.Compiled | RegexOptions.RightToLeft);

        private static readonly Regex AbsWithMultiplyingRegex = new Regex($@"({Groups.IdentifierOrNumber})\s*{AbsPattern}", RegexOptions.Compiled|RegexOptions.RightToLeft);

        private static readonly Regex AbsSuperscriptRegex = new Regex(AbsSuperscriptPattern, RegexOptions.Compiled);

        private static readonly Regex AbsRegex = new Regex(AbsPattern, RegexOptions.Compiled);


        public string Replace(string code)
        {
            var result = code;

            while (AbsSuperscriptRegex.IsMatch(result))
            {
                result = AbsSuperscriptRegex.Replace(result, match =>
                {
                    var match2 = AbsSuperscriptWithMultiplyingRegex.Match(result, 0, match.Index + match.Length);
                    if (match2.Success &&
                        match2.Index < match.Index &&
                        match2.Index + match2.Length - 1 == match.Index + match.Length - 1)
                        return match.Result($@"{SpecialSymbols.DotSymbolSuperscript}⁽ᵃᵇˢ⁽$1⁾⁾$2$3");
                    return match.Result(@"⁽ᵃᵇˢ⁽$1⁾⁾$2$3");
                });
            }

            // A double bar can be a C# short-circuit operator or two adjacent
            // absolute-value delimiters. Protect complete logical expressions,
            // not every || token: |x||y| and ||x|| have missing C# operands.
            // Complete bare-name and comparison chains follow boolean syntax;
            // whole arithmetic operands retain the absolute-value meaning.
            // Use explicit multiplication or abs(...) for ambiguous math chains.
            // The temporary operator stays valid C# so later passes can recognize
            // a logical expression after its |...| operands have been rewritten.
            var logicalOrMarker = "&&/*__ComputatorLogicalOr" + Guid.NewGuid().ToString("N") + "__*/";
            while (true)
            {
                result = ProtectLogicalOrs(result, logicalOrMarker);
                var matches = AbsRegex.Matches(result).Cast<Match>().ToArray();
                if (matches.Length == 0) break;
                // A match between the inner bars of ||...|| is ambiguous until
                // ordinary absolute-value operands around it are translated.
                // This makes |x| > 0 || flag || other remain a boolean chain.
                var hasOrdinaryAbsoluteValue = matches.Any(match => !BetweenDoubleBars(result, match));
                result = AbsRegex.Replace(result, match =>
                {
                    if (hasOrdinaryAbsoluteValue && BetweenDoubleBars(result, match))
                        return match.Value;
                    var match2 = AbsWithMultiplyingRegex.Match(result, 0, match.Index+match.Length);
                    if (match2.Success &&
                        match2.Index < match.Index &&
                        match2.Index + match2.Length - 1 == match.Index + match.Length - 1)
                        return match.Result("*(abs($1))$2$3");
                    return match.Result("(abs($1))$2$3");
                });
            }

            return result.Replace(logicalOrMarker, "||");
        }

        private static bool BetweenDoubleBars(string code, Match match)
        {
            var closingBar = match.Groups[1].Index + match.Groups[1].Length;
            return match.Index > 0 && code[match.Index - 1] == '|'
                && closingBar + 1 < code.Length && code[closingBar + 1] == '|';
        }

        private static string ProtectLogicalOrs(string code, string marker)
        {
            if (!code.Contains("||")) return code;
            // These later TSL rewrites must not hide an otherwise complete
            // boolean operand from the C# parser. Keep every source position.
            var syntax = code.ToCharArray();
            for (var index = 0; index < syntax.Length; index++)
            {
                if (SpecialSymbols.SuperscriptsWithoutSpace.Contains(syntax[index])) syntax[index] = ' ';
                else if (syntax[index] == SpecialSymbols.DotSymbol) syntax[index] = '*';
                else if (syntax[index] == '∞') syntax[index] = '0';
            }
            var root = CSharpSyntaxTree.ParseText(new string(syntax),
                new CSharpParseOptions(LanguageVersion.CSharp14)).GetRoot();
            // Classify each contiguous chain once. Rechecking all its tokens for
            // every nested binary node would make long || chains quadratic.
            // A separate chain inside a call or parentheses has its own meaning:
            // its arithmetic or missing absolute-value operands must not classify
            // or rewrite the surrounding Boolean operators.
            var chains = root.DescendantNodes().OfType<BinaryExpressionSyntax>()
                .Where(IsLogicalOrChainRoot)
                .Where(expression => !expression.DescendantTokens(node =>
                    node == expression || !IsLogicalOrChainRoot(node)).Any(token => token.IsMissing))
                .Select(expression => LogicalOrChain(expression).ToArray())
                .Where(chain => !chain.Any(node => IsArithmeticOperand(node.Left) || IsArithmeticOperand(node.Right)));
            var changes = chains.SelectMany(chain => chain)
                .Select(expression => expression.OperatorToken.Span).Distinct()
                .Select(span => new TextChange(span, marker)).ToArray();
            return changes.Length == 0 ? code : SourceText.From(code).WithChanges(changes).ToString();
        }

        private static bool IsLogicalOrChainRoot(SyntaxNode node) =>
            node is BinaryExpressionSyntax expression && expression.IsKind(SyntaxKind.LogicalOrExpression)
            && (expression.Parent is not BinaryExpressionSyntax parent || !parent.IsKind(SyntaxKind.LogicalOrExpression));

        private static IEnumerable<BinaryExpressionSyntax> LogicalOrChain(BinaryExpressionSyntax root)
        {
            var pending = new Stack<BinaryExpressionSyntax>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var expression = pending.Pop();
                yield return expression;
                if (expression.Left is BinaryExpressionSyntax left && left.IsKind(SyntaxKind.LogicalOrExpression))
                    pending.Push(left);
                if (expression.Right is BinaryExpressionSyntax right && right.IsKind(SyntaxKind.LogicalOrExpression))
                    pending.Push(right);
            }
        }

        private static bool IsArithmeticOperand(ExpressionSyntax expression)
        {
            while (expression is ParenthesizedExpressionSyntax parentheses)
                expression = parentheses.Expression;
            // Preserve established multiplication by a nested absolute value,
            // such as 2||x||+3, x||y||+z or x||y||*z. A leading arithmetic
            // operator remains a cue for variables and function calls too;
            // multiplication is parsed as pointer indirection by C# here.
            // Comparisons and calls merely containing signs are still eligible
            // boolean operands; only the whole operand is inspected here.
            return expression.IsKind(SyntaxKind.NumericLiteralExpression)
                || expression.IsKind(SyntaxKind.UnaryPlusExpression)
                || expression.IsKind(SyntaxKind.UnaryMinusExpression)
                || expression.IsKind(SyntaxKind.PointerIndirectionExpression)
                || expression.IsKind(SyntaxKind.AddExpression)
                || expression.IsKind(SyntaxKind.SubtractExpression)
                || expression.IsKind(SyntaxKind.MultiplyExpression)
                || expression.IsKind(SyntaxKind.DivideExpression)
                || expression.IsKind(SyntaxKind.ModuloExpression);
        }
    }
}
