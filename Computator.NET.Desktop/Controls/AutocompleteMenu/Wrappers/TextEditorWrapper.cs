using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using KeyEventHandler = System.Windows.Forms.KeyEventHandler;

namespace Computator.NET.Desktop.Controls.AutocompleteMenu.Wrappers
{
    internal sealed class TextEditorWrapper : ITextBoxWrapper, IDisposable
    {
        private readonly TextEditorControl _editor;
        private readonly TextAreaControl _areaControl;
        private readonly TextArea _area;

        internal TextEditorWrapper(TextEditorControl editor)
        {
            _editor = editor;
            _areaControl = editor.ActiveTextAreaControl;
            _area = _areaControl.TextArea;
            _area.KeyDown += OnKeyDown;
            // Navigation, Enter and Tab are editor actions, dispatched before
            // WinForms KeyDown. Completion must get first refusal of those keys.
            _area.DoProcessDialogKey += ProcessDialogKey;
            _areaControl.HScrollBar.ValueChanged += OnScroll;
            _areaControl.VScrollBar.ValueChanged += OnScroll;
        }

        public Control TargetControl => _area;
        public string Text => _editor.Document.TextContent;
        public bool Readonly => _editor.Document.ReadOnly;

        public int SelectionStart
        {
            get
            {
                var selection = _area.SelectionManager.SelectionCollection.FirstOrDefault();
                return selection == null ? _area.Caret.Offset :
                    _editor.Document.PositionToOffset(selection.StartPosition);
            }
            set
            {
                _area.SelectionManager.ClearSelection();
                _area.Caret.Position = _editor.Document.OffsetToPosition(value);
            }
        }

        public int SelectionLength
        {
            get
            {
                var selection = _area.SelectionManager.SelectionCollection.FirstOrDefault();
                return selection == null ? 0 :
                    _editor.Document.PositionToOffset(selection.EndPosition) - SelectionStart;
            }
            set
            {
                var document = _editor.Document;
                var start = SelectionStart;
                _area.SelectionManager.ClearSelection();
                _area.Caret.Position = document.OffsetToPosition(start + value);
                // The legacy selection manager retains its original document.
                // Bind the selection explicitly to the currently displayed one.
                if (value > 0)
                    _area.SelectionManager.SetSelection(new DefaultSelection(document,
                        document.OffsetToPosition(start), document.OffsetToPosition(start + value)));
            }
        }

        public string SelectedText
        {
            get => _editor.Document.GetText(SelectionStart, SelectionLength);
            set
            {
                if (Readonly) return;
                var document = _editor.Document;
                var start = SelectionStart;
                var length = SelectionLength;
                var replacement = value ?? string.Empty;
                _editor.BeginUpdate();
                try
                {
                    // Replace is a native undo operation; replacing TextContent
                    // would discard the document's edit history.
                    document.Replace(start, length, replacement);
                    _area.SelectionManager.ClearSelection();
                    _area.Caret.Position = document.OffsetToPosition(start + replacement.Length);
                    _area.SetDesiredColumn();
                }
                finally { _editor.EndUpdate(); }
                _areaControl.ScrollToCaret();
                _editor.Refresh();
            }
        }

        public Point GetPositionFromCharIndex(int pos)
        {
            var document = _editor.Document;
            var position = document.OffsetToPosition(pos);
            return new Point(
                _area.TextView.DrawingPosition.X + _area.TextView.GetDrawingXPos(position.Line, position.Column),
                _area.TextView.DrawingPosition.Y + document.GetVisibleLine(position.Line) * _area.TextView.FontHeight
                    - _area.VirtualTop.Y);
        }

        public event KeyEventHandler KeyDown;

        private void OnKeyDown(object sender, KeyEventArgs e) => KeyDown?.Invoke(_area, e);

        private bool ProcessDialogKey(Keys keyData)
        {
            var args = new KeyEventArgs(keyData);
            OnKeyDown(_area, args);
            return args.Handled || args.SuppressKeyPress;
        }

        public event EventHandler LostFocus
        {
            add { _area.LostFocus += value; }
            remove { _area.LostFocus -= value; }
        }

        public event MouseEventHandler MouseDown
        {
            add { _area.MouseDown += value; }
            remove { _area.MouseDown -= value; }
        }

        public event ScrollEventHandler Scroll;

        // Wheel and caret scrolling change Value directly, without Scroll.
        private void OnScroll(object sender, EventArgs e)
            => Scroll?.Invoke(_area, new ScrollEventArgs(ScrollEventType.EndScroll, ((ScrollBar)sender).Value));

        public void Dispose()
        {
            _area.KeyDown -= OnKeyDown;
            _area.DoProcessDialogKey -= ProcessDialogKey;
            _areaControl.HScrollBar.ValueChanged -= OnScroll;
            _areaControl.VScrollBar.ValueChanged -= OnScroll;
        }
    }
}
