using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class AutocompleteApiTests
    {
        private TestServer _server;
        private HttpClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client?.Dispose();
            _server?.Dispose();
        }

        [TestCase("expression")]
        [TestCase("scripting")]
        public async Task ReturnsHttpOk(string service)
        {
            using (var response = await _client.GetAsync($"/api/autocomplete/{service}"))
                response.EnsureSuccessStatusCode();
        }

        [TestCase("expression")]
        [TestCase("scripting")]
        public async Task ReturnsCorrectEntries(string service)
        {
            TestContext.Progress.WriteLine("Requesting autocomplete: " + service);
            using (var response = await _client.GetAsync($"/api/autocomplete/{service}"))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                TestContext.Progress.WriteLine($"Received autocomplete {service}: {body.Length} characters");
                var items = JsonConvert.DeserializeObject<List<CompletionResponse>>(body);
                NUnit.Framework.Assert.That(items, Is.Not.Null);
                NUnit.Framework.Assert.That(items.Count, Is.GreaterThan(1));
                NUnit.Framework.Assert.That(items, Is.All.Not.Null);
                NUnit.Framework.Assert.That(items.Select(item => item.Text), Is.All.Not.Null.And.Not.Empty);
                NUnit.Framework.Assert.That(items.Select(item => item.MenuText), Is.All.Not.Null.And.Not.Empty);
                NUnit.Framework.Assert.That(items.Select(item => item.Details), Is.All.Not.Null);
                // The API deduplicates by insertion text. Check that actual contract
                // in linear time, not every pair of dynamic JSON object graphs.
                var unique = new HashSet<string>(items.Select(item => item.Text), StringComparer.Ordinal);
                NUnit.Framework.Assert.That(unique.Count, Is.EqualTo(items.Count), "Duplicate completion text");
            }
        }

        public class CompletionResponse
        {
            public string Text { get; set; }
            public string MenuText { get; set; }
            public Dictionary<string, string> Details { get; set; }
        }
    }
}
