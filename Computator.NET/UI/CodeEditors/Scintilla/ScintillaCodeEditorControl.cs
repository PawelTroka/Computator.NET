//#define SCINTILLA_23

#define SCINTILLA_30

using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.UI.CodeEditors
{
    public static class DocumentExtension
    {
        public static void RenameKey<TKey, TValue>(this System.Collections.Generic.IDictionary<TKey, TValue> dic,
            TKey fromKey, TKey toKey)
        {
            var value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
    }

    internal class ScintillaCodeEditorControl : ScintillaNET.Scintilla, ICodeEditorControl
    {
        private readonly AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;
        private readonly System.Collections.Generic.Dictionary<string, ScintillaNET.Document> _documents;
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
                TargetControlWrapper = new AutocompleteMenuNS.ScintillaWrapper(this),
                MaximumSize = new System.Drawing.Size(500, 180)
            };
            _autocompleteMenu.SetAutocompleteItems(Data.AutocompletionData.GetAutocompleteItemsForScripting());

            InitializeComponent();
            // this.BorderStyle=BorderStyle.None;
            Dock = System.Windows.Forms.DockStyle.Fill;
            _documents = new System.Collections.Generic.Dictionary<string, ScintillaNET.Document>
            {
                {"NewFile1", Document}
            };
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
            Document = ScintillaNET.Document.Empty;

            if (System.IO.File.Exists(filename))
                Text = System.IO.File.ReadAllText(filename);

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

        //Troka Scripting Language (*.tsl)|*.tsl
        //Troka Scripting Language Functions(*.tslf)|*.tslf

        //private 


        public void RenameDocument(string filename, string newFilename)
        {
            _documents.RenameKey(filename, newFilename);
        }

        public bool ExponentMode { get; set; }

        public override string Text
        {
            get { return base.Text.Replace('*', DataTypes.SpecialSymbols.DotSymbol); }
            set { base.Text = value.Replace('*', DataTypes.SpecialSymbols.DotSymbol); }
        }

        public void SetFont(System.Drawing.Font font)
        {
            // Configuring the default style with properties
            // we have common to every lexer style saves time.
            StyleResetDefault();
            if (font.FontFamily.Name == "Cambria")
            {
                Font = Config.MathCustomFonts.GetMathFont(font.Size);
                Styles[ScintillaNET.Style.Default].Font = Config.MathCustomFonts.GetMathFont(font.Size).Name;

                _autocompleteMenu.Font = Config.MathCustomFonts.GetMathFont(font.Size);
            }
            else
            {
                Styles[ScintillaNET.Style.Default].Font = font.Name;
                Font = font;
                _autocompleteMenu.Font = font;
            }
            Styles[ScintillaNET.Style.Default].Size = (int) font.Size;

            StyleClearAll();

            // Configure the CPP (C#) lexer styles
            Styles[ScintillaNET.Style.Cpp.Default].ForeColor = System.Drawing.Color.Silver; /////////////////////
            Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = System.Drawing.Color.FromArgb(0, 128, 0); // Green
            Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = System.Drawing.Color.FromArgb(0, 128, 0); // Green
            Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
                // Gray
            Styles[ScintillaNET.Style.Cpp.Number].ForeColor = System.Drawing.Color.Olive;
            Styles[ScintillaNET.Style.Cpp.Word].ForeColor = System.Drawing.Color.Blue;
            Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = System.Drawing.Color.Teal;
            Styles[ScintillaNET.Style.Cpp.String].ForeColor = System.Drawing.Color.FromArgb(163, 21, 21); // Red
            Styles[ScintillaNET.Style.Cpp.Character].ForeColor = System.Drawing.Color.FromArgb(163, 21, 21); // Red
            Styles[ScintillaNET.Style.Cpp.Verbatim].ForeColor = System.Drawing.Color.FromArgb(163, 21, 21); // Red
            Styles[ScintillaNET.Style.Cpp.StringEol].BackColor = System.Drawing.Color.Pink;
            Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = System.Drawing.Color.Purple;
            Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = System.Drawing.Color.Maroon;
            Styles[ScintillaNET.Style.BraceLight].BackColor = System.Drawing.Color.LightGray;
            Styles[ScintillaNET.Style.BraceLight].ForeColor = System.Drawing.Color.BlueViolet;
            Styles[ScintillaNET.Style.BraceBad].ForeColor = System.Drawing.Color.Red;
            //this.Styles[Style.Cpp.CommentDoc]

            Lexer = ScintillaNET.Lexer.Cpp;
        }

        private void scintilla_TextChanged(object sender, System.EventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var maxLineNumberCharLength = Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == this.maxLineNumberCharLength)
                return;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int padding = 2;
            Margins[0].Width = TextWidth(ScintillaNET.Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) +
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
            Margins[2].Type = ScintillaNET.MarginType.Symbol;
            Margins[2].Mask = ScintillaNET.Marker.MaskFolders;
            Margins[2].Sensitive = true;
            Margins[2].Width = 20;

            // Set colors for all folding markers
            for (var i = 25; i <= 31; i++)
            {
                Markers[i].SetForeColor(System.Drawing.SystemColors.ControlLightLight);
                Markers[i].SetBackColor(System.Drawing.SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            Markers[ScintillaNET.Marker.Folder].Symbol = ScintillaNET.MarkerSymbol.BoxPlus;
            Markers[ScintillaNET.Marker.FolderOpen].Symbol = ScintillaNET.MarkerSymbol.BoxMinus;
            Markers[ScintillaNET.Marker.FolderEnd].Symbol = ScintillaNET.MarkerSymbol.BoxPlusConnected;
            Markers[ScintillaNET.Marker.FolderMidTail].Symbol = ScintillaNET.MarkerSymbol.TCorner;
            Markers[ScintillaNET.Marker.FolderOpenMid].Symbol = ScintillaNET.MarkerSymbol.BoxMinusConnected;
            Markers[ScintillaNET.Marker.FolderSub].Symbol = ScintillaNET.MarkerSymbol.VLine;
            Markers[ScintillaNET.Marker.FolderTail].Symbol = ScintillaNET.MarkerSymbol.LCorner;

            // Enable automatic folding
            AutomaticFold = (ScintillaNET.AutomaticFold.Show | ScintillaNET.AutomaticFold.Click |
                             ScintillaNET.AutomaticFold.Change);
        }

        private void InitializeComponent()
        {
            KeyPress += SciriptingRichTextBox_KeyPress;
            TextChanged += scintilla_TextChanged;


            Margins[0].Width = 40;
            Margins[2].Width = 20;
            Lexer = ScintillaNET.Lexer.Cpp;
            LexerLanguage = "cs";
            IndentationGuides = ScintillaNET.IndentView.LookBoth;
            UseTabs = true;
            IndentWidth = 4;

            InitDocument();
        }

        private void InitDocument()
        {
            SetFont(Properties.Settings.Default.ScriptingFont);
            SetKeywords(0, Compilation.TslCompiler.KeywordsList0);
            SetKeywords(1, Compilation.TslCompiler.KeywordsList1);
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
            var array = Data.AutocompletionData.GetAutocompleteItemsForScripting();


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
            var acList = new System.Collections.Generic.List<string>();
            acList.AddRange(Enumerable.Select(array, t => t.Text));

            var sb = new System.Text.StringBuilder();
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

        private void SciriptingRichTextBox_KeyPress(object s, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar < 32)
            {
                // Prevent control characters from getting inserted into the text buffer
                e.Handled = true;
                return;
            }

            if (ExponentMode)
            {
                if (Enumerable.Contains(DataTypes.SpecialSymbols.AsciiForSuperscripts, e.KeyChar))
                {
                    e.KeyChar = DataTypes.SpecialSymbols.AsciiToSuperscript(e.KeyChar);
                }
            }

            if (IsOperator(e.KeyChar))
            {
                if (e.KeyChar == DataTypes.SpecialSymbols.ExponentModeSymbol)
                {
                    ExponentMode = !ExponentMode;
                    //_showCaret();
                    e.Handled = true;
                    //return;
                    //this.Refresh();
                }

                if (e.KeyChar == '*')
                {
                    e.KeyChar = DataTypes.SpecialSymbols.DotSymbol;
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