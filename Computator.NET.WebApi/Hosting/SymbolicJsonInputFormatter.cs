using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Computator.NET.Engine.Symbolic;
using Computator.NET.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Computator.NET.WebApi.Hosting
{
    /// <summary>Only SymbolicRequest bypasses Newtonsoft's string coercion.</summary>
    public sealed class SymbolicJsonInputFormatter : InputFormatter
    {
        public SymbolicJsonInputFormatter() { SupportedMediaTypes.Add("application/json"); }
        protected override bool CanReadType(Type type) => type == typeof(SymbolicRequest);

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var cancellation = context.HttpContext.RequestAborted;
            try
            {
                using var body = new MemoryStream();
                var buffer = new byte[8192];
                int count;
                while ((count = await request.Body.ReadAsync(buffer.AsMemory(), cancellation)) != 0)
                {
                    if (body.Length + count > SymbolicRequestJson.MaximumBodyBytes)
                        throw new BadHttpRequestException("Symbolic request exceeds 256 KiB.", StatusCodes.Status413PayloadTooLarge);
                    await body.WriteAsync(buffer.AsMemory(0, count), cancellation);
                }
                return await InputFormatterResult.SuccessAsync(SymbolicRequestJson.Parse(body.ToArray()));
            }
            catch (Exception exception) when (exception is JsonException || exception is ArgumentException)
            {
                context.ModelState.TryAddModelError(context.ModelName, exception.Message);
                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}
