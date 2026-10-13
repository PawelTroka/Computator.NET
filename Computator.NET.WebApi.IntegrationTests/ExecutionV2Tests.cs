using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public sealed class ExecutionV2Tests
    {
        private TestServer _server;
        private HttpClient _client;
        [OneTimeSetUp] public void Start() { _server = new TestServer(new WebHostBuilder().UseStartup<Startup>()); _client = _server.CreateClient(); }
        [OneTimeTearDown] public void Stop() { _client?.Dispose(); _server?.Dispose(); }
        private Task<HttpResponseMessage> Post(object value, string endpoint = "execute") =>
            _client.PostAsync("/api/v2/" + endpoint, new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"));
        private async Task<string> Result(object value)
        {
            using (var response = await Post(value))
            {
                var body = await response.Content.ReadAsStringAsync();
                NUnit.Framework.Assert.That(response.IsSuccessStatusCode, Is.True, body);
                return (string)JObject.Parse(body)["result"];
            }
        }
        [TestCase("6/3", "2")]
        [TestCase("2·3", "6")]
        [TestCase("1+2", "3")]
        public async Task JsonExpressionsPreserveDivisionAndUnicode(string expression, string expected) =>
            NUnit.Framework.Assert.That(await Result(new { operation = "calculate", expression }), Is.EqualTo(expected));
        [Test]
        public async Task MultilineDefinitionsAndUnsavedUpdatesAreRequestLocal()
        {
            foreach (var number in new[] { 42, 73 })
            {
                var code = "// UTF-8: zażółć\nstatic real CustomProbe(real x) { return x + " + number + "; }";
                NUnit.Framework.Assert.That(await Result(new { operation = "calculate", expression = "CustomProbe(0)", customFunctions = code }), Is.EqualTo(number.ToString()));
                NUnit.Framework.Assert.That(await Result(new { operation = "script", expression = "write(CustomProbe(0));", customFunctions = code }), Is.EqualTo(number.ToString()));
            }
            using (var response = await Post(new { operation = "calculate", expression = "CustomProbe(0)", customFunctions = "" }))
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "No process-global function definitions may leak into the next request.");
        }
        [Test]
        public async Task ConcurrentRequestsDoNotExchangeDefinitions()
        {
            var tasks = new Task[8];
            for (var index = 0; index < tasks.Length; index++)
            {
                var number = index + 10;
                tasks[index] = Task.Run(async () => NUnit.Framework.Assert.That(await Result(new
                {
                    operation = "calculate", expression = "Probe(0)", customFunctions = "static real Probe(real x) { return " + number + "; }"
                }), Is.EqualTo(number.ToString())));
            }
            await Task.WhenAll(tasks);
        }
        [TestCase("calculate")]
        [TestCase("script")]
        public async Task GeneratedStaticStateIsNotReusedByJsonRequests(string operation)
        {
            var expression = operation == "calculate" ? "Probe(0)" : "write(Probe(0));";
            const string definitions = "static real counter = 0; static real Probe(real x) { counter += 1; return counter; }";
            for (var index = 0; index < 2; index++)
                NUnit.Framework.Assert.That(await Result(new { operation, expression, customFunctions = definitions }), Is.EqualTo("1"));
        }
        [TestCase("calculate")]
        [TestCase("script")]
        public async Task LegacyRequestsDoNotReuseAnotherCallersGeneratedStaticState(string operation)
        {
            var expression = operation == "calculate" ? "Probe(0)" : "write(Probe(0));";
            const string definitions = "static real counter = 0; static real Probe(real x) { counter += 1; return counter; }";
            var path = operation == "calculate" ? "/api/calculate/real/" + Uri.EscapeDataString(expression) + "/0/"
                : "/api/script/" + Uri.EscapeDataString(expression) + "/";
            for (var index = 0; index < 2; index++)
            {
                using (var response = await _client.GetAsync(path + Uri.EscapeDataString(definitions)))
                {
                    response.EnsureSuccessStatusCode();
                    NUnit.Framework.Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("1"));
                }
            }
        }
        [Test]
        public async Task InvalidRequestsReturnProblemsWithoutBreakingTheNextRequest()
        {
            using (var response = await Post(new { operation = "calculate", expression = "unknownFunction(2)" }))
            {
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                NUnit.Framework.Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Execution failed"));
            }
            NUnit.Framework.Assert.That(await Result(new { operation = "calculate", expression = "2+2" }), Is.EqualTo("4"));
        }
        [TestCase("unknown")]
        [TestCase("")]
        public async Task UnknownOperationsAreRejected(string operation)
        {
            using (var response = await Post(new { operation, expression = "1" }))
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task InvalidSizesAndMethodsAreRejected()
        {
            using (var response = await Post(new { expression = "x", width = 100000, height = 100000 }, "chart"))
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            using (var response = await Post(new { expression = "x", operation = "integral", method = "not-a-method" }))
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            using (var response = await Post(new { expression = new string('1', 65537) }))
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task JsonChartIncludesCustomFunctions()
        {
            using (var response = await Post(new { expression = "Probe(x)", customFunctions = "static real Probe(real x) { return x/2; }", width = 300, height = 300 }, "chart"))
            {
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
                NUnit.Framework.Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("image/png"));
                NUnit.Framework.Assert.That(bytes.Length, Is.GreaterThan(1024));
                NUnit.Framework.Assert.That(bytes[0], Is.EqualTo(137));
            }
        }
    }
}
