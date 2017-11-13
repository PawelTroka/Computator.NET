using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading.Tasks;
using Computator.NET.Core.Evaluation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class CalculateApiTests
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
        public async Task ReturnsHttpOk()
        {
            // Act
            var response = await _client.GetAsync(@"/api/calculate/real/2x/1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task PlusSignInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/calculate/real/{Uri.EscapeDataString("2x+2")}/1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        [Ignore("Bug in ASP.NET Core causes this test to fail, see https://github.com/aspnet/HttpAbstractions/issues/912")]
        public async Task DivideSignInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/calculate/real/{Uri.EscapeDataString("2x/2")}/1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task EmptySpacesInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/calculate/real/{Uri.EscapeDataString("    2x   -    2    -   1.1")}/{Uri.EscapeDataString("   -   1    ")}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ReturnsCorrectRealValue()
        {
            // Act
            var response = await _client.GetAsync(@"/api/calculate/real/2x/1.1");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual((2*1.1).ToMathString(),responseString);
        }

        [Test]
        public async Task ReturnsCorrect3DValue()
        {
            //arrange
            Func<double, double, double> func = (x, y) => 1 - x - y;
            var funcCode = "1-x-y";//TODO: 1+2x/3y //or 1+2x/y
            var url = $@"/api/calculate/3d/{Uri.EscapeDataString(funcCode)}/1.1/2.2";


            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(func(1.1,2.2).ToMathString(), responseString);
        }

        [Test]
        public async Task ReturnsCorrectComplexValue()
        {
            // Act
            var response = await _client.GetAsync(@"/api/calculate/complex/2z/1/0");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual((2 * new Complex(1,0)).ToMathString(), responseString);
        }

        [Test]
        public async Task CustomFunctionsWork()
        {
            //arrange
            var x = 10.0;
            var customFunctionCode = @"
            static real Custom123(real x)
            {
                return System.Math.Sqrt(x)-System.Math.Cos(1-x);
            }
            ";
            var url = $@"/api/calculate/real/{Uri.EscapeDataString("2Custom123(x)")}/{x}/{Uri.EscapeDataString(customFunctionCode)}";
            
            // Act
            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual((2 * (System.Math.Sqrt(x) - System.Math.Cos(1 - x))).ToMathString(), responseString);
        }
    }
}
