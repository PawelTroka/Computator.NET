using System;
using System.Globalization;
using System.Net;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Computator.NET.DataTypes.Utility;

namespace Computator.NET.Core.Autocompletion
{
    /// <summary>Document shell for the trusted, checked-in HTML reference fragments.</summary>
    public static class FunctionReferenceDocument
    {
        public static string Render(FunctionInfo info, string displayedSignature = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            // Keep the optional argument for existing callers, but captions no longer
            // decide whether signatures are repeated inside a description.
            var description = PrepareFragment(info.Description);
            // Description is the locally shipped reference, verified by content
            // tests. This helper is not a sanitizer for arbitrary user HTML.
            // Keep the stylesheet compatible with the offline Windows browser;
            // no remote fonts, scripts or equation-image services are required.
            return "<!doctype html><html><head><meta charset=\"utf-8\">" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>" + WebUtility.HtmlEncode(info.Title) +
                "</title><style>body{font:18px/1.6 'Segoe UI',Arial,sans-serif;margin:0;padding:16px;color:#24262b;background:#fff;word-wrap:break-word;}" +
                "h1{font-size:24px;line-height:1.3;margin:0 0 16px;}p{margin:12px 0;}" +
                "code{font-size:1em;font-family:Consolas,'Cambria Math',monospace;white-space:normal;word-wrap:break-word;}" +
                ".reference-note{border-left:3px solid #b07934;padding-left:12px;}" +
                ".reference-formula{padding:12px;background:#f4f6f8;border-left:3px solid #75879b;}" +
                ".reference-formula code{font-family:'Cambria Math','Segoe UI',serif;font-size:1.125em;}" +
                ".reference-equation{overflow-x:auto;padding:12px;background:#fff;border-left:3px solid #75879b;}" +
                ".reference-math{display:block;max-width:none;}" +
                ".reference-math-inline{display:inline-block;max-width:none;}" +
                ".reference-math-text{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0;}" +
                ".reference-worked-example{padding:12px;border:1px solid #d5dce3;background:#f8fafc;}" +
                "pre{white-space:pre-wrap;word-wrap:break-word;margin:8px 0;}" +
                ".reference-source{font-size:15px;}a{color:#305c85;text-decoration:underline;}</style></head><body><h1>" +
                WebUtility.HtmlEncode(info.Title) + "</h1>" + description + "</body></html>";
        }
        // Signatures belong in autocomplete, not the explanatory article. This
        // applies to both tooltip modes and the standalone reader, even when a
        // caller has no caption or uses a different spelling of that caption.
        private static string PrepareFragment(string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment)) return fragment ?? "";
            using var input = new StringReader("<reference-root>" + fragment + "</reference-root>");
            using var reader = XmlReader.Create(input, new XmlReaderSettings
                { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            var root = XElement.Load(reader, LoadOptions.PreserveWhitespace);
            foreach (var signature in root.Descendants().Where(element =>
                ((string)element.Attribute("class") ?? "")
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                    .Contains("reference-signature")).ToArray())
                signature.Remove();
            foreach (var image in root.Descendants("img").ToArray())
            {
                var source = (string)image.Attribute("src") ?? "";
                if (!Regex.IsMatch(source, @"\Areference-equations/[a-f0-9]{64}\.png\z", RegexOptions.CultureInvariant))
                    throw new InvalidDataException("The reference contains a nonlocal equation image.");
                var imageClass = (string)image.Attribute("class") ?? "";
                var inline = imageClass == "reference-math-inline";
                var alternative = (string)image.Attribute("alt");
                if ((!inline && imageClass != "reference-math") || string.IsNullOrWhiteSpace(alternative))
                    throw new InvalidDataException("The reference image needs a mathematical class and alternative.");
                var width = EquationDimension(image, "width", inline ? 320 : 2400);
                var height = EquationDimension(image, "height", inline ? 100 : 800);
                var depth = 0;
                var depthText = (string)image.Attribute("data-depth") ?? "";
                if (inline)
                {
                    if (!Regex.IsMatch(depthText, @"\A(?:0|-?[1-9][0-9]{0,3})\z", RegexOptions.CultureInvariant) ||
                        !int.TryParse(depthText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out depth) ||
                        depth < -1000 || depth >= height * 50)
                        throw new InvalidDataException("Invalid inline equation baseline.");
                }
                else if (image.Attribute("data-depth") != null)
                    throw new InvalidDataException("Display equations must not have inline layout metadata.");
                var path = PathUtility.GetFullPath("Static", "reference-equations", source.Substring("reference-equations/".Length));
                using var stream = File.OpenRead(path);
                if (stream.Length < 24 || stream.Length > 512000)
                    throw new InvalidDataException("Invalid reference equation size.");
                var bytes = new byte[(int)stream.Length];
                stream.ReadExactly(bytes);
                if (!bytes.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                    throw new InvalidDataException("A reference equation is not a PNG.");
                // DocumentText has an about:blank origin. Inline only this article's
                // reviewed PNGs instead of granting local-file/network access or
                // embedding the entire catalogue's images into every API response.
                // Rebuild image attributes; source CSS/handlers never become a
                // way to inject arbitrary styles through inline math metadata.
                image.RemoveAttributes();
                image.SetAttributeValue("class", imageClass);
                image.SetAttributeValue("alt", alternative);
                image.SetAttributeValue("width", width);
                image.SetAttributeValue("height", height);
                image.SetAttributeValue("src", "data:image/png;base64," + Convert.ToBase64String(bytes));
                if (inline)
                {
                    image.SetAttributeValue("data-depth", depth);
                    image.SetAttributeValue("style", "width:" + Em(width / 20m) + ";height:" + Em(height / 20m) +
                        ";vertical-align:" + Em(-depth / 1000m) + ";");
                    image.SetAttributeValue("aria-hidden", "true");
                    image.SetAttributeValue("alt", "");
                    // Reconstruct selectable/readable text from the validated
                    // alternative. Never accept an authored hidden-text class.
                    var wrapper = new XElement("span", new XAttribute("class", "reference-inline"));
                    image.ReplaceWith(wrapper);
                    wrapper.Add(image, new XElement("span", new XAttribute("class", "reference-math-text"), alternative));
                }
            }
            return string.Concat(root.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting)));
        }

        private static int EquationDimension(XElement image, string name, int maximum)
        {
            var text = (string)image.Attribute(name) ?? "";
            if (!Regex.IsMatch(text, @"\A[1-9][0-9]{0,3}\z", RegexOptions.CultureInvariant) ||
                !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value > maximum)
                throw new InvalidDataException("Invalid reference image dimension.");
            return value;
        }

        private static string Em(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture) + "em";
    }
}
