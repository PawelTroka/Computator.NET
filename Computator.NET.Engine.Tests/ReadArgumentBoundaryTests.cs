using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ReadArgumentBoundaryTests
{
    [TestCase("spread")]
    [TestCase("bread")]
    [TestCase("_read")]
    [TestCase("αread")]
    [TestCase("żread")]
    [TestCase("e\u0301read")]
    [TestCase("a\u200Cread")]
    [TestCase("@spread")]
    public void ReadSuffixedCustomFunctionsKeepReferenceArguments(string name)
    {
        var definitions = "public static void " + name + "(ref integer value) { value += 2; }";
        var script = "integer value=5; " + name + "(&value); write(value);";

        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("7"));
    }

    [TestCase("spread(&value)")]
    [TestCase("spread (\n\t&value)")]
    [TestCase("spread(/* argument */ &value)")]
    [TestCase("spread(value: &value)")]
    [TestCase("@spread(&value)")]
    public void MemberCallsKeepReferenceArguments(string call)
    {
        const string definitions = """
            public sealed class Reader
            {
                public void spread(ref integer value) { value += 2; }
            }
            """;
        var script = "integer value=5; var reader=new Reader(); reader." + call + "; write(value);";

        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("7"));
    }

    [Test]
    public void CustomDefinitionsUseTheSameReferenceArgumentRule()
    {
        const string definitions = """
            public static void spread(ref integer value) { value += 2; }
            public static integer Probe() { integer value=5; spread(&value); return value; }
            """;

        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Probe()", customFunctions: definitions),
            Is.EqualTo(7));
    }

    [Test]
    public void ReferenceTargetsAreEvaluatedOnceAndKeepTheirInitialValue()
    {
        const string definitions = "public static void spread(ref integer value) { value += 2; }";
        const string script = """
            var values=new integer[]{5,20};
            var index=0;
            spread(&values[index++]);
            write(values[0]); write(":"); write(values[1]); write(":"); write(index);
            """;

        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("7:20:1"));
    }

    [TestCase("read(&value)")]
    [TestCase("@read(&value)")]
    [TestCase("read (\n\t&value)")]
    [TestCase("read(/* argument */ &value)")]
    public void ExactReadNameKeepsItsOutArgumentConvention(string call)
    {
        const string definitions = "public static void read(out integer value) { value=42; }";

        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("integer value; " + call + "; write(value);",
            definitions), Is.EqualTo("42"));
    }

    [Test]
    public void QualifiedExactReadNameKeepsItsExistingOutArgumentConvention()
    {
        const string definitions = """
            public static class Reader
            {
                public static void read(out integer value) { value=42; }
            }
            """;

        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "integer value; Reader.read(&value); write(value);", definitions), Is.EqualTo("42"));
    }

    [Test]
    public void TranslationKeepsTheWholeCustomNameAndTheRefModifier()
    {
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp("spread(&value);"),
            Is.EqualTo("spread( ref value);"));
    }
}
