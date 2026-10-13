using Computator.NET.WebClient.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Computator.NET.WebClient
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = "dist" });
            using var frontendFiles = await DevelopmentAssets.PrepareAsync(builder);
            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            LocalApiProxy.Register(builder);
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
            LocalApiProxy.Map(app);
            if (string.IsNullOrWhiteSpace(app.Configuration["MathematicalApi:BaseUrl"]))
                app.MapGet("/api/{**path}", () => Microsoft.AspNetCore.Http.Results.NotFound());
            app.MapFallbackToFile("index.html");
            await app.RunAsync();
        }
    }
}
