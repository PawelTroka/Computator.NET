using System;
using System.Net.Http;
using System.Threading.Tasks;
using Computator.NET.Core.Evaluation;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class ScriptApiTests
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
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("writeln(2);"))) response.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task PlusSignInExpressionWorks()
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("writeln(2+2);"))) response.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task DivideSignInExpressionWorks()
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("write(2/2);")))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("1", await response.Content.ReadAsStringAsync()); }
        }
        [TestCase("%20")]
        [TestCase("100%")]
        [TestCase("/")]
        [TestCase("+")]
        public async Task EscapedStringLiteralsAreNotDecodedTwice(string text)
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("write(\"" + text + "\");")))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual(text, await response.Content.ReadAsStringAsync()); }
        }
        [TestCase("real F = 3; write(101%2F);", "3")]
        [TestCase("real f = 3; write(101%2f);", "3")]
        [TestCase("real F = 3; write(101%252F);", "303")]
        public async Task LiteralPercentSequencesRemainTslOperators(string script, string expected)
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString(script)))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual(expected, await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task DivisionAndLiteralPercentInCustomDefinitionsArePreserved()
        {
            var code = "static real Probe(real x) { return x/2; } static string Text() { return \"%20\"; }";
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("write(Probe(6)); write(Text());") + "/" + Uri.EscapeDataString(code)))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("3%20", await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task EmptySpacesInExpressionWorks()
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("writeln(2     -    2             -     1  );"))) response.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task ReturnsCorrectValue()
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("write(312312421);")))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("312312421", await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task ReturnsCorrectString()
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("write(\"This is a correct string.\");")))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("This is a correct string.", await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task CustomFunctionsWork()
        {
            var x = 10.0;
            var script = "var x = 10.0; write(2Custom123(x));";
            var code = "static real Custom123(real x) { return System.Math.Sqrt(x)-System.Math.Cos(1-x); }";
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString(script) + "/" + Uri.EscapeDataString(code)))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual((2 * (Math.Sqrt(x) - Math.Cos(1 - x))).ToMathString(), await response.Content.ReadAsStringAsync()); }
        }
        [Test]
        public async Task WritingMultipleTimesWorks()
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("for(var x = 9; x >= 0; x--) write(x);")))
            { response.EnsureSuccessStatusCode(); Assert.AreEqual("9876543210", await response.Content.ReadAsStringAsync()); }
        }
    }
}
