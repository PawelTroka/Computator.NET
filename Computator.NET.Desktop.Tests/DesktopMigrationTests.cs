using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Computator.NET.Core.Natives;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Views;
using Computator.NET.Localization;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DesktopMigrationTests
    {
        [Test]
        public void MainWindowAndBothEditorsResolveAndCreateANativeHandle()
        {
            GSLInitializer.Initialize();
            var bootstrapper = new WinFormsBootstrapper();
            using (bootstrapper.Container)
            using (var window = bootstrapper.Create<MainView>())
            {
                var scripting = bootstrapper.Create<ScriptingView>();
                var custom = bootstrapper.Create<CustomFunctionsView>();
                NUnit.Framework.Assert.That(scripting.CodeEditorView, Is.Not.Null);
                NUnit.Framework.Assert.That(custom.CustomFunctionsEditor, Is.Not.Null);
                NUnit.Framework.Assert.That(scripting.CodeEditorView, Is.Not.SameAs(custom.CustomFunctionsEditor));
                NUnit.Framework.Assert.That(window.Handle, Is.Not.EqualTo(IntPtr.Zero));
                NUnit.Framework.Assert.That(window.Icon, Is.Not.Null);
            }
        }

        [TestCase("pl", "Błąd składni")]
        [TestCase("de", "Syntax Fehler")]
        [TestCase("cs", "Chyba syntaxe")]
        public void SatelliteResourcesKeepTheirOriginalNames(string culture, string expected)
        {
            NUnit.Framework.Assert.That(Strings.ResourceManager.GetString("Syntax_error",
                CultureInfo.GetCultureInfo(culture)), Is.EqualTo(expected));
        }

        [Test]
        public void CompilerAndEditorDataAreCopiedTransitively()
        {
            foreach (var file in new[] { "Static/functions.xml", "Static/TSL-Syntax.xshd", "CHANGELOG" })
                NUnit.Framework.Assert.That(File.Exists(Path.Combine(AppContext.BaseDirectory, file)),
                    Is.True, "Missing runtime content: " + file);
        }
    }
}
