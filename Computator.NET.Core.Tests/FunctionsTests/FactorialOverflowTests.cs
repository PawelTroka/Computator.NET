using System;
using System.Collections.Generic;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class FactorialOverflowTests
    {
        private Func<double, double> _generated;

        [OneTimeSetUp]
        public void CompileGeneratedFactorialOnce()
        {
            _generated = new ExpressionsEvaluator().Evaluate("factorial(x)", string.Empty, CalculationsMode.Real);
        }

        private static IEnumerable<TestCaseData> Cases
        {
            get
            {
                // 170! is the last factorial representable as a finite double.
                // The retained loop rounds at each multiplication, so its large
                // finite results are checked against an independent reference
                // with a relative tolerance, not an identical multiplication loop.
                const double factorial170 = 7.257415615307999e306;
                var inputs = new (double Input, double Expected)[]
                {
                    (0, 1), (1, 1), (5, 120), (10, 3628800), (20, 2432902008176640000),
                    (170, factorial170), (170.5, factorial170), (Math.BitDecrement(171), factorial170),
                    (171, double.PositiveInfinity), (171.5, double.PositiveInfinity),
                    (int.MaxValue, double.PositiveInfinity), ((double)ulong.MaxValue, double.PositiveInfinity),
                    (double.MaxValue, double.PositiveInfinity), (double.PositiveInfinity, double.PositiveInfinity),
                    // This change bounds overflow only. Preserve the historical
                    // fractional, negative and NaN behavior below the threshold.
                    (0.5, 1), (5.5, 120), (-1, 1), (double.MinValue, 1),
                    (double.NegativeInfinity, 1), (double.NaN, 1)
                };
                foreach (var input in inputs)
                    foreach (var generated in new[] { false, true })
                        yield return new TestCaseData(input.Input, input.Expected, generated);
            }
        }

        [TestCaseSource(nameof(Cases))]
        public void FactorialBoundsOverflowAndPreservesEarlierResults(double input, double expected, bool generated)
        {
            var actual = generated ? _generated(input) : ElementaryFunctions.factorial(input);
            if (double.IsPositiveInfinity(expected))
                NUnit.Framework.Assert.That(actual, Is.EqualTo(double.PositiveInfinity));
            else if (expected > 1e100)
                NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(expected * 1e-14));
            else
                NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
