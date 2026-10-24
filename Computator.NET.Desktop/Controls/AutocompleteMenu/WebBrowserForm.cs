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
#if __MonoCS__
        private readonly MonoReferenceText referenceText;
#else
        private readonly WebBrowser webBrowser;
        private readonly ReferenceDocumentLoader documentLoader;
#endif
        public WebBrowserForm()
        {
            FormClosing += Form_FormClosing;
            Text = Strings.Functions_and_Constants_Details;
            Icon = GraphicsResources.computator_net_icon;
            TopMost = true;
            ShowInTaskbar = false;
            AutoSize = false;
            AutoScaleMode = AutoScaleMode.Font;
            StartPosition = FormStartPosition.CenterScreen;

            // This is a reading window, not a tooltip. Start with usable space,
            // respect small displays and keep the user's chosen size thereafter.
            var available = Screen.FromControl(this).WorkingArea.Size;
            var preferred = new Size(900, 760).DpiScale();
            Size = new Size(System.Math.Min(preferred.Width, available.Width),
                System.Math.Min(preferred.Height, available.Height));
            var minimum = new Size(360, 280).DpiScale();
            MinimumSize = new Size(System.Math.Min(minimum.Width, Width),
                System.Math.Min(minimum.Height, Height));

#if __MonoCS__
            referenceText = new MonoReferenceText();
            Controls.Add(referenceText);
#else
            webBrowser = new WebBrowser { ScrollBarsEnabled = true, Dock = DockStyle.Fill };
            documentLoader = new ReferenceDocumentLoader(webBrowser);
            Controls.Add(webBrowser);
#endif
        }
        public void Show(FunctionInfo functionInfo) { Show(); SetFunctionInfo(functionInfo); }
        public void SetFunctionInfo(FunctionInfo functionInfo) => SetFunctionInfo(functionInfo, null);
        public void SetFunctionInfo(FunctionInfo functionInfo, string displayedSignature)
        {
            // Document loading must not shrink/reset a manually resized window.
            // DockStyle.Fill handles layout; long articles remain scrollable.
#if __MonoCS__
            referenceText.ShowDocument(functionInfo);
#else
            documentLoader.ShowDocument(FunctionReferenceDocument.Render(functionInfo, displayedSignature));
#endif
        }
        private void Form_FormClosing(object sender, FormClosingEventArgs e) { e.Cancel = true; Hide(); }
    }
}
