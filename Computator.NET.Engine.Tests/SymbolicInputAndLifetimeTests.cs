using System.Globalization;
using AngouriMath;
using AngouriMath.Core;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using PeterO.Numbers;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class SymbolicInputAndLifetimeTests
{
    private readonly SymbolicEngine engine = new();

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("x+")]
    [TestCase("sin(")]
    [TestCase("unknown(x)")]
    [TestCase("f(x)")]
    [TestCase("sin^2(x)")]
    [TestCase("sin")]
    [TestCase("sin(x,x)")]
    [TestCase("x2")]
    [TestCase("√x")]
    [TestCase("|x")]
    [TestCase("System.IO.File.Delete(1)")]
    [TestCase("x; x+1")]
    [TestCase("\"x\"")]
    [TestCase("x\0+1")]
    [TestCase("²x")]
    [TestCase(".foo")]
    [TestCase("10^100000000")]
    [TestCase("9^(9^9)")]
    [TestCase("exp(exp(1000))")]
    [TestCase("1000000!")]
    [TestCase("factorial(1000000)")]
    [TestCase("gamma(1000000)")]
    [TestCase("sum(k!,k,1,1000000)")]
    [TestCase("product(k,k,1,1000000)")]
    [TestCase("prime(100000000)")]
    [TestCase("phi(10000000000000000000000000)")]
    [TestCase("1e99999999")]
    public void InvalidOrExcessiveExpressionsFailAndTheNextRequestRecovers(string expression)
    {
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Expression = expression }));
        Check.That(engine.Execute(new SymbolicRequest { Expression = "2+2" }).Result, Is.EqualTo("4"));
    }

    [Test]
    public void InputLengthDepthAndFlatOperatorBudgetsAreCheckedBeforeParsing()
    {
        foreach (var expression in new[] { new string('x', SymbolicEngine.MaximumExpressionCharacters + 1),
            new string('(', 65) + "x" + new string(')', 65), new string('1', 129), string.Join("+", Enumerable.Repeat("x", 514)) })
            Check.Throws<ArgumentException>(() => SymbolicEngine.Validate(new SymbolicRequest { Expression = expression }));
    }

    [TestCase("x+1")]
    [TestCase("pi")]
    [TestCase("π")]
    [TestCase("i")]
    [TestCase("sin")]
    [TestCase("x2")]
    [TestCase("")]
    public void InvalidVariableNamesAreRejected(string variable) =>
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Expression = "x", Variable = variable }));

    [TestCase("operation")]
    [TestCase("domain")]
    [TestCase("direction")]
    [TestCase("order-zero")]
    [TestCase("order-large")]
    [TestCase("precision-zero")]
    [TestCase("precision-large")]
    [TestCase("null-expression")]
    [TestCase("null-substitutions")]
    [TestCase("null-equations")]
    [TestCase("null-variables")]
    [TestCase("ignored-substitutions")]
    public void InvalidRequestMetadataIsNotSilentlyIgnored(string error)
    {
        var request = new SymbolicRequest { Expression = "x" };
        switch (error)
        {
            case "operation": request.Operation = "unknown"; break;
            case "domain": request.Domain = "integer"; break;
            case "direction": request.Direction = "up"; break;
            case "order-zero": request.Order = 0; break;
            case "order-large": request.Order = 21; break;
            case "precision-zero": request.Precision = 0; break;
            case "precision-large": request.Precision = 101; break;
            case "null-expression": request.Expression = null!; break;
            case "null-substitutions": request.Substitutions = null!; break;
            case "null-equations": request.Equations = null!; break;
            case "null-variables": request.Variables = null!; break;
            case "ignored-substitutions": request.Substitutions["x"] = "1"; break;
        }
        Check.Throws<ArgumentException>(() => SymbolicEngine.Validate(request));
    }

    [Test]
    public void CollectionsAndMatrixDimensionsHaveExplicitLimits()
    {
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Operation = "substitute", Expression = "x",
            Substitutions = Enumerable.Range(1, 33).ToDictionary(value => "x_" + value, _ => "1") }));
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Operation = "solve-system",
            Equations = new[] { "x=1" }, Variables = new[] { "x", "x" } }));
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Operation = "solve-system", Equations = Array.Empty<string>(), Variables = new[] { "x" } }));
        var matrix = "[[" + string.Join(",", Enumerable.Repeat("1", 9)) + "]]";
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Operation = "transpose", Expression = matrix }));
    }

    [TestCase("limit")]
    [TestCase("taylor")]
    public void CalculusPointCannotContainItsBoundVariable(string operation) =>
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Operation = operation, Expression = "x^2", Point = "x" }));

    [Test]
    public void DefiniteIntegralBoundsCannotContainTheIntegrationVariable() =>
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Operation = "definite-integrate", Expression = "x^2", LowerBound = "x", UpperBound = "2" }));

    [TestCase("pl-PL")]
    [TestCase("de-DE")]
    [TestCase("tr-TR")]
    public void MathematicalParsingAndOutputAreCultureIndependent(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var result = engine.Execute(new SymbolicRequest { Expression = "0.1+0.2" });
            Check.That(result.Result, Is.EqualTo("3/10"));
            result = engine.Execute(new SymbolicRequest { Operation = "evaluate", Expression = "1/8" });
            Check.That(result.Result, Is.EqualTo("0.125"));
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = previousUi; }
    }

    [Test]
    public void PrecancelledExecutionDoesNoWorkAndDoesNotPoisonSubsequentCalls()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Check.Throws<OperationCanceledException>(() => engine.Execute(new SymbolicRequest { Expression = "x+1" }, cancellation.Token));
        Check.That(engine.Execute(new SymbolicRequest { Expression = "2+2" }).Result, Is.EqualTo("4"));
    }

    [Test]
    public void AnInterruptedActualCalculationRecovers()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(1));
        Check.Throws<OperationCanceledException>(() => engine.Execute(new SymbolicRequest { Operation = "integrate", Expression = "sin(x^x)" }, cancellation.Token));
        Check.That(engine.Execute(new SymbolicRequest { Expression = "2+2" }).Result, Is.EqualTo("4"));
    }

    [Test]
    public void CallerPrecisionAndDomainSettingsAreRestoredAfterSuccessAndFailure()
    {
        var callerPrecision = EContext.ForPrecision(71);
        using var precision = MathS.Settings.DecimalPrecisionContext.Set(callerPrecision);
        using var domain = MathS.Settings.Codomain.Set(Domain.Real);
        using var roots = MathS.Settings.AllowNewton.Set(true);
        engine.Execute(new SymbolicRequest { Operation = "evaluate", Expression = "pi", Precision = 17, Domain = "complex" });
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Expression = "invalid(" }));
        Check.That(MathS.Settings.DecimalPrecisionContext.Value, Is.SameAs(callerPrecision));
        Check.That(MathS.Settings.Codomain.Value, Is.EqualTo(Domain.Real));
        Check.That(MathS.Settings.AllowNewton.Value, Is.True);
    }

    [Test]
    public async Task ParallelRequestsDoNotSharePrecisionOrDomain()
    {
        var jobs = Enumerable.Range(0, 24).Select(index => Task.Run(() =>
        {
            var precision = 10 + index;
            var decimalResult = engine.Execute(new SymbolicRequest { Operation = "evaluate", Expression = "1/3", Precision = precision });
            var real = index % 2 == 0;
            var domainResult = engine.Execute(new SymbolicRequest { Expression = "sqrt(-1)", Domain = real ? "real" : "complex" });
            return (precision, decimalResult, real, domainResult);
        }));
        foreach (var result in await Task.WhenAll(jobs))
        {
            Check.That(result.decimalResult.Result, Is.EqualTo("0." + new string('3', result.precision)));
            Check.That(result.domainResult.Status, Is.EqualTo(result.real ? "undefined" : "exact"));
        }
    }

    [Test]
    public void SuppressedExecutionContextStillRunsAndRestoresFlow()
    {
        using (ExecutionContext.SuppressFlow())
            Check.That(engine.Execute(new SymbolicRequest { Expression = "2+2" }).Result, Is.EqualTo("4"));
        Check.That(ExecutionContext.IsFlowSuppressed(), Is.False);
    }

    [Test]
    public void LatexIsInertTextFromTheSameMathematicalResult()
    {
        var result = engine.Execute(new SymbolicRequest { Operation = "integrate", Expression = "x^2" });
        Check.That(result.Latex, Does.Contain("\\frac"));
        Check.That(result.Latex, Does.Not.Contain("<script"));
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Expression = "<script>alert(1)</script>" }));
    }
}
