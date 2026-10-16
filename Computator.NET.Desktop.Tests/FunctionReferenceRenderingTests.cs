using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class FunctionReferenceRenderingTests
    {
        [TestCase("gamma(x)", "Gamma function")]
        [TestCase("ElementaryCharge", "1.602176634")]
        [TestCase("Normal.CDF(mean,stddev,x)", "standard deviation")]
        public void ShippedHtmlRendersOfflineInTheDesktopBrowser(string signature, string expected)
        {
            var entry = new FunctionsDetailsFileSource().Details[signature];
            using var window = new Form { Size = new Size(700, 650), ShowInTaskbar = false };
            using var browser = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = false };
            window.Controls.Add(browser);
            var navigations = new System.Collections.Generic.List<string>();
            browser.Navigating += (_, args) => navigations.Add(args.Url.Scheme);
            window.Show();
            browser.DocumentText = FunctionReferenceDocument.Render(entry);
            WaitForText(browser, expected);
            NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain("Arguments / value"));
            NUnit.Framework.Assert.That(browser.Document.Images.Count, Is.Zero);
            NUnit.Framework.Assert.That(navigations.All(scheme => scheme == "about"), Is.True);
            // Check computed styles in the actual Windows HTML engine, rather
            // than merely finding desired numbers in the stylesheet source.
            dynamic body = browser.Document.Body.DomElement;
            dynamic heading = browser.Document.GetElementsByTagName("h1")[0].DomElement;
            NUnit.Framework.Assert.That((string)body.currentStyle.fontSize, Is.EqualTo("18px"));
            NUnit.Framework.Assert.That((string)heading.currentStyle.fontSize, Is.EqualTo("24px"));
            var source = browser.Document.GetElementsByTagName("p").Cast<HtmlElement>()
                .First(element => element.GetAttribute("className") == "reference-source");
            dynamic sourceElement = source.DomElement;
            NUnit.Framework.Assert.That((string)sourceElement.currentStyle.fontSize, Is.EqualTo("15px"));
            window.Close();
        }

        [Test]
        public void LongSignaturesWrapWithoutHorizontalOverflow()
        {
            const string title = "Reference <example> & details";
            var entry = new FunctionInfo
            {
                Title = title,
                Description = "<div class=\"function-reference\"><p class=\"reference-signature\"><code>" +
                    new string('W', 500) + "</code></p></div>"
            };
            using var window = new Form { ClientSize = new Size(360, 480), ShowInTaskbar = false };
            using var browser = new WebBrowser { Dock = DockStyle.Fill };
            window.Controls.Add(browser);
            window.Show();
            browser.DocumentText = FunctionReferenceDocument.Render(entry);
            WaitForText(browser, title);
            NUnit.Framework.Assert.That(browser.Document.Body.ScrollRectangle.Width, Is.LessThanOrEqualTo(browser.ClientSize.Width));
            NUnit.Framework.Assert.That(browser.Document.GetElementsByTagName("example").Count, Is.Zero,
                "The title remains encoded text, not injected markup.");
            window.Close();
        }

        [Test]
        public void DetailsWindowKeepsTheReadersSizeAcrossArticlesAndReopening()
        {
            // Exercise the production internal Form through its public service
            // contract without widening the application's public UI surface.
            var type = Assembly.Load("Computator.NET.Desktop")
                .GetType("Computator.NET.Desktop.Controls.AutocompleteMenu.WebBrowserForm", true);
            using var window = (Form)Activator.CreateInstance(type, true);
            var service = (IShowFunctionDetails)window;
            var browser = window.Controls.OfType<WebBrowser>().Single();
            var entries = new FunctionsDetailsFileSource().Details;
            NUnit.Framework.Assert.That(window.AutoSize, Is.False);
            service.Show(entries["gamma(x)"]);
            WaitForText(browser, "Gamma function");
            window.ClientSize = new Size(720, 520);
            var chosenSize = window.ClientSize;
            service.Show(entries["ElementaryCharge"]);
            WaitForText(browser, "1.602176634");
            NUnit.Framework.Assert.That(window.ClientSize, Is.EqualTo(chosenSize));
            NUnit.Framework.Assert.That(browser.Dock, Is.EqualTo(DockStyle.Fill));
            NUnit.Framework.Assert.That(browser.ScrollBarsEnabled, Is.True);
            window.Close();
            NUnit.Framework.Assert.That(window.Visible, Is.False);
            service.Show(entries["gamma(x)"]);
            WaitForText(browser, "Gamma function");
            NUnit.Framework.Assert.That(window.ClientSize, Is.EqualTo(chosenSize));
        }

        private static void WaitForText(WebBrowser browser, string expected)
        {
            var clock = Stopwatch.StartNew();
            while ((browser.ReadyState != WebBrowserReadyState.Complete || browser.Document?.Body?.InnerText?.Contains(expected) != true)
                && clock.Elapsed < TimeSpan.FromSeconds(15))
            {
                Application.DoEvents(); Thread.Sleep(10);
            }
            NUnit.Framework.Assert.That(browser.Document?.Body?.InnerText, Does.Contain(expected));
        }
    }
}
