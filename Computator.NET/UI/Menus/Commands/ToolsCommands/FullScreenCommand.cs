using System.Windows.Forms;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class FullScreenCommand : CommandBase
    {

        private readonly IMainForm mainFormView;

        public FullScreenCommand(IMainForm mainFormView)
        {
            //this.Icon = Resources;
            this.Text = MenuStrings.fullscreenToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.fullscreenToolStripMenuItem_Text;
            //   this.CheckOnClick = true;
            this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            this.Checked = !this.Checked;
            if (this.Checked)
            {
                // this.TopMost = true;
                mainFormView.FormBorderStyle = FormBorderStyle.None;
                mainFormView.WindowState = FormWindowState.Maximized;
            }
            else
            {
                // this.TopMost = false;
                mainFormView.FormBorderStyle = FormBorderStyle.Sizable;
                mainFormView.WindowState = FormWindowState.Normal;
            }
        }
    }
}