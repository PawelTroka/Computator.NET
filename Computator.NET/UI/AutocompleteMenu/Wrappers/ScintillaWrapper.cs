namespace AutocompleteMenuNS
{
    public class ScintillaWrapper : ITextBoxWrapper
    {
        public ScintillaNET.Scintilla target;

        public ScintillaWrapper(ScintillaNET.Scintilla trgt)
        {
            target = trgt;
        }

        public bool Readonly
        {
            get { return target.ReadOnly; }
        }

        public string SelectedText
        {
            get { return target.SelectedText; }
            set
            {
                //Store the start of the selection.
                var start = target.SelectionStart;

                //Delete the current text between selections.
                target.DeleteRange(target.SelectionStart, (target.SelectionEnd - target.SelectionStart));

                //Add the text in the same postion.
                target.InsertText(start, value);

                //Clear selection and make sure the caret is at the end.
                target.SelectionStart = (start + value.Length);
                target.SelectionEnd = (start + value.Length);
            }
        }

        public int SelectionLength
        {
            get { return (target.SelectionEnd - target.SelectionStart); }
            set { target.SelectionEnd = (target.SelectionStart + value); }
        }

        public int SelectionStart
        {
            get { return target.SelectionStart; }
            set { target.SelectionStart = value; }
        }

        public System.Windows.Forms.Control TargetControl
        {
            get { return target; }
        }

        public string Text
        {
            get { return target.Text; }
        }

        public System.Drawing.Point GetPositionFromCharIndex(int pos)
        {
            return new System.Drawing.Point(target.PointXFromPosition(pos), target.PointYFromPosition(pos));
        }

        //Events
        public virtual event System.Windows.Forms.KeyEventHandler KeyDown
        {
            add { target.KeyDown += value; }
            remove { target.KeyDown -= value; }
        }

        public virtual event System.EventHandler LostFocus
        {
            add { target.LostFocus += value; }
            remove { target.LostFocus -= value; }
        }

        public virtual event System.Windows.Forms.MouseEventHandler MouseDown
        {
            add { target.MouseDown += value; }
            remove { target.MouseDown -= value; }
        }

        public virtual event System.Windows.Forms.ScrollEventHandler Scroll;
            //There is no any scroll events in ScintillaNET, So on hold.
    }
}