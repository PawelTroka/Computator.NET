using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using File = System.IO.File;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.DataTypes.Utility;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable, Apartment(ApartmentState.STA)]
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

        private static IEnumerable<(string Code, string Editor, string Result)> ReadWorkedExamples(IEnumerable<FunctionInfo> entries)
        {
            foreach (var entry in entries)
                foreach (var example in XElement.Parse(entry.Description).Descendants("div").Where(div => (string)div.Attribute("class") == "reference-worked-example"))
                {
                    var code = example.Element("pre")?.Element("code")?.Value;
                    if (string.IsNullOrWhiteSpace(code)) throw new InvalidDataException("Empty worked example: " + entry.Signature);
                    var contexts = example.Elements("p").Where(p => (string)p.Attribute("class") == "reference-example-context").Select(p => p.Value).ToArray();
                    if (contexts.Length > 1 || (contexts.Length == 1 && contexts[0] != "Scripting editor"))
                        throw new InvalidDataException("Unknown worked-example editor: " + entry.Signature);
                    var editor = contexts.Length == 0 ? "expression" : "scripting";
                    var result = example.Elements("p").First(p => p.Element("strong")?.Value == "Result:").Value.Substring("Result:".Length).Trim();
                    yield return (code, editor, result);
                }
        }

        private static FunctionInfo ExampleEntry(string code, string context)
        {
            var example = new XElement("div", new XAttribute("class", "reference-worked-example"),
                new XElement("pre", new XElement("code", code)),
                new XElement("p", new XElement("strong", "Result:"), " 5"));
            if (context != null) example.AddFirst(new XElement("p", new XAttribute("class", "reference-example-context"), context));
            return new FunctionInfo { Description = new XElement("div", example).ToString(SaveOptions.DisableFormatting) };
        }

        [TestCase(null, "expression", "gamma(5)")]
        [TestCase("Scripting editor", "scripting", "writeln(det(matrix({{3,1},{1,2}})));")]
        public void ExampleReaderPreservesTheRequiredEditorAndCompleteDisplayedCode(string context, string editor, string code)
        {
            var example = ReadWorkedExamples(new[] { ExampleEntry(code, context) }).Single();
            NUnit.Framework.Assert.That(example.Editor, Is.EqualTo(editor));
            NUnit.Framework.Assert.That(example.Code, Is.EqualTo(code), "Execute the text readers can copy, without adding hidden imports or statements.");
        }

        [TestCase("")]
        [TestCase("Console")]
        [TestCase("scripting")]
        public void ExampleReaderRejectsUnknownContextInsteadOfGuessingAnEvaluator(string context)
        {
            NUnit.Framework.Assert.Throws<InvalidDataException>(() => ReadWorkedExamples(new[] { ExampleEntry("1", context) }).ToArray());
        }

        [Test]
        public void DesktopRenderingRetainsScriptingInstructionsAndRunnableCode()
        {
            const string code = "writeln(det(matrix({{3,1},{1,2}})));";
            var rendered = FunctionReferenceDocument.Render(ExampleEntry(code, "Scripting editor"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("Scripting editor"));
            NUnit.Framework.Assert.That(rendered, Does.Contain("<code>" + code + "</code>"));
        }

        [Test]
        public void ShippedMatrixExamplesIdentifyTheScriptingEditor()
        {
            var examples = ReadWorkedExamples(new FunctionsDetailsFileSource().Details.Values.Where(entry => entry.Type == "MatrixFunctions")).ToArray();
            NUnit.Framework.Assert.That(examples.Length, Is.GreaterThanOrEqualTo(42));
            foreach (var example in examples)
            {
                NUnit.Framework.Assert.That(example.Editor, Is.EqualTo("scripting"), example.Code);
                NUnit.Framework.Assert.That(example.Code, Does.StartWith("writeln(").And.EndWith(");"));
            }
        }

        public static IEnumerable<TestCaseData> WorkedExamples()
        {
            var examples = new Dictionary<(string Editor, string Code), string>();
            foreach (var example in ReadWorkedExamples(new FunctionsDetailsFileSource().Details.Values))
            {
                var key = (example.Editor, example.Code);
                if (examples.TryGetValue(key, out var prior) && prior != example.Result)
                    throw new InvalidDataException("Conflicting example answers: " + example.Code);
                examples[key] = example.Result;
            }
            if (examples.Count < 65) throw new InvalidDataException("Too few executable worked examples.");
            foreach (var pair in examples.OrderBy(pair => pair.Key.Editor, StringComparer.Ordinal).ThenBy(pair => pair.Key.Code, StringComparer.Ordinal))
            {
                var text = pair.Value.Replace("approximately ", "");
                var expected = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                // Use a fixed relative budget of 1e-9, an absolute budget of 1e-12
                // for zero, and a 1e-300 absolute floor for nonzero answers.
                // The number of displayed decimal digits does not change this budget.
                var tolerance = Math.Max(Math.Abs(expected) * 1e-9, expected == 0 ? 1e-12 : 1e-300);
                yield return new TestCaseData(pair.Key.Editor, pair.Key.Code, expected, tolerance).SetName("Reference worked example (" + pair.Key.Editor + "): " + pair.Key.Code);
            }
        }

        [TestCaseSource(nameof(WorkedExamples))]
        public void EveryNewNumericWorkedExampleExecutesThroughTheActualTslEvaluator(string editor, string code, double expected, double tolerance)
        {
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previousNotation = settings.NumericalOutputNotation;
            var previousWorkingDirectory = settings.WorkingDirectory;
            var previousDirectory = Directory.GetCurrentDirectory();
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
                settings.NumericalOutputNotation = NumericalOutputNotationType.EngineeringNotation;
                settings.WorkingDirectory = previousDirectory;
                GSLInitializer.Initialize();
                double actual;
                if (editor == "expression")
                {
                    var function = new ExpressionsEvaluator().Evaluate(code, "", CalculationsMode.Real);
                    actual = Convert.ToDouble(function.Evaluate(0.0), CultureInfo.InvariantCulture);
                }
                else if (editor == "scripting")
                {
                    var output = new StringBuilder();
                    var script = new ScriptEvaluator().Evaluate(code, "");
                    script.Evaluate(value => output.Append(value));
                    NUnit.Framework.Assert.That(double.TryParse(output.ToString().Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out actual),
                        Is.True, "Expected one numeric scripting output from " + code + ": " + output);
                }
                else throw new InvalidDataException("Unsupported worked-example editor: " + editor);
                NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(tolerance), code);
            }
            finally
            {
                Directory.SetCurrentDirectory(previousDirectory);
                settings.WorkingDirectory = previousWorkingDirectory;
                settings.NumericalOutputNotation = previousNotation;
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }
    }
}
