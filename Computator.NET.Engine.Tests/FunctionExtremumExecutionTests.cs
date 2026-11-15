using System.Globalization;
using System.Text.Json;
using Computator.NET.PortableWeb;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class FunctionExtremumExecutionTests
{
    [TestCase("FunctionExtremum.minimize(t => (t-2)*(t-2),0,5)", 2)]
    [TestCase("FunctionExtremum.maximize(t => 7-(t-2)*(t-2),0,5)", 2)]
    [TestCase("FunctionExtremum.minimize(t => t,0,5)", 0)]
    [TestCase("FunctionExtremum.maximize(t => t,0,5)", 5)]
    public void ExpressionsUseTheSharedSolver(string source, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(source), Is.EqualTo(expected).Within(1e-7));
    }

    [Test]
    public void ScriptsCanEvaluateTheFunctionAtTheReturnedLocation()
    {
        var output = new PortableEngine().ExecuteScript("""
            function f(real t) = 7-(t-2)*(t-2);
            var location = FunctionExtremum.maximize(f,0,5);
            write(f(location));
            """);
        NUnit.Framework.Assert.That(double.Parse(output, CultureInfo.InvariantCulture), Is.EqualTo(7).Within(1e-12));
    }

    [Test]
    public void CustomDefinitionsSupplyTheObjective()
    {
        var engine = new PortableEngine();
        var result = engine.EvaluateReal("FunctionExtremum.minimize(Cost,0,5)", customFunctions:
            "static real Cost(real t) { return (t-2)*(t-2)+7; }");
        NUnit.Framework.Assert.That(result, Is.EqualTo(2).Within(1e-7));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void PortableCompletionUsesQualifiedCallableNames(bool scripting)
    {
        using var catalog = JsonDocument.Parse(JsonSerializer.Serialize(PortableCatalog.Items(scripting)));
        var entries = catalog.RootElement.EnumerateArray().Where(entry =>
            entry.GetProperty("details").GetProperty("category").GetString() == "Function extrema").ToArray();
        NUnit.Framework.Assert.That(entries.Select(entry => entry.GetProperty("text").GetString()),
            Is.EquivalentTo(new[] { "FunctionExtremum.minimize", "FunctionExtremum.maximize" }));
        foreach (var entry in entries)
        {
            var name = entry.GetProperty("text").GetString()!;
            var expected = name.EndsWith("minimize", StringComparison.Ordinal) ? 0 : 5;
            NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(name + "(t => t,0,5)"), Is.EqualTo(expected));
        }
    }
}
