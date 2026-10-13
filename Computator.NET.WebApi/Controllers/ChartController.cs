using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Hosting;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    [LegacyCodeRoute]
    public class ChartController : Controller
    {
        private const double DefaultXMax = 5, DefaultXMin = -5, DefaultYMax = 3, DefaultYMin = -3;
        private const int DefaultWidth = 1920, DefaultHeight = 1080;
        private static readonly ImageFormat DefaultImageFormat = ImageFormat.Png;
        private readonly IChartFactory _chartFactory;
        private readonly IModeDeterminer _modeDeterminer;
        private readonly IFunctionsProvider _functionsProvider;
        public ChartController(IChartFactory chartFactory, IModeDeterminer modeDeterminer, IFunctionsProvider functionsProvider)
        { _chartFactory = chartFactory; _modeDeterminer = modeDeterminer; _functionsProvider = functionsProvider; }
        [HttpGet("{equation}")]
        public IActionResult Get(string equation) => Get(DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(double x0, double xn, double y0, double yn, string equation) => Get(DefaultWidth, DefaultHeight, x0, xn, y0, yn, equation);
        [HttpGet("{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(int width, int height, double x0, double xn, double y0, double yn, string equation) => Get(DefaultImageFormat, width, height, x0, xn, y0, yn, equation);
        [HttpGet("{width}/{height}/{equation}")]
        public IActionResult Get(int width, int height, string equation) => Get(DefaultImageFormat, width, height, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{imageFormat}/{width}/{height}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, string equation) => Get(imageFormat, width, height, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{imageFormat}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, string equation) => Get(imageFormat, DefaultWidth, DefaultHeight, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        [HttpGet("{imageFormat}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, double x0, double xn, double y0, double yn, string equation) => Get(imageFormat, DefaultWidth, DefaultHeight, x0, xn, y0, yn, equation);
        [HttpGet("{imageFormat}/{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, double x0, double xn, double y0, double yn, string equation)
        {
            // The same policy also protects direct controller calls, not only HTTP filters.
            var errors = ExecutionPolicy.ValidateLegacy("Chart", "Get", new System.Collections.Generic.Dictionary<string, object>
            { ["equation"] = equation, ["width"] = width, ["height"] = height, ["x0"] = x0, ["xn"] = xn, ["y0"] = y0, ["yn"] = yn });
            if (errors.Count != 0) return BadRequest(new ValidationProblemDetails(errors));
            var previousContext = SynchronizationContext.Current;
            IChart chart = null;
            try
            {
                var mode = _modeDeterminer.DetermineMode(equation);
                chart = _chartFactory.Create(mode); chart.AddFunction(_functionsProvider.GetFunction(equation, mode, ""));
                chart.XMax = xn; chart.XMin = x0; chart.YMin = y0; chart.YMax = yn; chart.Visible = true;
                using (var image = chart.GetImage(width, height))
                using (var stream = new MemoryStream())
                { image.Save(stream, imageFormat); return File(stream.ToArray(), $"image/{imageFormat.ToString().ToLowerInvariant()}"); }
            }
            finally
            {
                try { (chart as IDisposable)?.Dispose(); }
                finally { SynchronizationContext.SetSynchronizationContext(previousContext); }
            }
        }
    }
}
