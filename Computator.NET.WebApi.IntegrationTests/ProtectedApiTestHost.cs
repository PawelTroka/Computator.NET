using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Computator.NET.WebApi.IntegrationTests
{
    internal static class ProtectedApiTestHost
    {
        public static async Task<(WebApplication Server, HttpClient Client)> Start()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var server = Program.BuildApplication(new[] { "--urls", "http://127.0.0.1:0", "--applicationName",
                typeof(Program).Assembly.GetName().Name, "--ExecutionAccess:Token", token });
            try
            {
                await server.StartAsync();
                var client = new HttpClient { BaseAddress = new Uri(server.Urls.Single()), Timeout = TimeSpan.FromSeconds(30) };
                client.DefaultRequestHeaders.Add("X-Computator-Session", token);
                return (server, client);
            }
            catch { await server.DisposeAsync(); throw; }
        }
    }
}
