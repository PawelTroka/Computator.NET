using System.Drawing.Imaging;
using System.Threading;
using Computator.NET.Charting;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.WebApi.Controllers;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class Net10ChartContextTests
    {
        [Test]
        public void RenderingRestoresTheHttpThreadsSynchronizationContext()
        {
            GSLInitializer.Initialize();
            var previous = SynchronizationContext.Current;
            var bootstrapper = new CoreBootstrapper();
            using (bootstrapper.Container)
            {
                var mode = bootstrapper.Create<IModeDeterminer>();
                var functions = new FunctionsProvider(mode, bootstrapper.Create<IExpressionsEvaluator>());
                var controller = new ChartController(new ChartFactory(), mode, functions);
                var result = (FileContentResult)controller.Get(ImageFormat.Bmp, 300, 300, 0, 5, 0, 5, "x");
                NUnit.Framework.Assert.That(SynchronizationContext.Current, Is.SameAs(previous));
                NUnit.Framework.Assert.That(result.FileContents.Length, Is.GreaterThan(64 * 1024));
            }
        }
    }
}
