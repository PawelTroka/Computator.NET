using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Encodings.Web;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Constants;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture]
    public class FunctionReferenceCoverageTests
    {
        [Test]
        public void ExportRuntimeCatalogForReview()
        {
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());
            var items = provider.ScriptingAutocompleteItems.Select(item => new {
                signature = item.Text, menu = item.MenuText, type = item.Details.Type,
                title = item.Details.Title, category = item.Details.Category
            }).ToArray();
            var constantTypes = new[] { typeof(MathematicalConstants), typeof(PhysicalConstants) }
                .SelectMany(type => new[] { type }.Concat(type.GetNestedTypes()));
            var constants = constantTypes.SelectMany(type =>
                type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(field => field.Name != "ToCode")
                .Select(field => new { name = field.Name, declaringType = type.FullName,
                    value = Convert.ToString(field.GetValue(null), CultureInfo.InvariantCulture) })).ToArray();
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "function-reference-inventory.json");
            File.WriteAllText(path, JsonSerializer.Serialize(new { items, constants }, new JsonSerializerOptions {
                WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            TestContext.AddTestAttachment(path, "Runtime-discovered function and constant inventory");
            NUnit.Framework.Assert.That(items.Length, Is.GreaterThan(100));
        }
    }
}
