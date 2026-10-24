using System;
using System.Runtime.InteropServices;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.InitializationTests
{
    [TestFixture]
    [NonParallelizable]
    public class GslInitializationTests
    {
        [Test]
        public void InitializationShouldNotThrow()
        {
            Assert.DoesNotThrow(GSLInitializer.Initialize);
        }

        [Test]
        public void InitializationShouldNotCallMessengingServiceUnlessThereIsAnError()
        {
            var messengingServiceMock = new Mock<IMessagingService>();
            messengingServiceMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            GSLInitializer.Initialize();
            messengingServiceMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void AfterInitializationCallToGslMethodShouldNotThrow()
        {
            GSLInitializer.Initialize();
            IntPtr previous = IntPtr.Zero;
            bool changed = false;
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    previous = NativeMethods.gsl_set_error_handler_off();
                    changed = true;
                });
            }
            finally
            {
                // Initialize installs the callback only once. Leaving it off
                // breaks native error-policy tests and later application calls.
                // The initializer retains its original managed delegate root.
                if (changed)
                {
                    var handler = previous == IntPtr.Zero ? null :
                        Marshal.GetDelegateForFunctionPointer<gsl_error_handler_t>(previous);
                    NativeMethods.gsl_set_error_handler(handler);
                    GC.KeepAlive(handler);
                }
            }
        }

        [Test]
        public void AfterInitializationCallToGslSpecialFunctionShouldNotThrow()
        {
            GSLInitializer.Initialize();
            var x = double.MinValue;
            Assert.DoesNotThrow(() => x = SpecialFunctions.Debye(2, 10));
            Assert.AreNotEqual(double.MinValue, x);
        }
    }
}
