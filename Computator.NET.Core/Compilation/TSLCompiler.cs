using System.Linq;
using Computator.NET.Core.Compilation.TslHandlers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Computator.NET.Core.Compilation
{
    public interface ITslCompiler
    {
        string TransformToCSharp(string tslCode);
#if NETFRAMEWORK
        string TransformToCSharp(string tslCode, string customFunctionsCode, bool deferSymbolicFunctionPowers);
#else
        string TransformToCSharp(string tslCode, string customFunctionsCode, bool deferSymbolicFunctionPowers) =>
            TransformToCSharp(tslCode);
#endif
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
        
        
        public string TransformToCSharp(string tslCode) => TransformToCSharp(tslCode, string.Empty);

        /// <summary>
        /// Translates a caller with the member declarations supplied by its
        /// separately authored custom-functions fragment. No names are retained
        /// between compilations.
        /// </summary>
        public string TransformToCSharp(string tslCode, string customFunctionsCode) =>
            TransformToCSharp(tslCode, customFunctionsCode, false);

        /// <summary>
        /// Evaluation defers bare symbolic call names until the complete program
        /// can bind them. The existing overloads retain syntax-only translation
        /// for editor previews. Mode analysis uses the same deferred binding as evaluation.
        /// </summary>
        public string TransformToCSharp(string tslCode, string customFunctionsCode, bool deferSymbolicFunctionPowers)
        {
            // TSL notation is executable syntax, never literal text. Let the
            // existing C# lexer recognize escaped/verbatim/raw strings, chars,
            // interpolated text and comments even before notation is translated.
            var source = SourceText.From(tslCode);
            var root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14)).GetRoot();
            // A declaration establishes a literal identifier, even when it resembles
            // reserved elementary-function shorthand. Protect its references in
            // this fragment too, including members from the separate definitions.
            var declaredPowerNames = GetSharedPowerNames(customFunctionsCode);
            var constructedTypeSpans = new HashSet<TextSpan>();
            foreach (var node in root.DescendantNodes())
            {
                var identifier = node switch
                {
                    MethodDeclarationSyntax method => method.Identifier,
                    LocalFunctionStatementSyntax function => function.Identifier,
                    DelegateDeclarationSyntax declaration => declaration.Identifier,
                    VariableDeclaratorSyntax variable => variable.Identifier,
                    ParameterSyntax parameter => parameter.Identifier,
                    PropertyDeclarationSyntax property => property.Identifier,
                    BaseTypeDeclarationSyntax type => type.Identifier,
                    _ => default
                };
                if (PowHandler.IsSymbolicPowerIdentifier(identifier.ValueText))
                    declaredPowerNames.Add(identifier.ValueText);

                // Constructors (including attributes and primary-constructor
                // base calls) name types, never function-power shorthand. Only
                // protect the type tokens here, so a separate cosᶻ(z) beside
                // new cosᶻ(3) still denotes the requested cosine power.
                var constructedType = node switch
                {
                    ObjectCreationExpressionSyntax creation => creation.Type,
                    AttributeSyntax attribute => attribute.Name,
                    PrimaryConstructorBaseTypeSyntax baseType => baseType.Type,
                    _ => null
                };
                if (constructedType != null)
                    foreach (var name in constructedType.DescendantTokens())
                        if (PowHandler.IsSymbolicPowerIdentifier(name.ValueText))
                            constructedTypeSpans.Add(name.Span);
            }
            var literals = new List<string>();
            var changes = new List<TextChange>();
            var prefix = "__ComputatorLiteral" + Guid.NewGuid().ToString("N") + "_";
            foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.Span.Length == 0 || (!IsLiteralText(token.Kind()) &&
                    !(token.IsKind(SyntaxKind.IdentifierToken) &&
                        (declaredPowerNames.Contains(token.ValueText) || constructedTypeSpans.Contains(token.Span))))) continue;
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
            var result = TslHandlers.Aggregate(protectedCode, (current, handler) =>
                handler is PowHandler powers ? powers.Replace(current, deferSymbolicFunctionPowers) : handler.Replace(current));
            return Regex.Replace(result, "(@?)" + prefix + @"(\d+)__", match =>
            {
                var literal = literals[int.Parse(match.Groups[2].Value)];
                return match.Groups[1].Length != 0 && !literal.StartsWith("@", StringComparison.Ordinal)
                    ? "@" + literal : literal;
            });
        }

        private static HashSet<string> GetSharedPowerNames(string customFunctionsCode)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(customFunctionsCode)) return names;
            var root = CSharpSyntaxTree.ParseText(customFunctionsCode,
                new CSharpParseOptions(LanguageVersion.CSharp14)).GetRoot();
            foreach (var node in root.DescendantNodes())
            {
                // The fragment is inserted into the generated program class.
                // Only its direct members are visible to a separate caller;
                // nested types, parameters and function-local names are not.
                if (node.Ancestors().Any(parent => parent is BaseTypeDeclarationSyntax
                    or BaseMethodDeclarationSyntax or LocalFunctionStatementSyntax
                    or AnonymousFunctionExpressionSyntax or AccessorDeclarationSyntax)) continue;
                var identifier = node switch
                {
                    MethodDeclarationSyntax method => method.Identifier,
                    LocalFunctionStatementSyntax function => function.Identifier,
                    VariableDeclaratorSyntax variable => variable.Identifier,
                    PropertyDeclarationSyntax property => property.Identifier,
                    _ => default
                };
                if (PowHandler.IsSymbolicPowerIdentifier(identifier.ValueText))
                    names.Add(identifier.ValueText);
            }
            return names;
        }

        private static bool IsLiteralText(SyntaxKind kind) => kind is
            SyntaxKind.StringLiteralToken or SyntaxKind.CharacterLiteralToken or SyntaxKind.InterpolatedStringTextToken
            or SyntaxKind.SingleLineRawStringLiteralToken or SyntaxKind.MultiLineRawStringLiteralToken
            or SyntaxKind.Utf8StringLiteralToken or SyntaxKind.Utf8SingleLineRawStringLiteralToken
            or SyntaxKind.Utf8MultiLineRawStringLiteralToken;
    }
}
