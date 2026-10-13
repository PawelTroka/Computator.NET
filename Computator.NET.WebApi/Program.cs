using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Computator.NET.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var app = BuildApplication(args)) app.Run();
        }

        public static WebApplication BuildApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Preserve the local default without overriding an explicitly supplied
            // --urls / ASPNETCORE_URLS value (including isolated test ports).
            if (string.IsNullOrWhiteSpace(builder.Configuration[WebHostDefaults.ServerUrlsKey]))
                builder.WebHost.UseUrls("http://localhost:60996");
            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);
            var app = builder.Build();
            startup.Configure(app, app.Environment);
            return app;
        }
    }
}
