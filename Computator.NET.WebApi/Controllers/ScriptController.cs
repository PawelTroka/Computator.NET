using System;
using System.Net;
using System.Numerics;
using System.Text;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ScriptController : Controller
    {
        private readonly IScriptsProvider _scriptsProvider;
        private readonly ILogger<ScriptController> _logger;

        public ScriptController(IScriptsProvider scriptsProvider, ILogger<ScriptController> logger)
        {
            _scriptsProvider = scriptsProvider;
            _logger = logger;
        }
        
        // GET api/script/writeln(2*cos(10));
        [HttpGet("{tslCode}")]
        [HttpGet("{tslCode}/{customFunctionsCode}")]
        //[HttpGet("real/{equation}/{customFunctionsCode}")]
        public string Execute(string tslCode, string customFunctionsCode="")
        {
            var decodedTslCode = (tslCode);
            _logger.LogInformation($"Decoded tsl code '{tslCode}' to '{decodedTslCode}'");

            var decodedCustomFunctions = (customFunctionsCode);
            _logger.LogInformation($"Decoded custom functions code '{customFunctionsCode}' to '{decodedCustomFunctions}'");

            var func = _scriptsProvider.GetScript(decodedTslCode, decodedCustomFunctions);
            var output = new StringBuilder();

            void ConsoleCallback(string s)
            {
                output.Append(s);
            }

            func.Evaluate(ConsoleCallback);

            return output.ToString();
        }
    }
}
