using System;
using System.Windows.Forms;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Desktop.Controls.AutocompleteMenu.Wrappers;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu
{
    public class SelectingEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }

    public class HoveredEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
    }


    public class WrapperNeededEventArgs : EventArgs
    {
        public WrapperNeededEventArgs(Control targetControl)
        {
            TargetControl = targetControl;
        }

        public Control TargetControl { get; private set; }
        public ITextBoxWrapper Wrapper { get; set; }
    }
}