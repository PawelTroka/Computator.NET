using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
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
            var clock = Stopwatch.StartNew();
            while ((browser.ReadyState != WebBrowserReadyState.Complete || browser.Document?.Body?.InnerText?.Contains(expected) != true)
                && clock.Elapsed < TimeSpan.FromSeconds(15))
            {
                Application.DoEvents(); Thread.Sleep(10);
            }
            NUnit.Framework.Assert.That(browser.Document?.Body?.InnerText, Does.Contain(expected));
            NUnit.Framework.Assert.That(browser.Document.Body.InnerText, Does.Contain("Arguments / value"));
            NUnit.Framework.Assert.That(browser.Document.Images.Count, Is.Zero);
            NUnit.Framework.Assert.That(navigations.All(scheme => scheme == "about"), Is.True);
            window.Close();
        }
    }
}
