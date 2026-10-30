using Computator.NET.Core.NumericalCalculations;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class DifferentialEquationTests
{
    private static Vector<double> Vector(params double[] values) => MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(values);

    [TestCase(0.0, 2.0, 3.0)]
    [TestCase(2.0, 0.0, 7.0)]
    [TestCase(-3.0, -1.0, 9.0)]
    public void ScalarSolutionMatchesTheAnalyticPolynomialAtEverySample(double start, double end, double initial)
    {
        var actual = DifferentialEquation.rungeKutta4((t, state) => 2 * t, initial, start, end, 41);
        NUnit.Framework.Assert.That(actual.Length, Is.EqualTo(41));
        for (var i = 0; i < actual.Length; i++)
        {
            var time = start + i * (end - start) / 40;
            NUnit.Framework.Assert.That(actual[i], Is.EqualTo(initial + time * time - start * start).Within(2e-13));
        }
    }

    [Test]
    public void HalvingTheStepShowsFourthOrderConvergenceForExponentialGrowth()
    {
        var coarse = DifferentialEquation.rungeKutta4((t, state) => state, 1.0, 0.0, 1.0, 11).Last();
        var fine = DifferentialEquation.rungeKutta4((t, state) => state, 1.0, 0.0, 1.0, 21).Last();
        var ratio = (Math.E - coarse) / (Math.E - fine);
        NUnit.Framework.Assert.That(ratio, Is.InRange(15.0, 16.0));
        NUnit.Framework.Assert.That(fine, Is.EqualTo(Math.E).Within(2e-7));
    }

    [Test]
    public void ScalarAndVectorEquationsUseTheSameTimeStages()
    {
        var scalar = DifferentialEquation.rungeKutta4((t, state) => t * t * t, 2.0, 1.0, 2.0, 11);
        var system = DifferentialEquation.rungeKutta4((t, state) => Vector(t * t * t, 2 * t), Vector(2, 3), 1.0, 2.0, 11);
        for (var i = 0; i < scalar.Length; i++)
        {
            var time = 1 + i * 0.1;
            NUnit.Framework.Assert.That(system[i][0], Is.EqualTo(scalar[i]).Within(1e-14));
            NUnit.Framework.Assert.That(system[i][0], Is.EqualTo(2 + (Math.Pow(time, 4) - 1) / 4).Within(2e-13));
            NUnit.Framework.Assert.That(system[i][1], Is.EqualTo(2 + time * time).Within(1e-13));
        }
    }

    [Test]
    public void CoupledOscillatorFollowsBothComponentsOverAFullPeriod()
    {
        var actual = DifferentialEquation.rungeKutta4((t, state) => Vector(state[1], -state[0]), Vector(1, 0), 0, 2 * Math.PI, 401);
        for (var i = 0; i < actual.Length; i++)
        {
            var time = i * 2 * Math.PI / 400;
            NUnit.Framework.Assert.That(actual[i][0], Is.EqualTo(Math.Cos(time)).Within(4e-9));
            NUnit.Framework.Assert.That(actual[i][1], Is.EqualTo(-Math.Sin(time)).Within(4e-9));
        }
    }

    [Test]
    public void TwoSamplesMeanOneStepAndTheDefaultIncludesTheInitialValue()
    {
        var calls = 0;
        var answer = DifferentialEquation.rungeKutta4((t, state) => { calls++; return 3; }, 5.0, 1.0, 2.0, 2);
        NUnit.Framework.Assert.That(answer, Is.EqualTo(new[] { 5.0, 8.0 }));
        NUnit.Framework.Assert.That(calls, Is.EqualTo(4));
        NUnit.Framework.Assert.That(DifferentialEquation.rungeKutta4((t, state) => 0.0, 2.0, 0, 1), Has.Length.EqualTo(101));
    }

    [Test]
    public void ZeroLengthIntervalDoesNotEvaluateTheDerivativeOrAliasVectorSamples()
    {
        var scalar = DifferentialEquation.rungeKutta4((t, state) => throw new Exception("Must not run"), 7.0, 4, 4, 3);
        var initial = Vector(2, 3);
        var system = DifferentialEquation.rungeKutta4((t, state) => throw new Exception("Must not run"), initial, 4, 4, 3);
        NUnit.Framework.Assert.That(scalar, Is.EqualTo(new[] { 7.0, 7.0, 7.0 }));
        system[1][0] = 99;
        NUnit.Framework.Assert.That(initial[0], Is.EqualTo(2));
        NUnit.Framework.Assert.That(system[0][0], Is.EqualTo(2));
        NUnit.Framework.Assert.That(system[2][0], Is.EqualTo(2));
    }

    [Test]
    public void MutatingACallbackStateCannotChangeTheInitialValueOrEarlierSamples()
    {
        var initial = Vector(4);
        var heldStates = new List<Vector<double>>();
        var answer = DifferentialEquation.rungeKutta4((time, state) =>
        {
            heldStates.Add(state);
            state[0] = 900;
            return Vector(1);
        }, initial, 0, 1, 11);
        foreach (var state in heldStates) state[0] = -1;
        NUnit.Framework.Assert.That(initial[0], Is.EqualTo(4));
        for (var i = 0; i < answer.Length; i++)
            NUnit.Framework.Assert.That(answer[i][0], Is.EqualTo(4 + i * 0.1).Within(1e-13));
        answer[1][0] = 999;
        NUnit.Framework.Assert.That(answer[0][0], Is.EqualTo(4));
        NUnit.Framework.Assert.That(answer[2][0], Is.EqualTo(4.2).Within(1e-13));
    }

    [Test]
    public void ReusingADerivativeBufferDoesNotOverwriteEarlierStages()
    {
        var buffer = Vector(0);
        var answer = DifferentialEquation.rungeKutta4((time, state) =>
        {
            buffer[0] = 2 * time;
            return buffer;
        }, Vector(0), 0, 1, 2);
        NUnit.Framework.Assert.That(answer[1][0], Is.EqualTo(1).Within(1e-15));
        buffer[0] = 999;
        NUnit.Framework.Assert.That(answer[1][0], Is.EqualTo(1).Within(1e-15));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(1000002)]
    [TestCase(int.MaxValue)]
    public void InvalidSampleBudgetsFailBeforeExecutingEitherCallback(int samples)
    {
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEquation.rungeKutta4(
            (t, state) => throw new Exception("Must not run"), 0.0, 0, 1, samples));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEquation.rungeKutta4(
            (t, state) => throw new Exception("Must not run"), Vector(0), 0, 1, samples));
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void NonfiniteInputsFailBeforeExecution(double bad)
    {
        Func<double, double, double> scalar = (t, state) => throw new Exception("Must not run");
        Func<double, Vector<double>, Vector<double>> system = (t, state) => throw new Exception("Must not run");
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEquation.rungeKutta4(scalar, bad, 0, 1));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEquation.rungeKutta4(scalar, 0, bad, 1));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEquation.rungeKutta4(scalar, 0, 0, bad));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEquation.rungeKutta4(system, Vector(1, bad), 0, 1));
    }

    [TestCase(-double.MaxValue, double.MaxValue)]
    [TestCase(0.0, double.Epsilon)]
    [TestCase(1.0, 1.0000000000000002)]
    public void UnrepresentableTimeStepsFailBeforeExecution(double start, double end)
    {
        NUnit.Framework.Assert.Throws<ArgumentException>(() => DifferentialEquation.rungeKutta4(
            (t, state) => throw new Exception("Must not run"), 0.0, start, end, 2));
        NUnit.Framework.Assert.Throws<ArgumentException>(() => DifferentialEquation.rungeKutta4(
            (t, state) => throw new Exception("Must not run"), Vector(0), start, end, 2));
    }

    [Test]
    public void NullOrEmptyInputsAreRejected()
    {
        NUnit.Framework.Assert.Throws<ArgumentNullException>(() => DifferentialEquation.rungeKutta4(null!, 0.0, 0, 1));
        NUnit.Framework.Assert.Throws<ArgumentNullException>(() => DifferentialEquation.rungeKutta4(null!, Vector(0), 0, 1));
        NUnit.Framework.Assert.Throws<ArgumentNullException>(() => DifferentialEquation.rungeKutta4((t, state) => state, (Vector<double>)null!, 0, 1));
        NUnit.Framework.Assert.Throws<ArgumentException>(() => DifferentialEquation.rungeKutta4((t, state) => state, Vector(), 0, 1));
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void NonfiniteDerivativesFailClearly(double bad)
    {
        NUnit.Framework.Assert.Throws<ArithmeticException>(() => DifferentialEquation.rungeKutta4((t, state) => bad, 1.0, 0, 1));
        NUnit.Framework.Assert.Throws<ArithmeticException>(() => DifferentialEquation.rungeKutta4((t, state) => Vector(bad), Vector(1), 0, 1));
    }

    [Test]
    public void WrongSizedOrNullDerivativesFailClearly()
    {
        NUnit.Framework.Assert.Throws<InvalidOperationException>(() => DifferentialEquation.rungeKutta4((t, state) => Vector(1, 2), Vector(1), 0, 1));
        NUnit.Framework.Assert.Throws<InvalidOperationException>(() => DifferentialEquation.rungeKutta4((t, state) => null!, Vector(1), 0, 1));
    }

    [Test]
    public void OverflowInTheLastUpdateIsRejectedEvenWhenAllDerivativeValuesWereFinite()
    {
        NUnit.Framework.Assert.Throws<ArithmeticException>(() => DifferentialEquation.rungeKutta4((t, state) => double.MaxValue / 4, 0.0, 0, 16, 2));
        NUnit.Framework.Assert.Throws<ArithmeticException>(() => DifferentialEquation.rungeKutta4((t, state) => Vector(double.MaxValue / 4), Vector(0), 0, 16, 2));
    }

    [Test]
    public void CallbackExceptionsRemainVisibleToTheCaller()
    {
        var expected = new InvalidOperationException("equation outside its domain");
        var actual = NUnit.Framework.Assert.Throws<InvalidOperationException>(() => DifferentialEquation.rungeKutta4((t, state) => throw expected, 0.0, 0, 1));
        NUnit.Framework.Assert.That(actual, Is.SameAs(expected));
    }
}
