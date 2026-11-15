using System.Globalization;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableScriptOutputTests
{
    [Test]
    public void WriteLinePreservesValuesAndUsesThePlatformNewline()
    {
        var result = new PortableEngine().ExecuteScript("write(\"start:\"); writeln(\"Paweł ∑\"); writeln(null); writeln(42);");
        Assert.That(result, Is.EqualTo("start:Paweł ∑" + Environment.NewLine + Environment.NewLine + "42" + Environment.NewLine));
    }

    [Test]
    public void CustomFunctionsCanWriteToTheCurrentScript()
    {
        var result = new PortableEngine().ExecuteScript("Report(3); Report(4);",
            "static void Report(real x) { write(\"value=\"); writeln(x); }");
        Assert.That(result, Is.EqualTo("value=3" + Environment.NewLine + "value=4" + Environment.NewLine));
    }

    [Test]
    public void CustomStaticInitializersCanWriteBeforeTheScriptBody()
    {
        var result = new PortableEngine().ExecuteScript("write(Seed);",
            "static int Seed = Initialize(); static int Initialize() { writeln(\"initializing\"); return 7; }");
        Assert.That(result, Is.EqualTo("initializing" + Environment.NewLine + "7"));
    }

    [TestCase("pl-PL")]
    [TestCase("de-DE")]
    [NonParallelizable]
    public void CustomOutputUsesTheExistingInvariantFormatting(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var result = new PortableEngine().ExecuteScript("Report();",
                "static void Report() { write(1.25); write(\":\"); writeln(decimal.Parse(\"2.5\", System.Globalization.CultureInfo.InvariantCulture)); }");
            Assert.That(result, Is.EqualTo("1.25:2.5" + Environment.NewLine));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Test]
    public void WriteLineCanFillTheExactOutputLimit()
    {
        var count = PortableEngine.MaximumSourceCharacters - Environment.NewLine.Length;
        var result = new PortableEngine().ExecuteScript($"Report({count});",
            "static void Report(int count) { writeln(new string('x', count)); }");
        Assert.That(result, Is.EqualTo(new string('x', count) + Environment.NewLine));
    }

    [Test]
    public void WriteLineRejectsAnOversizedLineWithoutAppendingPartOfIt()
    {
        var prefix = PortableEngine.MaximumSourceCharacters - 1;
        var result = new PortableEngine().ExecuteScript($$"""
            write(new string('x', {{prefix}}));
            try { writeln("y"); } catch (InvalidOperationException) { }
            write("z");
            """);
        Assert.That(result, Is.EqualTo(new string('x', prefix) + "z"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CustomOutputCannotBypassTheOutputLimit(bool newline)
    {
        var call = newline ? "writeln" : "write";
        Assert.Throws<InvalidOperationException>(() => new PortableEngine().ExecuteScript("Report();",
            $"static void Report() {{ {call}(new string('x', {PortableEngine.MaximumSourceCharacters + 1})); }}"));
    }

    [Test]
    public void ParallelScriptsAndTheirChildTasksKeepSeparateOutput()
    {
        var key = "portable-output-barrier-" + Guid.NewGuid().ToString("N");
        using var barrier = new Barrier(2);
        AppDomain.CurrentDomain.SetData(key, barrier);
        try
        {
            var engine = new PortableEngine();
            var tasks = Enumerable.Range(0, 2).Select(index => Task.Run(() => engine.ExecuteScript($"""
                if (!((System.Threading.Barrier)AppDomain.CurrentDomain.GetData("{key}")).SignalAndWait(TimeSpan.FromSeconds(20)))
                    throw new InvalidOperationException("Concurrent script did not arrive.");
                System.Threading.Tasks.Parallel.For(0, 16, _ => Report());
                """, $"static void Report() {{ writeln(\"script-{index}\"); }}"))).ToArray();
            Assert.That(Task.WaitAll(tasks, TimeSpan.FromSeconds(40)), Is.True);
            for (var index = 0; index < tasks.Length; index++)
                Assert.That(tasks[index].Result, Is.EqualTo(string.Concat(Enumerable.Repeat("script-" + index + Environment.NewLine, 16))));
        }
        finally { AppDomain.CurrentDomain.SetData(key, null); }
    }

    [Test]
    public void NestedScriptsRestoreTheOuterOutputScope()
    {
        var result = new PortableEngine().ExecuteScript("""
            Report("before");
            write(new PortableEngine().ExecuteScript("writeln(\"inner\");"));
            Report("after");
            """, "static void Report(string value) { writeln(value); }");
        Assert.That(result, Is.EqualTo("before" + Environment.NewLine + "inner" + Environment.NewLine + "after" + Environment.NewLine));
    }

    [TestCase("writeln(\"discarded\"); throw new InvalidOperationException(\"expected\");")]
    [TestCase("writeln(missing);")]
    public void FailedNestedScriptsRestoreTheOuterOutputScope(string inner)
    {
        var literal = Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(inner, true);
        var result = new PortableEngine().ExecuteScript($$"""
            Report("before");
            try { new PortableEngine().ExecuteScript({{literal}}); } catch (Exception) { }
            Report("after");
            """, "static void Report(string value) { writeln(value); }");
        Assert.That(result, Is.EqualTo("before" + Environment.NewLine + "after" + Environment.NewLine));
    }

    [Test]
    public void LateOutputFromAnInheritedExecutionContextIsIgnored()
    {
        var key = "portable-output-late-" + Guid.NewGuid().ToString("N");
        try
        {
            var engine = new PortableEngine();
            var first = engine.ExecuteScript($$"""
                var context = System.Threading.ExecutionContext.Capture();
                Action emit = () => { write("late-local"); Report(); };
                AppDomain.CurrentDomain.SetData("{{key}}", new Action(() =>
                    System.Threading.ExecutionContext.Run(context, _ => emit(), null)));
                write("complete");
                """, "static void Report() { write(\"late-function\"); writeln(\"late-line\"); }");
            Assert.That(first, Is.EqualTo("complete"));
            var next = engine.ExecuteScript($"""
                write("next:");
                dynamic delayed = AppDomain.CurrentDomain.GetData("{key}");
                delayed();
                writeln("ok");
                """);
            Assert.That(next, Is.EqualTo("next:ok" + Environment.NewLine));
        }
        finally { AppDomain.CurrentDomain.SetData(key, null); }
    }

    [Test]
    public void ExistingLocalWritePrecedenceIsPreserved()
    {
        var result = new PortableEngine().ExecuteScript("Report(); write(Seen);",
            "static string Seen; static void write(object value) { Seen = \"custom:\" + value; } static void Report() { write(\"helper\"); }");
        Assert.That(result, Is.EqualTo("custom:helper"));
    }

    [Test]
    public void ExistingCustomWriteLineStillTakesPrecedence()
    {
        var result = new PortableEngine().ExecuteScript("writeln(\"helper\"); write(Seen);",
            "static string Seen; static void writeln(object value) { Seen = \"custom:\" + value; }");
        Assert.That(result, Is.EqualTo("custom:helper"));
    }

    [TestCase("writeln(missing);", "", "script.tsl")]
    [TestCase("Report();", "static void Report() { writeln(missing); }", "custom-functions.tslf")]
    public void OutputErrorsStillIdentifyTheUserDocument(string script, string definitions, string file)
    {
        var error = Assert.Throws<EngineCompilationException>(() => new PortableEngine().ExecuteScript(script, definitions));
        Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.Code == "CS0103" && diagnostic.File == file && diagnostic.Line == 1), Is.True);
    }

    [Test]
    public void OutputHelpersAreNotImportedIntoExpressions()
    {
        var error = Assert.Throws<EngineCompilationException>(() => new PortableEngine().EvaluateReal("writeln(1)"));
        Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.Code == "CS0103"), Is.True);
    }
}
