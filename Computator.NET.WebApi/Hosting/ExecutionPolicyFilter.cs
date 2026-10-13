using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.WebApi.Controllers;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Computator.NET.WebApi.Hosting
{
    public sealed class ExecutionPolicyFilter : IActionFilter, IExceptionFilter, IResourceFilter, IOrderedFilter
    {
        public int Order => -1000;
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor action) || action.ControllerName == "ExecutionV2") return;
            // Preserve legacy signed numeric input such as '-   1', but never turn
            // '1 2' into 12 or apply numeric normalization to source-code strings.
            foreach (var parameter in action.Parameters.OfType<ControllerParameterDescriptor>())
            {
                if (parameter.ParameterInfo.ParameterType != typeof(double) ||
                    !context.RouteData.Values.TryGetValue(parameter.Name, out var raw) || !(raw is string text)) continue;
                context.RouteData.Values[parameter.Name] = Regex.Replace(text, @"^(\s*[+-])\s+(?=\d)", "$1",
                    RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
            }
        }
        public void OnResourceExecuted(ResourceExecutedContext context) { }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor action) ||
                !new[] { "Calculate", "Script", "Chart", "NumericalCalculations", "ExecutionV2" }.Contains(action.ControllerName)) return;
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState) { Title = "Invalid execution parameters." });
                return;
            }
            var request = context.ActionArguments.Values.OfType<ExecutionRequest>().FirstOrDefault();
            var errors = action.ControllerName == "ExecutionV2"
                ? ExecutionPolicy.Validate(request, action.ActionName == "Chart")
                : ExecutionPolicy.ValidateLegacy(action.ControllerName, action.ActionName, context.ActionArguments);
            if (errors.Count != 0)
                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors) { Title = "Invalid execution parameters.", Status = 400 });
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is CompilationException || context.Exception is EvaluationException || context.Exception is ArgumentException)
            {
                context.Result = new BadRequestObjectResult(new ProblemDetails { Title = "Execution failed.", Detail = context.Exception.Message, Status = 400 });
                context.ExceptionHandled = true;
            }
        }
    }
}
