using System;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture("host")]
    [TestFixture("generated")]
    [TestFixture("precompiled expression")]
    [TestFixture("precompiled script")]
    [NonParallelizable]
    public class LambertWBranchPointTests
    {
        private const double BranchPoint = -1.0 / Math.E;
        private readonly string _surface;
        private NativeCompiler _compiler;
        private Func<double, double> _principal;
        private Func<double, double> _lower;
#if !__MonoCS__
        private IntPtr _gsl;
#endif

        public LambertWBranchPointTests(string surface) => _surface = surface;

        [OneTimeSetUp]
        public void LoadTheProductionHostOrCompleteShippedDefinitions()
        {
            GSLInitializer.Initialize();
            _principal = SpecialFunctions.LambertW0;
            _lower = SpecialFunctions.LambertWm1;
            if (_surface == "precompiled expression")
            {
                _principal = PrecompiledExpressionSurface.LambertW0;
                _lower = PrecompiledExpressionSurface.LambertWm1;
            }
            else if (_surface == "precompiled script")
            {
                _principal = PrecompiledScriptMathSurface.LambertW0;
                _lower = PrecompiledScriptMathSurface.LambertWm1;
            }
            else if (_surface == "generated")
            {
                _compiler = new NativeCompiler();
                _compiler.AddDll(typeof(GslConfig).Assembly.Location);
                var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
#if !__MonoCS__
                _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
                NativeLibrary.SetDllImportResolver(assembly,
                    (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
#endif
                var type = assembly.GetType("EmittedFunctions");
                _principal = (Func<double, double>)Delegate.CreateDelegate(typeof(Func<double, double>), type.GetMethod("LambertW0"));
                _lower = (Func<double, double>)Delegate.CreateDelegate(typeof(Func<double, double>), type.GetMethod("LambertWm1"));
            }
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedDefinitions()
        {
            _principal = null;
            _lower = null;
            _compiler?.Dispose();
#if !__MonoCS__
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
#endif
        }

        // DLMF 4.13.E2 gives W0(-1/e) = W-1(-1/e) = -1. Use the same
        // rounded binary64 endpoint as GSL's real-domain convention.
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void BothBranchesIncludeTheirSharedEndpoint(bool lower, bool throwOnError)
        {
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = throwOnError;
                NUnit.Framework.Assert.That((lower ? _lower : _principal)(BranchPoint), Is.EqualTo(-1.0));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InputsBelowTheBranchPointStillReturnNaN(bool throwOnError)
        {
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = throwOnError;
                // For a negative double, increasing its signed bit pattern
                // produces the immediately smaller representable value.
                var below = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(BranchPoint) + 1);
                foreach (var x in new[] { below, -1.0, double.NegativeInfinity })
                {
                    NUnit.Framework.Assert.That(_principal(x), Is.NaN);
                    NUnit.Framework.Assert.That(_lower(x), Is.NaN);
                }
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }

        [Test]
        public void TheNextRepresentableInputRetainsTwoDistinctBranches()
        {
            var above = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(BranchPoint) - 1);
            var principal = _principal(above);
            var lower = _lower(above);
            NUnit.Framework.Assert.That(principal, Is.GreaterThan(-1).And.LessThan(0));
            NUnit.Framework.Assert.That(lower, Is.LessThan(-1));
            // Check the defining equation, without imposing a new accuracy
            // requirement on GSL's ill-conditioned values near the branch point.
            NUnit.Framework.Assert.That(principal * Math.Exp(principal), Is.EqualTo(above).Within(2).Ulps);
            NUnit.Framework.Assert.That(lower * Math.Exp(lower), Is.EqualTo(above).Within(2).Ulps);
        }

        [TestCase(false, -0.5)]
        [TestCase(false, 0.0)]
        [TestCase(false, 1.0)]
        [TestCase(true, -2.0)]
        [TestCase(true, -4.0)]
        public void OrdinaryInputsStillInvertWTimesExpW(bool lower, double expected)
        {
            var input = expected * Math.Exp(expected);
            NUnit.Framework.Assert.That((lower ? _lower : _principal)(input), Is.EqualTo(expected).Within(2e-14));
        }

        [Test]
        public void TheExistingLowerBranchContinuationAtNonnegativeInputsIsUnchanged()
        {
            // The documented GSL continuation at x>=0 is retained by this
            // endpoint fix; it is not the negative real branch of Lambert W.
            NUnit.Framework.Assert.That(_lower(0), Is.EqualTo(0));
            NUnit.Framework.Assert.That(_lower(Math.E), Is.EqualTo(1).Within(2e-14));
        }
    }

    [TestFixture, NonParallelizable]
    public class LambertWExpressionTests
    {
        private sealed class LegacyEvaluator : ExpressionsEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [OneTimeSetUp]
        public void InitializeNativeFunctions() => GSLInitializer.Initialize();

        [TestCase("LambertW0(-1/e)", false)]
        [TestCase("LambertW0(-1/e)", true)]
        [TestCase("LambertWm1(-1/e)", false)]
        [TestCase("LambertWm1(-1/e)", true)]
        public void CalculatorExpressionsIncludeTheSharedBranchPoint(string expression, bool precompiled)
        {
            var evaluator = precompiled ? new ExpressionsEvaluator() : new LegacyEvaluator();
            Func<double, double> function = evaluator.Evaluate(expression, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(-1.0));
        }
    }
}
