using Computator.NET.Core.Functions;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;
namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PortableScienceTests
{
    [Test]
    public void DesktopScientificTypesAreOwnedByThePortableAssembly()
    {
        foreach (var type in new[] { typeof(MatrixFunctions), typeof(Integral), typeof(Derivative), typeof(FunctionRoot) })
            NUnit.Framework.Assert.That(type.Assembly, Is.SameAs(typeof(PortableEngine).Assembly));
    }
    [Test]
    public void MatrixDeterminantAndLinearSolveUseTheExistingImplementation()
    {
        var matrix = MatrixFunctions.matrix(new double[,] { { 3, 1 }, { 1, 2 } });
        NUnit.Framework.Assert.That(MatrixFunctions.det(matrix), Is.EqualTo(5).Within(1e-12));
        var answer = matrix.Solve(MatrixFunctions.vector(9.0, 8.0));
        NUnit.Framework.Assert.That(answer[0], Is.EqualTo(2).Within(1e-12));
        NUnit.Framework.Assert.That(answer[1], Is.EqualTo(3).Within(1e-12));
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("det(matrix({{3,1},{1,2}}))"), Is.EqualTo(5).Within(1e-12));
    }
    [TestCase("x²", 0.0, 1.0, 1.0 / 3)]
    [TestCase("sin(x)", 0.0, 3.141592653589793, 2.0)]
    public void IntegrationPreservesKnownResults(string source, double a, double b, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().Integrate(source, a, b), Is.EqualTo(expected).Within(1e-10));
    [Test]
    public void DerivativeAndRootPreserveCustomDefinitions()
    {
        var engine = new PortableEngine();
        const string functions = "static real Probe(real x) { return x*x; }";
        NUnit.Framework.Assert.That(engine.Differentiate("Probe(x)", 3, customFunctions: functions), Is.EqualTo(6).Within(1e-7));
        NUnit.Framework.Assert.That(engine.FindRoot("Probe(x)-2", 0, 2, customFunctions: functions), Is.EqualTo(Math.Sqrt(2)).Within(1e-8));
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => engine.Differentiate("Probe(x)", 3));
    }
    [Test]
    public void InvalidWorkBudgetsAndIntervalsFailBeforeExecution()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.Throws<ArgumentException>(() => engine.Integrate("missing()", 2, 1));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => engine.Integrate("missing()", 0, 1, 3));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => engine.FindRoot("missing()", 0, 1, 0));
    }
}
