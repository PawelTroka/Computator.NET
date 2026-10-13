using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Computator.NET.WebApi.Hosting
{
    public static class LocalExecutionAccess
    {
        public static void UseLocalExecutionAccess(this IApplicationBuilder app, IConfiguration configuration)
        {
            var token = configuration["ExecutionAccess:Token"];
            var development = app.ApplicationServices.GetRequiredService<IHostEnvironment>().IsDevelopment();
            var explicitDevelopmentMode = development && configuration.GetValue<bool>("ExecutionAccess:AllowUnauthenticatedDevelopment");
            if (string.IsNullOrWhiteSpace(token) && !explicitDevelopmentMode)
                throw new InvalidOperationException("ExecutionAccess:Token is required. Launch the packaged local workspace, configure a private server token, or explicitly choose the unauthenticated Development-only IDE profile.");
            if (!string.IsNullOrWhiteSpace(token) && (token.Length < 32 || token.Length > 1024))
                throw new InvalidOperationException("ExecutionAccess:Token must contain between 32 and 1,024 characters.");
            var expected = string.IsNullOrWhiteSpace(token) ? null : Encoding.UTF8.GetBytes(token);
            app.Use(async (context, next) =>
            {
                var host = context.Request.Host.Host.Trim('[', ']');
                var localHost = string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                    (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
                var supplied = context.Request.Headers["X-Computator-Session"].ToString();
                if (context.Connection.RemoteIpAddress == null || !IPAddress.IsLoopback(context.Connection.RemoteIpAddress) ||
                    !localHost || (expected != null && (supplied.Length > 1024 ||
                    !CryptographicOperations.FixedTimeEquals(expected, Encoding.UTF8.GetBytes(supplied)))))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                await next();
            });
        }
    }
}
