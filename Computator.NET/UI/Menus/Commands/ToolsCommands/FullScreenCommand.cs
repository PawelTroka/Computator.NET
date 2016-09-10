using System.Windows.Forms;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    internal class FullScreenCommand : CommandBase
    {
        private readonly IMainForm _mainFormView;

        public FullScreenCommand(IMainForm mainFormView)
        {
            //this.Icon = Resources;
            Text = MenuStrings.fullscreenToolStripMenuItem_Text;
            ToolTip = MenuStrings.fullscreenToolStripMenuItem_Text;
            //   this.CheckOnClick = true;
            this._mainFormView = mainFormView;
        }


        public override void Execute()
        {
            Checked = !Checked;
            if (Checked)
            {
                // this.TopMost = true;
                _mainFormView.FormBorderStyle = FormBorderStyle.None;
                _mainFormView.WindowState = FormWindowState.Maximized;
            }
            else
            {
                // this.TopMost = false;
                _mainFormView.FormBorderStyle = FormBorderStyle.Sizable;
                _mainFormView.WindowState = FormWindowState.Normal;
            }
        }
    }
}