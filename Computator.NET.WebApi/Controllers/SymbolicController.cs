using System;
using Computator.NET.Engine.Symbolic;
using Microsoft.AspNetCore.Mvc;

namespace Computator.NET.WebApi.Controllers
{
    [ApiController]
    [Route("api/v2/symbolic")]
    public sealed class SymbolicController : ControllerBase
    {
        // MVC dispatches this action through WorkerExecutionFilter. The worker
        // calls it directly, sharing the exact request and response semantics.
        [HttpPost]
        [Consumes("application/json")]
        [RequestSizeLimit(262144)]
        public IActionResult Execute([FromBody] SymbolicRequest request)
        {
            try
            {
                SymbolicEngine.Validate(request);
                return Ok(new SymbolicEngine().Execute(request));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid symbolic request.",
                    Detail = exception.Message, Status = 400 });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(504, new ProblemDetails { Title = "Symbolic calculation exceeded its time limit.",
                    Detail = "Try a smaller expression or a lower order.", Status = 504 });
            }
        }
    }
}
