using System.Linq;
using Computator.NET.Core.Compilation.TslHandlers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Computator.NET.Core.Compilation
{
    public interface ITslCompiler
    {
        string TransformToCSharp(string tslCode);
    }

    public class TslCompiler : ITslCompiler
    {
        public const string TypesAliases = @"
        using real = System.Double;
        using complex = System.Numerics.Complex;
        using natural = System.UInt64;
        using integer = System.Int64;";

        public static readonly string[] Keywords =
        {
            "real", "complex", "function", "var", "void", "Matrix", "string", "integer", "natural", "abstract", "as",
            "base", "break",
            "case", "catch", "checked", "continue", "default", "delegate", "do", "else", "event", "explicit", "extern",
            "false", "finally", "fixed", "for", "foreach", "goto", "if", "implicit", "in", "interface", "internal", "is",
            "lock", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
            "protected",
            "public", "readonly", "ref", "return", "sealed", "sizeof", "stackalloc", "switch", "this", "throw", "true",
            "try", "typeof", "unchecked", "unsafe", "using", "virtual", "while"
        };

        public static readonly string KeywordsList0 =
            "var abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while";

        public static readonly string KeywordsList1 =
            "natural integer real complex function Matrix bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void";


        private static readonly ITslHandler[] TslHandlers = {
            new MatrixHandler(),
            new RefOutHandler(),
            new AbsHandler(), 
            new FunctionsHandler(),
            new PowHandler(),
            new DivisionHandler(),
            new MultiplyingHandler(),
            new SpecialSymbolsHandler(), 
        };
        
        
        public string TransformToCSharp(string tslCode)
        {
            // TSL notation is executable syntax, never literal text. Let the
            // existing C# lexer recognize escaped/verbatim/raw strings, chars,
            // interpolated text and comments even before notation is translated.
            var source = SourceText.From(tslCode);
            var root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14)).GetRoot();
            var literals = new List<string>();
            var changes = new List<TextChange>();
            var prefix = "__ComputatorLiteral" + Guid.NewGuid().ToString("N") + "_";
            foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.Span.Length == 0 || !IsLiteralText(token.Kind())) continue;
                changes.Add(new TextChange(token.Span, prefix + literals.Count + "__"));
                literals.Add(token.Text);
            }
            foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                if (trivia.Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia
                    or SyntaxKind.SingleLineDocumentationCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia
                    or SyntaxKind.DisabledTextTrivia)
                {
                    // Comments behave as whitespace. Keep every line break so
                    // diagnostics still point to the original document's lines.
                    changes.Add(new TextChange(trivia.Span, new string(source.ToString(trivia.Span).Select(character =>
                        character is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029' ? character : ' ').ToArray())));
                }
            }
            // Structured trivia can contain tokens of its own; its outer span
            // takes precedence over any nested literal already collected.
            var ordered = new List<TextChange>();
            var end = 0;
            foreach (var change in changes.OrderBy(change => change.Span.Start).ThenByDescending(change => change.Span.Length))
            {
                if (change.Span.Start < end) continue;
                ordered.Add(change);
                end = change.Span.End;
            }
            var protectedCode = source.WithChanges(ordered).ToString();
            var result = TslHandlers.Aggregate(protectedCode, (current, handler) => handler.Replace(current));
            return Regex.Replace(result, prefix + @"(\d+)__", match => literals[int.Parse(match.Groups[1].Value)]);
        }

        private static bool IsLiteralText(SyntaxKind kind) => kind is
            SyntaxKind.StringLiteralToken or SyntaxKind.CharacterLiteralToken or SyntaxKind.InterpolatedStringTextToken
            or SyntaxKind.SingleLineRawStringLiteralToken or SyntaxKind.MultiLineRawStringLiteralToken
            or SyntaxKind.Utf8StringLiteralToken or SyntaxKind.Utf8SingleLineRawStringLiteralToken
            or SyntaxKind.Utf8MultiLineRawStringLiteralToken;
    }
}
