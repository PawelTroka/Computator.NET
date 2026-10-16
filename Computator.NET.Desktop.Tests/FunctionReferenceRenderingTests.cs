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
            // A background test window can render correctly while an unrelated
            // runner console occludes CopyFromScreen. Keep this owned fixture
            // visible; wait for its pixels as well as its DOM before saving
            // visual evidence.
            using var window = new Form
            {
                Size = new Size(700, 650), ShowInTaskbar = false,
                TopMost = true, StartPosition = FormStartPosition.CenterScreen
            };
            using var browser = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = false };
            window.Controls.Add(browser);
            var navigations = new System.Collections.Generic.List<string>();
            browser.Navigating += (_, args) => navigations.Add(args.Url.Scheme);
            window.Show();
            browser.DocumentText = FunctionReferenceDocument.Render(entry);
            WaitForText(browser, expected);
            NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain("Arguments / value"));
            NUnit.Framework.Assert.That(browser.Document.Images.Count, Is.GreaterThan(0));
            foreach (HtmlElement image in browser.Document.Images)
            {
                NUnit.Framework.Assert.That(image.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
                dynamic rendered = image.DomElement;
                NUnit.Framework.Assert.That((bool)rendered.complete, Is.True);
                NUnit.Framework.Assert.That((int)rendered.naturalWidth, Is.GreaterThan(0));
                NUnit.Framework.Assert.That(image.GetAttribute("alt"), Is.Not.Empty);
            }
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
            window.BringToFront();
            window.Activate();
            var screenshot = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "reference-" + signature.Split('(')[0].Replace('.', '-') + ".png");
            SavePaintedReference(window, browser, screenshot);
            window.Close();
        }

        private static void SavePaintedReference(Form window, WebBrowser browser, string screenshot)
        {
            // MSHTML can expose a complete DOM, decoded images and computed
            // styles before the hosted surface reaches the desktop compositor.
            // A single Refresh/DoEvents followed by CopyFromScreen produced
            // blank white evidence for gamma and ElementaryCharge in CI.
            var title = browser.Document.GetElementsByTagName("h1")[0];
            var summary = browser.Document.GetElementsByTagName("p").Cast<HtmlElement>()
                .First(element => element.GetAttribute("className") == "reference-summary");
            var equation = browser.Document.Images[0];
            var equationAtTop = browser.ClientRectangle.Contains(ViewportBounds(equation));
            var equationPainted = true;
            if (!equationAtTop)
            {
                // Long articles can put their first equation below the fold.
                // Capture that actual math surface as separate evidence, then
                // return to the article heading for the main screenshot.
                try
                {
                    equation.ScrollIntoView(true);
                    equationPainted = CaptureDocumentPaint(window, browser, new[] { equation },
                        System.IO.Path.ChangeExtension(screenshot, null) + "-equation.png");
                }
                finally
                {
                    browser.Document.Window.ScrollTo(0, 0);
                }
            }
            var visibleContent = equationAtTop ? new[] { title, summary, equation } : new[] { title, summary };
            var pagePainted = CaptureDocumentPaint(window, browser, visibleContent, screenshot);
            NUnit.Framework.Assert.That(pagePainted && equationPainted, Is.True,
                $"The reference DOM loaded, but its desktop pixels were not ready (article={pagePainted}, equation={equationPainted}). Inspect the attached frames.");
        }

        private static bool CaptureDocumentPaint(Form window, WebBrowser browser, HtmlElement[] visibleContent, string screenshot)
        {
            var clock = Stopwatch.StartNew();
            var painted = false;
            using var bitmap = new Bitmap(window.Width, window.Height);
            using var graphics = Graphics.FromImage(bitmap);
            do
            {
                window.Refresh();
                Application.DoEvents();
                graphics.CopyFromScreen(window.Location, Point.Empty, window.Size);
                painted = visibleContent.All(element => ContainsPageText(bitmap, InScreenshot(element, browser, window)));
                if (!painted) Thread.Sleep(20);
            } while (!painted && clock.Elapsed < TimeSpan.FromSeconds(5));

            // Retain the last observed frame even when the gate fails so the
            // failure is reviewable instead of silently publishing blank proof.
            bitmap.Save(screenshot, System.Drawing.Imaging.ImageFormat.Png);
            TestContext.AddTestAttachment(screenshot, "Windows reference fixture: observed text/equation paint check");
            return painted;
        }

        private static Rectangle ViewportBounds(HtmlElement element)
        {
            dynamic bounds = element.DomElement.getBoundingClientRect();
            return Rectangle.FromLTRB((int)bounds.left, (int)bounds.top, (int)bounds.right, (int)bounds.bottom);
        }

        private static Rectangle InScreenshot(HtmlElement element, WebBrowser browser, Form window)
        {
            var bounds = ViewportBounds(element);
            // Never let an off-screen DOM rectangle include the native title
            // bar or the runner console outside the browser client surface.
            bounds.Intersect(browser.ClientRectangle);
            var origin = browser.PointToScreen(Point.Empty);
            bounds.Offset(origin.X - window.Left, origin.Y - window.Top);
            return bounds;
        }

        private static bool ContainsPageText(Bitmap bitmap, Rectangle bounds)
        {
            bounds.Intersect(new Rectangle(Point.Empty, bitmap.Size));
            var sampled = 0;
            var paper = 0;
            var ink = 0;
            for (var y = bounds.Top; y < bounds.Bottom; y += 2)
            for (var x = bounds.Left; x < bounds.Right; x += 2)
            {
                var pixel = bitmap.GetPixel(x, y);
                sampled++;
                if (pixel.R > 245 && pixel.G > 245 && pixel.B > 245) paper++;
                if (pixel.R < 150 && pixel.G < 150 && pixel.B < 170) ink++;
            }
            // Blank white pages have no ink; an occluding dark runner console
            // has no paper. Sample only DOM content, excluding window chrome.
            return sampled > 0 && paper > sampled / 2 && ink >= 20;
        }

        [Test]
        public void LongExampleCodeWrapsWithoutHorizontalOverflow()
        {
            const string title = "Reference <example> & details";
            var entry = new FunctionInfo
            {
                Title = title,
                Description = "<div class=\"function-reference\"><p class=\"reference-example\"><code>" +
                    new string('W', 500) + "</code></p></div>"
            };
            using var window = new Form { ClientSize = new Size(360, 480), ShowInTaskbar = false };
            using var browser = new WebBrowser { Dock = DockStyle.Fill };
            window.Controls.Add(browser);
            window.Show();
            browser.DocumentText = FunctionReferenceDocument.Render(entry);
            WaitForText(browser, title);
            NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain(new string('W', 500)),
                "The wrapping check must exercise retained content, not an empty removed signature.");
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
            NUnit.Framework.Assert.That(browser.Document.Body.InnerHtml, Does.Not.Contain("reference-signature"),
                "Descriptions must not repeat signatures, including in standalone windows.");
        }
    }
}
