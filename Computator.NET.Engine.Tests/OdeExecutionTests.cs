using System.Globalization;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class OdeExecutionTests
{
    [TestCase("DifferentialEquation.rungeKutta4((t,u) => u,1.0,0.0,1.0,101)[100]", 2.718281828459045, 3e-9)]
    [TestCase("DifferentialEquation.rungeKutta4((t,u) => 2*t,3.0,0.0,2.0,21)[20]", 7.0, 2e-13)]
    [TestCase("DifferentialEquation.rungeKutta4((t,u) => 3.0,5.0,2.0,1.0,2)[1]", 2.0, 1e-14)]
    public void RealExpressionsExposeScalarInitialValueSolutions(string expression, double expected, double tolerance) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected).Within(tolerance));

    [Test]
    public void ScriptCanSolveACoupledOscillator()
    {
        const string script = """
            var solution = DifferentialEquation.rungeKutta4(
                (t,state) => vector(state[1],-state[0]), vector(1.0,0.0), 0, 2*PI, 401);
            write(solution[400][0]);
            write(";");
            write(solution[400][1]);
            """;
        var values = new PortableEngine().ExecuteScript(script).Split(';').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        NUnit.Framework.Assert.That(values[0], Is.EqualTo(1).Within(4e-9));
        NUnit.Framework.Assert.That(values[1], Is.EqualTo(0).Within(4e-9));
    }

    [Test]
    public void CustomDefinitionsCanSupplyTheDerivative()
    {
        const string custom = "static real Velocity(real t,real u) { return 2*t; }";
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("DifferentialEquation.rungeKutta4(Velocity,3,0,2,21)[20]", customFunctions: custom),
            Is.EqualTo(7).Within(2e-13));
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => engine.EvaluateReal("DifferentialEquation.rungeKutta4(Velocity,3,0,2,21)[20]"));
    }

    [Test]
    public void ScriptUsesDefaultSampleCountAndIncludesTheInitialValue()
    {
        var answer = new PortableEngine().ExecuteScript("var values = DifferentialEquation.rungeKutta4((t,u) => 0.0,5.0,0,1); write(values.Length); write(\";\"); write(values[0]); write(\";\"); write(values[100]);");
        NUnit.Framework.Assert.That(answer, Is.EqualTo("101;5;5"));
    }
}
