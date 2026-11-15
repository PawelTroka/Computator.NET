using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using Computator.NET.PortableWeb;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PhysicalConstantParityTests
{
    // These are the existing desktop values, including historical constants.
    // Portability must not silently replace them with newer SI definitions.
    [TestCase("c", 299792458.0)]
    [TestCase("g", 9.80625)]
    [TestCase("StandardAccelerationOfGravity", 9.80665)]
    [TestCase("PlanckConstant", 6.62606957e-34)]
    [TestCase("ElementaryCharge", 1.602176565e-19)]
    [TestCase("ElectronMass", 9.10938291e-31)]
    [TestCase("AvogadroConstant", 6.02214129e23)]
    [TestCase("StandardAtmosphere", 101325.0)]
    [TestCase("ElectronVolt", 1.602176565e-19)]
    [TestCase("CuXUnit", 1.00207697e-13)]
    public void PhysicalConstantsExecuteWithTheirStoredDesktopValues(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected));

    [Test]
    public void EveryPhysicalConstantIsAvailableInOneScript()
    {
        var type = typeof(PortableEngine).Assembly.GetType("Computator.NET.Core.Constants.PhysicalConstants", true)!;
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(double)).OrderBy(field => field.Name).ToArray();
        NUnit.Framework.Assert.That(fields, Has.Length.EqualTo(342));
        var source = string.Join("\n", fields.Select(field => $"write({field.Name}); write(\"\\n\");"));
        var expected = string.Concat(fields.Select(field => ((double)field.GetValue(null)!).ToString(CultureInfo.InvariantCulture) + "\n"));
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo(expected));
    }

    [Test]
    public void ComplexExpressionsCanUsePhysicalConstants()
    {
        var result = new PortableEngine().EvaluateComplex("c+z", new Complex(3, 4));
        NUnit.Framework.Assert.That(result, Is.EqualTo(new Complex(299792461, 4)));
    }

    [Test]
    public void CustomFunctionsCanUsePhysicalConstantsInExpressionsAndScripts()
    {
        const string functions = "static real TravelTime(real distance) { return distance/c; }";
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("TravelTime(c)", customFunctions: functions), Is.EqualTo(1));
        NUnit.Framework.Assert.That(engine.ExecuteScript("write(TravelTime(2c));", functions), Is.EqualTo("2"));
    }

    [Test]
    public void NumericalServicesCompilePhysicalConstants()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.Integrate("c*x", 0, 1, 2), Is.EqualTo(149896229).Within(1e-6));
        NUnit.Framework.Assert.That(engine.Differentiate("c*x", 0), Is.EqualTo(299792458).Within(1e-6));
        NUnit.Framework.Assert.That(engine.FindRoot("c*(x-1)", 0, 2), Is.EqualTo(1).Within(1e-8));
    }

    [Test]
    public void ExistingNamesInUserCodeKeepTheirScope()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("c", customFunctions: "static real c = 7;"), Is.EqualTo(7));
        NUnit.Framework.Assert.That(engine.ExecuteScript("var c = 3; write(c);"), Is.EqualTo("3"));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probe(5)", customFunctions: "static real Probe(real c) { return c; }"), Is.EqualTo(5));
    }

    [TestCase("pl-PL")]
    [TestCase("de-DE")]
    [NonParallelizable]
    public void PhysicalValuesAndScriptOutputAreCultureIndependent(string name)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
            NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(g); write(\"|\"); write(StandardAccelerationOfGravity);"),
                Is.EqualTo("9.80625|9.80665"));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void BothCompletionCatalogsExposeOnlyPhysicalValuesWithHistoricalContext(bool scripting)
    {
        var catalog = JsonSerializer.SerializeToElement(PortableCatalog.Items(scripting)).EnumerateArray().ToArray();
        var constants = catalog.Where(item => item.GetProperty("details").GetProperty("category").GetString() == "Physical constants").ToArray();
        NUnit.Framework.Assert.That(constants, Has.Length.EqualTo(342));
        NUnit.Framework.Assert.That(constants.Select(item => item.GetProperty("text").GetString()), Is.Unique);
        foreach (var item in constants)
        {
            NUnit.Framework.Assert.That(item.GetProperty("menuText").GetString(), Is.EqualTo(item.GetProperty("text").GetString()));
            NUnit.Framework.Assert.That(item.GetProperty("details").GetProperty("description").GetString(), Does.Contain("Stored desktop value:").And.Contain("historical"));
        }
        var speed = constants.Single(item => item.GetProperty("text").GetString() == "c");
        NUnit.Framework.Assert.That(speed.GetProperty("details").GetProperty("description").GetString(), Does.Contain("299792458"));
        NUnit.Framework.Assert.That(catalog.Select(item => item.GetProperty("text").GetString()),
            Does.Not.Contain("ToCode").And.Not.Contain("parseConstantsFromNIST"));
    }
}
