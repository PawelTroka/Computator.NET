using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Functions;
using Computator.NET.Localization;

namespace Computator.NET.UI.AutocompleteMenu
{
    internal class WebBrowserForm : Form
    {
        public WebBrowser webBrowser;

        public WebBrowserForm()
        {
            FormClosing += Form_FormClosing;
            Text = "Functions & Constants Details";
            webBrowser = new WebBrowser();
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            webBrowser.MinimumSize = new Size(300, 195);
            webBrowser.ScrollBarsEnabled = true;

            Controls.Add(webBrowser);
            webBrowser.Dock = DockStyle.Fill;
        }

        public string HTMLCode
        {
            set
            {
                webBrowser.Size = webBrowser.MinimumSize;
                webBrowser.DocumentText = value;
            }
        }

        public void setFunctionInfo(FunctionInfo functionInfo)
        {
            HTMLCode = @"<b>" + functionInfo.Title + @"</b>" + @"<hr>" + functionInfo.Description + @" <br /><br /><i>" +
                       Strings.BrBrISourceBrAHref + @"<br /><a href=""" +
                       functionInfo.Url.Replace("http://en.wikipedia", "http://en.m.wikipedia") + @""">" +
                       functionInfo.Url + @"</a></i>";
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // this cancels the close event.
            Hide();
        }

        private void WebBrowser_DocumentCompleted(object sender,
            WebBrowserDocumentCompletedEventArgs e)
        {
            var webBrowser = sender as WebBrowser;

            var r = webBrowser.Document.Body.ScrollRectangle;

            int height;
            var overlapp = 18;

            if (r.Size.Height + overlapp < webBrowser.Size.Height)
                height = r.Size.Height + overlapp;
            else
                height = webBrowser.Size.Height;

            webBrowser.Size = new Size(r.Width + overlapp, height);
            //  webBrowser.Document.Body.Style = "zoom:80%;";
        }
    }

    internal class WebBrowserToolTip : ToolStripDropDown
    {
        //private Control ctl;
        //  private WebBrowserForm form;
        private readonly WebBrowser webBrowser;

        public WebBrowserToolTip()
        {
            webBrowser = new WebBrowser();
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            webBrowser.MinimumSize = new Size(300, 195);
            webBrowser.ScrollBarsEnabled = true;

            // this.ctl = webBrowser;
            Initialize();
            //  form = new WebBrowserForm(webBrowser);
            AutoClose = false;
        }

        public string HTMLCode
        {
            set
            {
                webBrowser.Size = webBrowser.MinimumSize;
                webBrowser.DocumentText = value;
            }
        }

        public void Initialize()
        {
            //  this.AutoSize = true;
            var host = new ToolStripControlHost(webBrowser);
            //this.Margin = Padding.Empty;
            // this.Padding = Padding.Empty;
            // host.Margin = Padding.Empty;
            // host.Padding = Padding.Empty;
            //  host.AutoSize = true;
            // host.Size = ctl.Size;
            // this.Size = ctl.Size;
            Items.Add(host);
            webBrowser.Dock = DockStyle.Fill;
            //webBrowser.BringToFront();
            //this.form.Controls.Add(new ComboBox());//ctl);
            // webBrowser.Url = new Uri("http://en.m.wikipedia.org/w/index.php?search=" + title);

            webBrowser.DocumentText =
                @"In <a href=""/wiki/Mathematics"" title=""Mathematics"">mathematics</a>, the family of <b>Debye functions</b> is defined by</p>
            <dl>
            <dd><img class=""mwe-math-fallback-png-inline tex"" alt=""D_n(x) = \frac{n}{x^n} \int_0^x \frac{t^n}{e^t - 1}\,dt."" src=""http://upload.wikimedia.org/math/a/4/9/a4981a548490410b838189b01911f320.png""></dd>
            </dl>
            <p>The functions are named in honor of <a href=""/wiki/Peter_Debye"" title=""Peter Debye"">Peter Debye</a>, who came across this function (with <i>n</i> = 3) in 1912 when he analytically computed the <a href=""/wiki/Heat_capacity"" title=""Heat capacity"">heat capacity</a> of what is now called the <a href=""/wiki/Debye_model"" title=""Debye model"">Debye model</a>.</p>
            <br/><br/><br/><br/><br/><br/><br/>
            ";
        }

        //<img class=""mwe-math-fallback-png-inline tex"" alt=""D_n(x) = \frac{n}{x^n} \int_0^x \frac{t^n}{e^t - 1}\,dt."" src=""http://upload.wikimedia.org/math/a/4/9/a4981a548490410b838189b01911f320.png"">

        public void ShowForm()
        {
            // webBrowser.Update();
            //var form2 = new WebBrowserForm(webBrowser);
            // form.webBrowser = webBrowser;
            //form.Show();
        }

        private void WebBrowser_DocumentCompleted(object sender,
            WebBrowserDocumentCompletedEventArgs e)
        {
            var webBrowser = sender as WebBrowser;

            var r = webBrowser.Document.Body.ScrollRectangle;

            int height;
            var overlapp = 18;

            if (r.Size.Height + overlapp < webBrowser.Size.Height)
                height = r.Size.Height + overlapp;
            else
                height = webBrowser.Size.Height;

            webBrowser.Size = new Size(r.Width + overlapp, height);
            //  webBrowser.Document.Body.Style = "zoom:80%;";
        }

        public void setFunctionInfo(FunctionInfo functionInfo)
        {
            HTMLCode = @"<b>" + functionInfo.Title + @"</b>" + @"<hr>" + functionInfo.Description + @" <br /><br /><i>" +
                       Strings.BrBrISourceBrAHref + @"<br /><a href=""" +
                       functionInfo.Url.Replace("http://en.wikipedia", "http://en.m.wikipedia") + @""">" +
                       functionInfo.Url + @"</a></i>";
        }
    }
}