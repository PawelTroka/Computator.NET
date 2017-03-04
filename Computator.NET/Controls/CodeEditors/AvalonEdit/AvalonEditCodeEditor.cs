#if !__MonoCS__
//#define NEW_AUTOCOMPLETE
//#define USE_FOLDING
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.AvalonEdit.Search;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Drawing.FontStyle;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace Computator.NET.Controls.CodeEditors.AvalonEdit
{
    public class AvalonEditCodeEditor : TextEditor, ICodeEditorControl, INotifyPropertyChanged
    {
        private ISharedViewState _sharedViewState;
        private readonly Dictionary<string, TextDocument>
            _documents;

        private readonly List<CompletionData> completionDatas;
        private CompletionWindow completionWindow;
        private bool ctrlPressed;
        /////////////////////private HighlightingManager highlightingManager;//TODO
        //void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        //{
        //  if(char.IsLetterOrDigit(e.Text[0]))
        //    completionWindow.Show();
        //}
        // public AvalonEditCodeEditorControl host;

        //protected OverloadInsightWindow insightWindow;
        private SearchPanel searchPanel;
        private readonly OffsetColorizer _offsetColorizer = new OffsetColorizer();

        public AvalonEditCodeEditor(ISharedViewState sharedViewState, IFunctionsDetails functionsDetails)
        {
            _sharedViewState = sharedViewState;
            completionDatas =
                CompletionDataConverter.ConvertAutocompleteItemsToCompletionDatas(
                    AutocompletionData.GetAutocompleteItemsForScripting(functionsDetails));
            InitializeComponent();
            _documents =
                new Dictionary<string, TextDocument>();
            TextArea.TextView.LineTransformers.Add(_offsetColorizer);
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

            Document = new TextDocument();

            if (File.Exists(filename))
                Text = File.ReadAllText(filename);

            _documents.Add(filename, Document);

            InitDocument();
        }

        public void SwitchDocument(string filename)
        {
            if (!_documents.ContainsKey(filename))
                return;

            var prevDocument = Document;

            Document = _documents[filename];
        }

        public void CloseDocument(string filename)
        {
            if (!_documents.ContainsKey(filename))
                return;

            var doc = _documents[filename];
            _documents.Remove(filename);
            //this.ReleaseDocument(doc);
            if (_documents.Count > 0)
                SwitchDocument(_documents.Keys.Last());
        }

        public void HighlightErrors(IEnumerable<CompilerError> errors)
        {
            _offsetColorizer.LinesWithErrors.Clear();
            foreach (var error in errors)
            {
                _offsetColorizer.LinesWithErrors.Add(error.Line);
            }
        }

        public IEnumerable<string> Documents
        {
            get { return _documents.Keys.ToList(); }
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

        public void ClearHighlightedErrors()
        {
            _offsetColorizer.LinesWithErrors.Clear();
            TextArea.TextView.Redraw();
        }

        public string SaveAs(string filename)
        {
            var dg = new SaveFileDialog {FileName = filename};
            if (dg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dg.FileName, Text);

                if (dg.FileName != filename)
                    _documents.RenameKey(filename, dg.FileName);

                return dg.FileName;
            }
            return null;
        }

        public string SaveDocument(string filename)
        {
            if (!File.Exists(filename))
            {
                var dg = new SaveFileDialog {FileName = filename};
                if (dg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dg.FileName, Text);

                    if (dg.FileName != filename)
                        _documents.RenameKey(filename, dg.FileName);

                    return dg.FileName;
                }
                return null;
            }
            File.WriteAllText(filename, Text);
            return filename;
        }

        private void InitDocument()
        {
            //throw new NotImplementedException();
        }

        private void InitializeComponent()
        {
            IHighlightingDefinition customHighlighting;

            using (
                XmlReader reader =
                    new XmlTextReader(PathUtility.GetFullPath("Static", "TSL-Syntax.xshd")))
            {
                customHighlighting = HighlightingLoader.Load(reader,
                    HighlightingManager.Instance);
            }
            HighlightingManager.Instance.RegisterHighlighting(
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
                new CSharpIndentationStrategy(Options);

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
            var ctrlSpace = new RoutedCommand();
            ctrlSpace.InputGestures.Add(new KeyGesture(Key.Space,
                ModifierKeys.Control));
            var cb = new CommandBinding(ctrlSpace, OnCtrlSpaceCommand);
            CommandBindings.Add(cb);
#endif


            searchPanel = SearchPanel.Install(TextArea);


            // searchPanel.
        }

        public void SetFont(Font font)
        {
            if (font.FontFamily.Name == "Cambria")
                FontFamily = new FontFamily(CustomFonts.GetMathFont(font.Size).FontFamily.Name);
            else if (font.FontFamily.Name == "Consolas")
                FontFamily = new FontFamily(CustomFonts.GetScriptingFont(font.Size).FontFamily.Name);
            else
                FontFamily = new FontFamily(font.FontFamily.Name);
            FontSize = font.Size;
            //this.FontWeight =  FontWeights.
            FontStyle = ConvertFontStyle(CreateFontStyle(font));
        }

        private static FontStyle CreateFontStyle(Font font)
        {
            if (font.Italic)
                return System.Drawing.FontStyle.Italic;
            return font.Bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular;
        }

        private static System.Windows.FontStyle ConvertFontStyle(FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case System.Drawing.FontStyle.Bold:
                    return FontStyles.Oblique;
                case System.Drawing.FontStyle.Italic:
                    return FontStyles.Italic;
                default:
                    return FontStyles.Normal;
            }
        }

        private void TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                ctrlPressed = false;
        }

        private void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                ctrlPressed = true;
        }

        private void ExperimentalCodeEditor_MouseWheel(object sender, MouseWheelEventArgs e)
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

        private char GetCharFromKey(Key key)
        {
            var ch = ' ';

            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            NativeMethods.GetKeyboardState(keyboardState);

            var scanCode = NativeMethods.MapVirtualKey((uint) virtualKey, NativeMethods.MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);

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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.D6 &&
                (Keyboard.Modifiers & ModifierKeys.Shift) ==
                ModifierKeys.Shift)
            {
                _sharedViewState.IsExponent = !_sharedViewState.IsExponent;

                e.Handled = true;
                return;
            }

            if (_sharedViewState.IsExponent)
            {
                var ch = GetCharFromKey(e.Key);
                if (SpecialSymbols.IsAscii(ch))
                {
                    var str = SpecialSymbols.AsciiToSuperscript(ch.ToString());
                    if (string.IsNullOrWhiteSpace(str))
                    {
                    }
                    else
                        TextArea.PerformTextInput(str);
                }
                e.Handled = true;
            }
            else
            {
                if (e.Key == Key.Multiply)
                    //     e = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, GlobalConfig.dotSymbol);
                {
                    TextArea.PerformTextInput(SpecialSymbols.DotSymbol + "");
                    e.Handled = true;
                }
                else
                    base.OnKeyDown(e);
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (AvalonEditCodeEditor) sender;
            using (tb.DeclareChangeBlock())
            {
                foreach (var c in e.Changes)
                {
                    if (c.AddedLength == 0) continue;
                    tb.Select(c.Offset, c.AddedLength);
                    if (tb.SelectedText.Contains('*'))
                    {
                        tb.SelectedText = tb.SelectedText.Replace('*', SpecialSymbols.DotSymbol);
                    }
                    tb.Select(c.Offset + c.AddedLength, 0);
                }
            }
        }

        #region Code Completion

        private void OnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            if (char.IsLetterOrDigit(textCompositionEventArgs.Text.Last()))
                ShowCompletion(textCompositionEventArgs.Text, false);
        }

        //private char[] enteredText = new char[]{' ',' '};

        private void OnCtrlSpaceCommand(object sender,
            ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            ShowCompletion("", true);
        }

        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            if (completionWindow != null) return;

            completionWindow = new CompletionWindow(TextArea)
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
            TextCompositionEventArgs textCompositionEventArgs)
        {
            // Debug.WriteLine("TextEntering: " + textCompositionEventArgs.Text);
            if (textCompositionEventArgs.Text.Length <= 0 || completionWindow == null) return;
            if (char.IsLetterOrDigit(textCompositionEventArgs.Text[0])) return;
            // Whenever a non-letter is typed while the completion window is open,
            // insert the currently selected element.

            if (textCompositionEventArgs.Text == Environment.NewLine || textCompositionEventArgs.Text == "\u000B" ||
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
        protected virtual IDocument GetCompletionDocument(out int offset)
        {
            offset = CaretOffset;
            return Document;
        }

        #endregion

        #region Folding

#if USE_FOLDING
        private FoldingManager foldingManager;
        private object foldingStrategy;

        private void UpdateFoldings()
        {
            if (foldingStrategy is BraceFoldingStrategy)
            {
                ((BraceFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager, Document);
            }
            if (foldingStrategy is XmlFoldingStrategy)
            {
                ((XmlFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager,
                    Document);
            }
        }
#endif

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
#endif