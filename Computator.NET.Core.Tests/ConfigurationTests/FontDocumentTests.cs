using System;
using System.Drawing;
using Computator.NET.Core.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ConfigurationTests
{
    [TestFixture]
    public class FontDocumentTests
    {
        [TestCase("Noto Serif", "Cambria")]
        [TestCase("Noto Sans Mono", "Consolas")]
        public void MonoDefaultProfilesUseInstalledFontsOrTheMatchingWindowsDefault(string saved, string fallback)
        {
            var document = new FontDocument
            {
                Name = saved, Size = 19.25f, Style = FontStyle.Bold | FontStyle.Italic,
                Unit = GraphicsUnit.Pixel, GdiCharSet = 1
            };
            using (var requested = new Font(saved, document.Size, document.Style, document.Unit))
            using (var windowsDefault = new Font(fallback, document.Size, document.Style, document.Unit))
            using (var restored = document.ToFont())
            {
                var installed = string.Equals(requested.Name, saved, StringComparison.OrdinalIgnoreCase);
                NUnit.Framework.Assert.Multiple(() =>
                {
                    NUnit.Framework.Assert.That(restored.Name, Is.EqualTo(installed ? requested.Name : windowsDefault.Name));
                    NUnit.Framework.Assert.That(restored.Size, Is.EqualTo(document.Size));
                    NUnit.Framework.Assert.That(restored.Style, Is.EqualTo(document.Style));
                    NUnit.Framework.Assert.That(restored.Unit, Is.EqualTo(document.Unit));
                    NUnit.Framework.Assert.That(restored.GdiCharSet, Is.EqualTo(document.GdiCharSet));
                });
            }
        }

        [TestCase("Cambria")]
        [TestCase("Consolas")]
        [TestCase("An intentionally unavailable custom family")]
        public void OtherSavedFontChoicesRetainTheirExistingRestorationBehavior(string saved)
        {
            using (var requested = new Font(saved, 17.5f, FontStyle.Italic, GraphicsUnit.Pixel))
            using (var restored = new FontDocument { Name = saved, Size = 17.5f, Style = FontStyle.Italic, Unit = GraphicsUnit.Pixel }.ToFont())
                NUnit.Framework.Assert.Multiple(() =>
                {
                    NUnit.Framework.Assert.That(restored.Name, Is.EqualTo(requested.Name));
                    NUnit.Framework.Assert.That(restored.Size, Is.EqualTo(requested.Size));
                    NUnit.Framework.Assert.That(restored.Style, Is.EqualTo(requested.Style));
                    NUnit.Framework.Assert.That(restored.Unit, Is.EqualTo(requested.Unit));
                });
        }
    }
}
