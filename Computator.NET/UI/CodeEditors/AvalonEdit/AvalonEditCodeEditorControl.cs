using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.Evaluation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET.UI.CodeEditors
{
    class AvalonEditCodeEditorControl : ElementHost, ICodeEditorControl
    {
        private readonly AvalonEditCodeEditor codeEditor;
        private AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;

        public AvalonEditCodeEditorControl()
        {
            codeEditor = new AvalonEditCodeEditor {host = this};
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            Child = codeEditor;
            //   codeEditor.TextArea.TextView.GetVisualPosition(new TextViewPosition(5) {Location = }, VisualYPosition.TextMiddle)
            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu
            {
                TargetControlWrapper = new AvalonWrapper(this)
            };
            _autocompleteMenu.SetAutocompleteItems(AutocompletionData.GetAutocompleteItemsForScripting());
            SetFont(Properties.Settings.Default.ScriptingFont);

            _autocompleteMenu.MaximumSize = new System.Drawing.Size(500, 180);
            //this.codeEditor.Document.
        }

        public void SetFont(Font font)
        {
            codeEditor.SetFont(font);
            _autocompleteMenu.Font = font.FontFamily.Name == "Cambria" ? MathCustomFonts.GetMathFont(font.Size) : font;
        }

        public string SelectedText { get { return codeEditor.SelectedText; } set { codeEditor.SelectedText = value; } }



        public int SelectionLength { get { return codeEditor.SelectionLength; } set { codeEditor.SelectionLength = value; } }

        public int SelectionStart { get { return codeEditor.SelectionStart; } set { codeEditor.SelectionStart = value; } }

        public int SelectionEnd
        {
            get
            {
                return codeEditor.SelectionStart + codeEditor.SelectionLength;
            }
            set
            {
                var diff = SelectionEnd - value;
                codeEditor.SelectionLength += diff;
            }
        }

        public void InsertText(int start,string val)
        {
            codeEditor.Document.Insert(start,val);
        }

        public AvalonEditCodeEditorControl(ICodeEditorControl codeEditor) : this()
        {
            Text = codeEditor.Text;
        }

        public override string Text
        {
            get { return codeEditor.Text; }
            set { codeEditor.Text = value; }
        }

        public void Undo()
        {
            this.codeEditor.Undo();
        }

        public void Redo()
        {
            this.codeEditor.Redo();
        }

        public void Cut()
        {
            this.codeEditor.Cut();
        }

        public void Paste()
        {
            this.codeEditor.Paste();
        }

        public void Copy()
        {
            this.codeEditor.Copy();
        }

        public void SelectAll()
        {
            this.codeEditor.SelectAll();
        }

        public bool IsReadOnly { get { return codeEditor.IsReadOnly; } }

        public void DeleteRange(int selectionStart, int i)
        {
            codeEditor.Document.Remove(selectionStart,i);
        }

        public System.Windows.Point GetPointFromPosition(int pos)
        {
            var lineAndColumn = GetLineAndColumn(pos);
            var point =
                codeEditor.TextArea.TextView.GetVisualPosition(
                    new TextViewPosition(lineAndColumn[0], lineAndColumn[1]), VisualYPosition.TextMiddle);
            return point;
        }

        private int[] GetLineAndColumn(int pos)
        {
            var substr = this.Text.Substring(0, pos+1);
            var lineIndex = substr.Count(c => c == '\n');

            var index = substr.LastIndexOf('\n');

            var columnIndex = substr.Substring(index + 1).Length;

            return new[] {lineIndex, columnIndex};
        }
    }
}