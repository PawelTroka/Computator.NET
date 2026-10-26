#if __MonoCS__
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    // Mono's WebBrowser does not render documents on the tested Linux runtime.
    // Read the same offline reference and mathematical image alternatives using
    // a managed control, without taking a dependency on an obsolete HTML engine.
    internal sealed class MonoReferenceText : RichTextBox
    {
        public MonoReferenceText()
        {
            ReadOnly = true;
            DetectUrls = false;
            BorderStyle = BorderStyle.None;
            Font = new Font(FontFamily.GenericSansSerif, 10f);
            BackColor = Color.White;
            ForeColor = Color.Black;
            Dock = DockStyle.Fill;
            ScrollBars = RichTextBoxScrollBars.Vertical;
            AccessibleName = "Text reference";
        }

        public void ShowDocument(FunctionInfo info)
        {
            Text = Render(info);
            SelectionStart = 0;
            SelectionLength = 0;
            ScrollToCaret();
        }

        internal static string Render(FunctionInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            using (var input = new StringReader("<reference-root>" + info.Description + "</reference-root>"))
            using (var reader = XmlReader.Create(input, new XmlReaderSettings
                { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                var root = XElement.Load(reader, LoadOptions.PreserveWhitespace);
                var text = new StringBuilder("Text reference").AppendLine().AppendLine()
                    .AppendLine(info.Title).AppendLine();
                if (!string.IsNullOrWhiteSpace(info.Signature))
                    text.AppendLine(info.Signature).AppendLine();
                AppendNodes(text, root);
                return text.ToString().Trim();
            }
        }

        private static void AppendNodes(StringBuilder text, XElement parent)
        {
            foreach (var node in parent.Nodes())
            {
                if (node is XText content) { text.Append(content.Value); continue; }
                if (!(node is XElement element)) continue;
                var classes = ((string)element.Attribute("class") ?? "")
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (classes.Contains("reference-signature")) continue;
                var tag = element.Name.LocalName;
                if (tag == "script" || tag == "style") continue;
                if (tag == "img") { text.Append((string)element.Attribute("alt")); continue; }
                if (tag == "br") { text.AppendLine(); continue; }
                if (tag == "sup" || tag == "sub")
                {
                    text.Append(tag == "sup" ? "^(" : "_(");
                    AppendNodes(text, element);
                    text.Append(')');
                    continue;
                }
                var block = tag == "p" || tag == "div" || tag == "section" || tag == "pre" ||
                    tag == "li" || tag == "tr" || tag == "blockquote" ||
                    tag == "h1" || tag == "h2" || tag == "h3" || tag == "h4";
                if (block && text.Length > 0 && text[text.Length - 1] != '\n') text.AppendLine();
                if (tag == "li") text.Append("• ");
                AppendNodes(text, element);
                if (tag == "a")
                {
                    var href = (string)element.Attribute("href");
                    if (!string.IsNullOrEmpty(href) && href != element.Value)
                        text.Append(" (").Append(href).Append(')');
                }
                if (tag == "td" || tag == "th") text.Append("  ");
                if (block) text.AppendLine();
            }
        }
    }
}
#endif
