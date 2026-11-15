using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class FunctionExtremumEvaluatorTests
    {
        private readonly bool _legacy;
        public FunctionExtremumEvaluatorTests(bool legacy) => _legacy = legacy;

        private sealed class LegacyExpressionEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }

        [TestCase("FunctionExtremum.minimize(t => (t-2)*(t-2),0,5)", 2)]
        [TestCase("FunctionExtremum.maximize(t => 7-(t-2)*(t-2),0,5)", 2)]
        [TestCase("FunctionExtremum.minimize(t => t,0,5)", 0)]
        public void ExpressionsUseTheSharedSolver(string source, double expected)
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyExpressionEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(expected).Within(1e-7));
        }

        [Test]
        public void ScriptsCanEvaluateTheFunctionAtTheReturnedLocation()
        {
            ScriptEvaluator evaluator = _legacy ? new LegacyScriptEvaluator() : new ScriptEvaluator();
            var output = new StringBuilder();
            evaluator.Evaluate("function f(real t)=7-(t-2)*(t-2); var location=FunctionExtremum.maximize(f,0,5); write(f(location));", "")
                .Evaluate(text => output.Append(text));
            NUnit.Framework.Assert.That(double.Parse(output.ToString(), CultureInfo.InvariantCulture), Is.EqualTo(7).Within(1e-12));
        }

        [Test]
        public void CustomDefinitionsSupplyTheObjective()
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyExpressionEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate("FunctionExtremum.minimize(Cost,0,5)",
                "static real Cost(real t) { return (t-2)*(t-2)+7; }", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(2).Within(1e-7));
        }
    }

    [TestFixture]
    public class FunctionExtremumCompletionTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void BothOperationsHaveAuthoredHelpInBothCompletionModes(bool scripting)
        {
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());
            var source = scripting ? provider.ScriptingAutocompleteItems : provider.ExpressionAutocompleteItems;
            var items = source.Where(item => item.Details.Type == "FunctionExtremum").ToArray();
            NUnit.Framework.Assert.That(items.Select(item => item.Text), Is.EquivalentTo(new[]
            {
                "FunctionExtremum.minimize(fx,a,b,tolerance,iterations)",
                "FunctionExtremum.maximize(fx,a,b,tolerance,iterations)"
            }));
            foreach (var item in items)
            {
                NUnit.Framework.Assert.That(item.Details.Category, Is.EqualTo("Function extrema"));
                NUnit.Framework.Assert.That(item.Details.Description, Does.Contain("returns the x coordinate"));
                NUnit.Framework.Assert.That(item.Details.Description, Does.Contain("unimodal"));
            }
        }
    }
}
