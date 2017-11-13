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
    public class ScriptApiTests
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
        [Ignore("Bug in ASP.NET Core causes this test to fail, see https://github.com/aspnet/HttpAbstractions/issues/912")]
        public async Task DivideSignInExpressionWorks()
        {
            // Act
            var response = await _client.GetAsync($@"/api/script/{Uri.EscapeDataString("writeln(2/2);")}");

            // Assert
            response.EnsureSuccessStatusCode();
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
