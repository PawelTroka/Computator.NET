//#define SCINTILLA_23

#define SCINTILLA_30

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutocompleteMenuNS;
using Computator.NET.Compilation;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using ScintillaNET;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET.UI.CodeEditors
{
    public static class DocumentExtension
    {
        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            TKey fromKey, TKey toKey)
        {
            var value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
    }

    internal class ScintillaCodeEditorControl : Scintilla, ICodeEditorControl
    {
        private readonly AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;
        private readonly Dictionary<string, Document> _documents;
        private string _autoCompleteList;
        private int lastCaretPos;
        private int maxLineNumberCharLength;

        public ScintillaCodeEditorControl(string code) : this()
        {
            Text = code;
        }

        public ScintillaCodeEditorControl()
        {
            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu
            {
                TargetControlWrapper = new ScintillaWrapper(this),
                MaximumSize = new Size(500, 180)
            };
            _autocompleteMenu.SetAutocompleteItems(AutocompletionData.GetAutocompleteItemsForScripting());

            InitializeComponent();
            // this.BorderStyle=BorderStyle.None;
            Dock = DockStyle.Fill;
            _documents = new Dictionary<string, Document>
            {
                {"NewFile1", Document}
            };
        }

        private void HighlightText(string text, Color color)
        {
            // Indicators 0-7 could be in use by a lexer
            // so we'll use indicator 8 to highlight words.
            const int NUM = 8;

            // Remove all uses of our indicator
            this.IndicatorCurrent = NUM;
            this.IndicatorClearRange(0, this.TextLength);

            // Update indicator appearance
            this.Indicators[NUM].Style = IndicatorStyle.StraightBox;
            this.Indicators[NUM].Under = true;
            this.Indicators[NUM].ForeColor = color;
            this.Indicators[NUM].OutlineAlpha = 50;
            this.Indicators[NUM].Alpha = 30;

            // Search the document
            this.TargetStart = 0;
            this.TargetEnd = this.TextLength;
            this.SearchFlags = SearchFlags.None;
            while (this.SearchInTarget(text) != -1)
            {
                // Mark the search results with the current indicator
                this.IndicatorFillRange(this.TargetStart, this.TargetEnd - this.TargetStart);

                // Search the remainder of the document
                this.TargetStart = this.TargetEnd;
                this.TargetEnd = this.TextLength;
            }
        }

        // Indicators 0-7 could be in use by a lexer
        // so we'll use indicator 8 to highlight words.
        const int NUM = 8;


        public void ClearHighlightedErrors()
        {
            // Remove all uses of our indicator
            this.IndicatorCurrent = NUM;
            this.IndicatorClearRange(0, this.TextLength);
        }

        private void HighlightLine(int line)
        {
            // Update indicator appearance
            this.Indicators[NUM].Style = IndicatorStyle.RoundBox;
            this.Indicators[NUM].Under = true;
            this.Indicators[NUM].ForeColor = Color.Red;
            this.Indicators[NUM].OutlineAlpha = 50;
            this.Indicators[NUM].Alpha = 100;
            //this.Indicators[NUM].Flags =IndicatorFlags.ValueFore;

            // Search the document
            //this.TargetStart = 0;
            //this.TargetEnd = this.TextLength;
            //this.SearchFlags = SearchFlags.None;
            //while (this.SearchInTarget() != -1)
            {
                // Mark the search results with the current indicator
                var lineWithError = Lines.FirstOrDefault(l => l.Index+1 == line);
                //this.IndicatorFillRange(this.TargetStart, this.TargetEnd - this.TargetStart);
                if (lineWithError != null)
                {
                    //lineWithError.MarkerAdd(BOOKMARK_MARKER);
                    IndicatorFillRange(lineWithError.Position, lineWithError.EndPosition- lineWithError.Position);
                }
                
                // Search the remainder of the document
            }
        }

        public bool ContainsDocument(string filename)
        {
            return _documents.ContainsKey(filename);
        }

        public void NewDocument(string filename)
        {
            if (_documents.ContainsKey(filename))
                return;

            var document = Document;
            AddRefDocument(document);

            // Replace the current document with a new one
            Document = Document.Empty;

            if (File.Exists(filename))
                Text = File.ReadAllText(filename);

            _documents.Add(filename, Document);

            //this.ClearDocumentStyle();//this.UpdateStyles();
            // SetFont(Settings.Default.ScriptingFont);
            InitDocument();
        }

        public void SwitchDocument(string filename)
        {
            if (!_documents.ContainsKey(filename))
                return;

            var prevDocument = Document;
            AddRefDocument(prevDocument);

            // Replace the current document and make Scintilla the owner
            // var nextDocument = new Document();

            // _documents.Add(name,nextDocument);

            Document = _documents[filename];
            ReleaseDocument(_documents[filename]);

            //  this.ClearDocumentStyle();
            // SetFont(Settings.Default.ScriptingFont);
        }

        public void CloseDocument(string filename)
        {
            if (!_documents.ContainsKey(filename))
                return;

            var doc = _documents[filename];
            _documents.Remove(filename);
            //this.ReleaseDocument(doc);
            SwitchDocument(Enumerable.Last(_documents.Keys));
        }

        public void HighlightErrors(CompilerErrorCollection errors)
        {
            foreach (CompilerError error in errors)
            {
                HighlightLine(error.Line);
            }
        }

        //Troka Scripting Language (*.tsl)|*.tsl
        //Troka Scripting Language Functions(*.tslf)|*.tslf


        public void RenameDocument(string filename, string newFilename)
        {
            _documents.RenameKey(filename, newFilename);
        }

        public bool ExponentMode { get; set; }

        public override string Text
        {
            get { return base.Text.Replace('*', SpecialSymbols.DotSymbol); }
            set { base.Text = value.Replace('*', SpecialSymbols.DotSymbol); }
        }

        public void SetFont(Font font)
        {
            // Configuring the default style with properties
            // we have common to every lexer style saves time.
            StyleResetDefault();
            if (font.FontFamily.Name == "Cambria")
            {
                Font = MathCustomFonts.GetMathFont(font.Size);
                Styles[Style.Default].Font = MathCustomFonts.GetMathFont(font.Size).Name;

                _autocompleteMenu.Font = MathCustomFonts.GetMathFont(font.Size);
            }
            else
            {
                Styles[Style.Default].Font = font.Name;
                Font = font;
                _autocompleteMenu.Font = font;
            }
            Styles[Style.Default].Size = (int) font.Size;

            StyleClearAll();

            // Configure the CPP (C#) lexer styles
            Styles[Style.Cpp.Default].ForeColor = Color.Silver; /////////////////////
            Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128);
            // Gray
            Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            Styles[Style.Cpp.Word2].ForeColor = Color.Teal;
            Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
            Styles[Style.BraceLight].BackColor = Color.LightGray;
            Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            Styles[Style.BraceBad].ForeColor = Color.Red;
            //this.Styles[Style.Cpp.CommentDoc]

            Lexer = Lexer.Cpp;
        }

        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var maxLineNumberCharLength = Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == this.maxLineNumberCharLength)
                return;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int padding = 2;
            Margins[0].Width = TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) +
                               padding;
            this.maxLineNumberCharLength = maxLineNumberCharLength;
        }

        /// <summary>
        ///     Raises the <see cref="CharAdded" /> event.
        /// </summary>
        /// <param name="e">An <see cref="ScintillaNET.CharAddedEventArgs" /> that contains the event data.</param>
        /*    protected override void OnCharAdded(CharAddedEventArgs e)
        {
            base.OnCharAdded(e);

            if (e.Char == '*' || e.Char == SpecialSymbols.DotSymbol)
            {
#if SCINTILLA_23
                var byteOffset = CurrentPosition;

                var range = GetTextRange(0, byteOffset);
                var charOffset = range.Length;

                var positionInDocument = charOffset;

                var caretPos = Caret.Position;
                Text = Text.Insert(positionInDocument - 1, " ");
                Text = Text.Insert(positionInDocument + 1, " ");

                GoTo.Position(caretPos + 2);
                Focus();
#elif SCINTILLA_30
                var byteOffset = CurrentPosition;

                var range = GetTextRange(0, byteOffset);
                var charOffset = range.Length;

                var positionInDocument = charOffset;

                var caretPos = AnchorPosition;//Caret.Position;
                Text = Text.Insert(positionInDocument - 1, " ");
                Text = Text.Insert(positionInDocument + 1, " ");
                GotoPosition(caretPos + 2);
                Focus();
#endif
            }

            if (AutoLaunchAutoComplete)
                LaunchAutoComplete();
        }*/
        private void SetFolding()
        {
            // Instruct the lexer to calculate folding
            SetProperty("fold", "1");
            SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            Margins[2].Type = MarginType.Symbol;
            Margins[2].Mask = Marker.MaskFolders;
            Margins[2].Sensitive = true;
            Margins[2].Width = 20;

            // Set colors for all folding markers
            for (var i = 25; i <= 31; i++)
            {
                Markers[i].SetForeColor(SystemColors.ControlLightLight);
                Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click |
                             AutomaticFold.Change);
        }

        private const int BOOKMARK_MARGIN = 1; // Conventionally the symbol margin
        private const int BOOKMARK_MARKER = 3; // Arbitrary. Any valid index would work.

        private void InitializeComponent()
        {
            KeyPress += SciriptingRichTextBox_KeyPress;
            TextChanged += scintilla_TextChanged;

            CaretLineVisible = true;
            CaretLineBackColor = Color.AliceBlue;
            //CaretLineBackColorAlpha = 240;

            Margins[0].Width = 40;
            Margins[2].Width = 20;
            Lexer = Lexer.Cpp;
            LexerLanguage = "cs";
            IndentationGuides = IndentView.LookBoth;
            UseTabs = true;
            IndentWidth = 4;


           /* var margin = this.Margins[BOOKMARK_MARGIN];
            margin.Width = 16;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = Marker.MaskAll;
            margin.Cursor = MarginCursor.Arrow;

            var marker = this.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Background;
            marker.SetBackColor(Color.IndianRed);
            marker.SetForeColor(Color.Black);*/

            InitDocument();
        }

        private void InitDocument()
        {
            SetFont(Settings.Default.ScriptingFont);
            SetKeywords(0, TslCompiler.KeywordsList0);
            SetKeywords(1, TslCompiler.KeywordsList1);
            SetFolding();
            SetBraceMatching();
        }

        private void SetBraceMatching()
        {
            UpdateUI += (o, e) =>
            {
                // Has the caret changed position?
                var caretPos = CurrentPosition;
                if (lastCaretPos != caretPos)
                {
                    lastCaretPos = caretPos;
                    var bracePos1 = -1;
                    var bracePos2 = -1;

                    // Is there a brace to the left or right?
                    if (caretPos > 0 && IsBrace(GetCharAt(caretPos - 1)))
                        bracePos1 = (caretPos - 1);
                    else if (IsBrace(GetCharAt(caretPos)))
                        bracePos1 = caretPos;

                    if (bracePos1 >= 0)
                    {
                        // Find the matching brace
                        bracePos2 = BraceMatch(bracePos1);
                        if (bracePos2 == InvalidPosition)
                        {
                            BraceBadLight(bracePos1);
                            HighlightGuide = 0;
                        }
                        else
                        {
                            BraceHighlight(bracePos1, bracePos2);
                            HighlightGuide = GetColumn(bracePos1);
                        }
                    }
                    else
                    {
                        // Turn off brace matching
                        BraceHighlight(InvalidPosition, InvalidPosition);
                        HighlightGuide = 0;
                    }
                }
            };
        }

        private static bool IsBrace(int c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                    return true;
            }

            return false;
        }

        private void SetupAutocomplete()
        {
            var array = AutocompletionData.GetAutocompleteItemsForScripting();


            //if (Sort)
            //   Array.Sort(array2, (a, b) => a.Text.CompareTo(b.Text));
            //_autocompleteMenu.SetAutocompleteItems(array2);

#if SCINTILLA_23
            AutoComplete.List.Clear(); //2.3
            foreach (var t in array2)
            {
                AutoComplete.List.Add(t.Text);
            }
            AutoComplete.List.AddRange(new[]
            {"real", "complex", "function", "var", "void", "string", "integer", "natural"});
            AutoComplete.List.Sort();

            AutoComplete.IsCaseSensitive = false;
            AutoComplete.AutoHide = true;
            //this.AutoComplete.StopCharacters += '·';
            // this.AutoComplete.StopCharacters = this.AutoComplete.StopCharacters.Replace(GlobalConfig.dotSymbol+"","");
            // this.AutoComplete.FillUpCharacters += GlobalConfig.dotSymbol;
            // MessageBox.Show(this.AutoComplete.FillUpCharacters);
            // MessageBox.Show(this.AutoComplete.StopCharacters);
            
            //this.AutoComplete.StopCharacters = GlobalConfig.dotSymbol+"";
            AutoComplete.FillUpCharacters = AutoComplete.FillUpCharacters.Trim('[', '(', '.', SpecialSymbols.dotSymbol);
            AutoComplete.AutomaticLengthEntered = true;

#elif SCINTILLA_30
            var acList = new List<string>();
            acList.AddRange(Enumerable.Select(array, t => t.Text));

            var sb = new StringBuilder();
            foreach (var item in acList)
            {
                sb.AppendFormat("{0} ", item);
            }
            sb.Remove(sb.Length - 1, 1);

            _autoCompleteList = sb.ToString();


            AutoCAutoHide = true;
            AutoCIgnoreCase = true;
#endif
        }

        private void SciriptingRichTextBox_KeyPress(object s, KeyPressEventArgs e)
        {
            if (e.KeyChar < 32)
            {
                // Prevent control characters from getting inserted into the text buffer
                e.Handled = true;
                return;
            }

            if (ExponentMode)
            {
                if (Enumerable.Contains(SpecialSymbols.AsciiForSuperscripts, e.KeyChar))
                {
                    e.KeyChar = SpecialSymbols.AsciiToSuperscript(e.KeyChar);
                }
            }

            if (IsOperator(e.KeyChar))
            {
                if (e.KeyChar == SpecialSymbols.ExponentModeSymbol)
                {
                    ExponentMode = !ExponentMode;
                    //_showCaret();
                    e.Handled = true;
                    //return;
                    //this.Refresh();
                }

                if (e.KeyChar == '*')
                {
                    e.KeyChar = SpecialSymbols.DotSymbol;
                    //for (int i = 0; i < this.AutoCompleteCustomSource.Count; i++)
                    // this.AutoCompleteCustomSource[i] += Text + e.KeyChar;
                }
            }


            /*
            if (e.KeyChar == '^')
            {
                ExponentMode = !ExponentMode;
                e.Handled = true;
                return;
            }

            if (ExponentMode)
            {
                e.Handled = true;

                if (SpecialSymbols.AsciiForSuperscripts.Contains(e.KeyChar))
                {
                    var str = SpecialSymbols.AsciiToSuperscript(e.KeyChar + "");

                    var byteOffset = CurrentPosition;

                    var range = GetTextRange(0, byteOffset);
                    var charOffset = range.Length;

                    var positionInDocument = charOffset;

                    var caretPos = AnchorPosition; //Caret.Position;
                    if (!string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
                    {
                        Text = Text.Insert(positionInDocument, str);
                        GotoPosition(caretPos + 1);
                        Focus();
                    }
                }
            }
            else if (e.KeyChar == '*')
            {
                e.KeyChar = SpecialSymbols.DotSymbol;
            }*/
        }

        /* public void RefreshSize()
        {
            _autocompleteMenu.MaximumSize = new Size(Size.Width, _autocompleteMenu.MaximumSize.Height);
        }*/

        private static bool IsOperator(char c)
        {
            if (c == '*' || c == '/' || c == '+' || c == '-' || c == '(' || c == '^' || c == '!')
                return true;
            return false;
        }
    }
}