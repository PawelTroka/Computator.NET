using System.Windows.Forms;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    internal class FullScreenCommand : CommandBase
    {
        private readonly IMainForm mainFormView;

        public FullScreenCommand(IMainForm mainFormView)
        {
            //this.Icon = Resources;
            Text = MenuStrings.fullscreenToolStripMenuItem_Text;
            ToolTip = MenuStrings.fullscreenToolStripMenuItem_Text;
            //   this.CheckOnClick = true;
            this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            Checked = !Checked;
            if (Checked)
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