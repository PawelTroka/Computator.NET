using System;
using System.IO;
using Computator.NET.Core.Configuration;
using Computator.NET.Core.Services;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ConfigurationTests
{
    [TestFixture]
    public class ProfileTrialTests
    {
        [TestCase("C:\\Żółć i fizyka\\Test.TSL", ".tsl", true)]
        [TestCase("C:\\Żółć i fizyka\\Test.TsLf", ".tslf", true)]
        [TestCase("example.tslf", ".tsl", false)]
        [TestCase("example.tsl.exe", ".tsl", false)]
        [TestCase("--expression=example.tsl", ".tsl", false)]
        public void ActivationKeepsWholeUnicodePathsAndMatchesExactExtensions(string file, string extension, bool expected)
        {
            var found = CommandLineHandler.TryGetDocument(new[] { "Computator.NET.exe", file }, extension, out var actual);
            NUnit.Framework.Assert.That(found, Is.EqualTo(expected));
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected ? file : null));
        }

        [Test]
        public void MissingArgumentsAreNotDocuments()
        {
            NUnit.Framework.Assert.That(CommandLineHandler.TryGetDocument(Array.Empty<string>(), ".tsl", out _), Is.False);
            NUnit.Framework.Assert.That(CommandLineHandler.TryGetDocument(null, ".tsl", out _), Is.False);
        }

        [TestCase("settings.json")]
        [TestCase("Settings.JSON")]
        public void TrialRebasesPreferencesAndPreservesOriginalSettingsAndDocuments(string settingsName)
        {
            var root = Path.Combine(Path.GetTempPath(), "Computator-Profile-" + Guid.NewGuid().ToString("N"));
            var source = Path.Combine(root, "source"); var trial = Path.Combine(root, "trial");
            Directory.CreateDirectory(Path.Combine(source, "functions"));
            try
            {
                var settings = new SettingsDocument { WorkingDirectory = source, ScriptingDirectory = source,
                    CustomFunctionsDirectory = Path.Combine(source, "functions"), Language = "pl-PL" };
                SettingsPersistence.Save(Path.Combine(source, "settings.dat"), settings);
                var original = File.ReadAllBytes(Path.Combine(source, "settings.json"));
                // A case-only rename is ambiguous on Windows; use a temporary name.
                File.Move(Path.Combine(source, "settings.json"), Path.Combine(source, "renamed.json"));
                File.Move(Path.Combine(source, "renamed.json"), Path.Combine(source, settingsName));
                File.WriteAllText(Path.Combine(source, "functions", "Żółć.TSLF"), "real f(real x) { return x+1; }");
                File.WriteAllText(Path.Combine(source, "settings.tsl"), "return 42;");
                var inventory = ProfileTrial.Prepare(source, trial);
                NUnit.Framework.Assert.That(inventory.Count, Is.EqualTo(3));
                NUnit.Framework.Assert.That(File.ReadAllBytes(Path.Combine(source, "settings.json")), Is.EqualTo(original));
                var loaded = SettingsPersistence.Load(Path.Combine(trial, "settings.dat"));
                NUnit.Framework.Assert.That(loaded.WorkingDirectory, Is.EqualTo(trial));
                NUnit.Framework.Assert.That(loaded.CustomFunctionsDirectory, Is.EqualTo(Path.Combine(trial, "functions")));
                NUnit.Framework.Assert.That(loaded.Language, Is.EqualTo("pl-PL"));
                NUnit.Framework.Assert.That(File.ReadAllBytes(Path.Combine(trial, settingsName + ".original")), Is.EqualTo(original));
                NUnit.Framework.Assert.That(File.ReadAllText(Path.Combine(trial, "settings.tsl")), Is.EqualTo("return 42;"));
                File.WriteAllText(Path.Combine(trial, "functions", "Żółć.TSLF"), "changed only in trial");
                NUnit.Framework.Assert.That(File.ReadAllText(Path.Combine(source, "functions", "Żółć.TSLF")), Does.StartWith("real f"));
            }
            finally { Directory.Delete(root, true); }
        }

        [Test]
        public void ExternalDirectoriesAndExistingDestinationsAreRejectedBeforeCopying()
        {
            var root = Path.Combine(Path.GetTempPath(), "Computator-Profile-" + Guid.NewGuid().ToString("N"));
            var source = Path.Combine(root, "source"); var target = Path.Combine(root, "trial");
            Directory.CreateDirectory(source);
            try
            {
                SettingsPersistence.Save(Path.Combine(source, "settings.dat"), new SettingsDocument {
                    WorkingDirectory = root, ScriptingDirectory = source, CustomFunctionsDirectory = source });
                NUnit.Framework.Assert.That(() => ProfileTrial.Prepare(source, target), Throws.TypeOf<InvalidDataException>());
                NUnit.Framework.Assert.That(Directory.Exists(target), Is.False);
                NUnit.Framework.Assert.That(() => ProfileTrial.Prepare(source, source), Throws.TypeOf<IOException>());
                NUnit.Framework.Assert.That(() => ProfileTrial.Prepare(source, Path.Combine(source, "nested")), Throws.TypeOf<IOException>());
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
