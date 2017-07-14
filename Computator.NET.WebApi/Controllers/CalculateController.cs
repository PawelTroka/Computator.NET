using System.Net;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Functions;
using Microsoft.AspNetCore.Mvc;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class CalculateController : Controller
    {
        private readonly IExpressionsEvaluator _expressionsEvaluator;

        public CalculateController(IExpressionsEvaluator expressionsEvaluator)
        {
            _expressionsEvaluator = expressionsEvaluator;
        }

        [HttpGet("func/{equation}/{calculationsMode}")]
        [HttpGet("func/{equation}/{calculationsMode}/{customFunctionsCode}")]
        public Function GetFunc(string equation, CalculationsMode calculationsMode, string customFunctionsCode="")
        {
            var decodedEquation = WebUtility.UrlDecode(equation);
            var decodedCustomFunctionsCode = WebUtility.UrlDecode(customFunctionsCode);
            var func = _expressionsEvaluator.Evaluate(decodedEquation, decodedCustomFunctionsCode, calculationsMode);
            return func;
        }

        // GET api/calculate/real/2*cos(x)/10.2
        [HttpGet("real/{equation}/{x}")]
        [HttpGet("real/{equation}/{x}/{customFunctionsCode}")]
        public string GetReal(string equation, double x, string customFunctionsCode="")
        {
            var func = GetFunc(equation, CalculationsMode.Real, customFunctionsCode);

            return func.Evaluate(x).ToMathString();
        }

        // GET api/calculate/complex/z*cos(z)/(1,2)
        [HttpGet("complex/{equation}/{z}")]
        [HttpGet("complex/{equation}/{z}/{customFunctionsCode}")]
        public string GetComplex(string equation, Complex z, string customFunctionsCode = "")
        {
            var func = GetFunc(equation, CalculationsMode.Complex, customFunctionsCode);

            return func.Evaluate(z).ToMathString();
        }

        // GET api/calculate/3d/x+y/2.1/10.2
        [HttpGet("3d/{equation}/{x}/{y}")]
        [HttpGet("3d/{equation}/{x}/{y}/{customFunctionsCode}")]
        public string Get3D(string equation, double x, double y, string customFunctionsCode = "")
        {
            var func = GetFunc(equation, CalculationsMode.Fxy, customFunctionsCode);

            return func.Evaluate(x,y).ToMathString();
        }
    }
}
