using System;
using System.Reflection;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    // The native candidate Cake target runs this suite as well as the full
    // numerical suite, so the error boundary is checked against both binaries.
    [TestFixture, NonParallelizable]
    public class GslNativeErrorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void NativeDomainErrorReturnsThroughTheBoundaryAndTheNextCallRecovers(bool throwOnError)
        {
            GSLInitializer.Initialize();
            var previous = GslErrorBoundary.ThrowOnError;
            var wrapper = typeof(SpecialFunctions).GetMethod("gsl_sf_gamma_inc_P", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(wrapper, Is.Not.Null);
            try
            {
                GslErrorBoundary.ThrowOnError = throwOnError;
                // Call the reviewed wrapper, bypassing the public convenience
                // method's early argument check so the native callback really runs.
                if (throwOnError)
                {
                    var exception = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        wrapper.Invoke(null, new object[] { -1.0, 1.0 }));
                    NUnit.Framework.Assert.That(exception.InnerException, Is.TypeOf<ArithmeticException>());
                    NUnit.Framework.Assert.That(exception.InnerException.Message, Does.Contain("GSL error"));
                }
                else
                    NUnit.Framework.Assert.That((double)wrapper.Invoke(null, new object[] { -1.0, 1.0 }), Is.NaN);
                var recovered = (double)wrapper.Invoke(null, new object[] { 1.0, 1.0 });
                NUnit.Framework.Assert.That(recovered, Is.EqualTo(1.0 - Math.Exp(-1.0)).Within(1e-12));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GeneratedNativeImportsUseTheSameErrorBoundaryAndRecover(bool throwOnError)
        {
            GSLInitializer.Initialize();
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = throwOnError;
                // Exercise the actual embedded source in the collectible TSL
                // assembly, including its separate native import resolution.
                var function = new ExpressionsEvaluator().Evaluate("gsl_sf_gamma_inc_P(x, 1)", "", CalculationsMode.Real);
                if (throwOnError)
                {
                    var exception = NUnit.Framework.Assert.Throws<CalculationException>(() => function.Evaluate(-1.0));
                    NUnit.Framework.Assert.That(exception.InnerException, Is.TypeOf<ArithmeticException>());
                }
                else NUnit.Framework.Assert.That(function.Evaluate(-1.0), Is.NaN);
                NUnit.Framework.Assert.That(function.Evaluate(1.0), Is.EqualTo(1.0 - Math.Exp(-1.0)).Within(1e-12));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SettingsChangesUpdateExistingHostAndGeneratedFunctionsWithoutReinitialization(bool generated)
        {
            var previous = Settings.Default.CalculationsErrors;
            try
            {
                Settings.Default.CalculationsErrors = CalculationsErrors.ReturnNAN;
                GSLInitializer.Initialize();
                Func<double, double> evaluate;
                if (generated)
                {
                    var function = new ExpressionsEvaluator().Evaluate("gsl_sf_gamma_inc_P(x, 1)", "", CalculationsMode.Real);
                    evaluate = x => function.Evaluate(x);
                }
                else
                {
                    var native = typeof(SpecialFunctions).GetMethod("gsl_sf_gamma_inc_P", BindingFlags.Static | BindingFlags.NonPublic)
                        .CreateDelegate<Func<double, double, double>>();
                    evaluate = x => native(x, 1.0);
                }

                foreach (var mode in new[] { CalculationsErrors.ShowError, CalculationsErrors.ReturnNAN, CalculationsErrors.ShowError })
                {
                    // Exercise the same property that the settings editor changes.
                    // Reuse the native handler and generated function throughout.
                    Settings.Default.CalculationsErrors = mode;
                    if (mode == CalculationsErrors.ReturnNAN)
                        NUnit.Framework.Assert.That(evaluate(-1.0), Is.NaN);
                    else if (generated)
                    {
                        var exception = NUnit.Framework.Assert.Throws<CalculationException>(() => evaluate(-1.0));
                        NUnit.Framework.Assert.That(exception.InnerException, Is.TypeOf<ArithmeticException>());
                    }
                    else NUnit.Framework.Assert.Throws<ArithmeticException>(() => evaluate(-1.0));
                    NUnit.Framework.Assert.That(evaluate(1.0), Is.EqualTo(1.0 - Math.Exp(-1.0)).Within(1e-12));
                }
            }
            finally
            {
                Settings.Default.CalculationsErrors = previous;
                GSLInitializer.Initialize();
            }
        }
    }
}
