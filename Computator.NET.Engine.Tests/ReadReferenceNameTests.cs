using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ReadReferenceNameTests
{
    [TestCase("spread")]
    [TestCase("thread")]
    [TestCase("readread")]
    [TestCase("_read")]
    [TestCase("step2read")]
    [TestCase("λread")]
    [TestCase("e\u0301read")]
    [TestCase("Ⅻread")]
    [TestCase("step\u200Dread")]
    [TestCase("@spread")]
    public void LongerFunctionNamesKeepReferenceShorthand(string name)
    {
        var functions = "public static void " + name + "(ref integer value) { value+=2; }";
        var script = "integer value=5; " + name + "(&value); write(value);";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, functions), Is.EqualTo("7"));
    }

    [Test]
    public void ReferenceCallsInsideCustomFunctionsKeepTheirTargetAndEvaluationCount()
    {
        const string functions = """
            public static void spread(ref integer value) { value+=2; }
            public static integer Probe()
            {
                var values = new integer[] { 5, 10 };
                var index = 0;
                spread(/* reference */ &values[index++]);
                return values[0]*10+index;
            }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Probe()", customFunctions: functions),
            Is.EqualTo(71));
    }

    [TestCase("read(&value)")]
    [TestCase("read (/* input */ &value)")]
    [TestCase("@read(&value)")]
    [TestCase("Provider.read(&value)")]
    public void ExactReadNamesStillUseOutShorthand(string call)
    {
        const string functions = """
            public static void read(out integer value) { value=42; }
            public static class Provider
            {
                public static void read(out integer value) { value=42; }
            }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "integer value; " + call + "; write(value);", functions), Is.EqualTo("42"));
    }

    [Test]
    public void SharedTranslationSeparatesReadFromMemberNamesAndPreservesLiteralText()
    {
        const string script = """
            Provider.spread(&value); spread(value: &value); read(&input);
            write("spread(&value); read(&input)");
            """;
        var translated = new TslCompiler().TransformToCSharp(script);
        NUnit.Framework.Assert.That(translated, Does.Contain("Provider.spread( ref value)"));
        NUnit.Framework.Assert.That(translated, Does.Contain("spread(value:  ref value)"));
        NUnit.Framework.Assert.That(translated, Does.Contain("read( out input)"));
        NUnit.Framework.Assert.That(translated, Does.Contain("\"spread(&value); read(&input)\""));
    }
}
