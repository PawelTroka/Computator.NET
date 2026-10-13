using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Threading;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Services;
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
            var errors = ExecutionPolicy.Validate(request, false);
            if (errors.Count != 0) return BadRequest(new ValidationProblemDetails(errors));
            try
            {
                if (request.Operation == "script")
                {
                    var output = new BoundedExecutionOutput();
                    new ScriptEvaluator().Evaluate(request.Expression, request.CustomFunctions).Evaluate(output.Append);
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
                        result = NumericalMethodsInfo.Instance.IntegrationMethods[request.Method].Invoke(x => function.Evaluate(x), request.A, request.B, request.N).ToMathString();
                        break;
                    case "derivative":
                        result = NumericalMethodsInfo.Instance.DerrivationMethods[request.Method].Invoke(x => function.Evaluate(x), request.X, request.Order, request.Epsilon).ToMathString();
                        break;
                    case "function-root":
                        result = NumericalMethodsInfo.Instance.FunctionRootMethods[request.Method].Invoke(x => function.Evaluate(x), request.A, request.B, request.Epsilon, request.N).ToMathString();
                        break;
                    default: return BadRequest(new ProblemDetails { Title = "Unknown operation." });
                }
                return Ok(new { result });
            }
            catch (Exception exception) when (exception is CompilationException || exception is EvaluationException || exception is ArgumentException)
            { return BadRequest(new ProblemDetails { Title = "Execution failed.", Detail = exception.Message, Status = 400 }); }
        }
        [HttpPost("chart")]
        [Consumes("application/json")]
        [RequestSizeLimit(262144)]
        public IActionResult Chart([FromBody] ExecutionRequest request)
        {
            var errors = ExecutionPolicy.Validate(request, true);
            if (errors.Count != 0) return BadRequest(new ValidationProblemDetails(errors));
            var previous = SynchronizationContext.Current;
            IChart chart = null;
            try
            {
                var mode = _mode.DetermineMode(request.Expression);
                if (mode == CalculationsMode.Fxy) return UnprocessableEntity(new ProblemDetails { Title = "Server-side 3D rendering is not implemented." });
                var function = new ExpressionsEvaluator().Evaluate(request.Expression, request.CustomFunctions, mode);
                chart = _charts.Create(mode); chart.AddFunction(function);
                chart.XMin = request.XMin; chart.XMax = request.XMax;
                chart.YMin = request.YMin; chart.YMax = request.YMax; chart.Visible = true;
                using (var image = chart.GetImage(request.Width, request.Height))
                using (var stream = new MemoryStream())
                { image.Save(stream, ImageFormat.Png); return File(stream.ToArray(), "image/png"); }
            }
            catch (Exception exception) when (exception is CompilationException || exception is EvaluationException || exception is ArgumentException)
            { return BadRequest(new ProblemDetails { Title = "Chart generation failed.", Detail = exception.Message, Status = 400 }); }
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
        [Required, StringLength(ExecutionPolicy.MaximumSourceCharacters)] public string Expression { get; set; }
        [Required(AllowEmptyStrings = true), StringLength(ExecutionPolicy.MaximumSourceCharacters)] public string CustomFunctions { get; set; } = "";
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
        public bool ValidNumbers() => ExecutionPolicy.ValidNumbers(this);
    }
}
