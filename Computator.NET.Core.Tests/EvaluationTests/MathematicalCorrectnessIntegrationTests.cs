using System;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture, NonParallelizable]
    public class MathematicalCorrectnessIntegrationTests
    {
        [OneTimeSetUp]
        public void InitializeNativeFunctions() => GSLInitializer.Initialize();

        [Test]
        public void OneExpressionUsesAllCorrectedNumericalHelpers()
        {
            const string source =
                "|inverseErf(-0.5) + inverseErfc(1.5)| + artanh(1e-20)/1e-20 + " +
                "arcoth(1e20)/1e-20 + CoulombGprime(1,0,0.5) + " +
                "Re(RiccatiBesselξ(0,0.5)) - Im(RiccatiBesselζ(0,0.5))";
            var function = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);

            // Independent inverse-error fixture and elementary zero-charge /
            // order-zero spherical formulas; no host helper supplies the oracle.
            const double rho = 0.5;
            var derivative = Math.Cos(rho) - Math.Sin(rho) / rho - Math.Cos(rho) / (rho * rho);
            var expected = 2 * 0.4769362762044699 + 2 + derivative + Math.Sin(rho) - Math.Cos(rho);
            NUnit.Framework.Assert.That(function.Evaluate(0.0), Is.EqualTo(expected).Within(2e-12));
        }

        [Test]
        public void TinyInputsRetainTheirValuesInTheCompleteComplexExpression()
        {
            const string source =
                "inverseErfc(System.Double.Epsilon) + " +
                "artanh(System.Double.Epsilon)/System.Double.Epsilon + " +
                "RiccatiBesselξ(0,System.Double.Epsilon)";
            var function = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Complex);
            var actual = function.Evaluate(Complex.Zero);

            NUnit.Framework.Assert.That(actual.Real, Is.EqualTo(28.21329321081295).Within(2e-14));
            NUnit.Framework.Assert.That(actual.Imaginary, Is.EqualTo(-1.0));
        }

        [Test]
        public void CustomFunctionCombinesNumericalCallsNestedAbsoluteValuesAndShortCircuiting()
        {
            const string custom = """
                static bool Unreachable() { throw new System.InvalidOperationException("must short-circuit"); }
                static real Check(real t)
                {
                    if (inverseErf(-0.5) > 0 || inverseErfc(1.5) > 0 || abs(t||t||+t) > 0 || Unreachable())
                        return artanh(1e-20)/1e-20;
                    return 0;
                }
                """;
            var function = new ExpressionsEvaluator().Evaluate("Check(x)", custom, CalculationsMode.Real);

            NUnit.Framework.Assert.That(function.Evaluate(2.0), Is.EqualTo(1.0));
        }
    }
}
