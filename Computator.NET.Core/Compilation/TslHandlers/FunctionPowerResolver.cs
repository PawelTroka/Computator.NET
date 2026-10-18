#nullable enable
using System;
using System.Linq;
using Computator.NET.DataTypes.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Computator.NET.Core.Compilation.TslHandlers
{
    /// <summary>
    /// Resolves symbolic function powers after built-ins, custom definitions and
    /// the expression have been combined. Existing callable names always win.
    /// </summary>
    public static class FunctionPowerResolver
    {
        public static CSharpCompilation Resolve(CSharpCompilation compilation)
        {
            if (compilation == null) throw new ArgumentNullException(nameof(compilation));
            foreach (var originalTree in compilation.SyntaxTrees.ToArray())
            {
                var tree = originalTree;
                while (true)
                {
                    var root = tree.GetRoot();
                    // Most programs need no binding pass. In particular, declarations
                    // and literal text alone must not activate symbolic lowering.
                    if (!root.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(IsCandidate)) break;
                    var rewritten = new Rewriter(compilation.GetSemanticModel(tree)).Visit(root)!;
                    if (ReferenceEquals(root, rewritten)) break;
                    var nextTree = tree.WithRootAndOptions(rewritten, tree.Options);
                    compilation = compilation.ReplaceSyntaxTree(tree, nextTree);
                    tree = nextTree;
                    // A changed var initializer can expose a receiver's type to a
                    // later call. Rebind only after progress; every replacement
                    // consumes an exponent suffix, so this reaches a fixed point.
                }
            }
            return compilation;
        }

        internal static bool IsSuperscriptLetter(char character) =>
            char.IsLetter(character) && SpecialSymbols.SuperscriptAlphabet.IndexOf(character) >= 0;

        private static IdentifierNameSyntax? TerminalName(ExpressionSyntax expression) => expression switch
        {
            IdentifierNameSyntax name => name,
            MemberAccessExpressionSyntax member => member.Name as IdentifierNameSyntax,
            _ => null
        };

        private static bool IsCandidate(InvocationExpressionSyntax invocation)
        {
            var name = TerminalName(invocation.Expression);
            if (name == null || name.Identifier.Text.StartsWith("@", StringComparison.Ordinal) ||
                name.Identifier.Text.Contains("\\")) return false;
            var value = name.Identifier.ValueText;
            return value.Length > 1 && IsSuperscriptLetter(value[value.Length - 1]);
        }

        private sealed class Rewriter : CSharpSyntaxRewriter
        {
            private readonly SemanticModel _model;

            public Rewriter(SemanticModel model) => _model = model;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                // Bind each proposed outer power with its already rewritten
                // arguments, so ordinary nested powers resolve in one tree pass.
                var visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;
                if (!IsCandidate(node) || HasExistingName(node.Expression) || HasDynamicReceiver(node) ||
                    HasRewrittenName(node, visited)) return visited;
                // An earlier rewrite can repair an inferred local's type. Do
                // not choose a split while such a value still affects binding.
                if (node.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Any(identifier => HasUnresolvedType(_model.GetSymbolInfo(identifier).Symbol))) return visited;

                var name = TerminalName(node.Expression)!.Identifier.ValueText;
                InvocationExpressionSyntax? replacement = null;
                for (var split = name.Length - 1; split > 0 && IsSuperscriptLetter(name[split]); split--)
                {
                    var baseName = SyntaxFactory.IdentifierName(name.Substring(0, split))
                        .WithTriviaFrom(TerminalName(visited.Expression)!);
                    var callable = visited.Expression is MemberAccessExpressionSyntax member
                        ? (ExpressionSyntax)member.WithName(baseName)
                        : baseName;
                    var call = visited.WithExpression(callable).WithoutTrivia();
                    var exponent = SyntaxFactory.ParseExpression(SpecialSymbols.SuperscriptsToAscii(name.Substring(split)));
                    if (HasUnresolvedType(_model.GetSpeculativeSymbolInfo(node.SpanStart, callable,
                            SpeculativeBindingOption.BindAsExpression).Symbol) ||
                        HasUnresolvedType(_model.GetSpeculativeSymbolInfo(node.SpanStart, exponent,
                            SpeculativeBindingOption.BindAsExpression).Symbol)) return visited;
                    // Binding pow alone can succeed even when its inner call has
                    // the wrong arity/types. Such a split is not an alternative.
                    if (!BindsMethod(call, node.SpanStart)) continue;
                    var exponentType = _model.GetSpeculativeTypeInfo(node.SpanStart, exponent,
                        SpeculativeBindingOption.BindAsExpression).Type;
                    if (exponentType == null || exponentType.TypeKind == TypeKind.Error) continue;
                    var power = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("pow"),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(call), SyntaxFactory.Argument(exponent)
                        })));
                    if (!BindsMethod(power, node.SpanStart)) continue;
                    // Two valid suffix splits can denote different mathematics.
                    // Keep the unresolved name and normal diagnostics; the author
                    // can choose a grouped prefix, postfix power or explicit pow.
                    if (replacement != null) return visited;
                    replacement = power.WithTriviaFrom(visited);
                }
                return replacement ?? visited;
            }

            private bool BindsMethod(InvocationExpressionSyntax invocation, int position)
            {
                var binding = _model.GetSpeculativeSymbolInfo(position, invocation,
                    SpeculativeBindingOption.BindAsExpression);
                if (binding.Symbol is IMethodSymbol) return true;
                // Desktop sqrt(double) returns dynamic. Its power, and outer
                // calls using that result, retain runtime overload selection
                // within a statically known method group.
                return binding.CandidateReason == CandidateReason.LateBound &&
                    binding.CandidateSymbols.Length != 0 &&
                    binding.CandidateSymbols.All(symbol => symbol is IMethodSymbol) &&
                    _model.GetSpeculativeTypeInfo(position, invocation,
                        SpeculativeBindingOption.BindAsExpression).Type?.TypeKind == TypeKind.Dynamic;
            }

            private static bool HasUnresolvedType(ISymbol? symbol) => symbol switch
            {
                ILocalSymbol local => HasErrorType(local.Type),
                IParameterSymbol parameter => HasErrorType(parameter.Type),
                IFieldSymbol field => HasErrorType(field.Type),
                IPropertySymbol property => HasErrorType(property.Type),
                _ => false
            };

            private static bool HasErrorType(ITypeSymbol type) =>
                type.TypeKind == TypeKind.Error ||
                type is IArrayTypeSymbol array && HasErrorType(array.ElementType) ||
                type is INamedTypeSymbol named && named.TypeArguments.Any(HasErrorType);

            private bool HasExistingName(ExpressionSyntax expression)
            {
                var symbol = _model.GetSymbolInfo(expression);
                // An invalid overload, inaccessible member or non-callable local
                // is an error on that exact name, not permission to reinterpret it.
                return IsValueOrCallable(symbol.Symbol) || symbol.CandidateSymbols.Any(IsValueOrCallable) ||
                    _model.GetMemberGroup(expression).Length != 0;
            }

            private static bool IsValueOrCallable(ISymbol? symbol) =>
                symbol != null && symbol is not INamespaceOrTypeSymbol && symbol is not IAliasSymbol;

            private bool HasDynamicReceiver(InvocationExpressionSyntax invocation) =>
                invocation.Expression is MemberAccessExpressionSyntax member &&
                _model.GetTypeInfo(member.Expression).Type?.TypeKind == TypeKind.Dynamic;

            private bool HasRewrittenName(InvocationExpressionSyntax original, InvocationExpressionSyntax visited)
            {
                if (ReferenceEquals(original.Expression, visited.Expression)) return false;
                // A rewritten receiver may expose a real superscript-letter
                // member that was unavailable on the original unresolved call.
                var symbol = _model.GetSpeculativeSymbolInfo(original.SpanStart, visited.Expression,
                    SpeculativeBindingOption.BindAsExpression);
                if (IsValueOrCallable(symbol.Symbol) || symbol.CandidateSymbols.Any(IsValueOrCallable)) return true;
                var invocation = _model.GetSpeculativeSymbolInfo(original.SpanStart, visited,
                    SpeculativeBindingOption.BindAsExpression);
                if (IsValueOrCallable(invocation.Symbol) || invocation.CandidateSymbols.Any(IsValueOrCallable)) return true;
                return visited.Expression is MemberAccessExpressionSyntax member &&
                    _model.GetSpeculativeTypeInfo(original.SpanStart, member.Expression,
                        SpeculativeBindingOption.BindAsExpression).Type?.TypeKind == TypeKind.Dynamic;
            }
        }
    }
}
