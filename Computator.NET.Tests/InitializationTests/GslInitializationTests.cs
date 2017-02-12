using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Tests.InitializationTests
{
    public class GslInitializationTests
    {
        [Test]
        public void InitializationShouldNotThrow()
        {
            var messengingServiceMock = new Mock<IMessagingService>();
            Assert.DoesNotThrow(() => GSLInitializer.Initialize(messengingServiceMock.Object));
        }

        [Test]
        public void InitializationShouldNotCallMessengingServiceUnlessThereIsAnError()
        {
            var messengingServiceMock = new Mock<IMessagingService>();
            messengingServiceMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            GSLInitializer.Initialize(messengingServiceMock.Object);

            messengingServiceMock.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>()),Times.Never);
        }

        [Test]
        public void AfterInitializationCallToGslMethodShouldNotThrow()
        {
            var messengingServiceMock = new Mock<IMessagingService>();

            GSLInitializer.Initialize(messengingServiceMock.Object);

            Assert.DoesNotThrow(() => NativeMethods.gsl_set_error_handler_off());
        }

        [Test]
        public void AfterInitializationCallToGslSpecialFunctionShouldNotThrow()
        {
            var messengingServiceMock = new Mock<IMessagingService>();

            GSLInitializer.Initialize(messengingServiceMock.Object);

            var x = double.MinValue;

            Assert.DoesNotThrow(() => x = SpecialFunctions.Debye(2, 10));

            Assert.AreNotEqual(double.MinValue,x);
        }
    }
}