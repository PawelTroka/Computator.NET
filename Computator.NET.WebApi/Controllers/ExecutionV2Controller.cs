using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Microsoft.AspNetCore.Mvc;

namespace Computator.NET.WebApi.Controllers
{
    [ApiController]
    [Route("api/v2")]
    public sealed class ExecutionV2Controller : ControllerBase
    {
        private readonly IModeDeterminer _mode;
        private readonly IChartFactory _charts;
        public ExecutionV2Controller(IModeDeterminer mode, IChartFactory charts) { _mode = mode; _charts = charts; }

        [HttpPost("execute")]
        [Consumes("application/json")]
        [RequestSizeLimit(262144)]
        public IActionResult Execute([FromBody] ExecutionRequest request)
        {
            if (!request.ValidNumbers()) return BadRequest(new ProblemDetails { Title = "Invalid numeric parameters." });
            try
            {
                // Evaluators contain mutable compilation state. Do not share them or
                // generated function instances across requests/users through a singleton cache.
                if (request.Operation == "script")
                {
                    var output = new StringBuilder();
                    new ScriptEvaluator().Evaluate(request.Expression, request.CustomFunctions).Evaluate(text =>
                    {
                        if (output.Length + text.Length > 65536) throw new ArgumentException("Script output exceeds 64 KiB.");
                        output.Append(text);
                    });
                    return Ok(new { result = output.ToString() });
                }
                var mode = request.Operation == "calculate" ? _mode.DetermineMode(request.Expression) : CalculationsMode.Real;
                var function = new ExpressionsEvaluator().Evaluate(request.Expression, request.CustomFunctions, mode);
                string result;
                switch (request.Operation)
                {
                    case "calculate":
                        result = mode == CalculationsMode.Complex ? function.Evaluate(new Complex(request.X, request.Y)).ToMathString()
                            : mode == CalculationsMode.Fxy ? function.Evaluate(request.X, request.Y).ToMathString()
                            : function.Evaluate(request.X).ToMathString();
                        break;
                    case "integral":
                        if (!NumericalMethodsInfo.Instance.IntegrationMethods.TryGetValue(request.Method ?? "", out var integral))
                            return BadRequest(new ProblemDetails { Title = "Unknown integration method." });
                        result = integral.Invoke(x => function.Evaluate(x), request.A, request.B, request.N).ToMathString();
                        break;
                    case "derivative":
                        if (!NumericalMethodsInfo.Instance.DerrivationMethods.TryGetValue(request.Method ?? "", out var derivative))
                            return BadRequest(new ProblemDetails { Title = "Unknown derivative method." });
                        result = derivative.Invoke(x => function.Evaluate(x), request.X, request.Order, request.Epsilon).ToMathString();
                        break;
                    case "function-root":
                        if (!NumericalMethodsInfo.Instance.FunctionRootMethods.TryGetValue(request.Method ?? "", out var root))
                            return BadRequest(new ProblemDetails { Title = "Unknown root method." });
                        result = root.Invoke(x => function.Evaluate(x), request.A, request.B, request.Epsilon, request.N).ToMathString();
                        break;
                    default: return BadRequest(new ProblemDetails { Title = "Unknown operation." });
                }
                return Ok(new { result });
            }
            catch (Exception exception) when (exception is CompilationException || exception is EvaluationException || exception is ArgumentException)
            {
                return BadRequest(new ProblemDetails { Title = "Execution failed.", Detail = exception.Message, Status = 400 });
            }
        }

        [HttpPost("chart")]
        [Consumes("application/json")]
        [RequestSizeLimit(262144)]
        public IActionResult Chart([FromBody] ExecutionRequest request)
        {
            if (!request.ValidNumbers() || request.Width < 100 || request.Height < 100 ||
                request.Width > 4096 || request.Height > 4096 || (long)request.Width * request.Height > 4194304 ||
                request.XMin >= request.XMax || request.YMin >= request.YMax)
                return BadRequest(new ProblemDetails { Title = "Invalid chart dimensions or bounds." });
            var previous = SynchronizationContext.Current;
            IChart chart = null;
            try
            {
                var mode = _mode.DetermineMode(request.Expression);
                if (mode == CalculationsMode.Fxy) return UnprocessableEntity(new ProblemDetails { Title = "Server-side 3D rendering is not implemented." });
                var function = new ExpressionsEvaluator().Evaluate(request.Expression, request.CustomFunctions, mode);
                chart = _charts.Create(mode);
                chart.AddFunction(function);
                chart.XMin = request.XMin; chart.XMax = request.XMax;
                chart.YMin = request.YMin; chart.YMax = request.YMax;
                chart.Visible = true;
                using (var image = chart.GetImage(request.Width, request.Height))
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
            catch (Exception exception) when (exception is CompilationException || exception is EvaluationException || exception is ArgumentException)
            {
                return BadRequest(new ProblemDetails { Title = "Chart generation failed.", Detail = exception.Message, Status = 400 });
            }
            finally
            {
                try { (chart as IDisposable)?.Dispose(); }
                finally { SynchronizationContext.SetSynchronizationContext(previous); }
            }
        }
    }

    public sealed class ExecutionRequest
    {
        [Required, StringLength(32)] public string Operation { get; set; } = "calculate";
        [Required, StringLength(65536)] public string Expression { get; set; }
        [Required(AllowEmptyStrings = true), StringLength(65536)] public string CustomFunctions { get; set; } = "";
        [StringLength(256)] public string Method { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double A { get; set; } = -1;
        public double B { get; set; } = 1;
        public uint N { get; set; } = 10000;
        public uint Order { get; set; } = 1;
        public double Epsilon { get; set; } = 1e-6;
        public int Width { get; set; } = 600;
        public int Height { get; set; } = 400;
        public double XMin { get; set; } = -5;
        public double XMax { get; set; } = 5;
        public double YMin { get; set; } = -5;
        public double YMax { get; set; } = 5;
        public bool ValidNumbers() => new[] { X, Y, A, B, Epsilon, XMin, XMax, YMin, YMax }.All(double.IsFinite)
            && N >= 1 && N <= 1000000 && Order >= 1 && Order <= 10 && Epsilon > 0 && Epsilon <= 1;
    }
}
