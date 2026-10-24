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
                var items = new[] { "sin(x)", "gamma(x)", "PI", "ElementaryCharge", "Normal.CDF(mean,stddev,x)", "Ackley(xi1,xi2, ...)", "MathieuAn(n,q)" }
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
                int? readingWidth = null;
                var maximumHeight = 0;
                foreach (var item in items)
                {
                    list.SelectedItemIndex = Array.IndexOf(items, item);
                    maximumHeight = Math.Max(maximumHeight, browser.Height);
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
                    if (mode == TooltipType.Default)
                    {
                        NUnit.Framework.Assert.That(browser.Width, Is.GreaterThanOrEqualTo(Math.Min(500, workArea.Width - 8)), item.Text);
                        NUnit.Framework.Assert.That(browser.Height, Is.InRange(1, maximumHeight), item.Text);
                        if (readingWidth.HasValue)
                            NUnit.Framework.Assert.That(browser.Width, Is.EqualTo(readingWidth.Value),
                                "Short articles and wide equations must keep the readable viewport width.");
                        readingWidth = browser.Width;
                    }
                    if (item.Text.StartsWith("Ackley("))
                    {
                        var equation = browser.Document.GetElementsByTagName("p").Cast<HtmlElement>()
                            .Single(element => element.GetAttribute("className") == "reference-equation");
                        dynamic rendered = equation.DomElement;
                        dynamic rectangle = rendered.getBoundingClientRect();
                        NUnit.Framework.Assert.That((double)rectangle.top, Is.GreaterThanOrEqualTo(0));
                        NUnit.Framework.Assert.That((double)rectangle.bottom, Is.LessThanOrEqualTo(browser.ClientSize.Height),
                            "Ackley's definition should be visible after its introduction without scrolling.");
                    }
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
        public void ShortReferencesFitTheirContentAfterLongAndRapidlyReplacedSelections()
        {
            var originalMode = Settings.Default.TooltipType;
            using var window = new Form { Size = new Size(600, 400), ShowInTaskbar = false };
            using var list = CreateList();
            var popup = Field<ToolStripDropDown>(list, "toolTip");
            var browser = (WebBrowser)popup.Items.OfType<ToolStripControlHost>().Single().Control;
            var loader = Field<ReferenceDocumentLoader>(popup, "documentLoader");
            var ready = false;
            loader.DocumentReady += (_, _) => ready = true;
            Exception uiError = null;
            ThreadExceptionEventHandler captureError = (_, args) => uiError = args.Exception;
            Application.ThreadException += captureError;
            try
            {
                Settings.Default.TooltipType = TooltipType.Default;
                window.Controls.Add(list);
                list.MaximumSize = new Size(400, 300);
                list.VisibleItems = new[]
                {
                    new AutocompleteItem("ShortReference") { Details = new FunctionInfo
                    {
                        Title = "Short reference",
                        Description = "<div><p>A short explanation with its last line visible.</p></div>"
                    } },
                    new AutocompleteItem("Ackley") { Details = new FunctionsDetailsFileSource().Details["Ackley(xi1,xi2, ...)"] }
                };
                window.Show();
                list.SelectedItemIndex = 0;
                var maximumSize = popup.Size;

                void WaitForReference()
                {
                    var timer = Stopwatch.StartNew();
                    while (!ready && timer.Elapsed < TimeSpan.FromSeconds(15))
                    {
                        Application.DoEvents();
                        if (uiError != null) throw new AssertionException(uiError.ToString());
                        Thread.Sleep(10);
                    }
                    NUnit.Framework.Assert.That(ready, Is.True, "The latest reference did not complete.");
                    NUnit.Framework.Assert.That(popup.Width, Is.EqualTo(maximumSize.Width));
                    NUnit.Framework.Assert.That(Screen.FromControl(browser).WorkingArea.Contains(popup.Bounds), Is.True);
                }

                void AssertShortReference()
                {
                    NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain("Short reference"));
                    NUnit.Framework.Assert.That(popup.Height, Is.LessThan(maximumSize.Height / 2),
                        "A short reference must not retain the long article's blank viewport.");
                    dynamic root = browser.Document.GetElementsByTagName("html")[0].DomElement;
                    dynamic paragraph = browser.Document.GetElementsByTagName("p")[0].DomElement;
                    dynamic bounds = paragraph.getBoundingClientRect();
                    NUnit.Framework.Assert.That((double)root.clientHeight - (double)bounds.bottom, Is.InRange(15.0, 40.0),
                        "Keep a little bottom padding, with no cropped last line or large empty area.");
                    NUnit.Framework.Assert.That((int)root.scrollHeight, Is.LessThanOrEqualTo((int)root.clientHeight + 2));
                }

                WaitForReference();
                AssertShortReference();
                for (var repeat = 0; repeat < 2; repeat++)
                {
                    ready = false;
                    list.SelectedItemIndex = 1;
                    WaitForReference();
                    NUnit.Framework.Assert.That(popup.Size, Is.EqualTo(maximumSize));
                    dynamic root = browser.Document.GetElementsByTagName("html")[0].DomElement;
                    NUnit.Framework.Assert.That((int)root.scrollHeight, Is.GreaterThan((int)root.clientHeight),
                        "Long illustrated references remain scrollable in the bounded viewport.");
                    NUnit.Framework.Assert.That(browser.Document.Images.Cast<HtmlElement>()
                        .Any(image => image.GetAttribute("className") == "reference-plot"), Is.True);

                    ready = false;
                    list.SelectedItemIndex = 0;
                    list.SelectedItemIndex = 1;
                    list.SelectedItemIndex = 0;
                    WaitForReference();
                    AssertShortReference();
                }
            }
            finally
            {
                Application.ThreadException -= captureError;
                Settings.Default.TooltipType = originalMode;
                list.CloseToolTip();
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
