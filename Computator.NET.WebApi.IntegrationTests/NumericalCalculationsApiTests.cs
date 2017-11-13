using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading.Tasks;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using Computator.NET.Localization;
using Newtonsoft.Json;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public class NumericalCalculationsApiTests
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

        [TestCase("integral")]
        [TestCase("derivative")]
        [TestCase("function-root")]
        public async Task GetMethodsReturnCorrectMethods(string operation)
        {
            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/{operation}/list-methods");
            var responseString = await response.Content.ReadAsStringAsync();
            var methods = JsonConvert.DeserializeObject<ICollection<string>>(responseString);

            // Assert
            response.EnsureSuccessStatusCode();
            CollectionAssert.AllItemsAreNotNull(methods);
            CollectionAssert.IsNotEmpty(methods);
            Assert.Greater(methods.Count, 1);
            CollectionAssert.AllItemsAreUnique(methods);
        }

        #region integral
        [Test]
        public async Task IntegralReturnsHttpOk()
        {
            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/integral/{Uri.EscapeDataString(Strings.trapezoidal_method)}/x/0/1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task IntegralReturnsCorectValue()
        {
            //arrange
            Func<double, double> func = x => 1 - x;
            var funcStr = "1-x";
            var a = 1;
            var b = 2;
            var n = 100;

            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/integral/{Uri.EscapeDataString(Strings.Simpson_s_method)}/{funcStr}/{a}/{b}/{n}");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual(Integral.simpsonMethod(func, a, b, n).ToMathString(), responseString);
        }

        [Test]
        public async Task IntegralWithCustomFunctionsReturnsCorectValue()
        {
            //arrange
            double CustomFunc(double x)
            {
                return x * x * x;
            }

            var customFuncStr = @"
            static double CustomFunc(double x)
            {
                return x * x * x;
            }
";
            Func<double, double> func = x => 1 - x - CustomFunc(x);
            var funcStr = "1 - x - CustomFunc(x)";

            var a = 2;
            var b = 5;
            var n = 100;

            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/integral/{Uri.EscapeDataString(Strings.non_adaptive_Gauss_Kronrod_method)}/{funcStr}/{a}/{b}/{n}/{Uri.EscapeDataString(customFuncStr)}");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var mathString = Integral.nonAdaptiveGaussKronrodMethod(func, a, b, n).ToMathString();
            Assert.AreEqual(mathString, responseString);
        }

        #endregion

        #region derivative

        [Test]
        public async Task DerivativeReturnsHttpOk()
        {
            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/derivative/{Uri.EscapeDataString(Strings.finite_difference_formula)}/2x/0");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task DerivativeReturnsCorectValue()
        {
            //arrange
            Func<double, double> func = x => 1 - x;
            var funcStr = "1-x";
            var point = 3;
            uint order = 1;
            var epsilon = 1e-6;

            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/derivative/{Uri.EscapeDataString(Strings.stable_finite_difference_formula)}/{funcStr}/{point}/{order}/{epsilon}");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var mathString = Derivative.stableFiniteDifferenceFormula(func, point, order, epsilon).ToMathString();
            Assert.AreEqual(mathString, responseString);
        }

        [Test]
        public async Task DerivativeWithCustomFunctionsReturnsCorectValue()
        {
            //arrange
            double CustomFunc(double x)
            {
                return x * x * x;
            }

            var customFuncStr = @"
            static double CustomFunc(double x)
            {
                return x * x * x;
            }
";
            Func<double, double> func = x => 1 - x - CustomFunc(x);
            var funcStr = "1 - x - CustomFunc(x)";

            var point = 3;
            uint order = 1;
            var epsilon = 1e-6;

            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/derivative/{Uri.EscapeDataString(Strings.centered_five_point_method)}/{funcStr}/{point}/{order}/{epsilon}/{Uri.EscapeDataString(customFuncStr)}");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var mathString = Derivative.centeredFivePointMethod(func, point, order, epsilon).ToMathString();
            Assert.AreEqual(mathString, responseString);
        }

        #endregion

        #region function root
        [Test]
        public async Task FunctionRootReturnsHttpOk()
        {
            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/function-root/{Uri.EscapeDataString(Strings.bisection_method)}/2x-1/-2/2");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task FunctionRootReturnsCorectValue()
        {
            //arrange
            Func<double, double> func = x => 1 - x;
            var funcStr = "1-x";
            var a = -5;
            var b = 5;
            uint n = 100;
            var epsilon = 1e-6;

            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/function-root/{Uri.EscapeDataString(Strings.secant_method)}/{funcStr}/{a}/{b}/{epsilon}/{n}");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual(FunctionRoot.secantMethod(func, a, b, epsilon, n).ToMathString(), responseString);
        }

        [Test]
        public async Task FunctionRootWithCustomFunctionsReturnsCorectValue()
        {
            //arrange
            double CustomFunc(double x)
            {
                return x * x * x;
            }

            var customFuncStr = @"
            static double CustomFunc(double x)
            {
                return x * x * x;
            }
";
            Func<double, double> func = x => 1 - x - CustomFunc(x);
            var funcStr = "1 - x - CustomFunc(x)";

            var a = -15;
            var b = 15;
            uint n = 100;
            var epsilon = 1e-6;

            // Act
            var response = await _client.GetAsync($@"/api/numerical-calculations/function-root/{Uri.EscapeDataString(Strings.Brent_s_method)}/{funcStr}/{a}/{b}/{epsilon}/{n}/{Uri.EscapeDataString(customFuncStr)}");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var mathString = FunctionRoot.BrentMethod(func, a, b, epsilon, n).ToMathString();
            Assert.AreEqual(mathString, responseString);
        }

        #endregion
    }
}
