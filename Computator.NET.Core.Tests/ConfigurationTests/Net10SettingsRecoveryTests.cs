using System;
using System.IO;
using System.Text.Json;
using Computator.NET.Core.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ConfigurationTests
{
    [TestFixture]
    public class Net10SettingsRecoveryTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitResetPreservesRejectedJsonAndLegacyBytes(bool futureSchema)
        {
            var directory = Path.Combine(Path.GetTempPath(), "Computator-Reset-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var legacy = Path.Combine(directory, "settings.dat");
                var document = new SettingsDocument
                {
                    WorkingDirectory = directory, ScriptingDirectory = directory, CustomFunctionsDirectory = directory
                };
                var previous = futureSchema ? JsonSerializer.Serialize(new { SchemaVersion = 99 }) : "{broken-json";
                File.WriteAllText(SettingsPersistence.JsonPath(legacy), previous);
                var legacyBytes = new byte[] { 5, 4, 3, 2, 1 };
                File.WriteAllBytes(legacy, legacyBytes);
                NUnit.Framework.Assert.That(() => SettingsPersistence.Save(legacy, document), Throws.Exception);
                SettingsPersistence.SaveAfterExplicitReset(legacy, document);
                NUnit.Framework.Assert.That(SettingsPersistence.Load(legacy).SchemaVersion, Is.EqualTo(1));
                var backups = Directory.GetFiles(directory, "settings.json.backup-*");
                NUnit.Framework.Assert.That(backups, Has.Length.EqualTo(1));
                NUnit.Framework.Assert.That(File.ReadAllText(backups[0]), Is.EqualTo(previous));
                NUnit.Framework.Assert.That(File.ReadAllBytes(legacy), Is.EqualTo(legacyBytes));
                NUnit.Framework.Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
                document.Language = "de-DE";
                SettingsPersistence.Save(legacy, document);
                NUnit.Framework.Assert.That(SettingsPersistence.Load(legacy).Language, Is.EqualTo("de-DE"));
            }
            finally { Directory.Delete(directory, true); }
        }

        [Test]
        public void ExplicitResetStillRejectsAnInvalidReplacement()
        {
            var directory = Path.Combine(Path.GetTempPath(), "Computator-Reset-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var legacy = Path.Combine(directory, "settings.dat");
                File.WriteAllText(SettingsPersistence.JsonPath(legacy), "original");
                NUnit.Framework.Assert.That(() => SettingsPersistence.SaveAfterExplicitReset(legacy, new SettingsDocument { SchemaVersion = 99 }), Throws.Exception);
                NUnit.Framework.Assert.That(File.ReadAllText(SettingsPersistence.JsonPath(legacy)), Is.EqualTo("original"));
                NUnit.Framework.Assert.That(Directory.GetFiles(directory).Length, Is.EqualTo(1));
            }
            finally { Directory.Delete(directory, true); }
        }
    }
}
