using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu.Wrappers
{
    public class ScintillaWrapper : ITextBoxWrapper
    {
        public Scintilla target;

        public ScintillaWrapper(Scintilla trgt)
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
                target.DeleteRange(target.SelectionStart, target.SelectionEnd - target.SelectionStart);

                //Add the text in the same postion.
                target.InsertText(start, value);

                //Clear selection and make sure the caret is at the end.
                target.SelectionStart = start + value.Length;
                target.SelectionEnd = start + value.Length;
            }
        }

        public int SelectionLength
        {
            get { return target.SelectionEnd - target.SelectionStart; }
            set { target.SelectionEnd = target.SelectionStart + value; }
        }

        public int SelectionStart
        {
            get { return target.SelectionStart; }
            set { target.SelectionStart = value; }
        }

        public Control TargetControl
        {
            get { return target; }
        }

        public string Text
        {
            get { return target.Text; }
        }

        public Point GetPositionFromCharIndex(int pos)
        {
            return new Point(target.PointXFromPosition(pos), target.PointYFromPosition(pos));
        }

        //Events
        public virtual event KeyEventHandler KeyDown
        {
            add { target.KeyDown += value; }
            remove { target.KeyDown -= value; }
        }

        public virtual event EventHandler LostFocus
        {
            add { target.LostFocus += value; }
            remove { target.LostFocus -= value; }
        }

        public virtual event MouseEventHandler MouseDown
        {
            add { target.MouseDown += value; }
            remove { target.MouseDown -= value; }
        }


        private readonly Dictionary<ScrollEventHandler, MouseEventHandler> _scrollHandlersMapped = new Dictionary<ScrollEventHandler, MouseEventHandler>();

        public virtual event ScrollEventHandler Scroll
        {
            add
            {
                if (value != null && !_scrollHandlersMapped.ContainsKey(value))
                {
                    MouseEventHandler ms = (o, s) => { value(o, ConvertMouseEventArgsToScrollEventArgs(s)); };
                    _scrollHandlersMapped.Add(value, ms);
                    target.MouseWheel += ms;
                }
            }
            remove
            {
                if (value != null && _scrollHandlersMapped.ContainsKey(value))
                {
                    var ms = _scrollHandlersMapped[value];
                    target.MouseWheel -= ms;
                }
            }
        }

        private static ScrollEventArgs ConvertMouseEventArgsToScrollEventArgs(MouseEventArgs s)
        {
            return new ScrollEventArgs(ScrollEventType.EndScroll, s.Delta);
        }
    }
}