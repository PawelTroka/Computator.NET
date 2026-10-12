using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ChartController : Controller
    {
        private const double DefaultXMax = 5;
        private const double DefaultXMin = -DefaultXMax;
        private const double DefaultYMax = 3;
        private const double DefaultYMin = -DefaultYMax;
        private const int DefaultWidth = 1920;
        private const int DefaultHeight = 1080;
        private static readonly ImageFormat DefaultImageFormat = ImageFormat.Png;
        private readonly IChartFactory _chartFactory;
        private readonly IModeDeterminer _modeDeterminer;
        private readonly IFunctionsProvider _functionsProvider;

        public ChartController(IChartFactory chartFactory, IModeDeterminer modeDeterminer, IFunctionsProvider functionsProvider)
        {
            _chartFactory = chartFactory;
            _modeDeterminer = modeDeterminer;
            _functionsProvider = functionsProvider;
        }

        [HttpGet("{equation}")]
        public IActionResult Get(string equation) => Get(DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(double x0, double xn, double y0, double yn, string equation) =>
            Get(DefaultWidth, DefaultHeight, x0, xn, y0, yn, equation);
        [HttpGet("{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(int width, int height, double x0, double xn, double y0, double yn, string equation) =>
            Get(DefaultImageFormat, width, height, x0, xn, y0, yn, equation);
        [HttpGet("{width}/{height}/{equation}")]
        public IActionResult Get(int width, int height, string equation) =>
            Get(DefaultImageFormat, width, height, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{imageFormat}/{width}/{height}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, string equation) =>
            Get(imageFormat, width, height, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{imageFormat}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, string equation) =>
            Get(imageFormat, DefaultWidth, DefaultHeight, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{imageFormat}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, double x0, double xn, double y0, double yn, string equation) =>
            Get(imageFormat, DefaultWidth, DefaultHeight, x0, xn, y0, yn, equation);

        [HttpGet("{imageFormat}/{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, double x0, double xn, double y0, double yn, string equation)
        {
            // A chart handle can install a WindowsFormsSynchronizationContext. HTTP
            // workers have no UI message loop to service asynchronous response writes.
            var previousContext = SynchronizationContext.Current;
            IChart chart = null;
            try
            {
                var mode = _modeDeterminer.DetermineMode(equation);
                chart = _chartFactory.Create(mode);
                chart.AddFunction(_functionsProvider.GetFunction(equation, mode, ""));
                chart.XMax = xn;
                chart.XMin = x0;
                chart.YMin = y0;
                chart.YMax = yn;
                chart.Visible = true;
                using (var image = chart.GetImage(width, height))
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, imageFormat);
                    return File(stream.ToArray(), $"image/{imageFormat.ToString().ToLowerInvariant()}");
                }
            }
            finally
            {
                try { (chart as IDisposable)?.Dispose(); }
                finally { SynchronizationContext.SetSynchronizationContext(previousContext); }
            }
        }
    }
}
