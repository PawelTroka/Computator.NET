using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Computator.NET.WebClient
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                WebRootPath = "dist"
            });
            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            // Preserve optional Application Insights without requiring telemetry
            // credentials to run a local scientific application.
            var connection = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
                ?? builder.Configuration["ApplicationInsights:ConnectionString"];
            var legacyKey = builder.Configuration["ApplicationInsights:InstrumentationKey"];
            if (string.IsNullOrWhiteSpace(connection) && !string.IsNullOrWhiteSpace(legacyKey))
                connection = "InstrumentationKey=" + legacyKey;
            if (!string.IsNullOrWhiteSpace(connection))
                builder.Services.AddApplicationInsightsTelemetry(options => options.ConnectionString = connection);

            var app = builder.Build();
            if (!app.Environment.IsDevelopment()) app.UseExceptionHandler();
            app.UseStaticFiles();
            app.MapControllers();
            // An unknown API URL must not masquerade as a successful HTML response.
            app.MapGet("/api/{**path}", () => Microsoft.AspNetCore.Http.Results.NotFound());
            app.MapFallbackToFile("index.html");
            app.Run();
        }
    }
}
