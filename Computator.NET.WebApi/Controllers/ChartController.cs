using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        // GET api/chart/2x
        [HttpGet("{equation}")]
        public IActionResult Get(string equation)
        {
            return Get(DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        }


        // GET api/chart/-5/5/-5/5/2x
        [HttpGet("{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(double x0, double xn, double y0, double yn, string equation)
        {
            return Get(DefaultWidth, DefaultHeight, x0, xn, y0, yn, equation);
        }


        // GET api/chart/1920/1080/-5/5/-5/5/2x
        [HttpGet("{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(int width, int height, double x0, double xn, double y0, double yn, string equation)
        {
            return Get(DefaultImageFormat, width, height, x0, xn, y0, yn, equation);
        }

        // GET api/chart/1920/1080/2x
        [HttpGet("{width}/{height}/{equation}")]
        public IActionResult Get(int width, int height, string equation)
        {
            return Get(DefaultImageFormat, width, height, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        }

        // GET api/chart/png/1920/1080/2x
        [HttpGet("{imageFormat}/{width}/{height}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, string equation)
        {
            return Get(imageFormat, width, height, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        }

        // GET api/chart/png/2x
        [HttpGet("{imageFormat}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, string equation)
        {
            return Get(imageFormat, DefaultWidth, DefaultHeight, DefaultXMin, DefaultXMax, DefaultYMin, DefaultYMax, equation);
        }

        // GET api/chart/png/-5/5/-5/5/2x
        [HttpGet("{imageFormat}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, double x0, double xn, double y0, double yn, string equation)
        {
            return Get(imageFormat, DefaultWidth, DefaultHeight, x0, xn, y0, yn, equation);
        }

        // GET api/chart/Png/1920/1080/-5/5/-5/5/2x
        [HttpGet("{imageFormat}/{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, double x0, double xn, double y0, double yn, string equation)
        {
            var equations = new[] { equation };
            var decodedEquations = equations.ToArray();

            var calculationsMode = CalculationsMode.Error;

            foreach (var eq in decodedEquations)
            {
                var mode = _modeDeterminer.DetermineMode(eq);

                if (mode == CalculationsMode.Complex)
                    calculationsMode = CalculationsMode.Complex;
                else if (mode == CalculationsMode.Fxy && calculationsMode != CalculationsMode.Complex)
                    calculationsMode = CalculationsMode.Fxy;
                else if (mode == CalculationsMode.Real && calculationsMode != CalculationsMode.Complex && calculationsMode != CalculationsMode.Fxy)
                    calculationsMode = CalculationsMode.Real;
            }

            var chart = _chartFactory.Create(calculationsMode);

            foreach (var eq in decodedEquations)
            {
                var func = _functionsProvider.GetFunction(eq, calculationsMode, "");
                chart.AddFunction(func);
            }

            chart.XMax = xn;
            chart.XMin = x0;
            chart.YMin = y0;
            chart.YMax = yn;
            chart.Visible = true;

            Image img = chart.GetImage(width, height);
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, imageFormat);
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(ms.ToArray());
                result.Content.Headers.ContentType = new MediaTypeHeaderValue($"image/{imageFormat.ToString().ToLowerInvariant()}");
                //return result;
                return File(ms.ToArray(), $"image/{imageFormat.ToString().ToLowerInvariant()}");
            }
        }
    }
}
