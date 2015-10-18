//#define SCINTILLA_23
#define SCINTILLA_30

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using AutocompleteMenuNS;
using Computator.NET.Compilation;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.Evaluation;
using ScintillaNET;
using File = System.IO.File;
using Settings = Computator.NET.Properties.Settings;


namespace Computator.NET.UI.CodeEditors
{
    internal class ScintillaCodeEditorControl : Scintilla, ICodeEditorControl
    {
        private readonly AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;

        private string _autoCompleteList;

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

            Dock = DockStyle.Fill;
        }

        public void SetFont(Font font)
        {
            // Configuring the default style with properties
            // we have common to every lexer style saves time.
            this.StyleResetDefault();
            if (font.FontFamily.Name == "Cambria")
            {
                this.Font = MathCustomFonts.GetMathFont(font.Size);
                this.Styles[Style.Default].Font = MathCustomFonts.GetMathFont(font.Size).Name;

                this._autocompleteMenu.Font = MathCustomFonts.GetMathFont(font.Size);
            }
            else
            {
                this.Styles[Style.Default].Font = font.Name;
                this.Font = font;
                this._autocompleteMenu.Font = font;
            }
            this.Styles[Style.Default].Size = (int)font.Size;

            this.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            this.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
            this.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            this.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            this.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            this.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            this.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            this.Styles[Style.Cpp.Word2].ForeColor = Color.Teal;
            this.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            this.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            this.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            this.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            this.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            this.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
            this.Styles[Style.BraceLight].BackColor = Color.LightGray;
            this.Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            this.Styles[Style.BraceBad].ForeColor = Color.Red;
            //this.Styles[Style.Cpp.CommentDoc]

            this.Lexer = Lexer.Cpp;
            this.TextChanged += scintilla_TextChanged;
        }


        private int maxLineNumberCharLength;
        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var maxLineNumberCharLength = this.Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == this.maxLineNumberCharLength)
                return;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int padding = 2;
            this.Margins[0].Width = this.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
            this.maxLineNumberCharLength = maxLineNumberCharLength;
        }

        public override string Text
        {
            get { return base.Text.Replace('*', SpecialSymbols.DotSymbol); }
            set { base.Text = value.Replace('*', SpecialSymbols.DotSymbol); }
        }

        /// <summary>
        ///     Raises the <see cref="CharAdded" /> event.
        /// </summary>
        /// <param name="e">An <see cref="CharAddedEventArgs" /> that contains the event data.</param>
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


        private void InitializeComponent()
        {
            Margins[0].Width = 40;
            Margins[2].Width = 20;
            //Margins[0].
            KeyPress += SciriptingRichTextBox_KeyPress;

#if SCINTILLA_30
            this.Lexer = Lexer.Cpp;
            this.LexerLanguage = "cs";
            this.IndentationGuides = IndentView.LookBoth;
            this.UseTabs = true;
            this.IndentWidth = 4;

            SetFont(Settings.Default.ScriptingFont);

            this.SetKeywords(0, TslCompiler.KeywordsList0);
            this.SetKeywords(1, TslCompiler.KeywordsList1);
            // Instruct the lexer to calculate folding
            this.SetProperty("fold", "1");
            this.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            this.Margins[2].Type = MarginType.Symbol;
            this.Margins[2].Mask = Marker.MaskFolders;
            this.Margins[2].Sensitive = true;
            this.Margins[2].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                this.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                this.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            this.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            this.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            this.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            this.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            this.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            this.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            this.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            this.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);



            this.UpdateUI += (o, e) =>
            {
                // Has the caret changed position?
                var caretPos = this.CurrentPosition;
                if (lastCaretPos != caretPos)
                {
                    lastCaretPos = caretPos;
                    var bracePos1 = -1;
                    var bracePos2 = -1;

                    // Is there a brace to the left or right?
                    if (caretPos > 0 && IsBrace(this.GetCharAt(caretPos - 1)))
                        bracePos1 = (caretPos - 1);
                    else if (IsBrace(this.GetCharAt(caretPos)))
                        bracePos1 = caretPos;

                    if (bracePos1 >= 0)
                    {
                        // Find the matching brace
                        bracePos2 = this.BraceMatch(bracePos1);
                        if (bracePos2 == InvalidPosition)
                        {
                            this.BraceBadLight(bracePos1);
                            this.HighlightGuide = 0;
                        }
                        else
                        {
                            this.BraceHighlight(bracePos1, bracePos2);
                            this.HighlightGuide = this.GetColumn(bracePos1);
                        }
                    }
                    else
                    {
                        // Turn off brace matching
                        this.BraceHighlight(InvalidPosition, InvalidPosition);
                        this.HighlightGuide = 0;
                    }
                }
            };

#elif SCINTILLA_23
            ConfigurationManager.Language = "cs";
            ConfigurationManager.IsBuiltInEnabled = true;
            ConfigurationManager.IsUserEnabled = true;
            ConfigurationManager.Configure( /*config*/);





            IsBraceMatching = true;
            //Name = "SciriptingRichTextBox";

            //this.Lexing.StyleNameMap.Add("OPERATOR",3);
            
            
            this.Lexing.PunctuationChars += "·";
            //this.Lexing.WordChars -= "·";
            Lexing.ReclassifyChars(new[] {SpecialSymbols.dotSymbol}, CharClassification.Punctuation);
            //this.Lexing.ReclassifyChars(GlobalConfig.Superscripts, CharClassification.Word);
            MatchBraces = true;

            

            Indentation.ShowGuides = true;
            Indentation.SmartIndentType = SmartIndent.Simple;
            Indentation.TabIndents = true;
            Indentation.BackspaceUnindents = true;
            Indentation.UseTabs = true;
            Indentation.IndentWidth = 4;

            Folding.IsEnabled = true;
            Folding.UseCompactFolding = true;


            Whitespace.Mode = WhitespaceMode.Invisible;
            Caret.HighlightCurrentLine = true; /////
            //this.Caret.CurrentLineBackgroundAlpha = 128;
            Caret.CurrentLineBackgroundColor = Color.AliceBlue;
#endif


            // MessageBox.Show(DescribeKeywordSets());
            //SetKeywords(0, "var");//ScintillaNET 3.0
            //SetKeywords(1, "real");//ScintillaNET 3.0
            // this.LoadKeywordsFromXml(GlobalConfig.FullPath("UI", "CodeEditors", "script_syntax.xml"), true);
            // MessageBox.Show(DescribeKeywordSets());

            //SetupAutocomplete();
        }



        int lastCaretPos = 0;

        public bool ExponentMode { get; private set; } = false;

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
            acList.AddRange(array.Select(t => t.Text));

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
            
            
            if (ExponentMode)
            {
                if (SpecialSymbols.AsciiForSuperscripts.Contains(e.KeyChar))
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