using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using File = System.IO.File;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Utility;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class FunctionReferenceRichContentTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("real Probe(real x)")]
        [TestCase("real   Probe(real x)")]
        [TestCase("Probe(x)")]
        [TestCase("REAL Probe(real x)")]
        public void DescriptionsNeverRepeatSignaturesRegardlessOfCaption(string context)
        {
            var entry = new FunctionInfo { Title = "Probe", Description =
                "<div><p class=\"reference-signature\"><code>real Probe(real x)</code><br /><code>complex Probe(complex x)</code></p>" +
                "<p class=\"reference-summary\">Meaning.</p><p class=\"reference-arguments\">Real and complex inputs.</p>" +
                "<pre><code>Probe(1)</code></pre></div>" };
            var original = entry.Description;
            var rendered = FunctionReferenceDocument.Render(entry, context);
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("reference-signature"));
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("real Probe(real x)"));
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("complex Probe(complex x)"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("Meaning."));
            NUnit.Framework.Assert.That(rendered, Does.Contain("Real and complex inputs."));
            NUnit.Framework.Assert.That(rendered, Does.Contain("<code>Probe(1)</code>"));
            NUnit.Framework.Assert.That(entry.Description, Is.EqualTo(original), "Rendering must not mutate the catalogue.");
        }

        [Test]
        public void LegacySignatureHeadingsAndExtraClassesDoNotLeakIntoDescriptions()
        {
            var entry = new FunctionInfo { Title = "Probe", Description =
                "<div><div class=\"legacy reference-signature extra\"><strong>Syntax</strong><code>real Probe(real x)</code></div>" +
                "<p>Meaning.</p><pre><code>real Example(real x) { return x; }</code></pre></div>" };
            var rendered = FunctionReferenceDocument.Render(entry);
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("reference-signature"));
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("Syntax"));
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("real Probe(real x)"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("Meaning."));
            NUnit.Framework.Assert.That(rendered, Does.Contain("real Example(real x) { return x; }"),
                "Example code is not signature metadata and must remain readable.");
        }

        [TestCase("gamma(x)")]
        [TestCase("ElementaryCharge")]
        [TestCase("ConicalP(μ,λ,x)")]
        public void StandaloneShippedDescriptionsStartWithContentNotSignatures(string signature)
        {
            var entry = new FunctionsDetailsFileSource().Details[signature];
            var rendered = FunctionReferenceDocument.Render(entry);
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("reference-signature"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("reference-summary"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("reference-explanation"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("reference-math"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("Arguments / value"));
        }

        [TestCase("https://example.invalid/a.png")]
        [TestCase("reference-equations/../settings.json")]
        [TestCase("data:image/svg+xml,unsafe")]
        [TestCase("reference-equations/not-a-hash.png")]
        public void DesktopEquationReadsRejectNoncataloguePaths(string source)
        {
            var entry = new FunctionInfo { Description = "<div><img src=\"" + source + "\" /></div>" };
            NUnit.Framework.Assert.Throws<InvalidDataException>(() => FunctionReferenceDocument.Render(entry));
        }

        private static XElement InlineLaguerreImage()
        {
            var entry = new FunctionsDetailsFileSource().Details["Laguerre(n,α,x)"];
            return XElement.Parse(entry.Description).Descendants("img")
                .First(image => (string)image.Attribute("alt") == "x^α exp(−x)");
        }

        [Test, SetCulture("pl-PL")]
        public void DesktopInlineMathReconstructsNumericLayoutWithoutTrustingSourceStyles()
        {
            var image = InlineLaguerreImage();
            var width = (int)image.Attribute("width");
            var height = (int)image.Attribute("height");
            var depth = (int)image.Attribute("data-depth");
            image.SetAttributeValue("style", "position:fixed;width:999px");
            image.SetAttributeValue("onerror", "window.referenceExecuted=true");
            var entry = new FunctionInfo { Description = new XElement("p", "The weight ", image, " stays in this sentence.").ToString() };
            var rendered = FunctionReferenceDocument.Render(entry);
            NUnit.Framework.Assert.That(rendered, Does.Contain("The weight <span class=\"reference-inline\"><img"));
            NUnit.Framework.Assert.That(rendered, Does.Contain(" stays in this sentence."));
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("position:fixed").And.Not.Contain("999px").And.Not.Contain("referenceExecuted"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("data:image/png;base64,"));
            var style = "width:" + (width / 20m).ToString("0.###", CultureInfo.InvariantCulture) + "em;height:" +
                (height / 20m).ToString("0.###", CultureInfo.InvariantCulture) + "em;vertical-align:" +
                (-depth / 1000m).ToString("0.###", CultureInfo.InvariantCulture) + "em;";
            NUnit.Framework.Assert.That(rendered, Does.Contain(style));
            var paragraph = XElement.Parse(rendered.Substring(rendered.IndexOf("<p>", StringComparison.Ordinal)).Split("</body>")[0]);
            NUnit.Framework.Assert.That(paragraph.Value, Is.EqualTo("The weight x^α exp(−x) stays in this sentence."));
            NUnit.Framework.Assert.That((string)paragraph.Descendants("img").Single().Attribute("aria-hidden"), Is.EqualTo("true"));
            NUnit.Framework.Assert.That(paragraph.Descendants("span").Count(span => (string)span.Attribute("class") == "reference-math-text"), Is.EqualTo(1));
        }

        [TestCase("data-depth", "")]
        [TestCase("data-depth", "NaN")]
        [TestCase("data-depth", "1;position:fixed")]
        [TestCase("data-depth", "-1001")]
        [TestCase("data-depth", "5000")]
        [TestCase("data-depth", "+1")]
        [TestCase("data-depth", "01")]
        [TestCase("width", "321")]
        [TestCase("height", "101")]
        [TestCase("class", "reference-math-inline extra")]
        [TestCase("alt", "")]
        public void DesktopInlineMathRejectsUnboundedOrMalformedMetadata(string name, string value)
        {
            var image = InlineLaguerreImage();
            image.SetAttributeValue(name, value);
            var entry = new FunctionInfo { Description = new XElement("p", image).ToString() };
            NUnit.Framework.Assert.Throws<InvalidDataException>(() => FunctionReferenceDocument.Render(entry));
        }

        [Test]
        public void EveryShippedEquationMatchesItsTexIdentityHashAndPixelSize()
        {
            var directory = PathUtility.GetFullPath("Static", "reference-equations");
            using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "manifest.json")));
            var equations = manifest.RootElement.GetProperty("equations");
            var ids = equations.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
            NUnit.Framework.Assert.That(ids.Count, Is.GreaterThanOrEqualTo(600));
            foreach (var pair in equations.EnumerateObject())
            {
                var item = pair.Value;
                var inline = item.TryGetProperty("mode", out var mode) && mode.GetString() == "inline";
                var identity = (inline ? "inline\0" : "") + item.GetProperty("tex").GetString();
                var textHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identity))).ToLowerInvariant();
                NUnit.Framework.Assert.That(pair.Name, Is.EqualTo(textHash));
                var bytes = File.ReadAllBytes(Path.Combine(directory, pair.Name + ".png"));
                NUnit.Framework.Assert.That(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), Is.EqualTo(item.GetProperty("sha256").GetString()));
                NUnit.Framework.Assert.That(bytes.Take(8), Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
                NUnit.Framework.Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)), Is.EqualTo(3 * item.GetProperty("width").GetInt32()));
                NUnit.Framework.Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)), Is.EqualTo(3 * item.GetProperty("height").GetInt32()));
            }
            NUnit.Framework.Assert.That(Directory.GetFiles(directory, "*.png").Select(Path.GetFileNameWithoutExtension).OrderBy(x => x), Is.EqualTo(ids.OrderBy(x => x)));
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in new FunctionsDetailsFileSource().Details.Values)
                foreach (var image in XElement.Parse(entry.Description).Descendants("img")
                             .Where(image => (string)image.Attribute("class") != "reference-plot"))
                {
                    var id = Path.GetFileNameWithoutExtension((string)image.Attribute("src"));
                    NUnit.Framework.Assert.That(ids, Does.Contain(id), entry.Signature);
                    NUnit.Framework.Assert.That((int)image.Attribute("width"), Is.EqualTo(equations.GetProperty(id).GetProperty("width").GetInt32()));
                    NUnit.Framework.Assert.That((int)image.Attribute("height"), Is.EqualTo(equations.GetProperty(id).GetProperty("height").GetInt32()));
                    var inline = (string)image.Attribute("class") == "reference-math-inline";
                    var asset = equations.GetProperty(id);
                    NUnit.Framework.Assert.That(asset.TryGetProperty("mode", out var mode) && mode.GetString() == "inline", Is.EqualTo(inline));
                    if (inline)
                        NUnit.Framework.Assert.That((int)image.Attribute("data-depth"), Is.EqualTo(asset.GetProperty("depth").GetInt32()));
                    used.Add(id);
                }
            NUnit.Framework.Assert.That(used.OrderBy(x => x), Is.EqualTo(ids.OrderBy(x => x)), "No stale or unused equation assets.");
        }

        [Test]
        public void EveryBenchmarkDiagramIsPackagedWithMatchingHashDimensionsAndAnExplanation()
        {
            var directory = PathUtility.GetFullPath("Static", "reference-figures");
            using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "manifest.json")));
            var figures = manifest.RootElement.GetProperty("figures");
            var ids = figures.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
            NUnit.Framework.Assert.That(ids.Count, Is.EqualTo(8));
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in new FunctionsDetailsFileSource().Details.Values)
                foreach (var image in XElement.Parse(entry.Description).Descendants("img")
                             .Where(image => (string)image.Attribute("class") == "reference-plot"))
                {
                    var id = Path.GetFileNameWithoutExtension((string)image.Attribute("src"));
                    NUnit.Framework.Assert.That(ids, Does.Contain(id), entry.Signature);
                    var item = figures.GetProperty(id);
                    NUnit.Framework.Assert.That(entry.Signature, Does.StartWith(item.GetProperty("name").GetString() + "("));
                    NUnit.Framework.Assert.That((int)image.Attribute("width"), Is.EqualTo(item.GetProperty("width").GetInt32()));
                    NUnit.Framework.Assert.That((int)image.Attribute("height"), Is.EqualTo(item.GetProperty("height").GetInt32()));
                    NUnit.Framework.Assert.That((string)image.Attribute("alt"), Is.EqualTo(item.GetProperty("alt").GetString()));
                    var bytes = File.ReadAllBytes(Path.Combine(directory, id + ".png"));
                    NUnit.Framework.Assert.That(bytes.Length, Is.InRange(24, 512000));
                    NUnit.Framework.Assert.That(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), Is.EqualTo(item.GetProperty("sha256").GetString()));
                    NUnit.Framework.Assert.That(bytes.Take(8), Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
                    NUnit.Framework.Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)), Is.EqualTo(3 * (int)image.Attribute("width")));
                    NUnit.Framework.Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)), Is.EqualTo(3 * (int)image.Attribute("height")));
                    NUnit.Framework.Assert.That(entry.Description, Does.Contain(item.GetProperty("caption").GetString()));
                    NUnit.Framework.Assert.That(FunctionReferenceDocument.Render(entry), Does.Contain("class=\"reference-plot\"").And.Contain("data:image/png;base64,"));
                    used.Add(id);
                }
            NUnit.Framework.Assert.That(used.OrderBy(x => x), Is.EqualTo(ids.OrderBy(x => x)));
            NUnit.Framework.Assert.That(Directory.GetFiles(directory, "*.png").Select(Path.GetFileNameWithoutExtension).OrderBy(x => x), Is.EqualTo(ids.OrderBy(x => x)));
        }

        [TestCase("src", "reference-figures/../settings.json")]
        [TestCase("src", "https://example.invalid/figure.png")]
        [TestCase("class", "reference-math")]
        [TestCase("width", "2401")]
        [TestCase("height", "801")]
        [TestCase("data-depth", "0")]
        [TestCase("alt", "")]
        public void DesktopDiagramReadsRequireTheLocalFigureClassAndBoundedMetadata(string name, string value)
        {
            var entry = new FunctionsDetailsFileSource().Details["Ackley(xi1,xi2, ...)"];
            var image = XElement.Parse(entry.Description).Descendants("img").Single(image => (string)image.Attribute("class") == "reference-plot");
            image.SetAttributeValue(name, value);
            NUnit.Framework.Assert.Throws<InvalidDataException>(() => FunctionReferenceDocument.Render(new FunctionInfo
                { Description = new XElement("div", image).ToString() }));
        }

        [Test]
        public void CatalogueHasTeachingContentAndNoRepeatedSyntaxHeadings()
        {
            var entries = new FunctionsDetailsFileSource().Details.Values.Select(entry => XElement.Parse(entry.Description)).ToArray();
            NUnit.Framework.Assert.That(entries.Count(entry => entry.Descendants("img").Any()), Is.GreaterThanOrEqualTo(1100));
            NUnit.Framework.Assert.That(entries.All(entry => entry.Elements("p").Any(p => (string)p.Attribute("class") == "reference-explanation")), Is.True);
            NUnit.Framework.Assert.That(entries.Count(entry => entry.Descendants("div").Any(div => (string)div.Attribute("class") == "reference-worked-example")), Is.GreaterThanOrEqualTo(320));
            foreach (var entry in entries)
                NUnit.Framework.Assert.That(entry.Elements("p").Where(p => (string)p.Attribute("class") == "reference-signature").Elements("strong"), Is.Empty);
        }

        public static IEnumerable<TestCaseData> WorkedExamples()
        {
            var examples = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in new FunctionsDetailsFileSource().Details.Values)
                foreach (var example in XElement.Parse(entry.Description).Descendants("div").Where(div => (string)div.Attribute("class") == "reference-worked-example"))
                {
                    var code = example.Element("pre")?.Element("code")?.Value;
                    var result = example.Elements("p").First(p => p.Element("strong")?.Value == "Result:").Value.Substring("Result:".Length).Trim();
                    if (examples.TryGetValue(code, out var prior) && prior != result) throw new InvalidDataException("Conflicting example answers: " + code);
                    examples[code] = result;
                }
            if (examples.Count < 65) throw new InvalidDataException("Too few executable worked examples.");
            foreach (var pair in examples.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var text = pair.Value.Replace("approximately ", "");
                var expected = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                // Published decimal answers are intentionally rounded. The budget
                // reflects their last displayed digit, not a weakened algorithm test.
                var tolerance = Math.Max(Math.Abs(expected) * 1e-9, expected == 0 ? 1e-12 : 1e-300);
                yield return new TestCaseData(pair.Key, expected, tolerance).SetName("Reference worked example: " + pair.Key);
            }
        }

        [TestCaseSource(nameof(WorkedExamples))]
        public void EveryNewNumericWorkedExampleExecutesThroughTheActualTslEvaluator(string code, double expected, double tolerance)
        {
            GSLInitializer.Initialize();
            var function = new ExpressionsEvaluator().Evaluate(code, "", CalculationsMode.Real);
            var actual = Convert.ToDouble(function.Evaluate(0.0), CultureInfo.InvariantCulture);
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(tolerance), code);
        }
    }
}
