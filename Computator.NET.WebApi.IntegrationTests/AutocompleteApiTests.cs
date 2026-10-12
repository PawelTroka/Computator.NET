using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Natives;
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
            TestContext.Progress.WriteLine("Autocomplete startup: native runtime");
            GSLInitializer.Initialize();
            TestContext.Progress.WriteLine("Autocomplete startup: function XML");
            var details = new FunctionsDetailsFileSource();
            TestContext.Progress.WriteLine("Autocomplete startup: direct reflection provider");
            var direct = new AutocompleteProvider(new AutocompleteReflectionSource(), details);
            TestContext.Progress.WriteLine("Autocomplete startup: Unity provider");
            var bootstrapper = new CoreBootstrapper();
            using (bootstrapper.Container) bootstrapper.Create<AutocompleteProvider>();
            TestContext.Progress.WriteLine("Autocomplete startup: HTTP host");
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
            _client.Timeout = TimeSpan.FromSeconds(30);
            TestContext.Progress.WriteLine("Autocomplete startup: HTTP host ready");
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
            TestContext.Progress.WriteLine("Autocomplete request: " + service);
            using (var response = await _client.GetAsync($"/api/autocomplete/{service}"))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var items = JsonConvert.DeserializeObject<ICollection<dynamic>>(body);
                CollectionAssert.AllItemsAreNotNull(items);
                CollectionAssert.IsNotEmpty(items);
                Assert.Greater(items.Count, 1);
                CollectionAssert.AllItemsAreUnique(items);
            }
        }
    }
}
