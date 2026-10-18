using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Constants;
using Computator.NET.Core.Properties;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class FunctionReferenceCoverageTests
    {
        private sealed class EmptyReference : IFunctionsDetailsFileSource
        {
            public IDictionary<string, FunctionInfo> Details { get; } = new Dictionary<string, FunctionInfo>();
        }
        private sealed record Entry(string signature, string menu, string type, string title, string category);
        private static Entry[] Catalog()
        {
            var settings = Settings.Default;
            var oldReturn = settings.ShowReturnTypeInScripting;
            var oldParameters = settings.ShowParametersTypeInScripting;
            try
            {
                settings.ShowReturnTypeInScripting = settings.ShowParametersTypeInScripting = true;
                // Required coverage comes from reflection, NOT the XML being tested.
                var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new EmptyReference());
                return provider.ScriptingAutocompleteItems.Select(item => new Entry(item.Text, item.MenuText,
                    item.Details.Type, item.Details.Title, item.Details.Category)).ToArray();
            }
            finally
            {
                settings.ShowReturnTypeInScripting = oldReturn;
                settings.ShowParametersTypeInScripting = oldParameters;
            }
        }
        private static IEnumerable<FieldInfo> ConstantFields() =>
            new[] { typeof(MathematicalConstants), typeof(PhysicalConstants) }
                .SelectMany(type => new[] { type }.Concat(type.GetNestedTypes()))
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
                .Where(field => field.Name != "ToCode");

        [Test]
        public void ExportRuntimeCatalogForReview()
        {
            var items = Catalog();
            var constants = ConstantFields().Select(field => new { name = field.Name,
                declaringType = field.DeclaringType.FullName,
                value = Convert.ToString(field.GetValue(null), CultureInfo.InvariantCulture) }).ToArray();
            var path = Environment.GetEnvironmentVariable("COMPUTATOR_REFERENCE_INVENTORY") ??
                Path.Combine(TestContext.CurrentContext.WorkDirectory, "function-reference-inventory.json");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path, JsonSerializer.Serialize(new { items, constants }, new JsonSerializerOptions
                { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            TestContext.AddTestAttachment(path, "Unaugmented runtime function/constant inventory");
            NUnit.Framework.Assert.That(items.Length, Is.GreaterThanOrEqualTo(1294), "Unexpected catalog shrinkage");
        }

        [Test]
        public void EveryNonKeywordHasAnExactReferenceAndThereAreNoOrphanEntries()
        {
            var expected = Catalog().Where(item => item.type != "Keyword").Select(item => item.signature).Distinct().ToArray();
            var actual = new FunctionsDetailsFileSource().Details;
            NUnit.Framework.Assert.That(expected.Length, Is.GreaterThanOrEqualTo(1220));
            NUnit.Framework.Assert.That(actual.Keys.OrderBy(value => value, StringComparer.Ordinal),
                Is.EqualTo(expected.OrderBy(value => value, StringComparer.Ordinal)));
        }

        [Test]
        public void EveryTypedOverloadIsPresentInItsSharedLookupEntry()
        {
            var reference = new FunctionsDetailsFileSource().Details;
            foreach (var item in Catalog().Where(item => item.type != "Keyword"))
            {
                var detail = reference[item.signature];
                var fragment = XElement.Parse(detail.Description);
                var syntaxes = fragment.Elements("p").Where(p => (string)p.Attribute("class") == "reference-signature")
                    .Elements("code").Select(code => code.Value).ToArray();
                NUnit.Framework.Assert.That(syntaxes, Does.Contain(item.menu), item.signature);
                NUnit.Framework.Assert.That(detail.Type, Is.EqualTo(item.type), item.signature);
                NUnit.Framework.Assert.That(detail.Signature, Is.EqualTo(item.signature));
            }
        }

        [Test]
        public void AllDescriptionsAreSelfContainedStructuredHtmlWithoutPlaceholders()
        {
            var tags = new HashSet<string> { "div", "p", "strong", "b", "code", "pre", "br", "a", "ul", "li", "sup", "sub", "img" };
            foreach (var pair in new FunctionsDetailsFileSource().Details)
            {
                var detail = pair.Value;
                NUnit.Framework.Assert.That(string.IsNullOrWhiteSpace(detail.Title), Is.False, pair.Key);
                NUnit.Framework.Assert.That(string.IsNullOrWhiteSpace(detail.Category), Is.False, pair.Key);
                NUnit.Framework.Assert.That(Uri.TryCreate(detail.Url, UriKind.Absolute, out var source) && source.Scheme == "https", Is.True, pair.Key);
                var fragment = XElement.Parse(detail.Description, LoadOptions.PreserveWhitespace);
                NUnit.Framework.Assert.That(fragment.Name.LocalName, Is.EqualTo("div"));
                NUnit.Framework.Assert.That((string)fragment.Attribute("class"), Is.EqualTo("function-reference"));
                foreach (var node in fragment.DescendantsAndSelf())
                {
                    NUnit.Framework.Assert.That(tags.Contains(node.Name.ToString()), Is.True, pair.Key + ": " + node.Name);
                    NUnit.Framework.Assert.That(node.Attributes().All(a => a.Name == "class" || a.Name == "href" || (node.Name == "img" && new[] { "src", "alt", "width", "height" }.Contains(a.Name.ToString()))), Is.True, pair.Key);
                    if (node.Name == "img")
                    {
                        NUnit.Framework.Assert.That((string)node.Attribute("src"), Does.Match(@"\Areference-equations/[a-f0-9]{64}\.png\z"));
                        NUnit.Framework.Assert.That((string)node.Attribute("class"), Is.EqualTo("reference-math"));
                        NUnit.Framework.Assert.That((string)node.Attribute("alt"), Is.Not.Null.And.Not.Empty);
                        NUnit.Framework.Assert.That(int.Parse((string)node.Attribute("width")), Is.InRange(1, 2400));
                        NUnit.Framework.Assert.That(int.Parse((string)node.Attribute("height")), Is.InRange(1, 800));
                    }
                    if (node.Name == "a")
                        NUnit.Framework.Assert.That(Uri.TryCreate((string)node.Attribute("href"), UriKind.Absolute, out var href) && href.Scheme == "https", Is.True, pair.Key);
                }
                foreach (var section in new[] { "reference-summary", "reference-arguments", "reference-source" })
                    NUnit.Framework.Assert.That(fragment.Elements("p").Any(p => (string)p.Attribute("class") == section && !string.IsNullOrWhiteSpace(p.Value)), Is.True, pair.Key + ": " + section);
                NUnit.Framework.Assert.That(Regex.IsMatch(fragment.Value, @"not done yet|_title_|\bTODO\b|\bTBD\b|lorem ipsum", RegexOptions.IgnoreCase), Is.False, pair.Key);
                NUnit.Framework.Assert.That(Regex.Matches(fragment.Value, @"\S+").Count, Is.GreaterThanOrEqualTo(20), pair.Key);
            }
        }

        [Test]
        public void ConstantsDescribeTheActualStoredValuesRatherThanSilentlySubstitutingNewOnes()
        {
            var reference = new FunctionsDetailsFileSource().Details;
            foreach (var field in ConstantFields())
            {
                var value = Convert.ToString(field.GetValue(null), CultureInfo.InvariantCulture);
                NUnit.Framework.Assert.That(XElement.Parse(reference[field.Name].Description).Value,
                    Does.Contain("Stored application value: " + value + "."), field.Name);
            }
            NUnit.Framework.Assert.That(XElement.Parse(reference["i"].Description).Value, Does.Contain("i²=−1"));
            NUnit.Framework.Assert.That(XElement.Parse(reference["g"].Description).Value, Does.Contain("9.80665"));
            NUnit.Framework.Assert.That(XElement.Parse(reference["ElementaryCharge"].Description).Value, Does.Contain("1.602176634"));
        }

        [Test]
        public void BothExpressionAndScriptingCompletionsAttachTheRenderedReference()
        {
            var reference = new FunctionsDetailsFileSource();
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), reference);
            foreach (var item in provider.ExpressionAutocompleteItems.Concat(provider.ScriptingAutocompleteItems)
                .Where(item => item.Details.Type != "Keyword"))
                NUnit.Framework.Assert.That(item.Details, Is.SameAs(reference.Details[item.Text]), item.Text);
        }

        [Test]
        public void EasilyConfusedConventionsHaveExplicitWarnings()
        {
            var reference = new FunctionsDetailsFileSource().Details;
            void Contains(string signature, string expected) => NUnit.Framework.Assert.That(
                XElement.Parse(reference[signature].Description).Value, Does.Contain(expected), signature);
            Contains("sinc(x)", "NOT sin(PI");
            Contains("BetaNorm(x,a,b)", "gsl_sf_beta_inc(x,a,b)");
            Contains("WeakMixingAngle", "NOT θW");
            Contains("SilverConstant", "NOT equal");
            Contains("ComplementaryTetrahedralAngle", "SUPPLEMENT");
            Contains("Geometric.PMF(p,k)", "INCLUDING the first success");
            Contains("Normal.PDF(mean,stddev,x)", "standard deviation, not the variance");
        }
    }
}
