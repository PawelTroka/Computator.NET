using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.UI.Menus.Commands
{
    public interface IToolbarCommand : INotifyPropertyChanged
    {
        bool Visible { get; set; }
        bool IsEnabled { get; set; }
        Image Icon { get; set; }
        bool IsOption { get; set; }
        string Text { get; set; }
        string ToolTip { get; set; }
        Keys ShortcutKey { get; set; }
        string ShortcutKeyString { get; set; }
        bool Checked { get; set; }
        bool CheckOnClick { get; set; }

        IEnumerable<IToolbarCommand> ChildrenCommands { get; set; }
        void Execute();
    }
}