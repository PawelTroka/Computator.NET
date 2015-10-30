//#define NEW_AUTOCOMPLETE
//#define USE_FOLDING

using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.UI.CodeEditors
{
    internal class AvalonEditCodeEditor : ICSharpCode.AvalonEdit.TextEditor, ICodeEditorControl
    {
        private readonly System.Collections.Generic.Dictionary<string, ICSharpCode.AvalonEdit.Document.TextDocument>
            _documents;

        private readonly System.Collections.Generic.List<CompletionData> completionDatas;
        private ICSharpCode.AvalonEdit.CodeCompletion.CompletionWindow completionWindow;
        private bool ctrlPressed;
        private ICSharpCode.AvalonEdit.Highlighting.HighlightingManager highlightingManager;
        //  void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        //{
        //  if(char.IsLetterOrDigit(e.Text[0]))
        //    completionWindow.Show();
        //}
        // public AvalonEditCodeEditorControl host;

        protected ICSharpCode.AvalonEdit.CodeCompletion.OverloadInsightWindow insightWindow;
        private ICSharpCode.AvalonEdit.Search.SearchPanel searchPanel;

        public AvalonEditCodeEditor()
        {
            completionDatas =
                Data.AutocompletionData.ConvertAutocompleteItemsToCompletionDatas(
                    Data.AutocompletionData.GetAutocompleteItemsForScripting());
            InitializeComponent();
            _documents =
                new System.Collections.Generic.Dictionary<string, ICSharpCode.AvalonEdit.Document.TextDocument>
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
            //this.AddRefDocument(document);

            // Replace the current document with a new one
            Document = new ICSharpCode.AvalonEdit.Document.TextDocument();

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
            //this.AddRefDocument(prevDocument);

            // Replace the current document and make Scintilla the owner
            // var nextDocument = new Document();

            // _documents.Add(name,nextDocument);

            Document = _documents[filename];
            //this.ReleaseDocument(_documents[filename]);

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

        public void RenameDocument(string filename, string newFilename)
        {
            _documents.RenameKey(filename, newFilename);
        }

        public new void Undo()
        {
            base.Undo();
        }

        public new void Redo()
        {
            base.Redo();
        }

        public bool ExponentMode { get; set; }

        public string SaveAs(string filename)
        {
            var dg = new System.Windows.Forms.SaveFileDialog {FileName = filename};
            if (dg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.File.WriteAllText(dg.FileName, Text);

                if (dg.FileName != filename)
                    _documents.RenameKey(filename, dg.FileName);

                return dg.FileName;
            }
            return null;
        }

        public string SaveDocument(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                var dg = new System.Windows.Forms.SaveFileDialog {FileName = filename};
                if (dg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.File.WriteAllText(dg.FileName, Text);

                    if (dg.FileName != filename)
                        _documents.RenameKey(filename, dg.FileName);

                    return dg.FileName;
                }
                return null;
            }
            System.IO.File.WriteAllText(filename, Text);
            return filename;
        }

        private void InitDocument()
        {
            //throw new NotImplementedException();
        }

        private void InitializeComponent()
        {
            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition customHighlighting;

            using (
                System.Xml.XmlReader reader =
                    new System.Xml.XmlTextReader(Config.GlobalConfig.FullPath("UI", "CodeEditors", "TSL-Syntax.xshd")))
            {
                customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            }
            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting(
                "Custom Highlighting", new[] {".cool"}, customHighlighting);

            SyntaxHighlighting = customHighlighting;

            //this.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

            // FontFamily = new FontFamily("Consolas");
            // FontSize = 18;
            //completionWindow.ShowActivated = true;
            Options.HighlightCurrentLine = true;
            //this.Options.ShowColumnRuler = true;
            ShowLineNumbers = true;
            // this.MouseWheel += ExperimentalCodeEditor_MouseWheel;
            TextArea.KeyUp += TextArea_KeyUp;
            TextArea.KeyDown += TextArea_KeyDown;
            TextArea.PreviewMouseWheel += ExperimentalCodeEditor_MouseWheel;

            TextArea.IndentationStrategy =
                new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(Options);

#if USE_FOLDING
            foldingStrategy = new BraceFoldingStrategy();
            foldingManager = FoldingManager.Install(TextArea);

            var foldingUpdateTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(5)};
            foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
            foldingUpdateTimer.Start();
#endif

#if !NEW_AUTOCOMPLETE
            TextArea.TextEntering += OnTextEntering;
            TextArea.TextEntered += OnTextEntered;
            var ctrlSpace = new System.Windows.Input.RoutedCommand();
            ctrlSpace.InputGestures.Add(new System.Windows.Input.KeyGesture(System.Windows.Input.Key.Space,
                System.Windows.Input.ModifierKeys.Control));
            var cb = new System.Windows.Input.CommandBinding(ctrlSpace, OnCtrlSpaceCommand);
            CommandBindings.Add(cb);
#endif


            searchPanel = ICSharpCode.AvalonEdit.Search.SearchPanel.Install(TextArea);


            // searchPanel.
        }

        public void SetFont(System.Drawing.Font font)
        {
            FontFamily = font.FontFamily.Name == "Cambria"
                ? new System.Windows.Media.FontFamily(Config.MathCustomFonts.GetMathFont(font.Size).FontFamily.Name)
                : new System.Windows.Media.FontFamily(font.FontFamily.Name);
            FontSize = font.Size;
            //this.FontWeight =  FontWeights.
            FontStyle = ConvertFontStyle(CreateFontStyle(font));
        }

        private static System.Drawing.FontStyle CreateFontStyle(System.Drawing.Font font)
        {
            if (font.Italic)
                return System.Drawing.FontStyle.Italic;
            return font.Bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular;
        }

        private static System.Windows.FontStyle ConvertFontStyle(System.Drawing.FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case System.Drawing.FontStyle.Bold:
                    return System.Windows.FontStyles.Oblique;
                case System.Drawing.FontStyle.Italic:
                    return System.Windows.FontStyles.Italic;
                default:
                    return System.Windows.FontStyles.Normal;
            }
        }

        private void TextArea_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftCtrl)
                ctrlPressed = false;
        }

        private void TextArea_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftCtrl)
                ctrlPressed = true;
        }

        private void ExperimentalCodeEditor_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (ctrlPressed)
            {
                if (e.Delta > 0)
                    FontSize++;
                else if (e.Delta < 0 && FontSize > 1)
                    FontSize--;
                e.Handled = true;
            }
        }

        private char GetCharFromKey(System.Windows.Input.Key key)
        {
            var ch = ' ';

            var virtualKey = System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            NativeMethods.GetKeyboardState(keyboardState);

            var scanCode = NativeMethods.MapVirtualKey((uint) virtualKey, NativeMethods.MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new System.Text.StringBuilder(2);

            var result = NativeMethods.ToUnicode((uint) virtualKey, scanCode, keyboardState, stringBuilder,
                stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                {
                    ch = stringBuilder[0];
                    break;
                }
                default:
                {
                    ch = stringBuilder[0];
                    break;
                }
            }
            return ch;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.D6 &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) ==
                System.Windows.Input.ModifierKeys.Shift)
            {
                IsExponentMode = !IsExponentMode;

                e.Handled = true;
                return;
            }

            if (IsExponentMode)
            {
                var ch = GetCharFromKey(e.Key);
                if (DataTypes.SpecialSymbols.IsAscii(ch))
                {
                    var str = DataTypes.SpecialSymbols.AsciiToSuperscript(ch.ToString());
                    if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                    {
                    }
                    else
                        TextArea.PerformTextInput(str);
                }
                e.Handled = true;
            }
            else
            {
                if (e.Key == System.Windows.Input.Key.Multiply)
                    //     e = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, GlobalConfig.dotSymbol);
                {
                    TextArea.PerformTextInput(DataTypes.SpecialSymbols.DotSymbol + "");
                    e.Handled = true;
                }
                else
                    base.OnKeyDown(e);
            }
        }

        private void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var tb = (AvalonEditCodeEditor) sender;
            using (tb.DeclareChangeBlock())
            {
                foreach (var c in e.Changes)
                {
                    if (c.AddedLength == 0) continue;
                    tb.Select(c.Offset, c.AddedLength);
                    if (Enumerable.Contains(tb.SelectedText, '*'))
                    {
                        tb.SelectedText = tb.SelectedText.Replace('*', DataTypes.SpecialSymbols.DotSymbol);
                    }
                    tb.Select(c.Offset + c.AddedLength, 0);
                }
            }
        }

        #region Code Completion

        private void OnTextEntered(object sender, System.Windows.Input.TextCompositionEventArgs textCompositionEventArgs)
        {
            if (char.IsLetterOrDigit(Enumerable.Last(textCompositionEventArgs.Text)))
                ShowCompletion(textCompositionEventArgs.Text, false);
        }

        //private char[] enteredText = new char[]{' ',' '};

        private void OnCtrlSpaceCommand(object sender,
            System.Windows.Input.ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            ShowCompletion("", true);
        }

        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            if (completionWindow != null) return;

            completionWindow = new ICSharpCode.AvalonEdit.CodeCompletion.CompletionWindow(TextArea)
            {
                CloseWhenCaretAtBeginning = controlSpace,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStyle = FontStyle
            };
            completionWindow.StartOffset -= enteredText.Length;
            // completionWindow.EndOffset -= enteredText.Length;
            foreach (var completionData in completionDatas)
            {
                if (controlSpace || completionData.Text.ToLower().Contains(enteredText.ToLower()))
                    completionWindow.CompletionList.CompletionData.Add(completionData);
            }
            // CodeCompletionResult results = null;
            //   try
            {
                var offset = 0;
                var doc = GetCompletionDocument(out offset);
                //       results = Completion.GetCompletions(doc, offset, controlSpace);
                //  }

                //  if (results == null)
                //       return;

                //   if (completionWindow == null && results != null && results.CompletionData.Any())
                //  {
                // Open code completion after the user has pressed dot:
                //      completionWindow = new CompletionWindow(TextArea);
                //     completionWindow.CloseWhenCaretAtBeginning = controlSpace;
                // completionWindow.StartOffset -= offset;
                // completionWindow.EndOffset -= offset;

                //     IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                //      foreach (var completion in results.CompletionData.OrderBy(item => item.Text))
                //     {
                //       data.Add(completion);
                //    }
                //    if (results.TriggerWordLength > 0)
                //   {
                //completionWindow.CompletionList.IsFiltering = false;
                //       completionWindow.CompletionList.SelectItem(results.TriggerWord);
                // }
                completionWindow.Show();
                completionWindow.Closed += (o, args) => completionWindow = null;
            }
        } //end method

        private void OnTextEntering(object sender,
            System.Windows.Input.TextCompositionEventArgs textCompositionEventArgs)
        {
            // Debug.WriteLine("TextEntering: " + textCompositionEventArgs.Text);
            if (textCompositionEventArgs.Text.Length <= 0 || completionWindow == null) return;
            if (char.IsLetterOrDigit(textCompositionEventArgs.Text[0])) return;
            // Whenever a non-letter is typed while the completion window is open,
            // insert the currently selected element.

            if (textCompositionEventArgs.Text == System.Environment.NewLine || textCompositionEventArgs.Text == "\u000B" ||
                textCompositionEventArgs.Text == "\u0009" || textCompositionEventArgs.Text == "\t")
                completionWindow.CompletionList.RequestInsertion(textCompositionEventArgs);
            else
            {
                //completionWindow = null;
                completionWindow.Close();
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        /// <summary>
        ///     Gets the document used for code completion, can be overridden to provide a custom document
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>The document of this text editor.</returns>
        protected virtual ICSharpCode.AvalonEdit.Document.IDocument GetCompletionDocument(out int offset)
        {
            offset = CaretOffset;
            return Document;
        }

        #endregion

        #region Folding

        private ICSharpCode.AvalonEdit.Folding.FoldingManager foldingManager;
        private object foldingStrategy;
        public bool IsExponentMode { get; set; }


        private void UpdateFoldings()
        {
            if (foldingStrategy is BraceFoldingStrategy)
            {
                ((BraceFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager, Document);
            }
            if (foldingStrategy is ICSharpCode.AvalonEdit.Folding.XmlFoldingStrategy)
            {
                ((ICSharpCode.AvalonEdit.Folding.XmlFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager,
                    Document);
            }
        }

        #endregion
    }
}