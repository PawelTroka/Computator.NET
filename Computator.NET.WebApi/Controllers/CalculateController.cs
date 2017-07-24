using System;
using System.Net;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes.Functions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class CalculateController : Controller
    {
        private readonly IExpressionsEvaluator _expressionsEvaluator;
        private readonly IModeDeterminer _modeDeterminer;
        private readonly ILogger<CalculateController> _logger;

        public CalculateController(IExpressionsEvaluator expressionsEvaluator, IModeDeterminer modeDeterminer, ILogger<CalculateController> logger)
        {
            _expressionsEvaluator = expressionsEvaluator;
            _modeDeterminer = modeDeterminer;
            _logger = logger;
        }
        
        private Function GetFunc(string equation, CalculationsMode calculationsMode, string customFunctionsCode)
        {
            var decodedEquation = WebUtility.UrlDecode(equation);
            _logger.LogInformation($"Decoded equation {equation} to {decodedEquation}");
            var decodedCustomFunctionsCode = WebUtility.UrlDecode(customFunctionsCode);
            _logger.LogInformation($"Decoded custom functions code {customFunctionsCode} to {decodedCustomFunctionsCode}");
            var func = _expressionsEvaluator.Evaluate(decodedEquation, decodedCustomFunctionsCode, calculationsMode);
            return func;
        }

        
        // GET api/calculate/real/2*cos(x)/10.2
        [HttpGet("real/{equation}")]
        [HttpGet("real/{equation}/{x}")]
        [HttpGet("real/{equation}/{x}/{customFunctionsCode}")]
        //[HttpGet("real/{equation}/{customFunctionsCode}")]
        public string GetReal(string equation, double x=0, string customFunctionsCode="")
        {
            return Get(CalculationsMode.Real, equation, x, 0, customFunctionsCode);
        }

        // GET api/calculate/complex/z*cos(z)/(1,2)
        [HttpGet("complex/{equation}")]
        [HttpGet("complex/{equation}/{reZ}/{imZ}")]
        [HttpGet("complex/{equation}/{reZ}/{imZ}/{customFunctionsCode}")]
        //[HttpGet("complex/{equation}/{customFunctionsCode}")]
        public string GetComplex(string equation, double reZ=0, double imZ=0, string customFunctionsCode = "")
        {
            return Get(CalculationsMode.Complex, equation, reZ, imZ, customFunctionsCode);
        }

        // GET api/calculate/3d/x+y/2.1/10.2
        [HttpGet("3d/{equation}")]
        [HttpGet("3d/{equation}/{x}/{y}")]
        [HttpGet("3d/{equation}/{x}/{y}/{customFunctionsCode}")]
        //[HttpGet("3d/{equation}/{customFunctionsCode}")]
        public string Get3D(string equation, double x=0, double y=0, string customFunctionsCode = "")
        {
            return Get(CalculationsMode.Fxy, equation, x, y, customFunctionsCode);
        }

        
        [HttpGet("{equation}/{x}/{y}/{customFunctionsCode}")]
        [HttpGet("{equation}/{x}/{y}")]
        [HttpGet("{equation}/{x}")]
        [HttpGet("{equation}")]
        //[HttpGet("{calculationsMode}/{equation}/{customFunctionsCode}")]
        public string Get(string equation, double x = 0, double y = 0, string customFunctionsCode = "")
        {
            var calculationsMode = _modeDeterminer.DetermineMode(WebUtility.UrlDecode(equation));

            return Get(calculationsMode, equation, x, y, customFunctionsCode);
        }

        private string Get(CalculationsMode calculationsMode, string equation, double x, double y, string customFunctionsCode)
        {
            var func = GetFunc(equation, calculationsMode, customFunctionsCode);

            switch (calculationsMode)
            {
                case CalculationsMode.Real:
                    return func.Evaluate(x).ToMathString();
                case CalculationsMode.Complex:
                    return func.Evaluate(new Complex(x,y)).ToMathString();
                case CalculationsMode.Fxy:
                    return func.Evaluate(x,y).ToMathString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(calculationsMode), calculationsMode, null);
            }
        }
    }
}
