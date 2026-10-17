using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls.AutocompleteMenu;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ReferenceTooltipWorkflowTests
    {
        private static T Field<T>(object instance, string name) =>
            (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);

        private static AutocompleteListView CreateList() =>
            (AutocompleteListView)Activator.CreateInstance(typeof(AutocompleteListView), nonPublic: true);

        [TestCase(TooltipType.Default)]
        [TestCase(TooltipType.Form)]
        public void SelectingCatalogItemsRendersTheirHtmlInTheActualTooltip(TooltipType mode)
        {
            var originalMode = Settings.Default.TooltipType;
            using var window = new Form { Size = new Size(600, 400), ShowInTaskbar = false };
            using var list = CreateList();
            var popup = Field<ToolStripDropDown>(list, "toolTip");
            var details = Field<Form>(list, "formTip");
            Exception uiError = null;
            ThreadExceptionEventHandler captureError = (_, args) => uiError = args.Exception;
            Application.ThreadException += captureError;
            try
            {
                Settings.Default.TooltipType = mode;
                window.Controls.Add(list);
                var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());
                var catalog = provider.ScriptingAutocompleteItems.ToArray();
                var items = new[] { "sin(x)", "gamma(x)", "PI", "ElementaryCharge", "Normal.CDF(mean,stddev,x)" }
                    .Select(signature => catalog.First(item => item.Text == signature))
                    .Append(catalog.First(item => item.Details.Type == "MatrixFunctions")).ToArray();
                list.MaximumSize = new Size(400, 300);
                list.VisibleItems = items;
                window.Show();
                var browser = mode == TooltipType.Default
                    ? (WebBrowser)popup.Items.OfType<ToolStripControlHost>().Single().Control
                    : details.Controls.OfType<WebBrowser>().Single();
                var navigations = new System.Collections.Generic.List<string>();
                browser.Navigating += (_, args) => navigations.Add(args.Url.Scheme);
                // Selection can change again while MSHTML is still loading a document.
                list.SelectedItemIndex = 2;
                list.CloseToolTip();
                details.Hide();
                Application.DoEvents();
                list.SelectedItemIndex = 1;
                foreach (var item in items)
                {
                    list.SelectedItemIndex = Array.IndexOf(items, item);
                    var timer = Stopwatch.StartNew();
                    while ((browser.ReadyState != WebBrowserReadyState.Complete ||
                            browser.Document?.GetElementsByTagName("h1").Cast<HtmlElement>().FirstOrDefault()?.InnerText != item.Details.Title)
                           && timer.Elapsed < TimeSpan.FromSeconds(15))
                    {
                        Application.DoEvents();
                        if (uiError != null) throw new AssertionException(uiError.ToString());
                        Thread.Sleep(10);
                    }
                    NUnit.Framework.Assert.That(browser.Document?.Body?.InnerText, Is.Not.Null,
                        $"{item.Text}: state={browser.ReadyState}, handle={browser.IsHandleCreated}, visible={browser.Visible}, popup={popup.Visible}, navigations={string.Join(",", navigations)}");
                    NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain(item.Details.Title), item.Text);
                    NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain("Arguments / value"), item.Text);
                    NUnit.Framework.Assert.That(browser.Document.GetElementsByTagName("strong").Count, Is.GreaterThanOrEqualTo(3), item.Text);
                    // Matching completion syntax is deliberately absent from its help;
                    // content, equations and any sibling overloads remain.
                    NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Not.Contain("Syntax"), item.Text);
                    if (item.Details.Description.Contains("reference-math"))
                    {
                        NUnit.Framework.Assert.That(browser.Document.Images.Count, Is.GreaterThan(0), item.Text);
                        foreach (HtmlElement image in browser.Document.Images)
                            NUnit.Framework.Assert.That(image.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
                    }
                    NUnit.Framework.Assert.That(mode == TooltipType.Default ? popup.Visible : details.Visible, Is.True, item.Text);
                    var bounds = mode == TooltipType.Default ? popup.Bounds : details.Bounds;
                    var workArea = Screen.FromControl(browser).WorkingArea;
                    NUnit.Framework.Assert.That(bounds.Width, Is.LessThanOrEqualTo(workArea.Width), item.Text);
                    NUnit.Framework.Assert.That(bounds.Height, Is.LessThanOrEqualTo(workArea.Height), item.Text);
                }
                NUnit.Framework.Assert.That(navigations.All(scheme => scheme == "about"), Is.True);
            }
            finally
            {
                Application.ThreadException -= captureError;
                Settings.Default.TooltipType = originalMode;
                list.CloseToolTip();
                details.Dispose();
                window.Close();
            }
        }

        [Test]
        public void DisposingTheAutocompleteListDisposesBothOwnedReferenceBrowsers()
        {
            var list = CreateList();
            var popup = Field<ToolStripDropDown>(list, "toolTip");
            var details = Field<Form>(list, "formTip");
            var popupBrowser = (WebBrowser)popup.Items.OfType<ToolStripControlHost>().Single().Control;
            var detailsBrowser = details.Controls.OfType<WebBrowser>().Single();
            try
            {
                list.Dispose();
                NUnit.Framework.Assert.That(popup.IsDisposed, Is.True);
                NUnit.Framework.Assert.That(popupBrowser.IsDisposed, Is.True);
                NUnit.Framework.Assert.That(details.IsDisposed, Is.True, "The standalone reference window is owned by the list too.");
                NUnit.Framework.Assert.That(detailsBrowser.IsDisposed, Is.True);
            }
            finally
            {
                list.Dispose();
                details.Dispose();
            }
        }
    }
}
