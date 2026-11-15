using System;
using System.IO;
using System.Reflection;
using System.Text;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Functions;
using Settings = Computator.NET.Core.Properties.Settings;
using File = System.IO.File;

internal static partial class Program
{
    private sealed class GeneratedWorkingDirectoryScript : ScriptEvaluator
    {
        protected override bool UsePrecompiledBuiltins => false;
    }

    private static void ScriptWorkingDirectories(bool generated)
    {
        var previousDirectory = Environment.CurrentDirectory;
        var previousSetting = Settings.Default.WorkingDirectory;
        var root = Path.Combine(outputDirectory, "script-working-directory-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            foreach (var name in new[]
            {
                "ordinary directory",
                "Paweł Ω \"physics\"",
                "literal\\backslash",
                "line\nbreak",
                "quotes\"\\and\nUnicode Ω",
                "code\"); write(\"INJECTED\"); setWorkingDirectory(@\"tail"
            })
            {
                var directory = Path.Combine(root, name);
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "input.txt"), "źródło Ω");
                // macOS may expose /var through /private/var. Compare the actual
                // current-directory spelling, while retaining the original input.
                Environment.CurrentDirectory = directory;
                var expectedDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = root;
                var compileDirectory = Environment.CurrentDirectory;
                Settings.Default.WorkingDirectory = directory;

                ScriptEvaluator evaluator = generated ? new GeneratedWorkingDirectoryScript() : new ScriptEvaluator();
                var script = evaluator.Evaluate(@"
                    write(Probe(3)); write(""|"");
                    write(System.IO.Directory.GetCurrentDirectory()); write(""|"");
                    write(System.IO.File.ReadAllText(""input.txt""));
                    System.IO.File.WriteAllText(""output.txt"", ""saved:"" + Probe(3));
                ", "static real Probe(real x) { return x*x+1; }");
                Equal(Environment.CurrentDirectory, compileDirectory);
                var callback = (Delegate)typeof(BaseFunction)
                    .GetField("_function", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
                Check(callback.Method.DeclaringType.BaseType.IsGenericType == !generated,
                    "The working-directory regression must execute the selected built-in implementation.");

                var output = new StringBuilder();
                script.Evaluate(text => output.Append(text));
                Equal(output.ToString(), "10|" + expectedDirectory + "|źródło Ω");
                Equal(Environment.CurrentDirectory, expectedDirectory);
                Equal(File.ReadAllText(Path.Combine(directory, "output.txt")), "saved:10");
                Check(!File.Exists(Path.Combine(root, "output.txt")), "Relative writes must use the configured directory.");
            }
        }
        finally
        {
            Settings.Default.WorkingDirectory = previousSetting;
            Environment.CurrentDirectory = previousDirectory;
            Directory.Delete(root, true);
        }
    }
}
