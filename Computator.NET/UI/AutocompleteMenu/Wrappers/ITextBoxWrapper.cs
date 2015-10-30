namespace AutocompleteMenuNS
{
    /// <summary>
    ///     Wrapper over the control like TextBox.
    /// </summary>
    public interface ITextBoxWrapper
    {
        System.Windows.Forms.Control TargetControl { get; }
        string Text { get; }
        string SelectedText { get; set; }
        int SelectionLength { get; set; }
        int SelectionStart { get; set; }
        bool Readonly { get; }
        System.Drawing.Point GetPositionFromCharIndex(int pos);
        event System.EventHandler LostFocus;
        event System.Windows.Forms.ScrollEventHandler Scroll;
        event System.Windows.Forms.KeyEventHandler KeyDown;
        event System.Windows.Forms.MouseEventHandler MouseDown;
    }
}