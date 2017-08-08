using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading.Tasks;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Evaluation;
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
            // Arrange
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestCase("expression")]
        [TestCase("scripting")]
        public async Task ReturnsHttpOk(string service)
        {
            // Act
            var response = await _client.GetAsync($@"/api/autocomplete/{service}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [TestCase("expression")]
        [TestCase("scripting")]
        public async Task ReturnsCorrectEntries(string service)
        {
            // Act
            var response = await _client.GetAsync($@"/api/autocomplete/{service}");
            var responseString = await response.Content.ReadAsStringAsync();
            var autocompleteItems = JsonConvert.DeserializeObject<ICollection<dynamic>>(responseString);

            //assert
            CollectionAssert.AllItemsAreNotNull(autocompleteItems);
            CollectionAssert.IsNotEmpty(autocompleteItems);
            Assert.Greater(autocompleteItems.Count, 1);
            CollectionAssert.AllItemsAreUnique(autocompleteItems);
        }
    }
}
