using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class EntryPointOverloadTests
{
    private const string Definitions = "public static real Run() { return 4; }";
    private const string GenericDefinitions = """
        public static real Run<T>(real x, real y) { throw new InvalidOperationException("Wrong real entry point."); }
        public static complex Run<T>(complex z) { throw new InvalidOperationException("Wrong complex entry point."); }
        public static real Run<T>(real x) { throw new InvalidOperationException("Wrong numerical entry point."); }
        public static void Run<T>(Action<string> output) { throw new InvalidOperationException("Wrong script entry point."); }
        """;

    [Test]
    public void RealCalculationCanCallPublicRunOverload() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Run()+x+y", 2, 3, Definitions), Is.EqualTo(9));

    [Test]
    public void ComplexCalculationCanCallPublicRunOverload() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateComplex("Run()+z", new Complex(2, 3), Definitions), Is.EqualTo(new Complex(6, 3)));

    [Test]
    public void ScriptCanCallPublicRunOverload() =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Run());", Definitions), Is.EqualTo("4"));

    [Test]
    public void IntegralCanCallPublicRunOverload() =>
        NUnit.Framework.Assert.That(new PortableEngine().Integrate("Run()*x", 0, 1, 2, Definitions), Is.EqualTo(2).Within(1e-12));

    [Test]
    public void DerivativeCanCallPublicRunOverload() =>
        NUnit.Framework.Assert.That(new PortableEngine().Differentiate("Run()*x", 2, 1, Definitions), Is.EqualTo(4).Within(1e-10));

    [Test]
    public void RootCanCallPublicRunOverload() =>
        NUnit.Framework.Assert.That(new PortableEngine().FindRoot("x-Run()", 0, 8, customFunctions: Definitions), Is.EqualTo(4).Within(1e-8));

    [Test]
    public void GenericOverloadsWithMatchingParameterTypesDoNotHideEntryPoints()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("x+y", 2, 3, GenericDefinitions), Is.EqualTo(5));
        NUnit.Framework.Assert.That(engine.EvaluateComplex("z", new Complex(2, 3), GenericDefinitions), Is.EqualTo(new Complex(2, 3)));
        NUnit.Framework.Assert.That(engine.ExecuteScript("write(7);", GenericDefinitions), Is.EqualTo("7"));
        NUnit.Framework.Assert.That(engine.Integrate("x", 0, 1, 2, GenericDefinitions), Is.EqualTo(0.5));
    }

    [Test]
    public void PublicRunOverloadDoesNotHaveToBeReferenced() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("2+2", customFunctions: Definitions), Is.EqualTo(4));

    [Test]
    public void PrivateRunOverloadKeepsWorking() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Run()", customFunctions:
            "static real Run() { return 4; }"), Is.EqualTo(4));

    [Test]
    public void DuplicateEntryPointSignatureRemainsACompilationError()
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() => new PortableEngine().EvaluateReal("2+2",
            customFunctions: "public static real Run(real x, real y) { return 9; }"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.Code == "CS0111"), Is.True);
    }
}
