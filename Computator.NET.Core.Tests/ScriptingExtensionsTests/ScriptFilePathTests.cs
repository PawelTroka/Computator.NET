using System;
using System.IO;
using System.Text;
using Computator.NET.Core.Evaluation;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ScriptingExtensionsTests
{
    [TestFixture]
    [NonParallelizable]
    public class ScriptFilePathTests
    {
        private sealed class GeneratedScriptEvaluator : ScriptEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [TestCase("host", "relative-existing")]
        [TestCase("host", "relative-new")]
        [TestCase("host", "dot-relative")]
        [TestCase("host", "absolute")]
        [TestCase("precompiled", "relative-existing")]
        [TestCase("precompiled", "relative-new")]
        [TestCase("precompiled", "dot-relative")]
        [TestCase("precompiled", "absolute")]
        [TestCase("generated", "relative-existing")]
        [TestCase("generated", "relative-new")]
        [TestCase("generated", "dot-relative")]
        [TestCase("generated", "absolute")]
        public void ClearKeepsTheOriginalFileAfterTheWorkingDirectoryChanges(string execution, string pathKind)
        {
            var previousDirectory = Environment.CurrentDirectory;
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousWorkingDirectory = settings.WorkingDirectory;
            var root = Path.Combine(Path.GetTempPath(), "Computator-ScriptFile-" + Guid.NewGuid().ToString("N"));
            const string name = "źródło data.txt";
            const string replacement = "replacement λ";
            const string unrelatedContent = "UNRELATED FILE MUST SURVIVE";
            try
            {
                var originalDirectory = Path.Combine(root, "original λ space");
                var changedDirectory = Path.Combine(root, "changed λ space");
                Directory.CreateDirectory(Path.Combine(originalDirectory, "nested"));
                Directory.CreateDirectory(Path.Combine(changedDirectory, "nested"));
                var originalFile = Path.Combine(originalDirectory, name);
                var unrelatedFile = Path.Combine(changedDirectory, name);
                if (pathKind != "relative-new")
                {
                    System.IO.File.WriteAllText(originalFile, "original content");
                    System.IO.File.WriteAllText(unrelatedFile, unrelatedContent);
                }
                var path = pathKind == "absolute" ? originalFile
                    : pathKind == "dot-relative" ? Path.Combine("nested", ".", "..", name) : name;
                settings.WorkingDirectory = originalDirectory;
                Environment.CurrentDirectory = originalDirectory;

                var output = execution == "host" ? ExerciseHost(path, changedDirectory)
                    : ExerciseScript(path, changedDirectory, execution == "generated");

                NUnit.Framework.Assert.That(output, Is.EqualTo("first λ|tail|" + replacement),
                    "Both clears must reopen the same file with usable read and write streams.");
                NUnit.Framework.Assert.That(System.IO.File.ReadAllText(originalFile), Is.EqualTo(replacement),
                    "Clear must replace the file opened in the original directory.");
                if (pathKind == "relative-new")
                    NUnit.Framework.Assert.That(System.IO.File.Exists(unrelatedFile), Is.False,
                        "Clear must not create a new file in the changed working directory.");
                else
                    NUnit.Framework.Assert.That(System.IO.File.ReadAllText(unrelatedFile), Is.EqualTo(unrelatedContent),
                        "Clear must not truncate or append to another file with the same relative name.");
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
                settings.WorkingDirectory = previousWorkingDirectory;
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        private static string ExerciseHost(string path, string changedDirectory)
        {
            var file = new Computator.NET.Core.Evaluation.File(path);
            try
            {
                Environment.CurrentDirectory = changedDirectory;
                file.clear();
                file.writeln("first λ");
                file.write("tail");
                var first = file.readln() + "|" + file.readAll();
                file.clear();
                file.write("replacement λ");
                return first + "|" + file.readAll();
            }
            finally { file.close(); }
        }

        private static string ExerciseScript(string path, string changedDirectory, bool generated)
        {
            var source = "var file = File(" + Literal(path) + @");
                try {
                    setWorkingDirectory(" + Literal(changedDirectory) + @");
                    file.clear();
                    file.writeln(""first λ"");
                    file.write(""tail"");
                    write(file.readln()); write(""|""); write(file.readAll());
                    file.clear();
                    file.write(""replacement λ"");
                    write(""|""); write(file.readAll());
                }
                finally { file.close(); }";
            ScriptEvaluator evaluator = generated ? new GeneratedScriptEvaluator() : new ScriptEvaluator();
            var output = new StringBuilder();
            evaluator.Evaluate(source, "").Evaluate(text => output.Append(text));
            return output.ToString();
        }

        private static string Literal(string value) => "@\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
