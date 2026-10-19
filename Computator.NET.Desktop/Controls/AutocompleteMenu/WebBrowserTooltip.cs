using System.Drawing;
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
        public WebBrowserToolTip()
        {
#if __MonoCS__
            referenceText = new MonoReferenceText { Size = new Size(340, 240).DpiScale() };
#else
            webBrowser = new WebBrowser
            {
                MinimumSize = new Size(300, 195).DpiScale(), ScrollBarsEnabled = true, AutoSize = true
            };
            documentLoader = new ReferenceDocumentLoader(webBrowser);
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
#endif
            Initialize(); AutoClose = false; AutoSize = true;
        }
#if !__MonoCS__
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string HTMLCode
        {
            set { webBrowser.Size = webBrowser.MinimumSize; documentLoader.ShowDocument(value); }
        }
#endif
        public void Initialize()
        {
#if __MonoCS__
            Items.Add(new ToolStripControlHost(referenceText));
#else
            Items.Add(new ToolStripControlHost(webBrowser));
            webBrowser.Dock = DockStyle.Fill;
#endif
        }
        public void ShowForm() { }
#if !__MonoCS__
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = (WebBrowser)sender;
            if (browser.Document?.Body == null) return;
            var rectangle = browser.Document.Body.ScrollRectangle;
            browser.Size = new Size(rectangle.Width + 18, System.Math.Min(rectangle.Height + 18, browser.Height));
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
