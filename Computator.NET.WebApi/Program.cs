using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Computator.NET.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var host = BuildWebHost(args)) host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
    }
}
