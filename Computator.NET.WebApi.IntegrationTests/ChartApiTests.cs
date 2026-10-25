using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using Computator.NET.DataTypes.Functions;
using Computator.NET.WebApi.Services;
using Newtonsoft.Json.Linq;
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

        private async Task<JObject> Surface(object request)
        {
            using var response = await _client.PostAsJsonAsync("/api/v2/chart", request);
            var text = await response.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);
            NUnit.Framework.Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            return JObject.Parse(text);
        }

        [TestCase("x*x + y*y", 8.0, 0.0)]
        [TestCase("x + y", -4.0, 0.0)]
        [TestCase("sin(x)*cos(y)", -0.37840124765396416, 0.0)]
        public async Task RealFxyApiSamplesKnownSurfaces(string expression, double corner, double center)
        {
            var grid = await Surface(new { expression, gridSize = 9, xMin = -2, xMax = 2,
                yMin = -2, yMax = 2, width = 300, height = 300 });
            NUnit.Framework.Assert.That((string)grid["kind"], Is.EqualTo("surface"));
            NUnit.Framework.Assert.That((int)grid["finiteSamples"], Is.EqualTo(81));
            NUnit.Framework.Assert.That((int)grid["missingSamples"], Is.Zero);
            NUnit.Framework.Assert.That(grid["x"].Count(), Is.EqualTo(9));
            NUnit.Framework.Assert.That(grid["y"].Count(), Is.EqualTo(9));
            NUnit.Framework.Assert.That((double)grid["x"][0], Is.EqualTo(-2));
            NUnit.Framework.Assert.That((double)grid["x"][8], Is.EqualTo(2));
            NUnit.Framework.Assert.That((double)grid["y"][0], Is.EqualTo(-2));
            NUnit.Framework.Assert.That((double)grid["y"][8], Is.EqualTo(2));
            NUnit.Framework.Assert.That((double)grid["z"][0][0], Is.EqualTo(corner).Within(1e-9));
            NUnit.Framework.Assert.That((double)grid["z"][4][4], Is.EqualTo(center).Within(1e-9));
        }

        [Test]
        public async Task UnsavedTwoArgumentCustomFunctionsStayRequestLocal()
        {
            for (var offset = 1; offset <= 2; offset++)
            {
                var grid = await Surface(new { expression = "SurfaceProbe(x,y)", gridSize = 9,
                    customFunctions = "static real SurfaceProbe(real x, real y) { return x*x+y*y+" + offset + "; }" });
                NUnit.Framework.Assert.That((double)grid["z"][4][4], Is.EqualTo(offset));
            }
            using var missing = await _client.PostAsJsonAsync("/api/v2/chart", new { expression = "SurfaceProbe(x,y)", gridSize = 9 });
            NUnit.Framework.Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task SingularitiesBecomeNullHolesRatherThanInvalidJsonOrFlattenedZeros()
        {
            var grid = await Surface(new { expression = "1/(x-y)", gridSize = 9, xMin = -2, xMax = 2, yMin = -2, yMax = 2 });
            NUnit.Framework.Assert.That((int)grid["missingSamples"], Is.GreaterThan(0));
            NUnit.Framework.Assert.That(grid["z"][0][0].Type, Is.EqualTo(JTokenType.Null));
            NUnit.Framework.Assert.That((double)grid["z"][0][8], Is.EqualTo(0.25).Within(1e-9));
        }

        [Test]
        public async Task AllSingularSurfaceIsRejectedWithoutCorruptingSubsequentRequests()
        {
            using var response = await _client.PostAsJsonAsync("/api/v2/chart", new { expression = "1/(x-x)", gridSize = 9 });
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
            await Surface(new { expression = "x+y", gridSize = 9 });
        }

        [TestCase(0)]
        [TestCase(8)]
        [TestCase(130)]
        [TestCase(100000)]
        public async Task SampleCountIsBoundedBeforeExpressionCompilation(int count)
        {
            using var response = await _client.PostAsJsonAsync("/api/v2/chart", new { expression = "not-valid(", gridSize = count });
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            NUnit.Framework.Assert.That(JObject.Parse(await response.Content.ReadAsStringAsync())["errors"]?["gridSize"], Is.Not.Null);
        }

        [Test]
        public void SurfaceSamplerHonorsCancellationBeforeEvaluating()
        {
            var function = new Function(new Func<double, double, double>((x, y) => x+y), FunctionType.Real3D);
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            NUnit.Framework.Assert.Throws<OperationCanceledException>(() =>
                SurfaceSampler.Sample(function, -2, 2, -2, 2, 129, cancelled.Token));
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
