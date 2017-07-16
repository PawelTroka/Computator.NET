using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ChartController : Controller
    {
        private IChartFactory _chartFactory;
        private IModeDeterminer _modeDeterminer;
        private IExpressionsEvaluator _expressionsEvaluator;

        public ChartController(IChartFactory chartFactory, IModeDeterminer modeDeterminer, IExpressionsEvaluator expressionsEvaluator)
        {
            _chartFactory = chartFactory;
            _modeDeterminer = modeDeterminer;
            _expressionsEvaluator = expressionsEvaluator;
        }

        // GET api/chart/2x
        [HttpGet("{equation}")]
        public IActionResult Get(string equation)
        {
            return Get(-5, 5, -3, 3, equation);
        }



        // GET api/chart/-5/5/-5/5/2x
        [HttpGet("{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(double x0, double xn, double y0, double yn, string equation)
        {
            return Get(1920, 1080, x0, xn, y0, yn, equation);
        }


        // GET api/chart/1920/1080/-5/5/-5/5/2x
        [HttpGet("{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(int width, int height, double x0, double xn, double y0, double yn, string equation)
        {
            return Get(ImageFormat.Png, width, height, x0, xn, y0, yn, equation);
        }

        // GET api/chart/Png/1920/1080/-5/5/-5/5/2x
        [HttpGet("{imageFormat}/{width}/{height}/{x0}/{xn}/{y0}/{yn}/{equation}")]
        public IActionResult Get(ImageFormat imageFormat, int width, int height, double x0, double xn, double y0, double yn, string equation)
        {
            var equations = new[] { equation };
            var calculationsMode = CalculationsMode.Error;

            foreach (var eq in equations)
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

            foreach (var expression in equations.Select(eq => _expressionsEvaluator.Evaluate(eq, "", calculationsMode)))
            {
                chart.AddFunction(expression);
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
