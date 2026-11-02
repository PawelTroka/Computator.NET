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
    public class DifferentialEquationEvaluatorTests
    {
        private readonly bool _legacy;
        public DifferentialEquationEvaluatorTests(bool legacy) => _legacy = legacy;

        private sealed class LegacyExpressionEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }

        [TestCase("DifferentialEquation.rungeKutta4((t,u) => 2*t,3.0,0.0,2.0,21)[20]", 7.0)]
        [TestCase("DifferentialEquation.rungeKutta4((t,u) => 3.0,5.0,2.0,1.0,2)[1]", 2.0)]
        public void RealExpressionsReachTheSharedSolver(string source, double expected)
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyExpressionEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(expected).Within(2e-13));
        }

        [Test]
        public void ScriptsCanSolveRealVectorSystems()
        {
            ScriptEvaluator evaluator = _legacy ? new LegacyScriptEvaluator() : new ScriptEvaluator();
            var output = new StringBuilder();
            evaluator.Evaluate("var values = DifferentialEquation.rungeKutta4((t,state) => vector(2*t,3.0), vector(1.0,2.0), 0, 1, 11); write(values[10][0]);", "")
                .Evaluate(text => output.Append(text));
            NUnit.Framework.Assert.That(double.Parse(output.ToString(), CultureInfo.InvariantCulture), Is.EqualTo(2).Within(1e-13));
        }

        [Test]
        public void CustomDefinitionsSupplyScalarDerivatives()
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyExpressionEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate("DifferentialEquation.rungeKutta4(Velocity,3,0,2,21)[20]",
                "static real Velocity(real t,real u) { return 2*t; }", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(7).Within(2e-13));
        }
    }

    [TestFixture]
    public class DifferentialEquationCompletionTests
    {
        [Test]
        public void BothOverloadsAttachTheAuthoredReferenceInScripting()
        {
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());
            var items = provider.ScriptingAutocompleteItems.Where(item => item.Text.StartsWith("DifferentialEquation.rungeKutta4("))
                .ToArray();
            NUnit.Framework.Assert.That(items, Has.Length.EqualTo(2));
            foreach (var item in items)
            {
                NUnit.Framework.Assert.That(item.Details.Category, Is.EqualTo("Ordinary differential equations"));
                NUnit.Framework.Assert.That(item.Details.Description, Does.Contain("samples includes the initial and final states"));
            }
        }
    }
}
