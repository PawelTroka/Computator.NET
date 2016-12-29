using System;
using System.Windows.Forms;

namespace Computator.NET.Core.Autocompletion
{
    public class SelectedEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; set; }
        public Control Control { get; set; }
    }
}