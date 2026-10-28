using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using Computator.NET.WebApi.Controllers;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Computator.NET.WebApi.IntegrationTests
{
    [TestFixture]
    public sealed class InteractiveChartApiTests
    {
        private TestServer _server;
        private HttpClient _client;

        [OneTimeSetUp]
        public void Start()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
            _client.Timeout = TimeSpan.FromSeconds(40);
        }

        [OneTimeTearDown]
        public void Stop() { _client?.Dispose(); _server?.Dispose(); }

        private static ExecutionRequest Request(string expression) => new ExecutionRequest
        {
            Expression = expression, Interactive2D = true, CurveSamples = 65, ComplexGridSize = 9,
            XMin = -2, XMax = 2, YMin = -2, YMax = 2, Width = 300, Height = 200
        };

        private Task<HttpResponseMessage> Post(ExecutionRequest request) =>
            _client.PostAsync("/api/v2/chart", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

        private async Task<JObject> Samples(ExecutionRequest request)
        {
            using var response = await Post(request);
            var body = await response.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
            NUnit.Framework.Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
            return JObject.Parse(body);
        }

        [TestCase("x*x", "square")]
        [TestCase("2*x+1", "line")]
        [TestCase("sin(x)", "sine")]
        public async Task RealSamplesMatchIndependentMathematics(string expression, string fixture)
        {
            var curve = await Samples(Request(expression));
            NUnit.Framework.Assert.That((string)curve["kind"], Is.EqualTo("curve"));
            var x = curve["x"].Values<double>().ToArray();
            var y = curve["y"].Values<double?>().ToArray();
            NUnit.Framework.Assert.That(x.Length, Is.EqualTo(65));
            NUnit.Framework.Assert.That(y.Length, Is.EqualTo(x.Length));
            NUnit.Framework.Assert.That(x[0], Is.EqualTo(-2)); NUnit.Framework.Assert.That(x.Last(), Is.EqualTo(2));
            NUnit.Framework.Assert.That((int)curve["finiteSamples"], Is.EqualTo(65));
            NUnit.Framework.Assert.That((int)curve["missingSamples"], Is.Zero);
            for (var index = 0; index < x.Length; index++)
            {
                var expected = fixture == "square" ? x[index] * x[index]
                    : fixture == "line" ? 2 * x[index] + 1 : Math.Sin(x[index]);
                NUnit.Framework.Assert.That(y[index], Is.EqualTo(expected).Within(1e-12), $"sample {index}");
                if (index > 0) NUnit.Framework.Assert.That(x[index], Is.GreaterThan(x[index - 1]));
            }
        }

        [Test]
        public async Task ComplexGridUsesAscendingDomainCoordinatesAndNumericalValues()
        {
            var grid = await Samples(Request("z*z"));
            NUnit.Framework.Assert.That((string)grid["kind"], Is.EqualTo("complex"));
            var x = grid["x"].Values<double>().ToArray();
            var y = grid["y"].Values<double>().ToArray();
            NUnit.Framework.Assert.That(x, Is.EqualTo(new[] { -2d, -1.5, -1, -0.5, 0, 0.5, 1, 1.5, 2 }));
            NUnit.Framework.Assert.That(y, Is.EqualTo(x));
            NUnit.Framework.Assert.That((int)grid["finiteSamples"], Is.EqualTo(81));
            NUnit.Framework.Assert.That((int)grid["missingSamples"], Is.Zero);
            for (var row = 0; row < y.Length; row++)
            for (var column = 0; column < x.Length; column++)
            {
                NUnit.Framework.Assert.That((double)grid["real"][row][column], Is.EqualTo(x[column] * x[column] - y[row] * y[row]).Within(1e-12));
                NUnit.Framework.Assert.That((double)grid["imaginary"][row][column], Is.EqualTo(2 * x[column] * y[row]).Within(1e-12));
            }
        }

        [Test]
        public async Task SingularSamplesAreNullHolesAndBetweenSamplePolesAreNotConnected()
        {
            var curve = await Samples(Request("1/x"));
            var curveX = curve["x"].Values<double>().ToArray();
            var curveY = curve["y"].Values<double?>().ToArray();
            NUnit.Framework.Assert.That(curveY[Array.IndexOf(curveX, 0d)], Is.Null);
            NUnit.Framework.Assert.That((int)curve["missingSamples"], Is.GreaterThanOrEqualTo(1));

            // 0.13 is deliberately not one of the base coordinates.
            var offGrid = await Samples(Request("1/(x-0.13)"));
            var x = offGrid["x"].Values<double>().ToArray();
            var y = offGrid["y"].Values<double?>().ToArray();
            var left = Array.FindLastIndex(x, value => value < 0.13);
            NUnit.Framework.Assert.That(y[left] == null || y[left + 1] == null, Is.True, "A pole between grid samples must break the polyline.");

            var complex = await Samples(Request("1/z"));
            NUnit.Framework.Assert.That(complex["real"][4][4].Type, Is.EqualTo(JTokenType.Null));
            NUnit.Framework.Assert.That(complex["imaginary"][4][4].Type, Is.EqualTo(JTokenType.Null));
            NUnit.Framework.Assert.That((int)complex["finiteSamples"], Is.EqualTo(80));
            NUnit.Framework.Assert.That((int)complex["missingSamples"], Is.EqualTo(1));
        }

        [TestCase("1/(x-x)")]
        [TestCase("z/(z-z)")]
        public async Task EntirelyUndefinedDomainsReturnAUsefulProblemAndAllowRecovery(string expression)
        {
            using (var response = await Post(Request(expression)))
            {
                NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
                NUnit.Framework.Assert.That((string)JObject.Parse(await response.Content.ReadAsStringAsync())["title"], Does.Contain("No finite"));
            }
            NUnit.Framework.Assert.That((string)(await Samples(Request("x")))["kind"], Is.EqualTo("curve"));
        }

        [TestCase("sqrt(x)", "curve")]
        [TestCase("z/(z-z)", "complex")]
        public async Task ExistingLayersCanBeResampledIntoAnEntirelyUndefinedViewport(string expression, string kind)
        {
            var request = Request(expression);
            request.XMin = -4; request.XMax = -1;
            using (var initial = await Post(request))
                NUnit.Framework.Assert.That(initial.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity),
                    "Initial Draw must still reject an entirely undefined chart by default.");

            request.AllowEmpty2D = true;
            var samples = await Samples(request);
            NUnit.Framework.Assert.That((string)samples["kind"], Is.EqualTo(kind));
            NUnit.Framework.Assert.That((int)samples["finiteSamples"], Is.Zero);
            NUnit.Framework.Assert.That((int)samples["missingSamples"], Is.EqualTo(kind == "curve" ? 65 : 81));
            NUnit.Framework.Assert.That((double)samples["x"][0], Is.EqualTo(-4));
            NUnit.Framework.Assert.That((double)samples["x"].Last(), Is.EqualTo(-1));
            if (kind == "curve")
                NUnit.Framework.Assert.That(samples["y"].All(value => value.Type == JTokenType.Null), Is.True);
            else
            {
                NUnit.Framework.Assert.That(samples["real"].Count(), Is.EqualTo(9));
                NUnit.Framework.Assert.That(samples["imaginary"].Count(), Is.EqualTo(9));
                foreach (var component in new[] { "real", "imaginary" })
                foreach (var row in samples[component])
                {
                    NUnit.Framework.Assert.That(row.Count(), Is.EqualTo(9));
                    NUnit.Framework.Assert.That(row.All(value => value.Type == JTokenType.Null), Is.True);
                }
            }
        }

        [Test]
        public async Task InteractiveDefinitionsRemainRequestLocalForBothFunctionKinds()
        {
            foreach (var offset in new[] { 7, 19 })
            {
                var curveRequest = Request("Probe(x)");
                curveRequest.CustomFunctions = "// Unsaved UTF-8: zażółć\nstatic real Probe(real x) { return x + " + offset + "; }";
                var curve = await Samples(curveRequest);
                NUnit.Framework.Assert.That((double)curve["y"][32], Is.EqualTo(offset));

                var complexRequest = Request("Probe(z)");
                complexRequest.CustomFunctions = "static complex Probe(complex z) { return z*z + " + offset + "; }";
                var grid = await Samples(complexRequest);
                NUnit.Framework.Assert.That((double)grid["real"][4][4], Is.EqualTo(offset));
                NUnit.Framework.Assert.That((double)grid["imaginary"][4][4], Is.Zero);
            }
            using var missingDefinition = await Post(Request("Probe(x)"));
            NUnit.Framework.Assert.That(missingDefinition.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCase("curveSamples", 32)]
        [TestCase("curveSamples", 4098)]
        [TestCase("complexGridSize", 8)]
        [TestCase("complexGridSize", 258)]
        public async Task SamplingLimitsAreRejectedBeforeCompilingInvalidSource(string field, int count)
        {
            var request = Request("not-valid(");
            if (field == "curveSamples") request.CurveSamples = count;
            else request.ComplexGridSize = count;
            using var response = await Post(request);
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            NUnit.Framework.Assert.That(JObject.Parse(await response.Content.ReadAsStringAsync())["errors"]?[field], Is.Not.Null);
        }

        [Test]
        public async Task OverflowingViewportSpansAreRejectedBeforeCompilation()
        {
            var request = Request("not-valid(");
            request.XMin = -double.MaxValue; request.XMax = double.MaxValue;
            using var response = await Post(request);
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            NUnit.Framework.Assert.That(JObject.Parse(await response.Content.ReadAsStringAsync())["errors"]?["bounds"], Is.Not.Null);
        }

        [TestCase("x", false)]
        [TestCase("z", false)]
        [TestCase("x²+y²=1", true)]
        public async Task DefaultAndImplicitRequestsKeepThePngContract(string expression, bool interactive)
        {
            var request = Request(expression); request.Interactive2D = interactive;
            using var response = await Post(request);
            var body = await response.Content.ReadAsByteArrayAsync();
            NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), Encoding.UTF8.GetString(body.Take(200).ToArray()));
            NUnit.Framework.Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("image/png"));
            NUnit.Framework.Assert.That(body.Take(8), Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
            NUnit.Framework.Assert.That(body.Length, Is.GreaterThan(100));
        }

        [Test]
        public void SteepStraightLinesStayConnectedAndEvaluationWorkIsBounded()
        {
            var evaluations = 0;
            var function = new Function(new Func<double, double>(x => { evaluations++; return 1e12 * x + 7; }), false);
            var samples = Chart2DSampler.SampleCurve(function, -1, 1, -5, 5, 65, CancellationToken.None);
            NUnit.Framework.Assert.That(samples.X.Length, Is.EqualTo(65));
            NUnit.Framework.Assert.That(samples.MissingSamples, Is.Zero);
            NUnit.Framework.Assert.That(evaluations, Is.LessThanOrEqualTo(65 + 8 * 64));
        }

        [Test]
        public void UnrepresentableSamplingCoordinatesFailInsteadOfPublishingDuplicateAxes()
        {
            var function = new Function(new Func<double, double>(x => x), false);
            NUnit.Framework.Assert.Throws<ArgumentException>(() => Chart2DSampler.SampleCurve(function, 1e16, 1e16 + 32,
                -5, 5, 65, CancellationToken.None));
        }

        [Test]
        public void ArithmeticFailuresBecomeHolesButApplicationFailuresAreNotHidden()
        {
            var arithmetic = new Function(new Func<double, double>(x => x == 0 ? throw new DivideByZeroException() : 1 / x), false);
            var samples = Chart2DSampler.SampleCurve(arithmetic, -1, 1, -5, 5, 33, CancellationToken.None);
            NUnit.Framework.Assert.That(samples.Y[Array.IndexOf(samples.X, 0d)], Is.Null);
            var invalid = new Function(new Func<double, double>(_ => throw new InvalidOperationException("Probe failure")), false);
            NUnit.Framework.Assert.Throws<CalculationException>(() => Chart2DSampler.SampleCurve(invalid, -1, 1, -5, 5, 33, CancellationToken.None));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BothSamplersObserveCancellationDuringEvaluation(bool complex)
        {
            using var cancellation = new CancellationTokenSource();
            var evaluations = 0;
            if (complex)
            {
                var function = new Function(new Func<Complex, Complex>(z =>
                { if (++evaluations == 7) cancellation.Cancel(); return z; }), false);
                NUnit.Framework.Assert.Throws<OperationCanceledException>(() => Chart2DSampler.SampleComplex(function,
                    -1, 1, -1, 1, 9, cancellation.Token));
            }
            else
            {
                var function = new Function(new Func<double, double>(x =>
                { if (++evaluations == 7) cancellation.Cancel(); return x; }), false);
                NUnit.Framework.Assert.Throws<OperationCanceledException>(() => Chart2DSampler.SampleCurve(function,
                    -1, 1, -1, 1, 33, cancellation.Token));
            }
            NUnit.Framework.Assert.That(evaluations, Is.EqualTo(7));
        }
    }
}
