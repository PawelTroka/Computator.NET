using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Computator.NET.WebApi.Hosting
{
    public static class LocalExecutionAccess
    {
        public static void UseLocalExecutionAccess(this IApplicationBuilder app, IConfiguration configuration)
        {
            var token = configuration["ExecutionAccess:Token"];
            // Preserve the existing explicitly trusted local API mode. The proxy
            // deployment documented by this PR always sets a private random token.
            if (string.IsNullOrWhiteSpace(token)) return;
            if (token.Length < 32) throw new InvalidOperationException("ExecutionAccess:Token must contain at least 32 random characters.");
            var expected = Encoding.UTF8.GetBytes(token);
            app.Use(async (context, next) =>
            {
                var supplied = context.Request.Headers["X-Computator-Session"].ToString();
                var host = context.Request.Host.Host.Trim('[', ']');
                var localHost = string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                    (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
                if (context.Connection.RemoteIpAddress == null || !IPAddress.IsLoopback(context.Connection.RemoteIpAddress) ||
                    !localHost || supplied.Length > 1024 ||
                    !CryptographicOperations.FixedTimeEquals(expected, Encoding.UTF8.GetBytes(supplied)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                await next();
            });
        }
    }
}
