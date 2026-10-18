using System;
using System.Numerics;
using System.Threading.Tasks;
using Computator.NET.Core.Functions;
using Computator.NET.Engine;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class RandomRangeTests
    {
        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(int.MaxValue)]
        public void InclusiveSingletonReturnsItsOnlyElement(int endpoint)
        {
            NUnit.Framework.Assert.That(StatisticsFunctions.random(endpoint, endpoint), Is.EqualTo((double)endpoint));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(int.MaxValue)]
        public void SingleBoundIncludesOnlyNonnegativeIntegers(int maximum)
        {
            for (var i = 0; i < 256; i++)
            {
                var value = StatisticsFunctions.random(maximum);
                NUnit.Framework.Assert.That(value, Is.InRange(0d, (double)maximum));
                NUnit.Framework.Assert.That(value, Is.EqualTo(Math.Truncate(value)));
            }
        }

        [TestCase(int.MinValue, int.MaxValue)]
        [TestCase(int.MaxValue - 1, int.MaxValue)]
        [TestCase(int.MinValue, int.MinValue + 1)]
        [TestCase(-10, -1)]
        [TestCase(-1, 1)]
        public void InclusiveRangesDoNotOverflow(int minimum, int maximum)
        {
            for (var i = 0; i < 256; i++)
            {
                var value = StatisticsFunctions.random(minimum, maximum);
                NUnit.Framework.Assert.That(value, Is.InRange((double)minimum, (double)maximum));
                NUnit.Framework.Assert.That(value, Is.EqualTo(Math.Truncate(value)));
            }
        }

        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void NegativeSingleBoundsRetainTheNaNContract(int maximum)
        {
            NUnit.Framework.Assert.That(double.IsNaN(StatisticsFunctions.random(maximum)), Is.True);
        }

        [TestCase(1, 0)]
        [TestCase(0, -1)]
        [TestCase(int.MaxValue, int.MinValue)]
        public void ReversedRangesAreRejectedInsteadOfReturningAnOutOfRangeValue(int minimum, int maximum)
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => StatisticsFunctions.random(minimum, maximum));
        }

        [Test]
        public void ConcurrentBuiltInCallsStayWithinTheirBounds()
        {
            Parallel.For(0, 1024, _ =>
            {
                NUnit.Framework.Assert.That(StatisticsFunctions.random(int.MaxValue), Is.InRange(0d, (double)int.MaxValue));
                NUnit.Framework.Assert.That(StatisticsFunctions.random(-10, 10), Is.InRange(-10d, 10d));
                NUnit.Framework.Assert.That(StatisticsFunctions.random(1.0), Is.GreaterThanOrEqualTo(0).And.LessThan(1));
                var value = StatisticsFunctions.random(new Complex(1, 1));
                NUnit.Framework.Assert.That(value.Real, Is.GreaterThanOrEqualTo(0).And.LessThan(1));
                NUnit.Framework.Assert.That(value.Imaginary, Is.GreaterThanOrEqualTo(0).And.LessThan(1));
            });
        }

        [TestCase("random(2147483647,2147483647)", 2147483647d)]
        [TestCase("random(-2147483648,-2147483648)", -2147483648d)]
        [TestCase("random(0)", 0d)]
        public void GeneratedScriptFunctionsRetainTheSameInclusiveEndpoints(string expression, double expected)
        {
            var result = new PortableEngine().EvaluateReal(expression, customFunctions: StatisticsFunctions.ToCode);
            NUnit.Framework.Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GeneratedScriptFunctionsValidateFullAndReversedRanges()
        {
            const string script = @"
                for (var i = 0; i < 256; i++) {
                    var value = random(-2147483648,2147483647);
                    if (value < -2147483648.0)
                        throw new System.InvalidOperationException();
                    if (value > 2147483647.0)
                        throw new System.InvalidOperationException();
                    if (value != System.Math.Truncate(value))
                        throw new System.InvalidOperationException();
                    var positive = random(2147483647);
                    if (positive < 0)
                        throw new System.InvalidOperationException();
                    if (positive > 2147483647.0)
                        throw new System.InvalidOperationException();
                }
                if (!System.Double.IsNaN(random(-1))) throw new System.InvalidOperationException();
                try { random(1,0); write(false); }
                catch (System.ArgumentOutOfRangeException) { write(true); }";
            NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, StatisticsFunctions.ToCode), Is.EqualTo("True"));
        }
    }
}
