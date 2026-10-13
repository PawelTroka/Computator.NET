using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Computator.NET.WebApi.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class LocalExecutionAccessTests
    {
        private const string Token = "a-test-only-token-with-at-least-32-characters";
        private static TestServer Create(string token, string environment = "Production", bool developmentOptIn = false)
        {
            var values = new Dictionary<string, string>
            {
                ["ExecutionAccess:Token"] = token,
                ["ExecutionAccess:AllowUnauthenticatedDevelopment"] = developmentOptIn.ToString()
            };
            return new TestServer(new WebHostBuilder().UseEnvironment(environment).Configure(app =>
            {
                app.Use(async (context, next) => { context.Connection.RemoteIpAddress = IPAddress.Loopback; await next(); });
                app.UseLocalExecutionAccess(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
                app.Run(context => context.Response.WriteAsync("ready"));
            }));
        }
        [TestCase("Production", false)]
        [TestCase("Production", true)]
        [TestCase("Development", false)]
        public void MissingTokenFailsClosed(string environment, bool optIn) =>
            NUnit.Framework.Assert.Throws<InvalidOperationException>(() => { using var server = Create(null, environment, optIn); });
        [Test]
        public async Task ExplicitDevelopmentProfileStillRequiresLoopbackHost()
        {
            using var server = Create(null, "Development", true);
            using var client = server.CreateClient();
            NUnit.Framework.Assert.That((await client.GetAsync("/")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var request = new HttpRequestMessage(HttpMethod.Get, "/"); request.Headers.Host = "remote.example";
            NUnit.Framework.Assert.That((await client.SendAsync(request)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }
        [TestCase(null, 403)]
        [TestCase("wrong", 403)]
        [TestCase(Token, 200)]
        public async Task ProductionRequiresThePrivateSessionToken(string token, int expected)
        {
            using var server = Create(Token);
            using var client = server.CreateClient();
            if (token != null) client.DefaultRequestHeaders.Add("X-Computator-Session", token);
            NUnit.Framework.Assert.That((int)(await client.GetAsync("/")).StatusCode, Is.EqualTo(expected));
        }
    }
}
