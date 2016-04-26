using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Computator.NET.UI.Controls.AutocompleteMenu.Wrappers
{
    /// <summary>
    ///     Wrapper over the control like TextBox.
    /// </summary>
    public class TextBoxWrapper : ITextBoxWrapper
    {
        private readonly Control target;
        private MethodInfo getPositionFromCharIndex;
        private PropertyInfo readonlyProperty;
        private PropertyInfo selectedText;
        private PropertyInfo selectionLength;
        private PropertyInfo selectionStart;

        private TextBoxWrapper(Control targetControl)
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

        public virtual Point GetPositionFromCharIndex(int pos)
        {
            return (Point) getPositionFromCharIndex.Invoke(target, new object[] {pos});
        }

        public virtual event EventHandler LostFocus
        {
            add { target.LostFocus += value; }
            remove { target.LostFocus -= value; }
        }

        public virtual event ScrollEventHandler Scroll
        {
            add
            {
                if (target is RichTextBox)
                    RTBScroll += value;
                else if (target is ScrollableControl) (target as ScrollableControl).Scroll += value;
            }
            remove
            {
                if (target is RichTextBox)
                    RTBScroll -= value;
                else if (target is ScrollableControl) (target as ScrollableControl).Scroll -= value;
            }
        }

        public virtual event KeyEventHandler KeyDown
        {
            add { target.KeyDown += value; }
            remove { target.KeyDown -= value; }
        }

        public virtual event MouseEventHandler MouseDown
        {
            add { target.MouseDown += value; }
            remove { target.MouseDown -= value; }
        }

        public virtual Control TargetControl
        {
            get { return target; }
        }


        public bool Readonly
        {
            get { return (bool) readonlyProperty.GetValue(target, null); }
        }

        private event ScrollEventHandler RTBScroll;

        protected virtual void Init()
        {
            var t = target.GetType();
            selectedText = t.GetProperty("SelectedText");
            selectionLength = t.GetProperty("SelectionLength");
            selectionStart = t.GetProperty("SelectionStart");
            readonlyProperty = t.GetProperty("ReadOnly");
            getPositionFromCharIndex = t.GetMethod("GetPositionFromCharIndex") ?? t.GetMethod("PositionToPoint");

            if (target is RichTextBox)
                (target as RichTextBox).VScroll += TextBoxWrapper_VScroll;
        }

        private void TextBoxWrapper_VScroll(object sender, EventArgs e)
        {
            if (RTBScroll != null)
                RTBScroll(sender, new ScrollEventArgs(ScrollEventType.EndScroll, 0, 1));
        }

        public static TextBoxWrapper Create(Control targetControl)
        {
            var result = new TextBoxWrapper(targetControl);

            if (result.selectedText == null || result.selectionLength == null || result.selectionStart == null ||
                result.getPositionFromCharIndex == null)
                return null;

            return result;
        }


        public virtual Form FindForm()
        {
            return target.FindForm();
        }
    }
}