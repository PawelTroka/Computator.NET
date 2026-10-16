using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ScriptLiteralTests
{
    [TestCase("""write("%2F 2x 6/2 x² ∞ · matrix({");""", "%2F 2x 6/2 x² ∞ · matrix({")]
    [TestCase(""""write(@"C:\2x\6/2 ""∞""");"""", "C:\\2x\\6/2 \"∞\"")]
    [TestCase("write(\"\"\"2x \"/2\" ∞\"\"\");", "2x \"/2\" ∞")]
    [TestCase("""write('∞'); write('·'); write('²');""", "∞·²")]
    [TestCase("""var x=3; write($"2x {2x} 6/2 {6/2} x² {x²} ∞");""", "2x 6 6/2 3 x² 9 ∞")]
    [TestCase(""""write($@"2x {6/2} ""∞""");"""", "2x 3 \"∞\"")]
    [TestCase("var x=3; write($$\"\"\"2x {literal} {{2x}} ∞\"\"\");", "2x {literal} 6 ∞")]
    [TestCase("""write($"{6/2:0.0'2x'}");""", "3.02x")]
    [TestCase("""write(System.Text.Encoding.UTF8.GetString("2x ∞"u8));""", "2x ∞")]
    [TestCase("""write($"x {System.String.Concat("2x","/2")} {6/2}");""", "x 2x/2 3")]
    [TestCase("write(\"\"\"\n2x\n∞\n\"\"\");", "2x\n∞")]
    [TestCase("write(System.Text.Encoding.UTF8.GetString(\"\"\"2x ∞\"\"\"u8));", "2x ∞")]
    [TestCase("write(System.Text.Encoding.UTF8.GetString(\"\"\"\n2x\n∞\n\"\"\"u8));", "2x\n∞")]
    [TestCase("#if false\nwrite(\"x² ∞ 6/2\");\n#endif\nwrite(6/2);", "3")]
    [TestCase("/* | ) matrix({ \n∞ 2x */ write(6 /* 2x */ / 2); // \" x² ∞\nwrite(2·3);", "36")]
    public void LiteralTextAndCommentsStaySeparateFromExecutableNotation(string script, string expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));

    [Test]
    public void StringReturningCustomDefinitionsPreserveTheirLiteralText()
    {
        var result = new PortableEngine().ExecuteScript("write(Probe());",
            """static string Probe() { return "%2F 6/2 ∞"; }""");
        NUnit.Framework.Assert.That(result, Is.EqualTo("%2F 6/2 ∞"));
    }

    [Test]
    public void CommentRemovalKeepsDiagnosticLineNumbers()
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().ExecuteScript("// 2x ∞\nwrite(\"2x\");\nwrite(missing);"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic =>
            diagnostic.File == "script.tsl" && diagnostic.Line == 3 && diagnostic.Code == "CS0103"), Is.True);
    }
}
