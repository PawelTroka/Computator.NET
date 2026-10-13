using Computator.NET.WebApi.Services;
using Computator.NET.WebApi.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Computator.NET.WebApi.Controllers
{
    [Route("api/[controller]")]
    [LegacyCodeRoute]
    public class ScriptController : Controller
    {
        private readonly IScriptsProvider _scriptsProvider;
        public ScriptController(IScriptsProvider scriptsProvider, ILogger<ScriptController> logger) { _scriptsProvider = scriptsProvider; }
        [HttpGet("{tslCode}")]
        [HttpGet("{tslCode}/{customFunctionsCode}")]
        public string Execute(string tslCode, string customFunctionsCode = "")
        {
            // Source has already been decoded exactly once by LegacyCodeRoute and
            // checked by ExecutionPolicyFilter. Do not log users' complete programs.
            var output = new BoundedExecutionOutput();
            _scriptsProvider.GetScript(tslCode, customFunctionsCode).Evaluate(output.Append);
            return output.ToString();
        }
    }
}
