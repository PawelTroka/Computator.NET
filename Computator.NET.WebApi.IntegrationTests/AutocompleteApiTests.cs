using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
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
            using (var response = await _client.GetAsync($"/api/autocomplete/{service}"))
            {
                response.EnsureSuccessStatusCode();
                var tokens = JArray.Parse(await response.Content.ReadAsStringAsync());
                var items = tokens.ToObject<List<CompletionResponse>>();
                NUnit.Framework.Assert.That(items, Is.Not.Null);
                NUnit.Framework.Assert.That(items.Count, Is.GreaterThan(1));
                NUnit.Framework.Assert.That(items, Is.All.Not.Null);
                NUnit.Framework.Assert.That(items.Select(item => item.Text), Is.All.Not.Null.And.Not.Empty);
                NUnit.Framework.Assert.That(items.Select(item => item.MenuText), Is.All.Not.Null.And.Not.Empty);
                NUnit.Framework.Assert.That(items.Select(item => item.Details), Is.All.Not.Null);
                // Preserve the original full-entry uniqueness contract. Overloads
                // can share insertion text but have different typed menu signatures.
                // NUnit's general-purpose comparison of dynamic object graphs is
                // quadratic here; Json.NET's hash/equality pair compares JSON values.
                NUnit.Framework.Assert.That(UniqueEntryCount(tokens), Is.EqualTo(tokens.Count),
                    "Duplicate autocomplete entries (including their signatures and metadata)");
            }
        }

        [Test]
        public void UniquenessCheckPreservesOverloadsWithTheSameInsertionText()
        {
            var items = JArray.Parse(@"[
                { 'text': 'f(x)', 'menuText': 'real f(real x)', 'details': { 'type': 'Functions' } },
                { 'text': 'f(x)', 'menuText': 'complex f(complex x)', 'details': { 'type': 'Functions' } }
            ]");
            NUnit.Framework.Assert.That(UniqueEntryCount(items), Is.EqualTo(2));
        }

        [Test]
        public void UniquenessCheckFindsDuplicateValuesRegardlessOfPropertyOrder()
        {
            var items = JArray.Parse(@"[
                { 'text': 'f(x)', 'menuText': 'f(real x)', 'details': { 'type': 'Functions', 'title': 'F' } },
                { 'details': { 'title': 'F', 'type': 'Functions' }, 'menuText': 'f(real x)', 'text': 'f(x)' }
            ]");
            NUnit.Framework.Assert.That(UniqueEntryCount(items), Is.EqualTo(1));
        }

        private static int UniqueEntryCount(IEnumerable<JToken> items) =>
            new HashSet<JToken>(items, new JTokenEqualityComparer()).Count;

        public class CompletionResponse
        {
            public string Text { get; set; }
            public string MenuText { get; set; }
            public Dictionary<string, string> Details { get; set; }
        }
    }
}
