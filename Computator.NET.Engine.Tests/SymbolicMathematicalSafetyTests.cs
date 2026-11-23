using AngouriMath;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class SymbolicMathematicalSafetyTests
{
    private readonly SymbolicEngine engine = new();

    private static Entity Read(SymbolicResult result) => MathS.FromString(SymbolicInput.Normalize(result.Result), useCache: false);

    [TestCase("real", "{ }")]
    [TestCase("complex", "[[-1, 0]]")]
    public void SystemSolutionsMustRespectTheOriginalRadicalDomain(string domain, string expected)
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Domain = domain,
            Equations = ["sqrt(x)^2=-1", "y=0"], Variables = ["x", "y"]
        });
        Check.That(Read(result), Is.EqualTo(MathS.FromString(expected)));
        Check.That(result.Status, Is.EqualTo("exact"));
        if (domain == "real") Check.That(result.Warnings.Any(warning => warning.StartsWith("Each row")), Is.False);
    }

    [Test]
    public void SystemSolutionsKeepValidRealRadicalValuesAndColumnOrder()
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Domain = "real",
            Equations = ["sqrt(x)^2=4", "y=3"], Variables = ["y", "x"]
        });
        Check.That(Read(result), Is.EqualTo(MathS.FromString("[[3,4]]")));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [TestCase("real", "{ }")]
    [TestCase("complex", "[[-1,1/2]]")]
    public void CandidateExponentSpecializationMustRetainItsNewRadicalDomain(string domain, string expected)
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Domain = domain,
            Equations = ["(x^y)^2=-1", "y=1/2"], Variables = ["x", "y"]
        });
        Check.That(Read(result), Is.EqualTo(MathS.FromString(expected)));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [Test]
    public void CandidateExponentSpecializationRetainsValidPositiveRadicals()
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Domain = "real",
            Equations = ["(x^y)^2=4", "y=1/2"], Variables = ["x", "y"]
        });
        Check.That(Read(result), Is.EqualTo(MathS.FromString("[[4,1/2]]")));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [TestCase("sqrt(x)^2")]
    [TestCase("sqrt(x)")]
    public void InactivePiecewiseRadicalBranchesDoNotRemoveValidNegativeSolutions(string positiveBranch)
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Domain = "real",
            Equations = [$"piecewise({positiveBranch} provided x>=0, -x provided x<0)=1", "y=0"], Variables = ["x", "y"]
        });
        Check.That(result.Status, Is.EqualTo("exact"));
        var solutions = (Entity.Matrix)Read(result);
        Check.That(solutions.RowCount, Is.EqualTo(2));
        Check.That(Enumerable.Range(0, 2).Select(row => solutions[row, 0]), Is.EquivalentTo(new Entity[] { -1, 1 }));
        Check.That(Enumerable.Range(0, 2).All(row => solutions[row, 1] == (Entity)0), Is.True);
    }

    [TestCase("real")]
    [TestCase("complex")]
    public void UndefinedSystemCoordinatesAreExcludedRatherThanCalledSolutions(string domain)
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Domain = domain,
            Equations = ["x^2=0", "1/x=y"], Variables = ["x", "y"]
        });
        Check.That(Read(result), Is.EqualTo(Entity.Set.Empty));
        Check.That(result.Status, Is.EqualTo("exact"));
        Check.That(result.Warnings.Any(warning => warning.StartsWith("Each row")), Is.False);
    }

    [Test]
    public void UnverifiedSystemFamilyRetainsTheOriginalExcludedPole()
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "solve-system", Equations = ["x/x=1", "y=0"], Variables = ["x", "y"]
        });
        Check.That(result.Status, Is.EqualTo("unevaluated"));
        Check.That(result.Result, Does.Contain("x / x"));
        Check.That(result.Warnings, Has.Some.Contains("domain restrictions"));
    }

    [TestCase("substitute", false)]
    [TestCase("substitute", true)]
    [TestCase("evaluate", false)]
    [TestCase("evaluate", true)]
    public void UnusedSubstitutionKeysCannotCaptureTemporaryNames(string operation, bool reverseOrder)
    {
        var replacements = new Dictionary<string, string>();
        if (reverseOrder) replacements["ComputatorSubstitution_1"] = "2";
        replacements["x"] = operation == "evaluate" ? "3" : "y";
        if (!reverseOrder) replacements["ComputatorSubstitution_1"] = "2";
        var result = engine.Execute(new SymbolicRequest { Operation = operation, Expression = "x", Substitutions = replacements });
        Check.That(result.Result, Is.EqualTo(operation == "evaluate" ? "3" : "y"));
        Check.That(result.Status, Is.EqualTo(operation == "evaluate" ? "approximate" : "exact"));
    }

    [Test]
    public void SimultaneousSwapStillWorksWithUnusedTemporaryNameKeys()
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = "x-y",
            Substitutions = new() { ["x"] = "y", ["y"] = "x", ["ComputatorSubstitution_1"] = "99", ["ComputatorSubstitution_2"] = "98" }
        });
        Check.That((Read(result) - MathS.FromString("y-x")).Simplify(), Is.EqualTo((Entity)0));
    }

    [TestCase("exp(a*x)", "0", "1", "0", "1")]
    [TestCase("x^a", "1", "2", "-1", "ln(2)")]
    [TestCase("exp(a*x)", "0", "1", "2", "(e^2-1)/2")]
    [TestCase("x^a", "1", "2", "2", "7/3")]
    public void ParameterIntegralSpecializationAgreesWithIntegratingTheSpecializedInput(
        string integrand, string lower, string upper, string parameter, string expected)
    {
        var generic = engine.Execute(new SymbolicRequest
        {
            Operation = "definite-integrate", Expression = integrand,
            LowerBound = lower, UpperBound = upper, Domain = "real"
        });
        Check.That(generic.Status, Is.EqualTo("unevaluated"));
        Check.That(Read(generic).Nodes.Any(node => node is Entity.Piecewise), Is.True);
        var specialized = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = generic.Result, Domain = "real",
            Substitutions = new() { ["a"] = parameter }
        });
        Check.That((Read(specialized) - MathS.FromString(expected)).Simplify(), Is.EqualTo((Entity)0));
        Check.That(specialized.Status, Is.EqualTo("exact"));
        var input = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = integrand, Domain = "real", Substitutions = new() { ["a"] = parameter }
        });
        var direct = engine.Execute(new SymbolicRequest
        {
            Operation = "definite-integrate", Expression = input.Result,
            LowerBound = lower, UpperBound = upper, Domain = "real"
        });
        Check.That((Read(specialized) - Read(direct)).Simplify(), Is.EqualTo((Entity)0));
    }

    [Test]
    public void ExceptionalDefiniteParameterBranchRetainsItsOriginalBounds()
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "definite-integrate", Expression = "exp(a*x)", LowerBound = "2", UpperBound = "5", Domain = "real"
        });
        var fallback = Read(result).Nodes.OfType<Entity.Integralf>().Single();
        Check.That(fallback.Range, Is.Not.Null);
        Check.That(fallback.Range!.Value.Item1, Is.EqualTo((Entity)2));
        Check.That(fallback.Range.Value.Item2, Is.EqualTo((Entity)5));
        var specialized = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = result.Result, Domain = "real", Substitutions = new() { ["a"] = "0" }
        });
        Check.That(specialized.Result, Is.EqualTo("3"));
    }

    [TestCase("exp(a*x)", "0", "1")]
    [TestCase("x^a", "-1", "1/x")]
    [TestCase("1/(a*x+b)", "0", "1/b")]
    public void ExceptionalIndefiniteParameterBranchStillDifferentiatesToTheIntegrand(string expression, string parameter, string expected)
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "integrate", Expression = expression, Domain = "real" });
        Check.That(result.Status, Is.EqualTo("unevaluated"));
        var specialized = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = result.Result, Domain = "real", Substitutions = new() { ["a"] = parameter }
        });
        var derivative = engine.Execute(new SymbolicRequest { Operation = "differentiate", Expression = specialized.Result, Domain = "real" });
        foreach (var x in new[] { -2, 2 })
        {
            var actual = Read(derivative).Substitute("x", x).Substitute("b", 2).InnerSimplified;
            var reference = MathS.FromString(expected).Substitute("x", x).Substitute("b", 2).InnerSimplified;
            Check.That((actual - reference).Simplify(), Is.EqualTo((Entity)0));
        }
    }

    [TestCase("simplify")]
    [TestCase("expand")]
    [TestCase("factor")]
    [TestCase("evaluate")]
    [TestCase("substitute")]
    public void AlgebraicReuseOfAGenericIntegralResultDoesNotLoseItsExceptionalParameterCase(string operation)
    {
        var generic = engine.Execute(new SymbolicRequest
        {
            Operation = "definite-integrate", Expression = "exp(a*x)", LowerBound = "0", UpperBound = "1", Domain = "real"
        });
        var simplified = engine.Execute(new SymbolicRequest { Operation = operation, Expression = generic.Result, Domain = "real" });
        var specialized = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = simplified.Result, Domain = "real", Substitutions = new() { ["a"] = "0" }
        });
        Check.That(specialized.Result, Is.EqualTo("1"));
    }

    [Test]
    public void PartialParameterSubstitutionRetainsTheRemainingExceptionalCase()
    {
        var generic = engine.Execute(new SymbolicRequest
        {
            Operation = "definite-integrate", Expression = "exp(a*b*x)", LowerBound = "0", UpperBound = "1", Domain = "real"
        });
        var partial = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = generic.Result, Domain = "real", Substitutions = new() { ["b"] = "2" }
        });
        var specialized = engine.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = partial.Result, Domain = "real", Substitutions = new() { ["a"] = "0" }
        });
        Check.That(partial.Status, Is.EqualTo("unevaluated"));
        Check.That(specialized.Result, Is.EqualTo("1"));
    }

    [Test]
    public void SafePolynomialCoefficientDoesNotMakeAnIntegralUnnecessarilyUnresolved()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "integrate", Expression = "x^2*y" });
        Check.That(result.Status, Is.EqualTo("exact"));
        Check.That((Read(result).Differentiate("x") - MathS.FromString("x^2*y")).Simplify(), Is.EqualTo((Entity)0));
        Check.That(Read(result).Nodes.Any(node => node is Entity.Piecewise), Is.False);
    }
}
