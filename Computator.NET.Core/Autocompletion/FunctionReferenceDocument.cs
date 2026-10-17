using System;
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
            foreach (var image in root.Descendants("img"))
            {
                var source = (string)image.Attribute("src") ?? "";
                if (!Regex.IsMatch(source, @"\Areference-equations/[a-f0-9]{64}\.png\z", RegexOptions.CultureInvariant))
                    throw new InvalidDataException("The reference contains a nonlocal equation image.");
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
                image.SetAttributeValue("src", "data:image/png;base64," + Convert.ToBase64String(bytes));
            }
            return string.Concat(root.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting)));
        }
    }
}
