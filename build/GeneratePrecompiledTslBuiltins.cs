// Build-time generator. The existing ToCode constants remain the source of truth.
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class GeneratePrecompiledTslBuiltins : Microsoft.Build.Utilities.Task
{
    [Microsoft.Build.Framework.Required]
    public Microsoft.Build.Framework.ITaskItem[] Sources { get; set; }

    [Microsoft.Build.Framework.Required]
    public string OutputFile { get; set; }

    public override bool Execute()
    {

        var fragments = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var sourceItem in Sources)
        {
            string source = File.ReadAllText(sourceItem.ItemSpec);
            var matches = Regex.Matches(source, @"\bpublic\s+const\s+string\s+ToCode\s*=");
            if (matches.Count != 1) throw new InvalidOperationException("Expected exactly one ToCode constant in " + sourceItem.ItemSpec);
            int position = matches[0].Index + matches[0].Length;
            while (position < source.Length && char.IsWhiteSpace(source[position])) position++;
            if (sourceItem.GetMetadata("Kind") == "ScriptingExtensionObjects")
            {
                // This one existing constant prefixes its verbatim literal with
                // ReadForm.ToCode. Admit exactly that audited concatenation.
                const string prefix = "Computator.NET.Core.Functions.ReadForm.ToCode";
                if (position + prefix.Length > source.Length ||
                    source.Substring(position, prefix.Length) != prefix)
                    throw new InvalidOperationException("Review script-object ToCode prefix: " + sourceItem.ItemSpec);
                position += prefix.Length;
                while (position < source.Length && char.IsWhiteSpace(source[position])) position++;
                if (position >= source.Length || source[position++] != '+')
                    throw new InvalidOperationException("Expected ReadForm.ToCode concatenation: " + sourceItem.ItemSpec);
                while (position < source.Length && char.IsWhiteSpace(source[position])) position++;
            }
            if (position + 1 >= source.Length || source[position] != '@' || source[position + 1] != '"')
                throw new InvalidOperationException("ToCode must remain a single verbatim literal: " + sourceItem.ItemSpec);
            position += 2;
            var fragment = new StringBuilder();
            bool closed = false;
            while (position < source.Length)
            {
                char character = source[position++];
                if (character != '"') { fragment.Append(character); continue; }
                if (position < source.Length && source[position] == '"') { fragment.Append('"'); position++; continue; }
                closed = true; break;
            }
            while (position < source.Length && char.IsWhiteSpace(source[position])) position++;
            if (!closed || position >= source.Length || source[position] != ';')
                throw new InvalidOperationException("Unsupported or unterminated ToCode initializer: " + sourceItem.ItemSpec);
            fragments.Add(sourceItem.GetMetadata("Kind"), fragment.ToString());
        }
        // sfResult is written/read immediately in CoulombC, CoulombGprime and
        // CoulombFprime. Preserve every native argument (including the repaired
        // irregular derivative order), but never share their output buffer across programs.
        const string scratchField = "private static gsl_sf_result sfResult;";
        string[] scratchCalls = {
            "gsl_sf_coulomb_CL_e(L, η, out sfResult);",
            "gsl_sf_coulomb_wave_FG_e(η, ρ, L, 0, out r1, out r2, out r3, out sfResult, out d1, out d2);",
            "gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out sfResult, out r3, out r2, out d1, out d2);"
        };
        string special = fragments["SpecialFunctions"];
        int uses = Regex.Matches(special, @"\bsfResult\b").Count;
        int declarations = Regex.Matches(special, Regex.Escape(scratchField)).Count;
        int returns = Regex.Matches(special, @"return\s+sfResult\.val\s*;").Count;
        if (uses != 7 || declarations != 1 || returns != 3)
            throw new InvalidOperationException("Review Coulomb scratch locality: uses=" + uses + ", fields=" + declarations + ", returns=" + returns);
        string localizedSpecial = special.Replace(scratchField, "");
        foreach (string call in scratchCalls)
        {
            int count = Regex.Matches(special, Regex.Escape(call)).Count;
            if (count != 1) throw new InvalidOperationException("Review Coulomb scratch call count " + count + ": " + call);
            localizedSpecial = localizedSpecial.Replace(call, "gsl_sf_result sfResult;\n            " + call);
        }
        string[] kinds = { "MathematicalConstants", "PhysicalConstants", "ElementaryFunctions", "SpecialFunctions",
            "StatisticsFunctions", "Integral", "Derivative", "FunctionRoot" };
        var common = new List<string>(); var immutable = new List<string>();
        foreach (string kind in kinds)
        {
            common.Add(fragments[kind]);
            if (kind != "StatisticsFunctions") immutable.Add(kind == "SpecialFunctions" ? localizedSpecial : fragments[kind]);
        }
        var script = new List<string>(common); script.Add(fragments["MatrixFunctions"]);
        var scriptImmutable = new List<string>(immutable); scriptImmutable.Add(fragments["MatrixFunctions"]);
        Func<string, string> hash = value => {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        };
        // These helpers contain only instance storage (File and ReadForm) or
        // stateless identity methods (TypeDeducer). Preserve the original bodies
        // with Roslyn syntax nodes, rather than copying or slicing C# text.
        Func<string, ClassDeclarationSyntax[]> parseClasses = text => {
            var root = CSharpSyntaxTree.ParseText(text,
                new CSharpParseOptions(LanguageVersion.CSharp14)).GetCompilationUnitRoot();
            if (root.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) ||
                root.Members.Any(member => !(member is ClassDeclarationSyntax)))
                throw new InvalidOperationException("Review script helper source: expected complete class declarations.");
            return root.Members.Cast<ClassDeclarationSyntax>().ToArray();
        };
        var scriptObjects = parseClasses(fragments["ScriptingExtensionObjects"]);
        if (scriptObjects.Length != 3 ||
            string.Join(",", scriptObjects.Select(type => type.Identifier.ValueText)) != "TypeDeducer,File,ScriptingExtensions")
            throw new InvalidOperationException("Review the script-object helper declarations.");
        var readForms = parseClasses(fragments["ReadForm"]);
        if (readForms.Length != 1 || readForms[0].Identifier.ValueText != "ReadForm" ||
            !readForms[0].Modifiers.Any(SyntaxKind.SealedKeyword))
            throw new InvalidOperationException("Review the sealed ReadForm helper declaration.");
        Func<ClassDeclarationSyntax, string, ClassDeclarationSyntax> helperBase = (original, name) => {
            if (original.TypeParameterList != null || original.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Any() ||
                original.DescendantNodes().OfType<FieldDeclarationSyntax>().Any(field => field.Modifiers.Any(SyntaxKind.StaticKeyword)) ||
                original.DescendantNodes().OfType<EventFieldDeclarationSyntax>().Any(field => field.Modifiers.Any(SyntaxKind.StaticKeyword)) ||
                original.DescendantNodes().OfType<PropertyDeclarationSyntax>().Any(property => property.Modifiers.Any(SyntaxKind.StaticKeyword)) ||
                original.DescendantNodes().OfType<EventDeclarationSyntax>().Any(evt => evt.Modifiers.Any(SyntaxKind.StaticKeyword)) ||
                original.Members.OfType<ConstructorDeclarationSyntax>().Any(ctor => ctor.Modifiers.Any(SyntaxKind.StaticKeyword)))
                throw new InvalidOperationException("Script helpers must not acquire shared static state or nested types: " + original.Identifier.ValueText);
            // A newly authored reference to the helper's own type needs explicit
            // review; only the declaration and constructor identifiers are renamed.
            if (original.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Any(identifier => identifier.Identifier.ValueText == original.Identifier.ValueText &&
                    !(identifier.Parent is MemberAccessExpressionSyntax &&
                      ((MemberAccessExpressionSyntax)identifier.Parent).Name == identifier &&
                      ((MemberAccessExpressionSyntax)identifier.Parent).Expression.ToString() == "System.IO")))
                throw new InvalidOperationException("Review script helper self-type reference: " + original.Identifier.ValueText);
            var members = original.Members.Select(member => member is ConstructorDeclarationSyntax
                ? (MemberDeclarationSyntax)((ConstructorDeclarationSyntax)member).WithIdentifier(
                    SyntaxFactory.Identifier(name).WithTriviaFrom(((ConstructorDeclarationSyntax)member).Identifier))
                : member);
            return original.WithIdentifier(SyntaxFactory.Identifier(name).WithTriviaFrom(original.Identifier))
                .WithMembers(SyntaxFactory.List(members))
                .WithModifiers(SyntaxFactory.TokenList(original.Modifiers.Where(token => !token.IsKind(SyntaxKind.SealedKeyword))));
        };
        var typeDeducerBase = helperBase(scriptObjects[0], "PrecompiledScriptTypeDeducer");
        var fileBase = helperBase(scriptObjects[1], "PrecompiledScriptFile");
        var readFormBase = helperBase(readForms[0], "PrecompiledScriptReadForm");
        string compactScriptObjects = @"
 public sealed class ReadForm : global::Computator.NET.Core.Compilation.Builtins.PrecompiledScriptReadForm {
     public ReadForm(string str) : base(str) { }
 }
 public class TypeDeducer : global::Computator.NET.Core.Compilation.Builtins.PrecompiledScriptTypeDeducer { }
 public class File : global::Computator.NET.Core.Compilation.Builtins.PrecompiledScriptFile {
     public File(string path) : base(path) { }
 }
" + scriptObjects[2].ToFullString();
        var generated = new StringBuilder();
        generated.AppendLine("// <auto-generated />");
        generated.AppendLine("#nullable disable");
        generated.AppendLine("#pragma warning disable CS8981 // Existing TSL type aliases.");
        generated.AppendLine("using real = System.Double; using complex = System.Numerics.Complex; using natural = System.UInt64; using integer = System.Int64;");
        generated.AppendLine("using MathNet.Numerics.Distributions;");
        generated.AppendLine("namespace Computator.NET.Core.Compilation.Builtins {");
        generated.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        generated.AppendLine("public class PrecompiledExpressionSurface {");
        foreach (string fragment in immutable) generated.AppendLine(fragment);
        generated.AppendLine("}");
        generated.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        generated.AppendLine("public class PrecompiledScriptMathSurface {");
        foreach (string fragment in scriptImmutable) generated.AppendLine(fragment);
        generated.AppendLine("}");
        generated.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        generated.AppendLine("public class PrecompiledExpressionBuiltins<TProgram> : PrecompiledExpressionSurface {");
        generated.AppendLine(fragments["StatisticsFunctions"]); generated.AppendLine("}");
        generated.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        generated.AppendLine("public class PrecompiledScriptMathBuiltins<TProgram> : PrecompiledScriptMathSurface {");
        generated.AppendLine(fragments["StatisticsFunctions"]); generated.AppendLine("}");
        generated.AppendLine(fragments["NumericalExtensions"]);
        generated.AppendLine(typeDeducerBase.ToFullString());
        generated.AppendLine(fileBase.ToFullString());
        generated.AppendLine(readFormBase.ToFullString());
        generated.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        generated.AppendLine("public static class PrecompiledScriptObjects {");
        generated.AppendLine("public const string ToCode = @\"" + compactScriptObjects.Replace("\"", "\"\"") + "\";");
        generated.AppendLine("}");
        generated.AppendLine("public static class PrecompiledBuiltinSourceHashes {");
        generated.AppendLine("public const string Expression = \"" + hash(string.Join("\0", common)) + "\";");
        generated.AppendLine("public const string ScriptMath = \"" + hash(string.Join("\0", script)) + "\";");
        generated.AppendLine("public const string LocalizedSpecial = \"" + hash(localizedSpecial) + "\";");
        generated.AppendLine("public const string NumericalExtensions = \"" + hash(fragments["NumericalExtensions"]) + "\";");
        generated.AppendLine("public const string ScriptObjects = \"" + hash(fragments["ReadForm"] + fragments["ScriptingExtensionObjects"]) + "\";");
        generated.AppendLine("public const string ScriptTypeDeducer = \"" + hash(scriptObjects[0].ToString()) + "\";");
        generated.AppendLine("public const string ScriptFile = \"" + hash(scriptObjects[1].ToString()) + "\";");
        generated.AppendLine("public const string ScriptReadForm = \"" + hash(readForms[0].ToString()) + "\";");
        generated.AppendLine("} }");
        Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
        string text = generated.ToString();
        if (!File.Exists(OutputFile) || File.ReadAllText(OutputFile) != text)
            File.WriteAllText(OutputFile, text, new UTF8Encoding(false));

        return true;
    }
}
