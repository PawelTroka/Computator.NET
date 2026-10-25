using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class DevelopmentEndpointTests
    {
        private static WebApplication Create(string urls = null)
        {
            var arguments = new List<string>
            {
                "--applicationName", typeof(Program).Assembly.GetName().Name,
                "--environment", "Development",
                "--ExecutionAccess:Token", "",
                "--ExecutionAccess:AllowUnauthenticatedDevelopment", "true"
            };
            if (urls != null) arguments.AddRange(new[] { "--urls", urls });
            return Program.BuildApplication(arguments.ToArray());
        }

        [TestCase("")]
        [TestCase(" \t ")]
        public void EmptyConfiguredUrlUsesStableLoopbackDefault(string urls)
        {
            using var application = Create(urls);
            var address = new Uri(application.Configuration[WebHostDefaults.ServerUrlsKey]);
            NUnit.Framework.Assert.That(address.AbsoluteUri, Is.EqualTo("http://localhost:5080/"));
            NUnit.Framework.Assert.That(address.IsLoopback, Is.True);
        }

        [TestCase("http://localhost:5084")]
        [TestCase("http://127.0.0.1:0")]
        [TestCase("http://[::1]:5084")]
        public void ExplicitCommandLineAddressIsPreserved(string urls)
        {
            using var application = Create(urls);
            NUnit.Framework.Assert.That(application.Configuration[WebHostDefaults.ServerUrlsKey], Is.EqualTo(urls));
        }

        [Test]
        public void EnvironmentAddressIsPreservedWhenThereIsNoCommandLineOverride()
        {
            var previous = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            try
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://127.0.0.1:5084");
                using var application = Create();
                NUnit.Framework.Assert.That(application.Configuration[WebHostDefaults.ServerUrlsKey],
                    Is.EqualTo("http://127.0.0.1:5084"));
            }
            finally { Environment.SetEnvironmentVariable("ASPNETCORE_URLS", previous); }
        }

        [TestCase("http://localhost:5081", true)]
        [TestCase("http://localhost:5082", true)]
        [TestCase("http://localhost:5173", true)]
        [TestCase("http://127.0.0.1:5173", true)]
        [TestCase("http://localhost:5083", false)]
        [TestCase("http://example.com:5081", false)]
        [TestCase("https://localhost:5081", false)]
        public async Task RealListenerAllowsOnlyTheConfiguredDevelopmentOrigins(string origin, bool allowed)
        {
            await using var application = Create("http://127.0.0.1:0");
            await application.StartAsync();
            using var client = new HttpClient(new HttpClientHandler { UseProxy = false })
                { BaseAddress = new Uri(application.Urls.Single()) };
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v2/execute");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "content-type");
            using var response = await client.SendAsync(request);
            var hasOrigin = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins);
            NUnit.Framework.Assert.That(hasOrigin, Is.EqualTo(allowed));
            if (allowed)
            {
                NUnit.Framework.Assert.That(response.IsSuccessStatusCode, Is.True);
                NUnit.Framework.Assert.That(origins.Single(), Is.EqualTo(origin));
            }
        }
    }
}
