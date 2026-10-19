using System;
using System.Linq;
using System.Reflection;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable]
    public class DirectExpressionBoundaryTests
    {
        private sealed class RoslynEvaluator : ExpressionsEvaluator { protected override bool UseDirectExpressions => false; }
        private sealed class DelegatingCompiler : ITslCompiler
        {
            private readonly TslCompiler _compiler = new TslCompiler();
            public string TransformToCSharp(string source) => _compiler.TransformToCSharp(source);
            public string TransformToCSharp(string source, string custom, bool defer) => _compiler.TransformToCSharp(source, custom, defer);
        }
        private sealed class RedirectingCompiler : ITslCompiler { public string TransformToCSharp(string input) => "y"; }
        private static Assembly AssemblyOf(BaseFunction f) => ((Delegate)typeof(BaseFunction).GetField("_function", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f)).Method.Module.Assembly;

        [TestCase("x/(-0)")]
        [TestCase("x/-(0)")]
        [TestCase("x/((-0))")]
        [TestCase("x/-(-0)")]
        [TestCase("x/(+0)")]
        [TestCase("x/(0.0)")]
        [TestCase("x/(-0.0)")]
        [TestCase("x/-(-0.0)")]
        [TestCase("x/(x/(-0))")]
        [TestCase("x/-0e0")]
        public void DenominatorPromotionUsesTheOriginalTslSignedZeroRules(string source)
        {
            var value = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(AssemblyOf(value).GetName().Name, Does.StartWith("Computator.Direct."));
            Func<double, double> actual = value, expected = new RoslynEvaluator().Evaluate(source, "", CalculationsMode.Real);
            foreach (double x in new[] { -3.0, -0.0, 0.0, 3.0, double.PositiveInfinity, double.NaN })
            {
                double a = actual(x), b = expected(x);
                if (double.IsNaN(b)) NUnit.Framework.Assert.That(double.IsNaN(a), Is.True, source);
                else NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(a), Is.EqualTo(BitConverter.DoubleToInt64Bits(b)), source);
            }
        }

        [TestCase("sin(x)+cos(x)+x*x/4", CalculationsMode.Real)]
        [TestCase("sin(x)*cos(y)+x*y", CalculationsMode.Fxy)]
        [TestCase("sin(z)+z*z", CalculationsMode.Complex)]
        [TestCase("x+y+z", CalculationsMode.Complex)]
        [TestCase("x+y", CalculationsMode.Fxy)]
        [TestCase("1/2", CalculationsMode.Real)]
        [TestCase("x²+y²=1", CalculationsMode.Real)]
        public void AutomaticRoutingMatchesOriginalAndDoesNotEmitPrograms(string source, CalculationsMode expected)
        {
            var direct = new ModeDeterminer(new TslCompiler());
            var full = new ModeDeterminer(new DelegatingCompiler());
            var before = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("Computator.Direct.") || a.GetName().Name.StartsWith("Computator.TSL.")).Select(a => a.FullName).ToArray();
            NUnit.Framework.Assert.That(direct.DetermineMode(source), Is.EqualTo(expected));
            var after = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("Computator.Direct.") || a.GetName().Name.StartsWith("Computator.TSL.")).Select(a => a.FullName).ToArray();
            NUnit.Framework.Assert.That(after, Is.EquivalentTo(before));
            NUnit.Framework.Assert.That(direct.DetermineMode(source), Is.EqualTo(full.DetermineMode(source)));
        }

        [Test]
        public void InjectedCompilerIsNeverBypassed() =>
            NUnit.Framework.Assert.That(new ModeDeterminer(new RedirectingCompiler()).DetermineMode("x"), Is.EqualTo(CalculationsMode.Fxy));

        [Test]
        public void DirectCompilerBoundsFallBackWithoutChangingTheValue()
        {
            foreach (string source in new[] { new string(' ', 4096)+"x", new string('(', 70)+"x"+new string(')', 70), string.Join("+", Enumerable.Repeat("x", 140)) })
            {
                var f = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);
                NUnit.Framework.Assert.That(AssemblyOf(f).GetName().Name, Does.StartWith("Computator.TSL."));
                Func<double, double> a = f, b = new RoslynEvaluator().Evaluate(source, "", CalculationsMode.Real);
                NUnit.Framework.Assert.That(a(.5), Is.EqualTo(b(.5)));
            }
        }
    }
}
