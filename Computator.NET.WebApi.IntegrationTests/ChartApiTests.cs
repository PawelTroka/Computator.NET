using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Numerics;
using System.Threading.Tasks;
using Computator.NET.Core.Evaluation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    /// <summary>
    /// Integration tests for chart api
    /// for now only positive test cases here
    /// </summary>
    [TestFixture]
    public class ChartApiTests
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
        
        [Test]
        public async Task ReturnsHttpOkForChart2D()
        {
            // Act
            var response = await _client.GetAsync(@"/api/chart/2x");

            // Assert
            response.EnsureSuccessStatusCode();
        }


        [Test]
        public async Task ReturnsHttpOkForComplexChart()
        {
            // Act
            var response = await _client.GetAsync(@"/api/chart/2z");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        [Ignore("Chart3D does not support server-side rendering yet")]
        public async Task ReturnsHttpOkForChart3D()
        {
            // Act
            var response = await _client.GetAsync(@"/api/chart/2y");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [TestCase("png/300/300/-2.5/2.5/-0.5/0.5/x")]
        [TestCase("bmp/300/300/-2.5/2.5/-0.5/0.5/x")]
        [TestCase("300/300/-2.5/2.5/-0.5/0.5/x")]
        [TestCase("1300/300/-2.5/2.5/-0.5/0.5/x")]
        [TestCase("-2.5/2.5/-0.5/0.5/x")]
        [TestCase("-2.5/2.5/-0.5/0.5/z")]
        [TestCase("x")]
        [TestCase("png/300/300/x")]
        [TestCase("png/x")]
        [TestCase("png/-2.5/2.5/-0.5/0.5/x")]
        public async Task ReturnsCorrectImageForCorrectQuery(string query)
        {
            // Act
            var response = await _client.GetAsync($@"/api/chart/{query}");

            // Assert
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.That(bytes, Is.Not.Null.And.Not.Empty.And.Not.EquivalentTo(Enumerable.Repeat(bytes[0], bytes.Length)));
        }

        [TestCase("png")]
        [TestCase("bmp")]
        [TestCase("tiff")]
        [TestCase("jpeg")]
        [TestCase("gif")]
        public async Task ImagesWorkForFormat(string format)
        {
            // Act
            var response = await _client.GetAsync($@"/api/chart/{format}/300/300/0/5/0/5/x");

            // Assert
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();

            Assert.That(bytes, Is.Not.Null.And.Not.Empty.And.Not.EquivalentTo(Enumerable.Repeat(bytes[0], bytes.Length)));
        }
    }
}
