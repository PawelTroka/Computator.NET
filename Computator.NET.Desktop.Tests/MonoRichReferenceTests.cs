using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.DataTypes.Utility;
using Computator.NET.Desktop.Controls.AutocompleteMenu;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    // Exercises the production Mono reader/layout without creating, activating
    // or sending input to a Windows window. The same code runs on Mono GDI+.
    [TestFixture]
    public class MonoRichReferenceTests
    {
        [Test]
        public void EveryShippedArticleLoadsItsReviewedOfflineAssetsAndReadableAlternatives()
        {
            var catalog = new FunctionsDetailsFileSource().Details;
            var assets = new HashSet<string>(StringComparer.Ordinal);
            NUnit.Framework.Assert.That(catalog.Count, Is.EqualTo(1251));
            foreach (var info in catalog.Values)
                using (var document = MonoReferenceDocument.Parse(info))
                {
                    NUnit.Framework.Assert.That(document.PlainText, Does.Contain(info.Title), info.Signature);
                    NUnit.Framework.Assert.That(document.PlainText, Does.Not.Contain("<img"), info.Signature);
                    assets.UnionWith(document.ImageSources);
                }
            NUnit.Framework.Assert.That(assets.Count, Is.EqualTo(775));
        }

        [TestCase("sin(x)", 360)]
        [TestCase("Normal.CDF(mean,stddev,x)", 360)]
        [TestCase("Normal.CDF(mean,stddev,x)", 700)]
        [TestCase("ElementaryCharge", 700)]
        [TestCase("Laguerre(n,α,x)", 360)]
        [TestCase("Ackley(xi1,xi2, ...)", 700)]
        public void RealArticlesRenderParagraphsEquationsAndDiagramsWithoutLeavingTheirVirtualCanvas(string signature, int width)
        {
            using var document = MonoReferenceDocument.Parse(new FunctionsDetailsFileSource().Details[signature]);
            using var probe = new Bitmap(1, 1);
            using var measurement = Graphics.FromImage(probe);
            using var font = new Font(FontFamily.GenericSansSerif, 12);
            using var layout = document.Layout(measurement, font, width);
            NUnit.Framework.Assert.That(layout.Items.Any(i => i.Image != null), Is.True);
            NUnit.Framework.Assert.That(layout.Items.Any(i => i.Font != null && i.Font.Bold), Is.True);
            foreach (var item in layout.Items)
            {
                NUnit.Framework.Assert.That(item.Bounds.Left, Is.GreaterThanOrEqualTo(0));
                NUnit.Framework.Assert.That(item.Bounds.Top, Is.GreaterThanOrEqualTo(0));
                NUnit.Framework.Assert.That(item.Bounds.Right, Is.LessThanOrEqualTo(layout.ContentSize.Width + 1));
                NUnit.Framework.Assert.That(item.Bounds.Bottom, Is.LessThanOrEqualTo(layout.ContentSize.Height + 1));
            }
            using var bitmap = new Bitmap(Math.Min(2600, layout.ContentSize.Width), Math.Min(2400, layout.ContentSize.Height));
            using (var graphics = Graphics.FromImage(bitmap)) { graphics.Clear(Color.White); layout.Draw(graphics, Point.Empty); }
            var ink = 0;
            for (var y = 0; y < bitmap.Height; y += 3)
                for (var x = 0; x < bitmap.Width; x += 3)
                    if (bitmap.GetPixel(x, y).GetBrightness() < .8f) ink++;
            NUnit.Framework.Assert.That(ink, Is.GreaterThan(300), "A successful layout alone must not count as rendered mathematical content.");
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "mono-rich-" + signature.Split('(')[0].Replace('.', '-') + "-" + width + ".png");
            bitmap.Save(path, ImageFormat.Png); TestContext.AddTestAttachment(path);
        }

        [TestCase(96)]
        [TestCase(144)]
        [TestCase(288)]
        public void InlineEquationsScaleWithTheFontAndAlignWithTheActualTextBaseline(int dpi)
        {
            var element = XElement.Parse(new FunctionsDetailsFileSource().Details["Laguerre(n,α,x)"].Description)
                .Descendants("img").First(e => (string)e.Attribute("class") == "reference-math-inline");
            using var document = MonoReferenceDocument.Parse(new FunctionInfo { Title = "Inline equation", Description = "<p>Before " + element + " after.</p>" });
            using var bitmap = new Bitmap(1, 1); bitmap.SetResolution(dpi, dpi);
            using var graphics = Graphics.FromImage(bitmap);
            using var font = new Font(FontFamily.GenericSansSerif, 12);
            using var layout = document.Layout(graphics, font, 900 * dpi / 96);
            var image = layout.Items.Single(i => i.Image != null);
            var text = layout.Items.Single(i => i.Text == "Before");
            var em = font.SizeInPoints * dpi / 72;
            NUnit.Framework.Assert.That(image.Bounds.Width, Is.EqualTo((int)element.Attribute("width") * em / 20).Within(.01));
            NUnit.Framework.Assert.That(image.Bounds.Top + image.Ascent, Is.EqualTo(text.Bounds.Top + text.Ascent).Within(.01));
            NUnit.Framework.Assert.That(image.Bounds.Bottom, Is.LessThan(layout.ContentSize.Height));
        }

        [Test]
        public void WideEquationsRemainAtReadingScaleAndProseStillWrapsToTheViewport()
        {
            var info = new FunctionsDetailsFileSource().Details.Values.First(f => XElement.Parse(f.Description).Descendants("img")
                .Any(e => (string)e.Attribute("class") == "reference-math" && (int)e.Attribute("width") > 500));
            using var document = MonoReferenceDocument.Parse(info);
            using var bitmap = new Bitmap(1, 1);
            using var graphics = Graphics.FromImage(bitmap);
            using var font = new Font(FontFamily.GenericSansSerif, 12);
            using var layout = document.Layout(graphics, font, 260);
            NUnit.Framework.Assert.That(layout.ContentSize.Width, Is.GreaterThan(260), "Wide display equations need a horizontal scroll extent instead of unreadable shrinking.");
            NUnit.Framework.Assert.That(layout.Items.Where(i => i.Text != null).All(i => i.Bounds.Right <= 260), Is.True);
        }

        [Test]
        public void MarkupAndSourceLinksAreInertAndTextRetainsMathematicalMeaning()
        {
            using var document = MonoReferenceDocument.Parse(new FunctionInfo
            {
                Title = "Reference", Description = "<script>must not execute</script><style>must not paint</style><iframe src=\"https://example.invalid\" />" +
                    "<p onclick=\"mustNotRun()\">x<sup>2</sup> + a<sub>1</sub> <a href=\"https://example.invalid/source\">Source</a></p>"
            });
            NUnit.Framework.Assert.That(document.PlainText, Does.Contain("x^(2) + a_(1)"));
            NUnit.Framework.Assert.That(document.PlainText, Does.Contain("Source (https://example.invalid/source)"));
            NUnit.Framework.Assert.That(document.PlainText, Does.Not.Contain("must not"));
            NUnit.Framework.Assert.That(document.ImageSources, Is.Empty);
        }

        [TestCase(64, 288)]
        [TestCase(160, 144)]
        [TestCase(260, 96)]
        public void NarrowHighDpiViewportsKeepPositiveGeometryAndCanPaintBothScrollExtremes(int width, int dpi)
        {
            using var document = MonoReferenceDocument.Parse(new FunctionInfo { Title = "", Description = "<p class=\"reference-note\"/><p>" + new string('x', 100) + "</p>" });
            using var bitmap = new Bitmap(width, 200); bitmap.SetResolution(dpi, dpi);
            using var graphics = Graphics.FromImage(bitmap);
            using var font = new Font(FontFamily.GenericSansSerif, 12);
            using var layout = document.Layout(graphics, font, width);
            NUnit.Framework.Assert.That(layout.Items.All(i => i.Bounds.Width >= 0 && i.Bounds.Height >= 0 && i.Bounds.Right <= layout.ContentSize.Width + 1), Is.True);
            graphics.SetClip(new Rectangle(0, 0, width, 200));
            layout.Draw(graphics, Point.Empty);
            layout.Draw(graphics, new Point(0, -Math.Max(0, layout.ContentSize.Height - 200)));
        }

        [TestCase("https://example.invalid/image.png")]
        [TestCase("file:///etc/passwd")]
        [TestCase("data:image/png;base64,AAAA")]
        [TestCase("reference-equations/../secret.png")]
        [TestCase("reference-equations/%2e%2e/secret.png")]
        [TestCase("reference-equations\\secret.png")]
        [TestCase("reference-equations/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png")]
        public void UnknownPathsAndImageProtocolsAreRejectedWithoutOpeningThem(string source)
        {
            NUnit.Framework.Assert.Throws<InvalidDataException>(() => MonoReferenceDocument.Parse(new FunctionInfo
            { Description = "<p><img class=\"reference-math\" src=\"" + source + "\" width=\"20\" height=\"20\" alt=\"test\" /></p>" }));
        }

        [TestCase("width", "9999")]
        [TestCase("height", "0")]
        [TestCase("class", "remote-image")]
        [TestCase("data-depth", "2147483647")]
        [TestCase("alt", "")]
        public void ImageMetadataMustMatchTheReviewedCatalog(string attribute, string value)
        {
            var element = XElement.Parse(new FunctionsDetailsFileSource().Details["Laguerre(n,α,x)"].Description)
                .Descendants("img").First(e => (string)e.Attribute("class") == "reference-math-inline");
            element.SetAttributeValue(attribute, value);
            NUnit.Framework.Assert.Throws<InvalidDataException>(() => MonoReferenceDocument.Parse(new FunctionInfo { Description = "<p>" + element + "</p>" }));
        }

        [TestCase("<!DOCTYPE root [<!ENTITY local SYSTEM 'file:///etc/passwd'>]><p>&local;</p>")]
        [TestCase("<p>Unclosed")]
        public void MalformedOrEntityExpandingDocumentsAreRejected(string fragment)
        {
            NUnit.Framework.Assert.Throws<XmlException>(() => MonoReferenceDocument.Parse(new FunctionInfo { Description = fragment }));
        }

        [Test]
        public void AnAlteredPngCannotBeSubstitutedForAKnownReferenceImage()
        {
            var image = XElement.Parse(new FunctionsDetailsFileSource().Details["sin(x)"].Description).Descendants("img").First();
            var source = (string)image.Attribute("src");
            var owned = Path.Combine(Path.GetTempPath(), "Computator-reference-" + Guid.NewGuid().ToString("N"));
            var files = new List<string>(); var directories = new List<string>();
            try
            {
                Directory.CreateDirectory(owned);
                foreach (var folder in new[] { "reference-equations", "reference-figures" })
                {
                    var directory = Path.Combine(owned, folder); Directory.CreateDirectory(directory); directories.Add(directory);
                    var file = Path.Combine(directory, "manifest.json"); File.Copy(PathUtility.GetFullPath("Static", folder, "manifest.json"), file); files.Add(file);
                }
                var bytes = File.ReadAllBytes(PathUtility.GetFullPath("Static", source.Replace('/', Path.DirectorySeparatorChar)));
                bytes[bytes.Length - 1] ^= 1;
                var destination = Path.Combine(owned, source.Replace('/', Path.DirectorySeparatorChar)); File.WriteAllBytes(destination, bytes); files.Add(destination);
                NUnit.Framework.Assert.Throws<InvalidDataException>(() => MonoReferenceDocument.Parse(new FunctionInfo { Description = "<p>" + image + "</p>" }, owned));
            }
            finally
            {
                foreach (var file in files) File.Delete(file);
                foreach (var directory in directories) Directory.Delete(directory);
                if (Directory.Exists(owned)) Directory.Delete(owned);
            }
        }
    }
}
