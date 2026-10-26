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
#if __MonoCS__
            ShowMonoReference(owner);
#else
            // Keep the readable width and a bounded height for long articles.
            // Once the current document has loaded, short articles give back
            // unused vertical space without widening for oversized equations.
            workingArea = Screen.FromControl(owner).WorkingArea;
            var preferred = new Size(660, 540).DpiScale();
            maximumHeight = Math.Min(preferred.Height, workingArea.Height);
            SetReadingSize(new Size(Math.Min(preferred.Width, workingArea.Width), maximumHeight));
            Show(owner, location);
#endif
        }

#if __MonoCS__
        protected override bool ShowWithoutActivation => true;

        private void ShowMonoReference(Control owner)
        {
            if (!owner.IsHandleCreated || !owner.Visible) return;
            workingArea = Screen.FromControl(owner).WorkingArea;
            var bounds = owner.RectangleToScreen(owner.ClientRectangle);
            var preferred = new Size(600, 460).DpiScale();
            var spaceRight = workingArea.Right - bounds.Right - 5;
            var spaceLeft = bounds.Left - workingArea.Left - 5;
            int width, x, y;
            if (Math.Max(spaceRight, spaceLeft) >= new Size(280, 0).DpiScale().Width)
            {
                // Open beside the actual completion list, not at a fixed
                // screen-relative offset that can push the help off-screen.
                var right = spaceRight >= spaceLeft;
                width = Math.Min(preferred.Width, right ? spaceRight : spaceLeft);
                x = right ? bounds.Right + 4 : bounds.Left - width - 4;
                maximumHeight = Math.Min(preferred.Height, workingArea.Height - 8);
                y = Math.Max(workingArea.Top + 4,
                    Math.Min(bounds.Top, workingArea.Bottom - maximumHeight - 4));
            }
            else
            {
                // Very narrow displays: place a compact scrollable reference
                // above or below the list instead of hiding it beyond an edge.
                width = Math.Min(preferred.Width, workingArea.Width - 8);
                x = Math.Max(workingArea.Left + 4,
                    Math.Min(bounds.Left, workingArea.Right - width - 4));
                var below = workingArea.Bottom - bounds.Bottom - 4;
                var above = bounds.Top - workingArea.Top - 4;
                if (below >= above && below >= 120)
                {
                    maximumHeight = Math.Min(preferred.Height, below);
                    y = bounds.Bottom + 4;
                }
                else if (above >= 120)
                {
                    maximumHeight = Math.Min(preferred.Height, above);
                    y = bounds.Top - maximumHeight - 4;
                }
                else
                {
                    // Last resort: a bounded overlay, still fully visible.
                    maximumHeight = Math.Min(preferred.Height, workingArea.Height - 8);
                    y = workingArea.Top + 4;
                }
            }
            SetReadingSize(new Size(Math.Max(1, width), Math.Max(1, maximumHeight)));
            Show(owner, owner.PointToClient(new Point(x, y)));
        }

        private void FitMonoReferenceHeight()
        {
            if (!Visible || referenceText.TextLength == 0 || maximumHeight <= 0) return;
            // The reference is plain text on Mono: use the actual RichTextBox
            // layout instead of Windows' HTML document dimensions.
            var end = referenceText.GetPositionFromCharIndex(referenceText.TextLength - 1);
            if (end.Y < 0 || end.Y >= referenceText.ClientSize.Height) return;
            var chrome = Height - referenceText.ClientSize.Height;
            var height = Math.Min(maximumHeight,
                Math.Max(140, end.Y + referenceText.Font.Height * 2 + chrome));
            if (height >= Height) return;
            SetReadingSize(new Size(Width, height));
            Location = new Point(
                Math.Max(workingArea.Left + 2, Math.Min(Left, workingArea.Right - Width - 2)),
                Math.Max(workingArea.Top + 2, Math.Min(Top, workingArea.Bottom - Height - 2)));
        }
#endif

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
            FitMonoReferenceHeight();
#else
            HTMLCode = FunctionReferenceDocument.Render(functionInfo, displayedSignature);
#endif
        }
    }
}
