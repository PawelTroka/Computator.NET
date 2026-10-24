using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class BraceFoldingRangesTests
{
    [TestCase("\n")]
    [TestCase("\r\n")]
    [TestCase("\r")]
    public void NestedMultilineBlocksAreSortedAndSingleLineBlocksAreNotFolded(string newline)
    {
        var inner = "{" + newline + "write(π·2);" + newline + "}";
        var text = "if (true) {" + newline + "if (true) " + inner + newline + "if (true) { write(1); }" + newline + "}";
        var ranges = BraceFoldingRanges.Create(text, '{', '}');

        NUnit.Framework.Assert.That(ranges.Count, Is.EqualTo(2));
        NUnit.Framework.Assert.That(text.Substring(ranges[0].Start, ranges[0].Length),
            Is.EqualTo(text.Substring(text.IndexOf('{'))));
        NUnit.Framework.Assert.That(text.Substring(ranges[1].Start, ranges[1].Length), Is.EqualTo(inner));
    }

    [TestCase("write(\"} \\\" {\");")]
    [TestCase("var c = '}';")]
    [TestCase("var c = '\\\''; // } {")]
    [TestCase("// }\n// {")]
    [TestCase("/* }\n { */")]
    [TestCase("/// <summary> } { </summary>")]
    [TestCase("/// <summary>}</summary>\n/// <summary>{</summary>")]
    [TestCase("write(@\"}\n\"\"{\");")]
    [TestCase("write(\"\"\"\n}\n{\n\"\"\");")]
    [TestCase("write($\"}} {1 + 2} {{\");")]
    [TestCase("write($@\"}}\n{1 + 2}\n{{\");")]
    [TestCase("write($$\"\"\"\n}\n{{1 + 2}}\n{\n\"\"\");")]
    public void LiteralAndCommentBracesCannotCloseOrCreateCodeFolds(string content)
    {
        var text = "if (true) {\n" + content + "\nwrite(42);\n}";
        var ranges = BraceFoldingRanges.Create(text, '{', '}');

        NUnit.Framework.Assert.That(ranges.Count, Is.EqualTo(1));
        NUnit.Framework.Assert.That(ranges[0].Start, Is.EqualTo(text.IndexOf('{')));
        NUnit.Framework.Assert.That(ranges[0].End, Is.EqualTo(text.Length));
    }

    [TestCase("")]
    [TestCase("if (true) {\nwrite(1);")]
    [TestCase("}\nwrite(1);")]
    [TestCase("write(@\"{\n}\");")]
    [TestCase("/* {\n} */")]
    [TestCase("#if false\n{\n}\n#endif")]
    [TestCase("/// <summary>{</summary>\n/// <summary>}</summary>")]
    public void MissingAndNonCodeBlocksDoNotProduceRanges(string text)
    {
        NUnit.Framework.Assert.That(BraceFoldingRanges.Create(text, '{', '}'), Is.Empty);
    }

    [Test]
    public void CompleteNestedBlockSurvivesAnUnfinishedOuterBlock()
    {
        const string text = "if (true) {\nif (true) {\nwrite(42);\n}";
        var range = BraceFoldingRanges.Create(text, '{', '}').Single();
        NUnit.Framework.Assert.That(range.Start, Is.EqualTo(text.LastIndexOf('{')));
        NUnit.Framework.Assert.That(range.End, Is.EqualTo(text.Length));
    }

    [Test]
    public void ConfigurableBraceCharactersKeepTheirExistingMeaning()
    {
        const string text = "var numbers = [\n1, 2\n];";
        var range = BraceFoldingRanges.Create(text, '[', ']').Single();
        NUnit.Framework.Assert.That(text.Substring(range.Start, range.Length), Is.EqualTo("[\n1, 2\n]"));
    }
}
