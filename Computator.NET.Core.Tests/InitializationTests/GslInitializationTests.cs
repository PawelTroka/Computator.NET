using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Tests.InitializationTests
{
    [TestFixture]
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

            messengingServiceMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()),Times.Never);
        }

        [Test]
        public void AfterInitializationCallToGslMethodShouldNotThrow()
        {
            GSLInitializer.Initialize();

            Assert.DoesNotThrow(() => NativeMethods.gsl_set_error_handler_off());
        }

        [Test]
        public void AfterInitializationCallToGslSpecialFunctionShouldNotThrow()
        {
            GSLInitializer.Initialize();

            var x = double.MinValue;

            Assert.DoesNotThrow(() => x = SpecialFunctions.Debye(2, 10));

            Assert.AreNotEqual(double.MinValue,x);
        }
    }
}