using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Computator.NET.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Computator.NET.WebApi.Execution
{
    public sealed class WorkerExecutionFilter : IAsyncActionFilter, IOrderedFilter
    {
        // Source decoding, model binding and common validation run first. Both
        // transports share the same bounded queue, deadline and owned worker job.
        public int Order => 1000;
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var legacyScript = context.Controller is ScriptController;
            ExecutionRequest request;
            var chart = false;
            if (legacyScript)
            {
                context.ActionArguments.TryGetValue("tslCode", out var source);
                context.ActionArguments.TryGetValue("customFunctionsCode", out var definitions);
                request = new ExecutionRequest { Operation = "script", Expression = source as string,
                    CustomFunctions = definitions as string ?? "" };
                // Never URI-decode these values again: LegacyCodeRoute already did.
                context.HttpContext.Response.Headers["Cache-Control"] = "no-store";
            }
            else if (context.Controller is ExecutionV2Controller &&
                context.ActionArguments.TryGetValue("request", out var argument) && argument is ExecutionRequest jsonRequest)
            {
                request = jsonRequest;
                chart = context.ActionDescriptor.RouteValues["action"] == "Chart";
            }
            else { await next(); return; }

            try
            {
                var reply = await WorkerProcess.RunAsync(request, chart, context.HttpContext.RequestAborted);
                context.Result = legacyScript && reply.Status == 200 ? LegacyScriptResult(reply) : WorkerProcess.ToResult(reply);
            }
            catch (WorkerBusyException) { context.Result = Problem(429, "The execution queue is full. Try again after another calculation finishes."); }
            catch (TimeoutException exception) { context.Result = Problem(504, exception.Message); }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
            { context.Result = new StatusCodeResult(499); }
            catch (ArgumentException exception) { context.Result = Problem(400, exception.Message); }
            catch (IOException) { context.Result = Problem(502, "The worker exited unexpectedly. The next request can start a fresh worker."); }
        }

        private static IActionResult LegacyScriptResult(WorkerReply reply)
        {
            try
            {
                using var json = JsonDocument.Parse(reply.Json ?? "");
                if (reply.Image != null || json.RootElement.ValueKind != JsonValueKind.Object ||
                    !json.RootElement.TryGetProperty("result", out var output) || output.ValueKind != JsonValueKind.String)
                    throw new IOException("Invalid script worker response.");
                // A string action originally used MVC's StringOutputFormatter and
                // content negotiation. Do not wrap legacy output in { result: ... }
                // or force text/plain for callers explicitly asking for JSON.
                return new ObjectResult(output.GetString()) { StatusCode = 200, DeclaredType = typeof(string) };
            }
            catch (JsonException exception) { throw new IOException("Invalid script worker JSON.", exception); }
        }

        private static ObjectResult Problem(int status, string title) =>
            new ObjectResult(new ProblemDetails { Status = status, Title = title }) { StatusCode = status };
    }
}
