using System.Globalization;
using System.Numerics;
using AngouriMath;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class SymbolicEngineTests
{
    private readonly SymbolicEngine engine = new();

    private SymbolicResult Run(string operation, string expression, string variable = "x", string domain = "complex") =>
        engine.Execute(new SymbolicRequest { Operation = operation, Expression = expression, Variable = variable, Domain = domain });

    private static Entity Read(SymbolicResult result) => MathS.FromString(result.Result, useCache: false);
    private static double At(Entity expression, double x, double y = 2) =>
        (double)expression.Substitute("x", x).Substitute("y", y).Substitute("C", 0).EvalNumerical();

    [TestCase("1/3+1/6", "1/2")]
    [TestCase("0.1+0.2", "3/10")]
    [TestCase(".5+.25", "3/4")]
    [TestCase("1e-3 + 2e-3", "3/1000")]
    [TestCase("2.5E+3", "2500")]
    [TestCase("1_000/2", "500")]
    [TestCase("123456789.123456789", "123456789123456789/1000000000")]
    [TestCase("0.333333333333333333333333333333", "333333333333333333333333333333/1000000000000000000000000000000")]
    [TestCase("2^3^2", "512")]
    [TestCase("-2²", "-4")]
    [TestCase("(-2)²", "4")]
    [TestCase("2⁻²", "1/4")]
    [TestCase("2⁽¹⁺²⁾", "8")]
    [TestCase("sin²(π/2)+cos²(π/2)", "1")]
    [TestCase("cos²(sin²(0))", "1")]
    [TestCase("√(9)", "3")]
    [TestCase("2×3 + 4÷2 − 1", "7")]
    [TestCase("log10(100)+log2(8)", "5")]
    [TestCase("log10²(100)", "4")]
    [TestCase("|-3|", "3")]
    [TestCase("|-3+|-2||", "1")]
    [TestCase("ln(exp(2))", "2")]
    [TestCase("sqrt(-1)", "i")]
    [TestCase("(1+i)^2", "2*i")]
    [TestCase("gamma(5)", "24")]
    [TestCase("5!", "120")]
    public void ExactArithmeticAndNotationMatchKnownValues(string input, string expected)
    {
        var result = Run("simplify", input);
        Check.That(Read(result).InnerSimplified, Is.EqualTo(MathS.FromString(expected).InnerSimplified), input);
        Check.That(result.Status, Is.EqualTo("exact"));
        Check.That(result.Latex, Is.Not.Empty);
    }

    [TestCase("2x+3(x+1)", 3, 18)]
    [TestCase("(x+1)2", 3, 8)]
    [TestCase("x²y³", 3, 72)]
    [TestCase("x⁻²", 2, .25)]
    [TestCase("-x²", 3, -9)]
    [TestCase("(-x)²", 3, 9)]
    [TestCase("sin⁽²⁾(x)", 0, 0)]
    public void UnicodeAndImplicitProductsPreservePrecedence(string input, double x, double expected) =>
        Check.That(At(Read(Run("simplify", input)), x), Is.EqualTo(expected).Within(1e-12));

    [Test]
    public void SimplificationRetainsTheExcludedPole()
    {
        var result = Run("simplify", "x/x");
        Check.That(result.Warnings.Any(warning => warning.Contains("conditions")), Is.True);
        Check.That(At(Read(result), 2), Is.EqualTo(1));
        Check.That(Read(result).Substitute("x", 0).InnerSimplified.IsNaN, Is.True);
    }

    [Test]
    public void SquareRootDoesNotInventPositivity()
    {
        var result = Run("simplify", "sqrt(x^2)");
        Check.That(At(Read(result), -3), Is.EqualTo(3));
        Check.That(Read(result), Is.Not.EqualTo(MathS.Var("x")));
    }

    [TestCase("simplify")]
    [TestCase("evaluate")]
    public void NonrealNumericResultIsUndefinedInRealDomain(string operation)
    {
        var result = Run(operation, "sqrt(-1)", domain: "real");
        Check.That(result.Status, Is.EqualTo("undefined"));
        Check.That(Read(result).IsNaN, Is.True);
    }

    [TestCase("x^2", "2*x")]
    [TestCase("sin(x)", "cos(x)")]
    [TestCase("cos(x)", "-sin(x)")]
    [TestCase("exp(x)", "exp(x)")]
    [TestCase("ln(x)", "1/x")]
    [TestCase("sin(x^2)", "2*x*cos(x^2)")]
    [TestCase("x^2*y^3", "2*x*y^3")]
    [TestCase("x^x", "x^x*(1+ln(x))")]
    [TestCase("1/(1+x)", "-1/(1+x)^2")]
    [TestCase("7", "0")]
    public void DerivativesMatchIndependentAnalyticExpressions(string expression, string expected)
    {
        var result = Run("differentiate", expression);
        Check.That(result.Status, Is.EqualTo("exact"));
        var actual = Read(result);
        var reference = MathS.FromString(expected);
        foreach (var point in new[] { .2, .75, 1.25, 2.0 })
            Check.That(At(actual, point), Is.EqualTo(At(reference, point)).Within(1e-10), expression + " at " + point);
    }

    [Test]
    public void HigherAndPartialDerivativesRespectTheSelectedVariable()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "differentiate", Expression = "x^5", Order = 3 });
        Check.That(At(Read(result), 2), Is.EqualTo(240));
        result = Run("differentiate", "x^2*y^3", "y");
        Check.That(At(Read(result), 2, 3), Is.EqualTo(108));
        result = Run("differentiate", "theta^3", "theta");
        Check.That((double)Read(result).Substitute("theta", 2).EvalNumerical(), Is.EqualTo(12));
    }

    [TestCase("x^2", "x^3/3")]
    [TestCase("sin(x)", "-cos(x)")]
    [TestCase("cos(x)", "sin(x)")]
    [TestCase("exp(x)", "exp(x)")]
    [TestCase("1/x", "ln(x)")]
    [TestCase("x*exp(x)", "(x-1)*exp(x)")]
    [TestCase("x^2*y", "y*x^3/3")]
    public void AntiderivativesMatchKnownPrimitivesWithOneConstant(string expression, string expected)
    {
        var result = Run("integrate", expression);
        var actual = Read(result);
        Check.That(result.Status, Is.EqualTo("exact"));
        Check.That(actual.Vars.Count(variable => variable.Name == "C"), Is.EqualTo(1));
        foreach (var point in new[] { .25, 1.0, 2.0 })
            Check.That(At(actual, point), Is.EqualTo(At(MathS.FromString(expected), point)).Within(1e-10));
    }

    [TestCase("x^2", "0", "3", "9")]
    [TestCase("x^2", "3", "0", "-9")]
    [TestCase("x^2", "2", "2", "0")]
    [TestCase("sin(x)", "0", "pi", "2")]
    [TestCase("exp(-x)", "0", "+oo", "1")]
    [TestCase("ln(x)", "0", "1", "-1")]
    [TestCase("1/x^2", "-1", "1", "+oo")]
    [TestCase("floor(x)", "0", "3", "3")]
    public void DefiniteIntegralsHandleOrientationEndpointsAndDiscontinuities(string expression, string lower, string upper, string expected)
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "definite-integrate", Expression = expression, LowerBound = lower, UpperBound = upper });
        Check.That(Read(result).InnerSimplified, Is.EqualTo(MathS.FromString(expected).InnerSimplified));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [Test]
    public void DivergentOppositeSidesAreNotReportedAsAPrincipalValue()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "definite-integrate", Expression = "1/x", LowerBound = "-1", UpperBound = "1" });
        Check.That(result.Status, Is.EqualTo("undefined"));
        Check.That(Read(result).IsNaN, Is.True);
    }

    [TestCase("sin(x)/x", "0", "both", "1")]
    [TestCase("(exp(x)-1)/x", "0", "both", "1")]
    [TestCase("(2*x^2+3)/(3*x^2-1)", "+oo", "both", "2/3")]
    [TestCase("1/x", "0", "right", "+oo")]
    [TestCase("1/x", "0", "left", "-oo")]
    public void LimitsRespectTheirApproach(string expression, string point, string direction, string expected)
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "limit", Expression = expression, Point = point, Direction = direction });
        Check.That(Read(result).InnerSimplified, Is.EqualTo(MathS.FromString(expected).InnerSimplified));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [TestCase("left", "unevaluated")]
    [TestCase("both", "unevaluated")]
    [TestCase("right", "exact")]
    public void RealSquareRootLimitDoesNotApproachThroughNonrealValues(string direction, string status)
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "limit", Expression = "sqrt(x)", Point = "0", Direction = direction, Domain = "real" });
        Check.That(result.Status, Is.EqualTo(status));
        if (direction == "right") Check.That(result.Result, Is.EqualTo("0"));
        else Check.That(Read(result).Nodes.Any(node => node is Entity.Limitf), Is.True);
    }

    [Test]
    public void RealDefiniteIntegralRejectsANonrealPartOfTheInterval()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "definite-integrate", Expression = "sqrt(x)", LowerBound = "-1", UpperBound = "1", Domain = "real" });
        Check.That(result.Status, Is.EqualTo("undefined"));
    }

    [TestCase("x^2=4", "{ -2, 2 }")]
    [TestCase("(x-1)^2=0", "{ 1 }")]
    [TestCase("x^3-6*x^2+11*x-6", "{ 1, 2, 3 }")]
    [TestCase("x^2+1=0", "{ i, -i }")]
    [TestCase("1=0", "{ }")]
    [TestCase("x=x", "CC")]
    [TestCase("1/x=0", "{ }")]
    [TestCase("sqrt(x)=-1", "{ }")]
    [TestCase("(x^2-1)/(x-1)=0", "{ -1 }")]
    public void ExactSolutionsDoNotInventOrLoseFiniteRoots(string equation, string expected)
    {
        var result = Run("solve", equation);
        Check.That(Read(result).InnerSimplified, Is.EqualTo(MathS.FromString(expected).InnerSimplified), result.Result);
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [Test]
    public void SolvingAnIdentityKeepsTheOriginalExcludedPole()
    {
        var result = Read(Run("solve", "x/x=1"));
        Check.That(((Entity)0).In(result).InnerSimplified, Is.EqualTo(Entity.Boolean.False));
        Check.That(((Entity)2).In(result).InnerSimplified, Is.EqualTo(Entity.Boolean.True));
    }

    [Test]
    public void RealSolutionsAndInequalitiesRetainDomainAndEndpoints()
    {
        Check.That(Read(Run("solve", "x^2+1=0", domain: "real")), Is.EqualTo(Entity.Set.Empty));
        var result = Read(Run("solve", "x² ≤ 1", domain: "real"));
        foreach (var point in new[] { -1, 0, 1 }) Check.That(((Entity)point).In(result).InnerSimplified, Is.EqualTo(Entity.Boolean.True));
        Check.That(((Entity)2).In(result).InnerSimplified, Is.EqualTo(Entity.Boolean.False));
    }

    [TestCase("ln(x)=ln(x)", false)]
    [TestCase("sqrt(x)=sqrt(x)", true)]
    public void RealIdentitiesDoNotIncludeUndefinedFunctionArguments(string equation, bool zeroAllowed)
    {
        var result = Read(Run("solve", equation, domain: "real"));
        Check.That(((Entity)(-1)).In(result).InnerSimplified, Is.EqualTo(Entity.Boolean.False));
        Check.That(((Entity)1).In(result).InnerSimplified, Is.EqualTo(Entity.Boolean.True));
        Check.That(((Entity)0).In(result).InnerSimplified, Is.EqualTo(zeroAllowed ? Entity.Boolean.True : Entity.Boolean.False));
    }

    [Test]
    public void RealSquareRootSquaringRetainsItsDomain()
    {
        Check.That(Read(Run("solve", "sqrt(x)^2=-1", domain: "real")), Is.EqualTo(Entity.Set.Empty));
        var simplified = Read(Run("simplify", "sqrt(x)^2", domain: "real"));
        Check.That(simplified.Substitute("x", -1).InnerSimplified.IsNaN, Is.True);
        Check.That(At(simplified, 4), Is.EqualTo(4));
    }

    [Test]
    public void RealLogarithmicPrimitiveUsesAbsoluteValue()
    {
        var result = Read(Run("integrate", "1/x", domain: "real"));
        Check.That(At(result, -2), Is.EqualTo(Math.Log(2)).Within(1e-12));
    }

    [Test]
    public void UndefinedZeroPowerIsNotTreatedAsAConstantPolynomial()
    {
        var result = Run("simplify", "0^0");
        Check.That(result.Status, Is.EqualTo("undefined"));
    }

    [TestCase("integrate", "sin(x^x)")]
    [TestCase("solve", "x*sin(x)=3")]
    [TestCase("solve", "a*x=b")]
    public void UnsolvedCasesAreNotZeroOrAnEmptySolutionSet(string operation, string expression)
    {
        var result = Run(operation, expression);
        Check.That(result.Status, Is.EqualTo("unevaluated"));
        Check.That(result.Warnings, Is.Not.Empty);
        Check.That(result.Result, Is.Not.EqualTo("0"));
        Check.That(Read(result), Is.Not.EqualTo(Entity.Set.Empty));
    }

    [Test]
    public void SubstitutionIsSimultaneousAndDoesNotMutateTheRequest()
    {
        var request = new SymbolicRequest { Operation = "substitute", Expression = "x-y", Substitutions = new() { ["x"] = "y", ["y"] = "x" } };
        var result = Read(engine.Execute(request));
        Check.That(At(result, 3, 7), Is.EqualTo(4));
        Check.That(request.Expression, Is.EqualTo("x-y"));
        Check.That(request.Substitutions["x"], Is.EqualTo("y"));
        request.Expression = "x+ComputatorSubstitution_1";
        request.Substitutions = new() { ["x"] = "ComputatorSubstitution_2" };
        result = Read(engine.Execute(request));
        Check.That(result.Vars.Select(variable => variable.Name), Is.EquivalentTo(new[] { "ComputatorSubstitution_1", "ComputatorSubstitution_2" }));
    }

    [TestCase(8)]
    [TestCase(30)]
    [TestCase(80)]
    public void NumericalEvaluationHonorsRequestedDecimalPrecision(int precision)
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "evaluate", Expression = "1/3", Precision = precision });
        Check.That(result.Result, Is.EqualTo("0." + new string('3', precision)));
        Check.That(result.Status, Is.EqualTo("approximate"));
        result = engine.Execute(new SymbolicRequest { Operation = "evaluate", Expression = "pi", Precision = precision });
        Check.That(double.Parse(result.Result, CultureInfo.InvariantCulture), Is.EqualTo(Math.PI).Within(5e-7));
        Check.That(result.Result.Length, Is.GreaterThanOrEqualTo(precision));
    }

    [Test]
    public void NumericalEvaluationReportsRemainingVariables()
    {
        var result = Run("evaluate", "sin(x)");
        Check.That(result.Status, Is.EqualTo("unevaluated"));
        result = engine.Execute(new SymbolicRequest { Operation = "evaluate", Expression = "x+y", Substitutions = new() { ["x"] = "2", ["y"] = "3" } });
        Check.That(double.Parse(result.Result, CultureInfo.InvariantCulture), Is.EqualTo(5));
    }

    [TestCase(0, "1")]
    [TestCase(1, "1+x")]
    [TestCase(3, "1+x+x^2/2+x^3/6")]
    [TestCase(5, "1+x+x^2/2+x^3/6+x^4/24+x^5/120")]
    public void TaylorOrderIsInclusivePolynomialDegree(int degree, string expected)
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "taylor", Expression = "exp(x)", Order = degree });
        Check.That((Read(result) - MathS.FromString(expected)).Expand().InnerSimplified, Is.EqualTo((Entity)0));
        Check.That(result.Warnings.Single(), Does.Contain("degree " + degree));
    }

    [Test]
    public void TaylorAtANonzeroCenterHasTheCorrectConstantAndSlope()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "taylor", Expression = "x^3", Point = "2", Order = 1 });
        Check.That(At(Read(result), 2), Is.EqualTo(8));
        Check.That(At(Read(result), 3), Is.EqualTo(20));
    }

    [TestCase("determinant", "-2")]
    [TestCase("trace", "5")]
    [TestCase("inverse", "[[-2,1],[3/2,-1/2]]")]
    [TestCase("transpose", "[[1,3],[2,4]]")]
    public void MatrixOperationsReturnExactReducedResults(string operation, string expected)
    {
        var result = Run(operation, "[[1,2],[3,4]]");
        Check.That(Read(result).InnerSimplified, Is.EqualTo(MathS.FromString(expected).InnerSimplified));
        Check.That(result.Status, Is.EqualTo("exact"));
        if (operation == "trace") Check.That(result.Result, Is.EqualTo("5"));
    }

    [Test]
    public void SingularInverseIsUndefinedAndRectangularTransposeWorks()
    {
        Check.That(Run("inverse", "[[1,2],[2,4]]").Status, Is.EqualTo("undefined"));
        var result = Run("transpose", "[[1,2,3],[4,5,6]]");
        Check.That(Read(result), Is.EqualTo(MathS.FromString("[[1,4],[2,5],[3,6]]")));
        Check.Throws<ArgumentException>(() => Run("determinant", "[[1,2,3],[4,5,6]]"));
    }

    [Test]
    public void EquationSystemPreservesVariableOrderAndBothEquations()
    {
        var request = new SymbolicRequest { Operation = "solve-system", Equations = new[] { "x+y=3", "x-y=1" }, Variables = new[] { "y", "x" } };
        var result = engine.Execute(request);
        Check.That(Read(result), Is.EqualTo(MathS.FromString("[[1,2]]")));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [Test]
    public void UnderdeterminedLinearSystemReturnsAnExactFamily()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "solve-system", Equations = new[] { "2*x-4*y=12" }, Variables = new[] { "x", "y" } });
        Check.That(result.Status, Is.EqualTo("exact"));
        var row = (Entity.Matrix)Read(result);
        Check.That((2 * row[0, 0] - 4 * row[0, 1] - 12).Simplify(), Is.EqualTo((Entity)0));
        Check.That(row.Vars, Has.Count.EqualTo(1));
    }

    [Test]
    public void ConsistentOverdeterminedSystemHasTheCorrectUniqueSolution()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "solve-system", Equations = new[] { "x+y=3", "x-y=1", "x=2" }, Variables = new[] { "x", "y" } });
        Check.That(Read(result), Is.EqualTo(MathS.FromString("[[2,1]]")));
        Check.That(result.Status, Is.EqualTo("exact"));
    }

    [Test]
    public void MoreThanEightSystemSolutionsAreNotMistakenForAnOversizedInputMatrix()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "solve-system", Equations = new[] { "x^2=1", "y^2=1", "z^2=1", "w^2=1" }, Variables = new[] { "x", "y", "z", "w" } });
        var solutions = (Entity.Matrix)Read(result);
        Check.That(result.Status, Is.EqualTo("exact"));
        Check.That(solutions.RowCount, Is.EqualTo(16));
        Check.That(solutions.ColumnCount, Is.EqualTo(4));
        var rows = new HashSet<string>();
        for (var row = 0; row < solutions.RowCount; row++)
        {
            var values = Enumerable.Range(0, 4).Select(column => (int)solutions[row, column].EvalNumerical()).ToArray();
            Check.That(values.All(value => value == -1 || value == 1), Is.True);
            rows.Add(string.Join(",", values));
        }
        Check.That(rows, Has.Count.EqualTo(16));
    }

    [Test]
    public void UnsupportedRectangularSystemRetainsItsEquations()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "solve-system", Equations = new[] { "sin(x)=y", "x*y=3", "x=2" }, Variables = new[] { "x", "y" } });
        Check.That(result.Status, Is.EqualTo("unevaluated"));
        Check.That(result.Result, Does.Contain("sin(x)"));
        Check.That(result.Result, Does.Contain("x * y"));
        Check.That(result.Warnings, Is.Not.Empty);
    }

    [TestCase(7)]
    [TestCase(19)]
    [TestCase(43)]
    [TestCase(79)]
    [TestCase(127)]
    [TestCase(251)]
    [TestCase(509)]
    [TestCase(1021)]
    public void GeneratedPolynomialsAgreeWithIndependentCoefficientCalculus(int seed)
    {
        var random = new Random(seed);
        var coefficients = Enumerable.Range(0, 7).Select(_ => random.Next(-4, 5)).ToArray();
        var expression = string.Join("+", coefficients.Select((coefficient, power) => "(" + coefficient.ToString(CultureInfo.InvariantCulture) + ")" + (power == 0 ? "" : "*x^" + power)));
        var derivative = Read(Run("differentiate", expression));
        var primitive = Read(Run("integrate", expression));
        var expanded = Read(Run("expand", expression));
        var factored = Read(Run("factor", expression));
        foreach (var x in new[] { -2.0, -1.0, -.25, 0, .5, 1.0, 2.0 })
        {
            var value = coefficients.Select((coefficient, power) => coefficient * Math.Pow(x, power)).Sum();
            var slope = coefficients.Skip(1).Select((coefficient, index) => (index + 1) * coefficient * Math.Pow(x, index)).Sum();
            var area = coefficients.Select((coefficient, power) => coefficient * Math.Pow(x, power + 1) / (power + 1)).Sum();
            Check.That(At(derivative, x), Is.EqualTo(slope).Within(1e-10));
            Check.That(At(primitive, x), Is.EqualTo(area).Within(1e-10));
            Check.That(At(expanded, x), Is.EqualTo(value).Within(1e-10));
            Check.That(At(factored, x), Is.EqualTo(value).Within(1e-10));
        }
    }
}
