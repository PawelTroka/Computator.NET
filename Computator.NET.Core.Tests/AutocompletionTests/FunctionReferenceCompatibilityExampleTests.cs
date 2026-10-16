using System;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class FunctionReferenceCompatibilityExampleTests
    {
        [Test]
        public void BetaDistributionExampleAvoidsTheDemonstratedNameCollision()
        {
            GSLInitializer.Initialize();
            var evaluator = new ExpressionsEvaluator();
            NUnit.Framework.Assert.Throws<CompilationException>(() => evaluator.Evaluate("Beta.PDF(2,2,0.5)", "", CalculationsMode.Real));
            var function = evaluator.Evaluate("MathNet.Numerics.Distributions.Beta.PDF(2,2,0.5)", "", CalculationsMode.Real);
            var value = Convert.ToDouble(function.Evaluate(0.0));
            NUnit.Framework.Assert.That(value, Is.EqualTo(1.5).Within(1e-12));
            var entries = new FunctionsDetailsFileSource().Details;
            foreach (var entry in entries)
                if (entry.Key.StartsWith("Beta.", StringComparison.Ordinal))
                {
                    NUnit.Framework.Assert.That(entry.Value.Description, Does.Contain("collides"));
                    NUnit.Framework.Assert.That(entry.Value.Description, Does.Contain("MathNet.Numerics.Distributions.Beta.PDF"));
                }
        }

        [Test]
        public void BohachevskyReferenceSeparatesStandardDefinitionFromPinnedImplementation()
        {
            var standardAtOrigin = -0.3 * Math.Cos(0) - 0.4 * Math.Cos(0) + 0.7;
            NUnit.Framework.Assert.That(standardAtOrigin, Is.EqualTo(0).Within(1e-12));
            var actual = SpecialFunctions.Bohachevsky1(0, 0);
            NUnit.Framework.Assert.That(actual, Is.EqualTo(-0.7).Within(1e-12));
            NUnit.Framework.Assert.That(actual + 0.7, Is.EqualTo(standardAtOrigin).Within(1e-12));
            var article = new FunctionsDetailsFileSource().Details["Bohachevsky1(x,y)"].Description;
            NUnit.Framework.Assert.That(article, Does.Contain("omits"));
            NUnit.Framework.Assert.That(article, Does.Contain("Bohachevsky1(0,0)+0.7"));
            NUnit.Framework.Assert.That(article, Does.Contain("-0.7"));
        }
    }
}
