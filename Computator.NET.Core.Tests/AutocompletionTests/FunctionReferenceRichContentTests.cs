using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using File = System.IO.File;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        // These use HTML (not an XML parser) for the document shell; isolate the
        // XML-compatible fragment when examining the retained code metadata.
        private static string[] VisibleSignatures(string document) => Regex.Matches(document,
            "<p class=\"reference-signature\">(.*?)</p>", RegexOptions.Singleline)
            .SelectMany(match => XElement.Parse(match.Value).Elements("code").Select(code => code.Value)).ToArray();

        [TestCase(null, 2)]
        [TestCase("real Probe(real x)", 1)]
        [TestCase("real   Probe(real x)", 1)]
        [TestCase("Probe(x)", 2)]
        [TestCase("REAL Probe(real x)", 2)]
        public void OnlyAnExactlyRepeatedVisibleSignatureIsRemoved(string context, int expected)
        {
            var entry = new FunctionInfo { Title = "Probe", Description =
                "<div><p class=\"reference-signature\"><code>real Probe(real x)</code><br /><code>complex Probe(complex x)</code></p><p>Meaning.</p></div>" };
            var retained = VisibleSignatures(FunctionReferenceDocument.Render(entry, context));
            NUnit.Framework.Assert.That(retained.Length, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(retained, Does.Contain("complex Probe(complex x)"));
        }

        [Test]
        public void RemovingTheOnlyRepeatedSignatureDoesNotLeaveAnEmptySection()
        {
            var entry = new FunctionInfo { Title = "Probe", Description =
                "<div><p class=\"reference-signature\"><code>real Probe(real x)</code></p><p>Meaning.</p></div>" };
            var rendered = FunctionReferenceDocument.Render(entry, "real Probe(real x)");
            NUnit.Framework.Assert.That(rendered, Does.Not.Contain("<p class=\"reference-signature\">"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("Meaning."));
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
                var textHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(item.GetProperty("tex").GetString()))).ToLowerInvariant();
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
                foreach (var image in XElement.Parse(entry.Description).Descendants("img"))
                {
                    var id = Path.GetFileNameWithoutExtension((string)image.Attribute("src"));
                    NUnit.Framework.Assert.That(ids, Does.Contain(id), entry.Signature);
                    NUnit.Framework.Assert.That((int)image.Attribute("width"), Is.EqualTo(equations.GetProperty(id).GetProperty("width").GetInt32()));
                    NUnit.Framework.Assert.That((int)image.Attribute("height"), Is.EqualTo(equations.GetProperty(id).GetProperty("height").GetInt32()));
                    used.Add(id);
                }
            NUnit.Framework.Assert.That(used.OrderBy(x => x), Is.EqualTo(ids.OrderBy(x => x)), "No stale or unused equation assets.");
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
