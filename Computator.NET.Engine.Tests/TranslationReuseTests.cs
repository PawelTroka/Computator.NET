using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class TranslationReuseTests
{
    [Test]
    public void AlternatingPlainEmptyAndProtectedFragmentsPreservesNotationAndText()
    {
        var compiler = new TslCompiler();
        const string literal = "write(\"2x x² ∞\");";

        NUnit.Framework.Assert.That(compiler.TransformToCSharp("2x"), Is.EqualTo("2*x"));
        NUnit.Framework.Assert.That(compiler.TransformToCSharp(literal), Is.EqualTo(literal));
        NUnit.Framework.Assert.That(compiler.TransformToCSharp("x²"), Is.EqualTo("pow(x,2)"));
        NUnit.Framework.Assert.That(compiler.TransformToCSharp(string.Empty), Is.Empty);
        NUnit.Framework.Assert.That(compiler.TransformToCSharp(literal), Is.EqualTo(literal));
        NUnit.Framework.Assert.That(compiler.TransformToCSharp("2x"), Is.EqualTo("2*x"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EmptyFragmentStaysEmptyWithSeparateCustomPowerNames(bool deferPowers)
    {
        const string custom = "static real sinᶻ(real x) { return x; }";
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(string.Empty, custom, deferPowers), Is.Empty);
    }

    [Test]
    public void OrdinaryScriptWithoutLiteralTextStillEvaluatesTranslatedNotation() =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("var x=3; write(2x+x²);"), Is.EqualTo("15"));
}
