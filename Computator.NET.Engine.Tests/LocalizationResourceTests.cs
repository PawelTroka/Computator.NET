using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class LocalizationResourceTests
{
    [TestCase("de")]
    [TestCase("cs")]
    public void UiTranslationsAreCompleteAndPreserveResourceContracts(string culture)
    {
        var directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Localization");
        var neutralFiles = Directory.GetFiles(directory, "*.resx", SearchOption.AllDirectories)
            .Where(path => !Path.GetFileNameWithoutExtension(path).Contains('.')).ToArray();
        NUnit.Framework.Assert.That(neutralFiles, Is.Not.Empty, "The shipped neutral resources must be copied into the test output.");

        NUnit.Framework.Assert.Multiple(() =>
        {
            foreach (var neutralFile in neutralFiles)
            {
                var localizedFile = Path.ChangeExtension(neutralFile, culture + ".resx");
                NUnit.Framework.Assert.That(File.Exists(localizedFile), Is.True, localizedFile);
                if (!File.Exists(localizedFile)) continue;

                var neutral = ReadStrings(neutralFile);
                var localized = ReadStrings(localizedFile);
                NUnit.Framework.Assert.That(neutral.Keys.Except(localized.Keys), Is.Empty,
                    $"{localizedFile}: translations must not rely on English fallback.");

                foreach (var (key, source) in neutral)
                {
                    if (!localized.TryGetValue(key, out var target)) continue;
                    var context = $"{localizedFile}: {key}";
                    if (!string.IsNullOrWhiteSpace(source.Value))
                        NUnit.Framework.Assert.That(target.Value, Is.Not.Empty.And.Not.WhiteSpace, context);

                    // Format parameters may change order, but must not be lost,
                    // duplicated or turned into malformed composite formats.
                    var parameters = FormatParameters(source.Value);
                    NUnit.Framework.Assert.That(FormatParameters(target.Value), Is.EquivalentTo(parameters), context);
                    if (parameters.Length > 0)
                        NUnit.Framework.Assert.That(() => CompositeFormat.Parse(target.Value), Throws.Nothing, context);

                    if (source.Comment.Contains("@Invariant", StringComparison.Ordinal))
                        NUnit.Framework.Assert.That(target.Value, Is.EqualTo(source.Value), context);

                    if (Path.GetFileName(neutralFile) == "MenuStrings.resx")
                    {
                        NUnit.Framework.Assert.That(Mnemonics(target.Value),
                            Is.EqualTo(Mnemonics(source.Value)), context + ": keyboard mnemonic");
                    }

                    if (key.EndsWith("_Filter", StringComparison.Ordinal))
                    {
                        var sourceParts = source.Value.Split('|');
                        var targetParts = target.Value.Split('|');
                        NUnit.Framework.Assert.That(targetParts.Length, Is.EqualTo(sourceParts.Length), context);
                        NUnit.Framework.Assert.That(targetParts.Where((_, index) => index % 2 == 1),
                            Is.EqualTo(sourceParts.Where((_, index) => index % 2 == 1)), context + ": file patterns");
                    }

                    NUnit.Framework.Assert.That(EmailAddresses(target.Value), Is.EqualTo(EmailAddresses(source.Value)),
                        context + ": contact addresses must match the neutral resource");
                }
            }
        });
    }

    private static Dictionary<string, ResourceString> ReadStrings(string path) =>
        XDocument.Load(path).Root!.Elements("data")
            .Where(element => element.Attribute("type") == null && element.Attribute("mimetype") == null)
            .ToDictionary(element => (string)element.Attribute("name")!,
                element => new ResourceString((string?)element.Element("value") ?? "", (string?)element.Element("comment") ?? ""));

    private static string[] FormatParameters(string value) =>
        Regex.Matches(value, @"(?<!\{)\{(\d+)(?:,[^}:]+)?(?::[^}]+)?\}(?!\})")
            .Select(match => match.Groups[1].Value).ToArray();

    private static int Mnemonics(string value) => Regex.Matches(value.Replace("&&", ""), "&.").Count;

    private static string[] EmailAddresses(string value) =>
        Regex.Matches(value, @"[\w.+-]+@[\w.-]+\.\w+").Select(match => match.Value).ToArray();

    private sealed record ResourceString(string Value, string Comment);
}
