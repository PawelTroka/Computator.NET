using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using Computator.NET.Core.Configuration;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;
namespace Computator.NET.Core.Tests.ConfigurationTests
{
    [TestFixture]
    public class Net10SettingsMigrationTests
    {
        private string _directory;
        private string LegacyPath => Path.Combine(_directory, "settings.dat");
        [SetUp] public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), "Computator-Settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }
        [TearDown] public void Cleanup() { Directory.Delete(_directory, true); }
        private SettingsDocument Sample() => new SettingsDocument
        {
            Language = "pl-PL", CodeEditor = CodeEditorType.AvalonEdit,
            ScriptingDirectory = Path.Combine(_directory, "scripts"),
            CustomFunctionsDirectory = Path.Combine(_directory, "custom"),
            WorkingDirectory = Path.Combine(_directory, "workspace")
        };
        [Test] public void RuntimeIsActuallyNet10()
        {
            NUnit.Framework.Assert.That(Environment.Version.Major, Is.EqualTo(10));
            NUnit.Framework.Assert.That(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, Does.StartWith(".NET 10."));
        }
        [Test] public void NewProfileHasNoPersistedSettings()
        {
            NUnit.Framework.Assert.That(SettingsPersistence.Load(LegacyPath), Is.Null);
            NUnit.Framework.Assert.That(File.Exists(LegacyPath), Is.False);
        }
        [Test] public void JsonRoundTripPreservesAllFields()
        {
            var original = Sample();
            original.ExpressionFont = new FontDocument { Name = "Cambria", Size = 17.5f, Style = FontStyle.Bold };
            SettingsPersistence.Save(LegacyPath, original);
            NUnit.Framework.Assert.That(JsonSerializer.Serialize(SettingsPersistence.Load(LegacyPath)), Is.EqualTo(JsonSerializer.Serialize(original)));
            NUnit.Framework.Assert.That(File.Exists(LegacyPath), Is.False);
        }
        [Test] public void ReplacementBacksUpThePreviousJson()
        {
            var original = Sample();
            SettingsPersistence.Save(LegacyPath, original);
            var previous = File.ReadAllBytes(SettingsPersistence.JsonPath(LegacyPath));
            original.Language = "de-DE";
            SettingsPersistence.Save(LegacyPath, original);
            NUnit.Framework.Assert.That(SettingsPersistence.Load(LegacyPath).Language, Is.EqualTo("de-DE"));
            var backups = Directory.GetFiles(_directory, "settings.json.backup-*");
            NUnit.Framework.Assert.That(backups, Has.Length.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllBytes(backups[0]), Is.EqualTo(previous));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory, "*.tmp"), Is.Empty);
        }
        [Test] public void CorruptLegacyDataIsPreserved()
        {
            var bytes = new byte[] { 12, 34, 56, 78 };
            File.WriteAllBytes(LegacyPath, bytes);
            NUnit.Framework.Assert.That(() => SettingsPersistence.Load(LegacyPath), Throws.Exception);
            NUnit.Framework.Assert.That(File.ReadAllBytes(LegacyPath), Is.EqualTo(bytes));
            NUnit.Framework.Assert.That(File.Exists(SettingsPersistence.JsonPath(LegacyPath)), Is.False);
        }
        [Test] public void CorruptJsonCannotBeSilentlyOverwritten()
        {
            var path = SettingsPersistence.JsonPath(LegacyPath);
            File.WriteAllText(path, "{ invalid json");
            NUnit.Framework.Assert.That(() => SettingsPersistence.Load(LegacyPath), Throws.Exception);
            NUnit.Framework.Assert.That(() => SettingsPersistence.Save(LegacyPath, Sample()), Throws.Exception);
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo("{ invalid json"));
        }
        [Test] public void FutureSchemaIsRejectedAndPreserved()
        {
            var document = Sample(); document.SchemaVersion = 99;
            var json = JsonSerializer.Serialize(document);
            File.WriteAllText(SettingsPersistence.JsonPath(LegacyPath), json);
            NUnit.Framework.Assert.That(() => SettingsPersistence.Load(LegacyPath), Throws.TypeOf<InvalidDataException>());
            NUnit.Framework.Assert.That(() => SettingsPersistence.Save(LegacyPath, Sample()), Throws.TypeOf<InvalidDataException>());
            NUnit.Framework.Assert.That(File.ReadAllText(SettingsPersistence.JsonPath(LegacyPath)), Is.EqualTo(json));
        }
        [Test] public void OversizedPayloadIsRejectedBeforeParsing()
        {
            using (var stream = File.Create(LegacyPath)) stream.SetLength(SettingsPersistence.MaximumBytes + 1L);
            NUnit.Framework.Assert.That(() => SettingsPersistence.Load(LegacyPath), Throws.TypeOf<InvalidDataException>());
        }
        [TestCase(-1)] [TestCase(900)] public void InvalidEnumsCannotBeSaved(int value)
        {
            var document = Sample(); document.CodeEditor = (CodeEditorType)value;
            NUnit.Framework.Assert.That(() => SettingsPersistence.Save(LegacyPath, document), Throws.TypeOf<InvalidDataException>());
        }
        [Test] public void RealFrameworkPayloadImportsWithoutChangingOriginal()
        {
            var fixture = Environment.GetEnvironmentVariable("COMPUTATOR_LEGACY_FIXTURE");
            NUnit.Framework.Assert.That(fixture, Is.Not.Null.And.Not.Empty, "Run build/Validate-Net10.ps1 to generate the Framework fixture.");
            File.Copy(fixture, LegacyPath);
            var bytes = File.ReadAllBytes(LegacyPath);
            var document = SettingsPersistence.Load(LegacyPath);
            NUnit.Framework.Assert.That(document.Language, Is.EqualTo("pl-PL"));
            NUnit.Framework.Assert.That(document.CodeEditor, Is.EqualTo(CodeEditorType.AvalonEdit));
            NUnit.Framework.Assert.That(document.CalculationsErrors, Is.EqualTo(CalculationsErrors.ShowError));
            NUnit.Framework.Assert.That(document.NumericalOutputNotation, Is.EqualTo(NumericalOutputNotationType.EngineeringNotation));
            NUnit.Framework.Assert.That(document.FunctionsOrder, Is.EqualTo(FunctionsOrder.Alphabetical));
            NUnit.Framework.Assert.That(document.TooltipType, Is.EqualTo(TooltipType.Form));
            NUnit.Framework.Assert.That(document.ExpressionFont.Size, Is.EqualTo(18.25f));
            NUnit.Framework.Assert.That(document.ExpressionFont.Style, Is.EqualTo(FontStyle.Bold | FontStyle.Underline));
            NUnit.Framework.Assert.That(document.ScriptingFont.Size, Is.EqualTo(14.5f));
            NUnit.Framework.Assert.That(document.ScriptingFont.Style, Is.EqualTo(FontStyle.Italic));
            NUnit.Framework.Assert.That(document.ScriptingDirectory, Is.EqualTo(@"C:\fixture\scripts"));
            NUnit.Framework.Assert.That(document.CustomFunctionsDirectory, Is.EqualTo(@"C:\fixture\custom functions"));
            NUnit.Framework.Assert.That(document.WorkingDirectory, Is.EqualTo(@"C:\fixture\workspace"));
            NUnit.Framework.Assert.That(document.ShowParametersTypeInScripting, Is.False);
            SettingsPersistence.Save(LegacyPath, document);
            NUnit.Framework.Assert.That(File.ReadAllBytes(LegacyPath), Is.EqualTo(bytes));
            NUnit.Framework.Assert.That(SettingsPersistence.Load(LegacyPath).Language, Is.EqualTo("pl-PL"));
        }
    }
}
