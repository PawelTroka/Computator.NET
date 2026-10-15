using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Desktop.Services;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    internal class WebBrowserToolTip : ToolStripDropDown
    {
        private readonly WebBrowser webBrowser;
        public WebBrowserToolTip()
        {
            webBrowser = new WebBrowser
            {
                MinimumSize = new Size(300, 195).DpiScale(), ScrollBarsEnabled = true, AutoSize = true
            };
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            Initialize(); AutoClose = false; AutoSize = true;
        }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string HTMLCode
        {
            set { webBrowser.Size = webBrowser.MinimumSize; webBrowser.DocumentText = value; }
        }
        public void Initialize()
        {
            Items.Add(new ToolStripControlHost(webBrowser));
            webBrowser.Dock = DockStyle.Fill;
            // No hard-coded remote Debye image while waiting for the actual item.
            webBrowser.DocumentText = FunctionReferenceDocument.Render(new FunctionInfo
            {
                Title = "Function reference", Description = "<p>Select a function or constant to see its definition and conventions.</p>"
            });
        }
        public void ShowForm() { }
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = (WebBrowser)sender;
            if (browser.Document?.Body == null) return;
            var rectangle = browser.Document.Body.ScrollRectangle;
            browser.Size = new Size(rectangle.Width + 18, System.Math.Min(rectangle.Height + 18, browser.Height));
        }
        public void setFunctionInfo(FunctionInfo functionInfo) => HTMLCode = FunctionReferenceDocument.Render(functionInfo);
    }
}
