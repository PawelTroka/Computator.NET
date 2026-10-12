using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Computator.NET.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build()) host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(web =>
                web.UseUrls("http://localhost:60996").UseStartup<Startup>());
    }
}
