using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Desktop.Services;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    internal class WebBrowserToolTip : ToolStripDropDown
    {
#if __MonoCS__
        private readonly MonoReferenceText referenceText;
#else
        private readonly WebBrowser webBrowser;
        private readonly ReferenceDocumentLoader documentLoader;
#endif
        private readonly ToolStripControlHost browserHost;
        private int maximumHeight;
        private Rectangle workingArea;
        public WebBrowserToolTip()
        {
#if __MonoCS__
            referenceText = new MonoReferenceText { Dock = DockStyle.Fill };
            browserHost = new ToolStripControlHost(referenceText)
            {
                AutoSize = false, Margin = Padding.Empty, Padding = Padding.Empty
            };
#else
            webBrowser = new WebBrowser
            {
                ScrollBarsEnabled = true, Dock = DockStyle.Fill
            };
            documentLoader = new ReferenceDocumentLoader(webBrowser);
            documentLoader.DocumentReady += (_, _) => FitDocumentHeight();
            browserHost = new ToolStripControlHost(webBrowser)
            {
                AutoSize = false, Margin = Padding.Empty, Padding = Padding.Empty
            };
#endif
            Items.Add(browserHost);
            Padding = new Padding(1);
            AutoClose = false;
            AutoSize = false;
        }
#if !__MonoCS__
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string HTMLCode
        {
            set { documentLoader.ShowDocument(value); }
        }
#endif
        public void ShowReference(Control owner, Point location)
        {
            // Keep the readable width and a bounded height for long articles.
            // Once the current document has loaded, short articles give back
            // unused vertical space without widening for oversized equations.
            workingArea = Screen.FromControl(owner).WorkingArea;
            var preferred = new Size(660, 540).DpiScale();
            maximumHeight = Math.Min(preferred.Height, workingArea.Height);
            SetReadingSize(new Size(Math.Min(preferred.Width, workingArea.Width), maximumHeight));
            Show(owner, location);
        }

        private void SetReadingSize(Size size)
        {
            Size = size;
            browserHost.Size = new Size(Math.Max(1, ClientSize.Width - Padding.Horizontal),
                Math.Max(1, ClientSize.Height - Padding.Vertical));
        }

#if !__MonoCS__
        private void FitDocumentHeight()
        {
            var document = webBrowser.Document;
            if (!Visible || document?.Body == null || maximumHeight <= 0) return;

            // Body.ScrollRectangle includes the current viewport even when the
            // article is short. A marker in normal flow measures the article's
            // intrinsic end instead, including margins after the final paragraph.
            var end = document.GetElementById("reference-tooltip-end");
            if (end == null)
            {
                end = document.CreateElement("div");
                end.Id = "reference-tooltip-end";
                end.Style = "height:1px;line-height:0;margin:0;padding:0;border:0;clear:both;";
                document.Body.AppendChild(end);
            }
            // Measure at the capped viewport first: a vertical scrollbar left
            // by the preceding article must not narrow and rewrap a short one.
            SetReadingSize(new Size(Width, maximumHeight));
            dynamic root = document.GetElementsByTagName("html")[0].DomElement;
            dynamic body = document.Body.DomElement;
            dynamic marker = end.DomElement;
            dynamic bounds = marker.getBoundingClientRect();
            var bottomPadding = double.Parse(((string)body.currentStyle.paddingBottom).Replace("px", ""), CultureInfo.InvariantCulture);
            var contentHeight = (double)bounds.bottom + (double)root.scrollTop + (double)body.scrollTop + bottomPadding;
            // MSHTML's DOM uses CSS pixels; the native control uses device
            // pixels. Derive the current scale from the actual viewport so a
            // high-DPI monitor does not crop the end of the article.
            var scale = webBrowser.ClientSize.Height / (double)root.clientHeight;
            var chrome = Height - webBrowser.ClientSize.Height;
            var height = Math.Min(maximumHeight, Math.Max(1, (int)Math.Ceiling(contentHeight * scale) + chrome));
            SetReadingSize(new Size(Width, height));
            Location = new Point(Math.Max(workingArea.Left, Math.Min(Left, workingArea.Right - Width)),
                Math.Max(workingArea.Top, Math.Min(Top, workingArea.Bottom - Height)));
        }
#endif
        public void setFunctionInfo(FunctionInfo functionInfo, string displayedSignature = null)
        {
#if __MonoCS__
            referenceText.ShowDocument(functionInfo);
#else
            HTMLCode = FunctionReferenceDocument.Render(functionInfo, displayedSignature);
#endif
        }
    }
}
