using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableDistributionTests
{
    [TestCase("Normal.CDF(0,1,0)", 0.5)]
    [TestCase("Normal.PDF(0,1,0)", 0.3989422804014327)]
    [TestCase("Normal.InvCDF(0,1,0.5)", 0)]
    [TestCase("Binomial.PMF(0.5,4,2)", 0.375)]
    [TestCase("Poisson.PMF(2,0)", 0.1353352832366127)]
    [TestCase("ContinuousUniform.CDF(-2,2,0)", 0.5)]
    public void DesktopDistributionNamesWorkInRealExpressions(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression),
            Is.EqualTo(expected).Within(1e-14));

    [Test]
    public void DistributionValuesComposeWithComplexExpressions()
    {
        var actual = new PortableEngine().EvaluateComplex("Normal.CDF(0,1,0)+z", new Complex(2, 3));
        NUnit.Framework.Assert.That(actual, Is.EqualTo(new Complex(2.5, 3)));
    }

    [Test]
    public void ScriptsCanConstructContinuousAndDiscreteDistributions()
    {
        const string script = """
            IContinuousDistribution bell = new Normal(2,3);
            IDiscreteDistribution coins = new Binomial(0.5,4);
            write(bell.CumulativeDistribution(2));
            write(",");
            write(coins.Probability(2));
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("0.5,0.375"));
    }

    [Test]
    public void CustomFunctionsCanReturnAndUseDistributionTypes()
    {
        const string definitions = """
            static Normal Bell() { return new Normal(0,1); }
            static real Probability(real value) { return Bell().CumulativeDistribution(value); }
            """;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probability(0)", customFunctions: definitions), Is.EqualTo(0.5));
        NUnit.Framework.Assert.That(engine.ExecuteScript("write(Probability(0));", definitions), Is.EqualTo("0.5"));
    }

    [TestCase("integral", 0.6826894921370859, 1e-10)]
    [TestCase("derivative", 0.3989422804014327, 1e-7)]
    [TestCase("root", 0, 1e-8)]
    public void NumericalServicesCanUseDistributionFunctions(string operation, double expected, double tolerance)
    {
        var engine = new PortableEngine();
        var actual = operation switch
        {
            "integral" => engine.Integrate("Normal.PDF(0,1,x)", -1, 1, 1000),
            "derivative" => engine.Differentiate("Normal.CDF(0,1,x)", 0),
            "root" => engine.FindRoot("Normal.CDF(0,1,x)-0.5", -3, 4),
            _ => throw new ArgumentException(nameof(operation))
        };
        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(tolerance));
    }

    [Test]
    public void CustomTypesStillTakePrecedenceOverImportedDistributionNames()
    {
        const string definitions = "static class Normal { public static real CDF(real value) { return value+7; } }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Normal.CDF(5)", customFunctions: definitions), Is.EqualTo(12));
    }

    [Test]
    public void FullyQualifiedDistributionCallsRemainAvailable() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("MathNet.Numerics.Distributions.Normal.CDF(0,1,0)"), Is.EqualTo(0.5));

    [Test]
    public void InvalidParametersKeepTheDistributionExceptionContract() =>
        NUnit.Framework.Assert.Throws<ArgumentException>(() => new PortableEngine().EvaluateReal("new Normal(0,-1).Density(0)"));

    [Test]
    public void UnknownMembersStillReportTheExpressionDocument()
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateReal("Normal.MissingMember(0)"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.Code == "CS0117" &&
            diagnostic.File == "expression.tsl" && diagnostic.Line == 1), Is.True);
    }
}
