using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit
{
    internal static class BraceFoldingRanges
    {
        private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.CSharp14);

        internal static IReadOnlyList<TextSpan> Create(string text, char openingBrace, char closingBrace)
        {
            var source = SourceText.From(text);
            var root = CSharpSyntaxTree.ParseText(source, ParseOptions).GetRoot();
            var ranges = new List<TextSpan>();
            var openings = new Stack<int>();
            var openingKind = SyntaxFactory.ParseToken(openingBrace.ToString()).Kind();
            var closingKind = SyntaxFactory.ParseToken(closingBrace.ToString()).Kind();

            // TSL uses C# strings and comments. The same lexer used by its
            // compiler keeps their braces out of the block structure, including
            // verbatim/raw strings and escaped interpolation delimiters.
            foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.Span.Length != 1 ||
                    token.Parent?.AncestorsAndSelf().Any(node => node is InterpolatedStringExpressionSyntax) == true)
                    continue;

                if (token.IsKind(openingKind) && token.Text[0] == openingBrace)
                {
                    openings.Push(token.SpanStart);
                }
                else if (token.IsKind(closingKind) && token.Text[0] == closingBrace && openings.Count > 0)
                {
                    var start = openings.Pop();
                    if (source.Lines.GetLineFromPosition(start).LineNumber <
                        source.Lines.GetLineFromPosition(token.SpanStart).LineNumber)
                        ranges.Add(TextSpan.FromBounds(start, token.Span.End));
                }
            }

            ranges.Sort((left, right) => left.Start.CompareTo(right.Start));
            return ranges;
        }
    }
}
