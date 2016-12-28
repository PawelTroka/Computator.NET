using System;
using System.Windows.Forms;
using Computator.NET.Core.Menu;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    public class FullScreenCommand : CommandBase
    {
        private readonly Lazy<IMainView> _mainFormView;

        public FullScreenCommand(Lazy<IMainView> mainFormView)
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
                _mainFormView.Value.FormBorderStyle = FormBorderStyle.None;
                _mainFormView.Value.WindowState = FormWindowState.Maximized;
            }
            else
            {
                // this.TopMost = false;
                _mainFormView.Value.FormBorderStyle = FormBorderStyle.Sizable;
                _mainFormView.Value.WindowState = FormWindowState.Normal;
            }
        }
    }
}