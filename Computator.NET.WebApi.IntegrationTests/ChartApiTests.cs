using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class ChartApiTests
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
        public void Cleanup() { _client?.Dispose(); _server?.Dispose(); }

        [Test]
        public async Task ReturnsHttpOkForChart2D()
        {
            using (var response = await _client.GetAsync("/api/chart/2x")) response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ReturnsHttpOkForComplexChart()
        {
            using (var response = await _client.GetAsync("/api/chart/2z")) response.EnsureSuccessStatusCode();
        }

        [Test]
        [Ignore("Chart3D does not support server-side rendering yet")]
        public async Task ReturnsHttpOkForChart3D()
        {
            using (var response = await _client.GetAsync("/api/chart/2y")) response.EnsureSuccessStatusCode();
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
            using (var response = await _client.GetAsync("/api/chart/" + query))
            {
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
                AssertImagePayload(bytes);
                using (var stream = new MemoryStream(bytes))
                using (var image = Image.FromStream(stream))
                {
                    NUnit.Framework.Assert.That(image.Width, Is.GreaterThan(0));
                    NUnit.Framework.Assert.That(image.Height, Is.GreaterThan(0));
                }
            }
        }

        [TestCase("png")]
        [TestCase("bmp")]
        [TestCase("tiff")]
        [TestCase("jpeg")]
        [TestCase("gif")]
        public async Task ImagesWorkForFormat(string format)
        {
            using (var response = await _client.GetAsync($"/api/chart/{format}/300/300/0/5/0/5/x"))
            {
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
                AssertImagePayload(bytes);
                using (var stream = new MemoryStream(bytes))
                using (var image = Image.FromStream(stream))
                {
                    var expected = format switch
                    {
                        "bmp" => ImageFormat.Bmp, "tiff" => ImageFormat.Tiff,
                        "jpeg" => ImageFormat.Jpeg, "gif" => ImageFormat.Gif, _ => ImageFormat.Png
                    };
                    NUnit.Framework.Assert.That(image.RawFormat.Guid, Is.EqualTo(expected.Guid));
                    NUnit.Framework.Assert.That(image.Width, Is.EqualTo(300));
                    NUnit.Framework.Assert.That(image.Height, Is.EqualTo(300));
                }
            }
        }

        private static void AssertImagePayload(byte[] bytes)
        {
            NUnit.Framework.Assert.That(bytes, Is.Not.Null.And.Not.Empty);
            // Equivalent to rejecting a buffer containing only its first byte, but
            // linear rather than NUnit's expensive multiset comparison of a BMP.
            NUnit.Framework.Assert.That(bytes.Any(value => value != bytes[0]), Is.True, "Constant image payload");
        }
    }
}
