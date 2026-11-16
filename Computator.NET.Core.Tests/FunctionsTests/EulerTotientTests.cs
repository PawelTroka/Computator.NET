using System;
using System.Text;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture, NonParallelizable]
    public class EulerTotientTests
    {
        [TestCase(1L, 1d)]
        [TestCase(2L, 1d)]
        [TestCase(9L, 6d)]
        [TestCase(36L, 12d)]
        [TestCase(65536L, 32768d)]
        [TestCase(2147483647L, 2147483646d)]
        [TestCase(long.MaxValue, 7713001620195508224d)]
        public void TotientHandlesLargeInputsWithoutAllocatingInputSizedArrays(long n, double expected)
        {
            NUnit.Framework.Assert.That(ElementaryFunctions.Eulerφ(n), Is.EqualTo(expected));
        }

        [Test]
        public void InvalidNonpositiveInputsReturnNaN()
        {
            NUnit.Framework.Assert.That(ElementaryFunctions.Eulerφ(0), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.Eulerφ(-1), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.Eulerφ(long.MinValue), Is.NaN);
        }

        [Test]
        public void SmallTotientsMatchIndependentCoprimeCounts()
        {
            for (long n = 1; n <= 200; n++)
            {
                var count = 0;
                for (long candidate = 1; candidate <= n; candidate++)
                {
                    long a = n, b = candidate;
                    while (b != 0)
                    {
                        var remainder = a % b;
                        a = b;
                        b = remainder;
                    }
                    if (a == 1) count++;
                }

                NUnit.Framework.Assert.That(ElementaryFunctions.Eulerφ(n), Is.EqualTo(count),
                    "Independent coprime count for n=" + n);
            }
        }

        [Test]
        public void EmittedTslTotientHandlesCompositePrimeAndMaximumLong()
        {
            // ScriptEvaluator compiles the separately maintained ElementaryFunctions.ToCode.
            const string script = @"
                write(Eulerφ(36) == 12);
                write(Eulerφ(2147483647) == 2147483646);
                write(Eulerφ(System.Int64.MaxValue) == 7713001620195508224.0);
            ";
            var output = new StringBuilder();
            new ScriptEvaluator().Evaluate(script, string.Empty).Evaluate(text => output.Append(text));
            NUnit.Framework.Assert.That(output.ToString(), Is.EqualTo("TrueTrueTrue"));
        }
    }
}
