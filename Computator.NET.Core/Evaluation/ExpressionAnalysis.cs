using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Computator.NET.Core.Evaluation
{
    // Analyze the compiler's C# output, not a second approximation of TSL.
    // This is syntactic routing, not semantic type inference or a sandbox.
    internal sealed class ExpressionAnalysis
    {
        private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.CSharp14);
        private readonly ExpressionSyntax _expression;
        private readonly AssignmentExpressionSyntax _equation;

        private ExpressionAnalysis(ExpressionSyntax expression)
        {
            _expression = expression;
            var unwrapped = Unwrap(expression);
            if (unwrapped is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                _equation = assignment;
        }

        public static ExpressionAnalysis Parse(string cSharp)
        {
            return new ExpressionAnalysis(SyntaxFactory.ParseExpression(cSharp, options: ParseOptions));
        }

        public bool IsEmpty => !_expression.DescendantTokens().Any(token => !token.IsMissing && token.Span.Length > 0);
        public bool IsImplicit => _equation != null;

        public bool ContainsVariable(string name)
        {
            return _expression.DescendantTokens(descendIntoTrivia: true).Any(token =>
                token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == name && IsValueName(token));
        }

        private static bool IsValueName(SyntaxToken token)
        {
            // obj.y and a named argument y: are not references to the input y.
            var name = token.Parent;
            return !(name?.Parent is MemberAccessExpressionSyntax access && access.Name == name)
                && !(name?.Parent is MemberBindingExpressionSyntax)
                && !(name?.Parent is QualifiedNameSyntax)
                && !(name?.Parent is AliasQualifiedNameSyntax)
                && !(name?.Parent is NameColonSyntax)
                && !(name?.Parent is NameEqualsSyntax);
        }

        public string ToExplicit()
        {
            if (_equation == null)
                return _expression.ToFullString();

            // Do not silently turn x = y = 1 into a parameter assignment.
            if (Unwrap(_equation.Right) is AssignmentExpressionSyntax)
                throw new ArgumentException("An implicit expression must contain one equation, not chained assignments.");

            var difference = SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression,
                SyntaxFactory.ParenthesizedExpression(_equation.Left),
                SyntaxFactory.ParenthesizedExpression(_equation.Right));
            return _expression.ReplaceNode(_equation, difference).ToFullString();
        }

        private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
        {
            while (expression is ParenthesizedExpressionSyntax parentheses)
                expression = parentheses.Expression;
            return expression;
        }
    }
}
