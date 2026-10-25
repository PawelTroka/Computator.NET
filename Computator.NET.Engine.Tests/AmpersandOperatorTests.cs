using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class AmpersandOperatorTests
{
    [TestCase("a&b", "1")]
    [TestCase("a &b", "1")]
    [TestCase("a& b", "1")]
    [TestCase("a & b", "1")]
    [TestCase("a\t&b", "1")]
    [TestCase("a\n&b", "1")]
    [TestCase("a /* mask */ &b", "1")]
    [TestCase("(a) &b", "1")]
    [TestCase("a &~b", "4")]
    [TestCase("a &b &c", "0")]
    public void BinaryMasksAreIndependentOfWhitespace(string expression, string expected)
    {
        var script = "integer a=5, b=3, c=2; write(" + expression + ");";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));
    }

    [TestCase("false &Probe(true)", "False1")]
    [TestCase("true &Probe(false)", "False1")]
    [TestCase("true &Probe(true)", "True1")]
    [TestCase("false &&Probe(true)", "False0")]
    [TestCase("true &&Probe(false)", "False1")]
    public void BooleanAndKeepsItsEvaluationSemantics(string expression, string expected)
    {
        const string functions = """
            public static int Calls;
            public static bool Probe(bool value) { Calls++; return value; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "write(" + expression + "); write(Calls);", functions), Is.EqualTo(expected));
    }

    [Test]
    public void BinaryAndInCustomDefinitionsAndMixedNotationExecutesNormally()
    {
        const string functions = """
            public static integer Mask(integer value, integer mask) { return value &mask; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Mask(5,3) + 2x²", 3,
            customFunctions: functions), Is.EqualTo(19.0));
    }

    [TestCase("Change(&value)")]
    [TestCase("Change( &value)")]
    [TestCase("Change(\n\t&value)")]
    [TestCase("Change(/* argument */ &value)")]
    [TestCase("Change(value: &value)")]
    [TestCase("Change(value:\n\t&value)")]
    public void ExistingReferenceArgumentShorthandIsPreserved(string call)
    {
        const string functions = "public static void Change(ref integer value) { value+=2; }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "integer value=5; " + call + "; write(value);", functions), Is.EqualTo("7"));
    }

    [Test]
    public void LaterReferenceArgumentsPreserveTargetEvaluationAndBinaryMasks()
    {
        const string functions = """
            public static void Change(integer mask, ref integer value, ref integer other)
            { value+=mask; other+=mask; }
            """;
        const string script = """
            integer a=5, b=3;
            var values=new integer[]{10,20};
            var index=0;
            Change(a &b, &values[index++], &values[index++]);
            write(values[0]); write(","); write(values[1]); write(","); write(index);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, functions), Is.EqualTo("11,21,2"));
    }

    [TestCase("read(&value)")]
    [TestCase("read (\n\t&value)")]
    [TestCase("read(/* argument */ &value)")]
    [TestCase("read(out value)")]
    public void ExistingReadOutArgumentConventionIsPreserved(string call)
    {
        const string functions = "public static void read(out integer value) { value=42; }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "integer value; " + call + "; write(value);", functions), Is.EqualTo("42"));
    }

    [Test]
    public void ExplicitRefAndOutArgumentsRemainUnchanged()
    {
        const string functions = """
            public static void Change(ref integer value, out integer previous)
            { previous=value; value+=2; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "integer value=5; integer previous; Change(ref value, out previous); write(value); write(previous);",
            functions), Is.EqualTo("75"));
    }

    [Test]
    public void LiteralAndCommentAmpersandsDoNotBecomeArguments()
    {
        const string script = """
            integer a=5, b=3;
            write("a &b; Change(&value); read(&value)");
            // &ref and read(&out) are not executable here.
            write(a /* &b */ &b);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script),
            Is.EqualTo("a &b; Change(&value); read(&value)1"));
    }
}
