using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
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
        public async Task Setup()
        {
            // Real HTTP preserves RawTarget; TestServer reconstructs a decoded path.
            _server = Program.BuildApplication(new[] { "--urls", "http://127.0.0.1:0", "--applicationName", typeof(Program).Assembly.GetName().Name });
            await _server.StartAsync();
            _client = new HttpClient { BaseAddress = new Uri(_server.Urls.Single()) };
        }

        [OneTimeTearDown]
        public async Task Stop() { _client?.Dispose(); if (_server != null) await _server.DisposeAsync(); }
        
        [Test]
        public async Task ReturnsHttpOk()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString("writeln(2);")}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task PlusSignInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString("writeln(2+2);")}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task DivideSignInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString("write(2/2);")}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("1", await response.Content.ReadAsStringAsync());
        }

        [TestCase("%20")]
        [TestCase("100%")]
        [TestCase("/")]
        [TestCase("+")]
        public async Task EscapedStringLiteralsAreNotDecodedTwice(string text)
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString("write(\"" + text + "\");")))
            {
                response.EnsureSuccessStatusCode();
                Assert.AreEqual(text, await response.Content.ReadAsStringAsync());
            }
        }

        [TestCase("real F = 3; write(101%2F);", "3")]
        [TestCase("real f = 3; write(101%2f);", "3")]
        [TestCase("real F = 3; write(101%252F);", "303")]
        public async Task LiteralPercentSequencesRemainTslOperators(string script, string expected)
        {
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString(script)))
            {
                response.EnsureSuccessStatusCode();
                Assert.AreEqual(expected, await response.Content.ReadAsStringAsync());
            }
        }

        [Test]
        public async Task DivisionAndLiteralPercentInCustomDefinitionsArePreserved()
        {
            var code = "static real Probe(real x) { return x/2; } static string Text() { return \"%20\"; }";
            var script = "write(Probe(6)); write(Text());";
            using (var response = await _client.GetAsync("/api/script/" + Uri.EscapeDataString(script) + "/" + Uri.EscapeDataString(code)))
            {
                response.EnsureSuccessStatusCode();
                Assert.AreEqual("3%20", await response.Content.ReadAsStringAsync());
            }
        }

        [Test]
        public async Task EmptySpacesInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString("writeln(2     -    2             -     1  );")}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ReturnsCorrectValue()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString("write(312312421);")}");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual("312312421", responseString);
        }

        [Test]
        public async Task ReturnsCorrectString()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString(@"write(""This is a correct string."");")}");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual("This is a correct string.", responseString);
        }


        [Test]
        public async Task CustomFunctionsWork()
        {
            //arrange
            var x = 10.0;

            var tslCode = @"
            var x = 10.0;
            write(2Custom123(x));
            ";
            var customFunctionCode = @"
            static real Custom123(real x)
            {
                return System.Math.Sqrt(x)-System.Math.Cos(1-x);
            }
            ";
            var url = $@"/api/script/{Uri.EscapeDataString(tslCode)}/{Uri.EscapeDataString(customFunctionCode)}";


            // Act
            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual((2 * (System.Math.Sqrt(x) - System.Math.Cos(1 - x))).ToMathString(), responseString);
        }

        [Test]
        public async Task WritingMultipleTimesWorks()
        {
            var tslCode = @"
            for(var x = 9; x >= 0; x--)
                write(x);
            ";

            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString(tslCode)}");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual("9876543210", responseString);
        }
    }
}
