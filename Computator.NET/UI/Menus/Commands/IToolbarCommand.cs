using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.UI.Commands
{
    public interface IToolbarCommand : INotifyPropertyChanged
    {
        bool IsEnabled { get; set; }
        Image Icon { get; set; }
        string ToolTip { get; set; }
        Keys ShortcutKey { get; set; }
        bool Checked { get; set; }
        bool CheckOnClick { get; set; }

        IEnumerable<IToolbarCommand> ChildrenCommands { get; set; }
        void Execute();
    }
}