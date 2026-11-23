using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Computator.NET.Engine.Symbolic;
using Computator.NET.WebApi.Execution;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    /// <summary>Actual protected Kestrel endpoint and owned worker, not controller mocks.</summary>
    [TestFixture, NonParallelizable]
    public sealed class SymbolicApiTests
    {
        private WebApplication _server;
        private HttpClient _client;
        [OneTimeSetUp] public async Task Start() { (_server, _client) = await ProtectedApiTestHost.Start(); }
        [OneTimeTearDown] public async Task Stop()
        {
            _client?.Dispose();
            if (_server != null) { await _server.StopAsync(); await _server.DisposeAsync(); }
        }

        private async Task<SymbolicResult> Execute(SymbolicRequest request)
        {
            using var response = await _client.PostAsJsonAsync("/api/v2/symbolic", request);
            var body = await response.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
            NUnit.Framework.Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
            NUnit.Framework.Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            var result = JsonSerializer.Deserialize<SymbolicResult>(body, WorkerProcess.JsonOptions);
            NUnit.Framework.Assert.That(result, Is.Not.Null);
            NUnit.Framework.Assert.That(result.Latex, Is.Not.Empty);
            NUnit.Framework.Assert.That(result.Warnings, Is.Not.Null);
            return result;
        }

        [TestCase("simplify", "(x + 1)^2 - x^2", "7")]
        [TestCase("expand", "(x + 1)^3", "64")]
        [TestCase("factor", "x^2 - 1", "8")]
        [TestCase("differentiate", "x^3", "27")]
        public async Task AlgebraAndCalculusResultsCanBeReusedAsExactExpressions(string operation, string expression, string expected)
        {
            var transformed = await Execute(new SymbolicRequest { Operation = operation, Expression = expression });
            NUnit.Framework.Assert.That(transformed.Status, Is.EqualTo("exact"));
            var value = await Execute(new SymbolicRequest { Operation = "substitute", Expression = transformed.Result,
                Substitutions = new Dictionary<string, string> { ["x"] = "3" } });
            NUnit.Framework.Assert.That(value.Result.Replace(" ", ""), Is.EqualTo(expected));
        }

        [Test]
        public async Task AntiderivativeIncludesItsConstantAndDifferentiatesBack()
        {
            var integral = await Execute(new SymbolicRequest { Operation = "integrate", Expression = "3*x^2" });
            NUnit.Framework.Assert.That(integral.Result, Does.Contain("C"));
            var derivative = await Execute(new SymbolicRequest { Operation = "differentiate", Expression = integral.Result });
            var value = await Execute(new SymbolicRequest { Operation = "substitute", Expression = derivative.Result,
                Substitutions = new Dictionary<string, string> { ["x"] = "3" } });
            NUnit.Framework.Assert.That(value.Result, Is.EqualTo("27"));
        }

        [TestCase("simplify", "1/3+1/6", "1/2")]
        [TestCase("limit", "sin(x)/x", "1")]
        [TestCase("determinant", "[[1,2],[3,4]]", "-2")]
        [TestCase("trace", "[[1,2],[3,4]]", "5")]
        public async Task ExactOperationsReturnKnownMathematicalValues(string operation, string expression, string expected)
        {
            var result = await Execute(new SymbolicRequest { Operation = operation, Expression = expression });
            NUnit.Framework.Assert.That(result.Result.Replace(" ", ""), Is.EqualTo(expected));
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("exact"));
        }

        [Test]
        public async Task DefiniteIntegrationKeepsBoundsAsExactExpressions()
        {
            var result = await Execute(new SymbolicRequest { Operation = "definite-integrate", Expression = "x^2",
                LowerBound = "0", UpperBound = "3" });
            NUnit.Framework.Assert.That(result.Result, Is.EqualTo("9"));
        }

        [Test]
        public async Task SimultaneousSubstitutionAndUnicodeSurviveJsonAndWorkerPipes()
        {
            var result = await Execute(new SymbolicRequest { Operation = "substitute", Expression = "α² + β",
                Substitutions = new Dictionary<string, string> { ["α"] = "2", ["β"] = "3" } });
            NUnit.Framework.Assert.That(result.Result, Is.EqualTo("7"));
            var swapped = await Execute(new SymbolicRequest { Operation = "substitute", Expression = "x-y",
                Substitutions = new Dictionary<string, string> { ["x"] = "y", ["y"] = "x" } });
            var value = await Execute(new SymbolicRequest { Operation = "substitute", Expression = swapped.Result,
                Substitutions = new Dictionary<string, string> { ["x"] = "2", ["y"] = "3" } });
            NUnit.Framework.Assert.That(value.Result, Is.EqualTo("1"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task SimultaneousSubstitutionCannotCaptureAUserNamedTemporary(bool reverse)
        {
            var substitutions = new Dictionary<string, string>();
            if (reverse) substitutions.Add("ComputatorSubstitution_1", "2");
            substitutions.Add("x", "y");
            if (!reverse) substitutions.Add("ComputatorSubstitution_1", "2");
            var result = await Execute(new SymbolicRequest { Operation = "substitute", Expression = "x", Substitutions = substitutions });
            NUnit.Framework.Assert.That(result.Result, Is.EqualTo("y"));
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("exact"));
        }

        [Test]
        public async Task RealSystemRejectsSolutionsOutsideTheOriginalRadicalDomain()
        {
            var result = await Execute(new SymbolicRequest { Operation = "solve-system", Domain = "real",
                Equations = new[] { "sqrt(x)^2=-1", "y=0" }, Variables = new[] { "x", "y" } });
            NUnit.Framework.Assert.That(result.Result.Replace(" ", ""), Is.EqualTo("{}").Or.EqualTo("∅"));
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("exact"));
        }

        [Test]
        public async Task NegativeInverseCotangentUsesTheTslPrincipalValue()
        {
            var result = await Execute(new SymbolicRequest { Operation = "evaluate", Expression = "arccot(-1)" });
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("approximate"));
            NUnit.Framework.Assert.That(double.Parse(result.Result, CultureInfo.InvariantCulture), Is.EqualTo(3 * Math.PI / 4).Within(1e-14));
        }

        [TestCase("2·x⁽ⁿ⁺¹⁾", "16")]
        [TestCase("2·x⁻²", "1/2")]
        [TestCase("sin²(x)+cos²(x)", "1")]
        [TestCase("sin³(PI/6)²", "1/64")]
        [TestCase("log(8,2)", "3")]
        public async Task NativeTslNotationKeepsItsMeaningThroughProtectedApiAndWorker(string expression, string expected)
        {
            var result = await Execute(new SymbolicRequest { Operation = "substitute", Expression = expression,
                Substitutions = new Dictionary<string, string> { ["x"] = "2", ["n"] = "2" } });
            NUnit.Framework.Assert.That(result.Result.Replace(" ", ""), Is.EqualTo(expected));
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("exact"));
        }

        [Test]
        public async Task NativeTslResultsCanBeReusedWithoutAFrontendFormatter()
        {
            var derivative = await Execute(new SymbolicRequest { Operation = "differentiate", Expression = "2·x³" });
            NUnit.Framework.Assert.That(derivative.Result, Does.Contain("·").And.Contain("²"));
            NUnit.Framework.Assert.That(derivative.Result, Does.Not.Contain("*").And.Not.Contain("^"));
            var value = await Execute(new SymbolicRequest { Operation = "substitute", Expression = derivative.Result,
                Substitutions = new Dictionary<string, string> { ["x"] = "2" } });
            NUnit.Framework.Assert.That(value.Result, Is.EqualTo("24"));
            using var numerical = await _client.PostAsJsonAsync("/api/v2/execute",
                new { operation = "calculate", expression = derivative.Result, x = 2 });
            NUnit.Framework.Assert.That(numerical.StatusCode, Is.EqualTo(HttpStatusCode.OK), await numerical.Content.ReadAsStringAsync());
            using var numericBody = JsonDocument.Parse(await numerical.Content.ReadAsStringAsync());
            NUnit.Framework.Assert.That(numericBody.RootElement.GetProperty("result").GetString(), Is.EqualTo("24"));
            var logarithm = await Execute(new SymbolicRequest { Expression = "log(x,2)" });
            var logarithmValue = await Execute(new SymbolicRequest { Operation = "substitute", Expression = logarithm.Result,
                Substitutions = new Dictionary<string, string> { ["x"] = "8" } });
            NUnit.Framework.Assert.That(logarithmValue.Result, Is.EqualTo("3"));
        }

        [Test]
        public async Task TaylorPolynomialAndArbitraryPrecisionEvaluationUseTheSharedEngine()
        {
            var polynomial = await Execute(new SymbolicRequest { Operation = "taylor", Expression = "sin(x)", Order = 3 });
            var value = await Execute(new SymbolicRequest { Operation = "substitute", Expression = polynomial.Result,
                Substitutions = new Dictionary<string, string> { ["x"] = "1" } });
            NUnit.Framework.Assert.That(value.Result.Replace(" ", ""), Is.EqualTo("5/6"));
            var approximate = await Execute(new SymbolicRequest { Operation = "evaluate", Expression = "sqrt(2)", Precision = 40 });
            NUnit.Framework.Assert.That(approximate.Result, Does.StartWith("1.414213562373095048801688724209698"));
            NUnit.Framework.Assert.That(approximate.Status, Is.EqualTo("approximate"));
        }

        [Test]
        public async Task ComplexEquationSolutionsAreRetained()
        {
            var result = await Execute(new SymbolicRequest { Operation = "solve", Expression = "x^2+1=0", Domain = "complex" });
            NUnit.Framework.Assert.That(result.Result, Does.Contain("i"));
            NUnit.Framework.Assert.That(result.Result, Does.Contain("-"));
        }

        [Test]
        public async Task UnresolvedIntegralsAreNotReportedAsSolved()
        {
            var result = await Execute(new SymbolicRequest { Operation = "integrate", Expression = "sin(x^x)" });
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("unevaluated"));
            NUnit.Framework.Assert.That(result.Warnings, Is.Not.Empty);
        }

        [TestCase("", "simplify")]
        [TestCase("x+", "simplify")]
        [TestCase("x", "invented")]
        [TestCase("System.IO.File.Delete(\"x\")", "simplify")]
        public async Task BadMathematicalInputIsAProblemAndNextRequestRecovers(string expression, string operation)
        {
            using var invalid = await _client.PostAsJsonAsync("/api/v2/symbolic", new SymbolicRequest { Expression = expression, Operation = operation });
            NUnit.Framework.Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            NUnit.Framework.Assert.That(await invalid.Content.ReadAsStringAsync(), Does.Contain("400"));
            NUnit.Framework.Assert.That((await Execute(new SymbolicRequest { Expression = "6*7" })).Result, Is.EqualTo("42"));
        }

        [Test]
        public async Task MissingTokenWrongMediaTypeAndOversizedInputAreRejected()
        {
            using var anonymous = new HttpClient { BaseAddress = _client.BaseAddress };
            using var forbidden = await anonymous.PostAsJsonAsync("/api/v2/symbolic", new { expression = "1" });
            NUnit.Framework.Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            using var media = await _client.PostAsync("/api/v2/symbolic", new StringContent("{}", Encoding.UTF8, "text/plain"));
            NUnit.Framework.Assert.That(media.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
            using var oversize = await _client.PostAsJsonAsync("/api/v2/symbolic", new { expression = new string('x', 8193) });
            NUnit.Framework.Assert.That(oversize.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        private static IEnumerable<TestCaseData> InvalidJsonRequests()
        {
            var fixture = Path.Combine(AppContext.BaseDirectory, "fixtures", "symbolic-invalid-json.json");
            using var json = JsonDocument.Parse(File.ReadAllText(fixture));
            foreach (var item in json.RootElement.EnumerateArray())
                yield return new TestCaseData(item.GetProperty("json").GetString())
                    .SetName("SymbolicRawJsonRejects_" + item.GetProperty("name").GetString());
        }

        [TestCaseSource(nameof(InvalidJsonRequests))]
        public async Task RawSymbolicJsonRejectsAmbiguousMembersAndImplicitStringCoercion(string raw)
        {
            using var response = await _client.PostAsync("/api/v2/symbolic", new StringContent(raw, Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), body);
            NUnit.Framework.Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/problem+json"));
            using var problem = JsonDocument.Parse(body);
            NUnit.Framework.Assert.That(problem.RootElement.GetProperty("status").GetInt32(), Is.EqualTo(400));
            NUnit.Framework.Assert.That(problem.RootElement.GetProperty("title").GetString(), Is.Not.Empty);
        }

        [TestCase("{\"Expression\":\"log(8,2)\"}", "3")]
        [TestCase("{\"operation\":\"substitute\",\"expression\":\"x-X\",\"substitutions\":{\"x\":\"3\",\"X\":\"2\"}}", "1")]
        public async Task RawSymbolicJsonPreservesPropertyCasingAndDistinctVariableNames(string raw, string expected)
        {
            using var response = await _client.PostAsync("/api/v2/symbolic", new StringContent(raw, Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
            var result = JsonSerializer.Deserialize<SymbolicResult>(body, WorkerProcess.JsonOptions);
            NUnit.Framework.Assert.That(result.Result, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(result.Status, Is.EqualTo("exact"));
        }

        [Test]
        public async Task OversizedSymbolicJsonIsRejectedBeforeUploadingItsBody()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v2/symbolic")
            {
                Content = new StringContent("{\"expression\":\"" + new string('x', 262145) + "\"}", Encoding.UTF8, "application/json")
            };
            request.Headers.ExpectContinue = true;
            using var response = await _client.SendAsync(request);
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RequestEntityTooLarge));
            NUnit.Framework.Assert.That((await Execute(new SymbolicRequest { Expression = "6*7" })).Result, Is.EqualTo("42"));
        }

        [Test]
        public async Task SymbolicWorkUsesAnOwnedWorkerAndCancellationReapsIt()
        {
            var pid = 0;
            using var cancellation = new CancellationTokenSource();
            try
            {
                await WorkerProcess.RunSymbolicAsync(new SymbolicRequest { Expression = "1+1" }, cancellation.Token,
                    started: id => { pid = id; cancellation.Cancel(); });
                NUnit.Framework.Assert.Fail("Expected cancellation after the child started.");
            }
            catch (OperationCanceledException) { }
            NUnit.Framework.Assert.That(pid, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(IsRunning(pid), Is.False, "Cancelling HTTP work must end its child process.");
            NUnit.Framework.Assert.That((await Execute(new SymbolicRequest { Expression = "7*6" })).Result, Is.EqualTo("42"));
        }
        private static bool IsRunning(int pid)
        {
            try { using var process = Process.GetProcessById(pid); return !process.HasExited; }
            catch (ArgumentException) { return false; }
        }
    }
}
