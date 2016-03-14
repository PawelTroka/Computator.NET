using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutocompleteMenuNS
{
    /// <summary>
    ///     Wrapper over the control like TextBox.
    /// </summary>
    public interface ITextBoxWrapper
    {
        Control TargetControl { get; }
        string Text { get; }
        string SelectedText { get; set; }
        int SelectionLength { get; set; }
        int SelectionStart { get; set; }
        bool Readonly { get; }
        Point GetPositionFromCharIndex(int pos);
        event EventHandler LostFocus;
        event ScrollEventHandler Scroll;
        event KeyEventHandler KeyDown;
        event MouseEventHandler MouseDown;
    }
}