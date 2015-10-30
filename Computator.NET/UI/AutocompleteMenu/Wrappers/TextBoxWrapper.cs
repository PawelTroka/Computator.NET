namespace AutocompleteMenuNS
{
    /// <summary>
    ///     Wrapper over the control like TextBox.
    /// </summary>
    public class TextBoxWrapper : ITextBoxWrapper
    {
        private readonly System.Windows.Forms.Control target;
        private System.Reflection.MethodInfo getPositionFromCharIndex;
        private System.Reflection.PropertyInfo readonlyProperty;
        private System.Reflection.PropertyInfo selectedText;
        private System.Reflection.PropertyInfo selectionLength;
        private System.Reflection.PropertyInfo selectionStart;

        private TextBoxWrapper(System.Windows.Forms.Control targetControl)
        {
            target = targetControl;
            Init();
        }

        public virtual string Text
        {
            get { return target.Text; }
            set { target.Text = value; }
        }

        public virtual string SelectedText
        {
            get { return (string) selectedText.GetValue(target, null); }
            set { selectedText.SetValue(target, value, null); }
        }

        public virtual int SelectionLength
        {
            get { return (int) selectionLength.GetValue(target, null); }
            set { selectionLength.SetValue(target, value, null); }
        }

        public virtual int SelectionStart
        {
            get { return (int) selectionStart.GetValue(target, null); }
            set { selectionStart.SetValue(target, value, null); }
        }

        public virtual System.Drawing.Point GetPositionFromCharIndex(int pos)
        {
            return (System.Drawing.Point) getPositionFromCharIndex.Invoke(target, new object[] {pos});
        }

        public virtual event System.EventHandler LostFocus
        {
            add { target.LostFocus += value; }
            remove { target.LostFocus -= value; }
        }

        public virtual event System.Windows.Forms.ScrollEventHandler Scroll
        {
            add
            {
                if (target is System.Windows.Forms.RichTextBox)
                    RTBScroll += value;
                else if (target is System.Windows.Forms.ScrollableControl)
                    (target as System.Windows.Forms.ScrollableControl).Scroll += value;
            }
            remove
            {
                if (target is System.Windows.Forms.RichTextBox)
                    RTBScroll -= value;
                else if (target is System.Windows.Forms.ScrollableControl)
                    (target as System.Windows.Forms.ScrollableControl).Scroll -= value;
            }
        }

        public virtual event System.Windows.Forms.KeyEventHandler KeyDown
        {
            add { target.KeyDown += value; }
            remove { target.KeyDown -= value; }
        }

        public virtual event System.Windows.Forms.MouseEventHandler MouseDown
        {
            add { target.MouseDown += value; }
            remove { target.MouseDown -= value; }
        }

        public virtual System.Windows.Forms.Control TargetControl
        {
            get { return target; }
        }

        public bool Readonly
        {
            get { return (bool) readonlyProperty.GetValue(target, null); }
        }

        private event System.Windows.Forms.ScrollEventHandler RTBScroll;

        protected virtual void Init()
        {
            var t = target.GetType();
            selectedText = t.GetProperty("SelectedText");
            selectionLength = t.GetProperty("SelectionLength");
            selectionStart = t.GetProperty("SelectionStart");
            readonlyProperty = t.GetProperty("ReadOnly");
            getPositionFromCharIndex = t.GetMethod("GetPositionFromCharIndex") ?? t.GetMethod("PositionToPoint");

            if (target is System.Windows.Forms.RichTextBox)
                (target as System.Windows.Forms.RichTextBox).VScroll += TextBoxWrapper_VScroll;
        }

        private void TextBoxWrapper_VScroll(object sender, System.EventArgs e)
        {
            if (RTBScroll != null)
                RTBScroll(sender,
                    new System.Windows.Forms.ScrollEventArgs(System.Windows.Forms.ScrollEventType.EndScroll, 0, 1));
        }

        public static TextBoxWrapper Create(System.Windows.Forms.Control targetControl)
        {
            var result = new TextBoxWrapper(targetControl);

            if (result.selectedText == null || result.selectionLength == null || result.selectionStart == null ||
                result.getPositionFromCharIndex == null)
                return null;

            return result;
        }

        public virtual System.Windows.Forms.Form FindForm()
        {
            return target.FindForm();
        }
    }
}