using System;
using System.Threading.Tasks;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;
namespace Computator.NET.Core.Tests.ConfigurationTests
{
    [TestFixture, NonParallelizable]
    public class GslErrorBoundaryTests
    {
        [Test]
        public void CallbackReturnsNormallyAndTranslationOccursAfterNativeCall()
        {
            var old = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = true;
                using (var scope = GslErrorBoundary.Enter())
                {
                    NUnit.Framework.Assert.DoesNotThrow(() => GslErrorBoundary.Record("domain", 1));
                    NUnit.Framework.Assert.Throws<ArithmeticException>(() => scope.ThrowIfError());
                }
                using (var recovered = GslErrorBoundary.Enter()) NUnit.Framework.Assert.DoesNotThrow(() => recovered.ThrowIfError());
            }
            finally { GslErrorBoundary.ThrowOnError = old; }
        }
        [Test]
        public void ReturnValueModeDoesNotThrowAndThreadsDoNotShareErrors()
        {
            var old = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = false;
                using (var scope = GslErrorBoundary.Enter()) { GslErrorBoundary.Record("domain", 1); scope.ThrowIfError(); }
                GslErrorBoundary.ThrowOnError = true;
                using (var outer = GslErrorBoundary.Enter())
                {
                    Task.Run(() => { using (var other = GslErrorBoundary.Enter()) GslErrorBoundary.Record("worker error", 1); }).GetAwaiter().GetResult();
                    NUnit.Framework.Assert.DoesNotThrow(() => outer.ThrowIfError());
                }
            }
            finally { GslErrorBoundary.ThrowOnError = old; }
        }
    }
}
