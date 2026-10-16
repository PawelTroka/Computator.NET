using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Autocompletion;
using Computator.NET.DataTypes.Properties;
using Computator.NET.Desktop.Services;
using Computator.NET.Localization;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    internal class WebBrowserForm : Form, IShowFunctionDetails
    {
        private readonly WebBrowser webBrowser;
        private readonly ReferenceDocumentLoader documentLoader;
        public WebBrowserForm()
        {
            FormClosing += Form_FormClosing;
            Text = Strings.Functions_and_Constants_Details;
            webBrowser = new WebBrowser
            {
                MinimumSize = new Size(300, 195).DpiScale(), ScrollBarsEnabled = true,
                Dock = DockStyle.Fill, AutoSize = true
            };
            documentLoader = new ReferenceDocumentLoader(webBrowser);
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            Icon = GraphicsResources.computator_net_icon;
            TopMost = true; ShowInTaskbar = false; AutoSize = true; AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(webBrowser);
        }
        public void Show(FunctionInfo functionInfo) { Show(); SetFunctionInfo(functionInfo); }
        public void SetFunctionInfo(FunctionInfo functionInfo)
        {
            webBrowser.Size = webBrowser.MinimumSize;
            documentLoader.ShowDocument(FunctionReferenceDocument.Render(functionInfo));
        }
        private void Form_FormClosing(object sender, FormClosingEventArgs e) { e.Cancel = true; Hide(); }
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = (WebBrowser)sender;
            if (browser.Document?.Body == null) return;
            var rectangle = browser.Document.Body.ScrollRectangle;
            var height = System.Math.Min(rectangle.Height + 18, browser.Height);
            browser.Size = new Size(rectangle.Width + 18, height);
        }
    }
}
