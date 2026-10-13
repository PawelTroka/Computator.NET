using System.Collections.Generic;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.WebApi.Hosting;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/numerical-calculations")]
    [LegacyCodeRoute]
    public class NumericalCalculationsController : Controller
    {
        private const uint ITERATIONS_MAX = 10000;
        private const double ERROR_MAX_MIN = 1e-8;
        private readonly IFunctionsProvider _functionsProvider;
        public NumericalCalculationsController(IExpressionsEvaluator expressionsEvaluator, ILogger<NumericalCalculationsController> logger, IFunctionsProvider functionsProvider)
        { _functionsProvider = functionsProvider; }
        [HttpGet("integral/list-methods")]
        public ICollection<string> GetIntegralMethods() => NumericalMethodsInfo.Instance.IntegrationMethods.Keys;
        [HttpGet("derivative/list-methods")]
        public ICollection<string> GetDerivativeMethods() => NumericalMethodsInfo.Instance.DerrivationMethods.Keys;
        [HttpGet("function-root/list-methods")]
        public ICollection<string> GetFunctionRootMethods() => NumericalMethodsInfo.Instance.FunctionRootMethods.Keys;
        [HttpGet("integral/{method}/{equation}/{a}/{b}")]
        [HttpGet("integral/{method}/{equation}/{a}/{b}/{n}")]
        [HttpGet("integral/{method}/{equation}/{a}/{b}/{n}/{customFunctionsCode}")]
        public string GetRealIntegral(string method, string equation, double a, double b, double n = ITERATIONS_MAX, string customFunctionsCode = "")
        {
            var func = _functionsProvider.GetFunction(equation, CalculationsMode.Real, customFunctionsCode);
            return NumericalMethodsInfo.Instance.IntegrationMethods[method].Invoke(x => func.Evaluate(x), a, b, n).ToMathString();
        }
        [HttpGet("derivative/{method}/{equation}/{x}")]
        [HttpGet("derivative/{method}/{equation}/{x}/{order}")]
        [HttpGet("derivative/{method}/{equation}/{x}/{order}/{epsilon}")]
        [HttpGet("derivative/{method}/{equation}/{x}/{order}/{epsilon}/{customFunctionsCode}")]
        public string GetRealDerrivative(string method, string equation, double x, uint order = 1, double epsilon = ERROR_MAX_MIN, string customFunctionsCode = "")
        {
            var func = _functionsProvider.GetFunction(equation, CalculationsMode.Real, customFunctionsCode);
            return NumericalMethodsInfo.Instance.DerrivationMethods[method].Invoke(value => func.Evaluate(value), x, order, epsilon).ToMathString();
        }
        [HttpGet("function-root/{method}/{equation}/{a}/{b}")]
        [HttpGet("function-root/{method}/{equation}/{a}/{b}/{maxError}")]
        [HttpGet("function-root/{method}/{equation}/{a}/{b}/{maxError}/{maxIterations}")]
        [HttpGet("function-root/{method}/{equation}/{a}/{b}/{maxError}/{maxIterations}/{customFunctionsCode}")]
        public string GetRealFunctionRoot(string method, string equation, double a, double b, double maxError = ERROR_MAX_MIN, uint maxIterations = ITERATIONS_MAX, string customFunctionsCode = "")
        {
            var func = _functionsProvider.GetFunction(equation, CalculationsMode.Real, customFunctionsCode);
            return NumericalMethodsInfo.Instance.FunctionRootMethods[method].Invoke(value => func.Evaluate(value), a, b, maxError, maxIterations).ToMathString();
        }
    }
}
