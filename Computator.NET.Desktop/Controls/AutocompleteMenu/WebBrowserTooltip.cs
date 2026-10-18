using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Desktop.Services;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    internal class WebBrowserToolTip : ToolStripDropDown
    {
        private readonly WebBrowser webBrowser;
        private readonly ReferenceDocumentLoader documentLoader;
        private readonly ToolStripControlHost browserHost;
        public WebBrowserToolTip()
        {
            webBrowser = new WebBrowser
            {
                ScrollBarsEnabled = true, Dock = DockStyle.Fill
            };
            documentLoader = new ReferenceDocumentLoader(webBrowser);
            browserHost = new ToolStripControlHost(webBrowser)
            {
                AutoSize = false, Margin = Padding.Empty, Padding = Padding.Empty
            };
            Items.Add(browserHost);
            Padding = new Padding(1);
            AutoClose = false;
            AutoSize = false;
        }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string HTMLCode
        {
            set { documentLoader.ShowDocument(value); }
        }
        public void ShowReference(Control owner, Point location)
        {
            // Size the reading viewport, not the document's scroll rectangle:
            // wide equations should scroll inside it instead of widening the
            // popup, and a new selection must not reset it to a tiny tooltip.
            var available = Screen.FromControl(owner).WorkingArea.Size;
            var preferred = new Size(660, 540).DpiScale();
            Size = new Size(System.Math.Min(preferred.Width, available.Width),
                System.Math.Min(preferred.Height, available.Height));
            browserHost.Size = new Size(System.Math.Max(1, ClientSize.Width - Padding.Horizontal),
                System.Math.Max(1, ClientSize.Height - Padding.Vertical));
            Show(owner, location);
        }
        public void setFunctionInfo(FunctionInfo functionInfo, string displayedSignature = null) => HTMLCode = FunctionReferenceDocument.Render(functionInfo, displayedSignature);
    }
}
