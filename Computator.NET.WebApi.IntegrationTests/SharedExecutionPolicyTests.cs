using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Computator.NET.WebApi.Controllers;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class SharedExecutionPolicyTests
    {
        private WebApplication _server;
        private HttpClient _client;
        [OneTimeSetUp]
        public async Task Start()
        {
            var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            _server = Program.BuildApplication(new[] { "--urls", "http://127.0.0.1:0", "--applicationName", typeof(Program).Assembly.GetName().Name, "--ExecutionAccess:Token", token });
            await _server.StartAsync();
            _client = new HttpClient { BaseAddress = new Uri(_server.Urls.Single()), Timeout = TimeSpan.FromSeconds(30) };
            _client.DefaultRequestHeaders.Add("X-Computator-Session", token);
        }
        [OneTimeTearDown]
        public async Task Stop() { _client?.Dispose(); if (_server != null) await _server.DisposeAsync(); }
        [TestCase(0, 300)]
        [TestCase(-1, 300)]
        [TestCase(4097, 300)]
        [TestCase(4096, 4096)]
        [TestCase(int.MaxValue, int.MaxValue)]
        public async Task BothChartRoutesRejectAllocationBeforeCompilingInvalidSource(int width, int height)
        {
            using var legacy = await _client.GetAsync($"/api/chart/png/{width}/{height}/0/5/0/5/not-valid(");
            using var json = await _client.PostAsJsonAsync("/api/v2/chart", new { expression = "not-valid(", width, height });
            foreach (var response in new[] { legacy, json })
            {
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                var problem = JObject.Parse(await response.Content.ReadAsStringAsync());
                NUnit.Framework.Assert.That(problem["errors"]?["dimensions"], Is.Not.Null);
            }
        }
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("-Infinity")]
        public async Task LegacyCalculationsRejectNonFiniteArguments(string value)
        {
            using var response = await _client.GetAsync("/api/calculate/real/x/" + value);
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task UnknownMethodsAreRejectedBeforeCompilation()
        {
            using var legacy = await _client.GetAsync("/api/numerical-calculations/integral/unknown/not-valid(/0/1/10");
            using var json = await _client.PostAsJsonAsync("/api/v2/execute", new { operation = "integral", method = "unknown", expression = "not-valid(" });
            foreach (var response in new[] { legacy, json })
            {
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                NUnit.Framework.Assert.That(JObject.Parse(await response.Content.ReadAsStringAsync())["errors"]?["method"], Is.Not.Null);
            }
        }
        [Test]
        public async Task SignedLegacyWhitespaceRemainsSupportedWithoutSilentlyUsingZero()
        {
            using var response = await _client.GetAsync("/api/calculate/real/x/" + Uri.EscapeDataString(" -   1 "));
            response.EnsureSuccessStatusCode();
            NUnit.Framework.Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("-1"));
        }
        [Test]
        public async Task BothScriptRoutesEnforceTheSameOutputLimitAndRecover()
        {
            var script = "write(new string('x', 65537));";
            using var legacy = await _client.GetAsync("/api/script/" + Uri.EscapeDataString(script));
            using var json = await _client.PostAsJsonAsync("/api/v2/execute", new { operation = "script", expression = script });
            NUnit.Framework.Assert.That(legacy.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            NUnit.Framework.Assert.That(json.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            using var next = await _client.PostAsJsonAsync("/api/v2/execute", new { operation = "script", expression = "write(42);" });
            next.EnsureSuccessStatusCode();
            NUnit.Framework.Assert.That((string)JObject.Parse(await next.Content.ReadAsStringAsync())["result"], Is.EqualTo("42"));
        }
        [TestCase(0)]
        [TestCase(1000001)]
        public void BothParameterModelsRejectInvalidIterationCounts(uint iterations)
        {
            NUnit.Framework.Assert.That(ExecutionPolicy.Validate(new ExecutionRequest { Expression = "x", N = iterations }, false), Is.Not.Empty);
            NUnit.Framework.Assert.That(ExecutionPolicy.ValidateLegacy("NumericalCalculations", "GetRealIntegral",
                new Dictionary<string, object> { ["n"] = (double)iterations }), Is.Not.Empty);
        }
        [Test]
        public void BoundedOutputAllowsTheExactLimitButNotOneCharacterMore()
        {
            var output = new BoundedExecutionOutput(); output.Append(new string('x', ExecutionPolicy.MaximumOutputCharacters));
            NUnit.Framework.Assert.That(output.ToString().Length, Is.EqualTo(ExecutionPolicy.MaximumOutputCharacters));
            NUnit.Framework.Assert.Throws<ArgumentException>(() => output.Append("x"));
            NUnit.Framework.Assert.That(output.ToString().Length, Is.EqualTo(ExecutionPolicy.MaximumOutputCharacters));
        }
    }
}
