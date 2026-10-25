using System;
using Computator.NET.WebApi.Hosting;
using Computator.NET.WebApi.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Computator.NET.WebApi
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "--execution-worker")
            {
                Environment.ExitCode = WorkerRuntime.Run(); return;
            }
            using (var app = BuildApplication(args)) app.Run();
        }
        public static WebApplication BuildApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Keep the development listener outside the default Windows dynamic
            // port range, where reservations can reject binding with WSAEACCES.
            // An explicit command-line/configuration URL always takes precedence.
            if (string.IsNullOrWhiteSpace(builder.Configuration[WebHostDefaults.ServerUrlsKey]))
                builder.WebHost.UseUrls("http://localhost:5080");
            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);
            var app = builder.Build();
            app.UseLocalExecutionAccess(builder.Configuration);
            startup.Configure(app, app.Environment);
            return app;
        }
    }
}
