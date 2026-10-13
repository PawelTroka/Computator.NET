using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Computator.NET.Core.Evaluation;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class CalculateApiTests
    {
        private WebApplication _server;
        private HttpClient _client;
        [OneTimeSetUp]
        public async Task Setup() { (_server, _client) = await ProtectedApiTestHost.Start(); }
        [OneTimeTearDown]
        public async Task Stop() { _client?.Dispose(); if (_server != null) await _server.DisposeAsync(); }

        [Test]
        public async Task ReturnsHttpOk()
        {
            using (var response = await _client.GetAsync("/api/calculate/real/2x/1")) response.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task PlusSignInExpressionWorks()
        {
            using (var response = await _client.GetAsync($"/api/calculate/real/{Uri.EscapeDataString("2x+2")}/1")) response.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task DivideSignInExpressionWorks()
        {
            using (var response = await _client.GetAsync($"/api/calculate/real/{Uri.EscapeDataString("2x/2")}/1"))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("1", await response.Content.ReadAsStringAsync()); }
        }
        [TestCase("6%2F3", "2")]
        [TestCase("real/6%2f3", "2")]
        [TestCase("complex/6%2F3", "2")]
        [TestCase("3d/y%2F2/0/6", "3")]
        [TestCase("y%2F2/0/6", "3")]
        [TestCase("real/100%2532", "4")]
        public async Task EscapedSegmentsRetainDivisionAndLiteralPercent(string path, string expected)
        {
            using (var response = await _client.GetAsync("/api/calculate/" + path))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual(expected, await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task DivisionInCustomDefinitionsIsDecodedOnce()
        {
            var code = "static real Probe(real x) { return x/2; }";
            using (var response = await _client.GetAsync("/api/calculate/real/Probe(x)/6/" + Uri.EscapeDataString(code)))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("3", await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task EmptySpacesInExpressionWorks()
        {
            using (var response = await _client.GetAsync($"/api/calculate/real/{Uri.EscapeDataString("    2x   -    2    -   1.1")}/{Uri.EscapeDataString("   -   1    ")}"))
                response.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task ReturnsCorrectRealValue()
        {
            using (var response = await _client.GetAsync("/api/calculate/real/2x/1.1"))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual((2 * 1.1).ToMathString(), await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task ReturnsCorrect3DValue()
        {
            Func<double, double, double> func = (x, y) => 1 - x - y;
            using (var response = await _client.GetAsync($"/api/calculate/3d/{Uri.EscapeDataString("1-x-y")}/1.1/2.2"))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual(func(1.1, 2.2).ToMathString(), await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task ReturnsCorrectComplexValue()
        {
            using (var response = await _client.GetAsync("/api/calculate/complex/2z/1/0"))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual((2 * new Complex(1, 0)).ToMathString(), await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task CustomFunctionsWork()
        {
            var x = 10.0;
            var code = "static real Custom123(real x) { return System.Math.Sqrt(x)-System.Math.Cos(1-x); }";
            using (var response = await _client.GetAsync($"/api/calculate/real/{Uri.EscapeDataString("2Custom123(x)")}/{x}/{Uri.EscapeDataString(code)}"))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual((2 * (Math.Sqrt(x) - Math.Cos(1 - x))).ToMathString(), await response.Content.ReadAsStringAsync()); }
        }
    }
}
