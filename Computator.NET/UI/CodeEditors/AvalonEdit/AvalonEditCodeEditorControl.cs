using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.Properties;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using Point = System.Windows.Point;

namespace Computator.NET.UI.CodeEditors
{
    internal class AvalonEditCodeEditorControl : ElementHost
    {
        private readonly AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;
        private readonly AvalonEditCodeEditor _codeEditor;

        public AvalonEditCodeEditorControl()
        {
            _codeEditor = new AvalonEditCodeEditor();

            BackColor = Color.White;
            Dock = DockStyle.Fill;
            Child = _codeEditor;
            //   codeEditor.TextArea.TextView.GetVisualPosition(new TextViewPosition(5) {Location = }, VisualYPosition.TextMiddle)
            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu
            {
                TargetControlWrapper = new AvalonWrapper(this)
            };
            _autocompleteMenu.SetAutocompleteItems(AutocompletionData.GetAutocompleteItemsForScripting());
            SetFont(Settings.Default.ScriptingFont);

            _autocompleteMenu.MaximumSize = new Size(500, 180);
            //this.codeEditor.Document.
        }

        public AvalonEditCodeEditorControl(ICodeEditorControl codeEditor) : this()
        {
            Text = codeEditor.Text;
        }

        public string SelectedText
        {
            get { return _codeEditor.SelectedText; }
            set { _codeEditor.SelectedText = value; }
        }

        public int SelectionLength
        {
            get { return _codeEditor.SelectionLength; }
            set { _codeEditor.SelectionLength = value; }
        }

        public int SelectionStart
        {
            get { return _codeEditor.SelectionStart; }
            set { _codeEditor.SelectionStart = value; }
        }

        public int SelectionEnd
        {
            get { return _codeEditor.SelectionStart + _codeEditor.SelectionLength; }
            set
            {
                var diff = SelectionEnd - value;
                _codeEditor.SelectionLength += diff;
            }
        }

        public bool IsReadOnly
        {
            get { return _codeEditor.IsReadOnly; }
        }

        public override string Text
        {
            get { return _codeEditor.Text; }
            set { _codeEditor.Text = value; }
        }

        public bool ExponentMode
        {
            get { return _codeEditor.IsExponentMode; }
            set { _codeEditor.IsExponentMode = value; }
        }

        public void Undo()
        {
            _codeEditor.Undo();
        }

        public void Redo()
        {
            _codeEditor.Redo();
        }

        public void Cut()
        {
            _codeEditor.Cut();
        }

        public void Paste()
        {
            _codeEditor.Paste();
        }

        public void Copy()
        {
            _codeEditor.Copy();
        }

        public void SelectAll()
        {
            _codeEditor.SelectAll();
        }

        public void AppendText(string text)
        {
            _codeEditor.AppendText(text);
        }

        public void SetFont(Font font)
        {
            _codeEditor.SetFont(font);
            _autocompleteMenu.Font = font.FontFamily.Name == "Cambria"
                ? CustomFonts.GetMathFont(font.Size)
                : font;
        }

        public void InsertText(int start, string val)
        {
            _codeEditor.Document.Insert(start, val);
        }

        public void DeleteRange(int selectionStart, int i)
        {
            _codeEditor.Document.Remove(selectionStart, i);
        }

        public Point GetPointFromPosition(int pos)
        {
            var lineAndColumn = GetLineAndColumn(pos);
            var point =
                _codeEditor.TextArea.TextView.GetVisualPosition(
                    new TextViewPosition(lineAndColumn[0], lineAndColumn[1]),
                    VisualYPosition.TextMiddle);
            return point;
        }

        private int[] GetLineAndColumn(int pos)
        {
            var substr = Text.Substring(0, pos + 1);
            var lineIndex = substr.Count(c => c == '\n');

            var index = substr.LastIndexOf('\n');

            var columnIndex = substr.Substring(index + 1).Length;

            return new[] {lineIndex, columnIndex};
        }
    }
}