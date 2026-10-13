using System;
using System.IO;
using System.Threading.Tasks;
using Computator.NET.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Computator.NET.WebApi.Execution
{
    public sealed class WorkerExecutionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!(context.Controller is ExecutionV2Controller) || !context.ActionArguments.TryGetValue("request", out var argument) || !(argument is ExecutionRequest request))
            {
                await next(); return;
            }
            try
            {
                var chart = context.ActionDescriptor.RouteValues["action"] == "Chart";
                context.Result = WorkerProcess.ToResult(await WorkerProcess.RunAsync(request, chart, context.HttpContext.RequestAborted));
            }
            catch (WorkerBusyException) { context.Result = Problem(429, "The execution queue is full. Try again after another calculation finishes."); }
            catch (TimeoutException exception) { context.Result = Problem(504, exception.Message); }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
            { context.Result = new StatusCodeResult(499); }
            catch (IOException) { context.Result = Problem(502, "The worker exited unexpectedly. The next request can start a fresh worker."); }
        }
        private static ObjectResult Problem(int status, string title) =>
            new ObjectResult(new ProblemDetails { Status = status, Title = title }) { StatusCode = status };
    }
}
