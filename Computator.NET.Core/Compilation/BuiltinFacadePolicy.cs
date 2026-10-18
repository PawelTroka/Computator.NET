using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Functions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Computator.NET.Core.Compilation
{
    /// <summary>
    /// Use precompiled helpers only when moving them out of the generated class
    /// preserves name binding. Ambiguous/custom/reflection cases keep the original
    /// full-source compiler; this is not a restricted language or security boundary.
    /// </summary>
    internal static class BuiltinFacadePolicy
    {
        private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.CSharp14);
        private static readonly Lazy<HashSet<string>> ExpressionNames = new Lazy<HashSet<string>>(
            () => Names(typeof(PrecompiledExpressionBuiltins<>), false));
        private static readonly Lazy<HashSet<string>> ScriptNames = new Lazy<HashSet<string>>(
            () => Names(typeof(PrecompiledScriptMathBuiltins<>), false));
        private static readonly Lazy<HashSet<string>> PrivateExpressionNames = new Lazy<HashSet<string>>(
            () => Names(typeof(PrecompiledExpressionBuiltins<>), true));
        private static readonly Lazy<HashSet<string>> PrivateScriptNames = new Lazy<HashSet<string>>(
            () => Names(typeof(PrecompiledScriptMathBuiltins<>), true));
        private static readonly Lazy<bool> ScriptPreludeCompatible = new Lazy<bool>(() =>
            !DeclaredNames(CSharpSyntaxTree.ParseText("class __Guard {" + ScriptingFunctions.ToCode + "}", ParseOptions).GetRoot())
                .Any(name => ScriptNames.Value.Contains(name)));
        private static readonly HashSet<string> IntrospectionNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "FunctionsCreator", "FunctionsCreatorNS", "GetType", "GetMethod", "GetMethods", "GetField", "GetFields",
            "GetMember", "GetMembers", "GetProperty", "GetProperties", "GetNestedType", "GetNestedTypes", "GetTypeInfo",
            "Reflection", "Assembly", "MethodBase", "MethodInfo", "FieldInfo", "MemberInfo", "BindingFlags",
            "StackTrace", "StackFrame", "BaseType", "DeclaringType", "GetExecutingAssembly", "GetCallingAssembly"
        };

        private static HashSet<string> Names(Type type, bool onlyNonPublic)
        {
            // Only open, application-owned generic definitions are inspected.
            // Never cache a constructed type containing a collectible program.
            return new HashSet<string>(type.GetMembers(Members).Where(member => !onlyNonPublic ||
                member is MethodBase method && !method.IsPublic || member is FieldInfo field && !field.IsPublic ||
                member is PropertyInfo property && property.GetAccessors(true).Any(accessor => !accessor.IsPublic) ||
                member is Type nested && !nested.IsNestedPublic).Select(member => member.Name.Split('`')[0]), StringComparer.Ordinal);
        }

        internal static bool CanUse(string customCode, string mainCode, bool scripting)
        {
            if (scripting && !ScriptPreludeCompatible.Value) return false;
            var root = CSharpSyntaxTree.ParseText("class __Guard { " + customCode +
                " void __Body() { " + (scripting ? mainCode : "return " + mainCode + ";") + " } }", ParseOptions).GetRoot();
            if (root.ContainsDiagnostics || root.ContainsDirectives) return false;
            var names = scripting ? ScriptNames.Value : ExpressionNames.Value;
            var privateNames = scripting ? PrivateScriptNames.Value : PrivateExpressionNames.Value;
            // A new overload in the derived class hides the inherited overload
            // group. A duplicate signature must still produce the legacy error.
            if (DeclaredNames(root).Any(names.Contains)) return false;
            if (root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Count() != 1) return false;
            foreach (var token in root.DescendantTokens())
            {
                if (token.IsKind(SyntaxKind.TypeOfKeyword) || token.IsKind(SyntaxKind.BaseKeyword) ||
                    token.IsKind(SyntaxKind.ThisKeyword)) return false;
                if (token.IsKind(SyntaxKind.IdentifierToken) &&
                    (privateNames.Contains(token.ValueText) || IntrospectionNames.Contains(token.ValueText) || token.ValueText == "dynamic"))
                    return false;
            }
            return true;
        }

        private static IEnumerable<string> DeclaredNames(SyntaxNode root)
        {
            var owner = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (owner == null) yield break;
            // Only class members can change binding inside a built-in body or
            // hide its inherited overload group. A local loop index named i,
            // for example, has the same scope on both compilation paths.
            foreach (var member in owner.Members)
            {
                if (member is BaseFieldDeclarationSyntax field)
                {
                    foreach (var variable in field.Declaration.Variables) yield return variable.Identifier.ValueText;
                    continue;
                }
                var identifier = member switch
                {
                    MethodDeclarationSyntax method => method.Identifier,
                    PropertyDeclarationSyntax property => property.Identifier,
                    EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier,
                    BaseTypeDeclarationSyntax type => type.Identifier,
                    DelegateDeclarationSyntax declaration => declaration.Identifier,
                    _ => default
                };
                if (identifier.RawKind != 0) yield return identifier.ValueText;
            }
        }
    }
}
