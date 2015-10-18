#define NEW_AUTOCOMPLETE
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Data;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.AvalonEdit.Search;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Drawing.FontStyle;



namespace Computator.NET.UI.CodeEditors
{
    internal class AvalonEditCodeEditor : TextEditor
    {
        private readonly List<CompletionData> completionDatas;
        //  void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        //{
        //  if(char.IsLetterOrDigit(e.Text[0]))
        //    completionWindow.Show();
        //}
        public AvalonEditCodeEditorControl host;
        private CompletionWindow completionWindow;
        private bool ctrlPressed;
        private HighlightingManager highlightingManager;
        protected OverloadInsightWindow insightWindow;
        private SearchPanel searchPanel;

        public AvalonEditCodeEditor()
        {
            completionDatas = AutocompletionData.ConvertAutocompleteItemsToCompletionDatas(AutocompletionData.GetAutocompleteItemsForScripting());
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Undo();
            this.Redo();
            this.Paste();
            this.Cut();
            this.SelectAll();
            

            IHighlightingDefinition customHighlighting;

            using (XmlReader reader = new XmlTextReader(GlobalConfig.FullPath("UI", "CodeEditors", "TSL-Syntax.xshd")))
            {
                customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            HighlightingManager.Instance.RegisterHighlighting("Custom Highlighting", new[] {".cool"}, customHighlighting);

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

            TextArea.IndentationStrategy = new CSharpIndentationStrategy(Options);
            foldingStrategy = new BraceFoldingStrategy();
            foldingManager = FoldingManager.Install(TextArea);

            var foldingUpdateTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
            foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
            foldingUpdateTimer.Start();

#if !NEW_AUTOCOMPLETE
            TextArea.TextEntering += OnTextEntering;
            TextArea.TextEntered += OnTextEntered;
            var ctrlSpace = new RoutedCommand();
            ctrlSpace.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
            var cb = new CommandBinding(ctrlSpace, OnCtrlSpaceCommand);
            CommandBindings.Add(cb);
#endif



            searchPanel = SearchPanel.Install(TextArea);


            // searchPanel.
        }

        public void SetFont(Font font)
        {
             this.FontFamily = font.FontFamily.Name == "Cambria" ? new FontFamily( MathCustomFonts.GetMathFont(font.Size).FontFamily.Name) : new FontFamily(font.FontFamily.Name);
             this.FontSize = font.Size;
            //this.FontWeight =  FontWeights.
            this.FontStyle = ConvertFontStyle(CreateFontStyle(font));



        }

        private static FontStyle CreateFontStyle(Font font)
        {
            if(font.Italic)
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



            // ReSharper disable InconsistentNaming
            public enum MapType : uint
            {
                MAPVK_VK_TO_VSC = 0x0,
                MAPVK_VSC_TO_VK = 0x1,
                MAPVK_VK_TO_CHAR = 0x2,
                MAPVK_VSC_TO_VK_EX = 0x3,
            }
            // ReSharper restore InconsistentNaming

            [DllImport("user32.dll")]
            public static extern int ToUnicode(
                uint wVirtKey,
                uint wScanCode,
                byte[] lpKeyState,
                [Out, MarshalAs( UnmanagedType.LPWStr, SizeParamIndex = 4 )]
        StringBuilder pwszBuff,
                int cchBuff,
                uint wFlags);

            [DllImport("user32.dll")]
            public static extern bool GetKeyboardState(byte[] lpKeyState);

            [DllImport("user32.dll")]
            public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

            private char GetCharFromKey(Key key)
            {
                char ch = ' ';

                int virtualKey = KeyInterop.VirtualKeyFromKey(key);
                var keyboardState = new byte[256];
                GetKeyboardState(keyboardState);

                uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
                var stringBuilder = new StringBuilder(2);

                int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
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
            if (e.Key == Key.D6 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                _IsExponentMode = !_IsExponentMode;
                
                e.Handled = true;
                return;
            }

            if (_IsExponentMode)
            {

                var ch = GetCharFromKey(e.Key);
                if (SpecialSymbols.IsAscii(ch))
                {
                    var str = SpecialSymbols.AsciiToSuperscript(ch.ToString());
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

        private void OnCtrlSpaceCommand(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            ShowCompletion("", true);
        }

        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            if (completionWindow != null) return;

            completionWindow = new CompletionWindow(TextArea) {CloseWhenCaretAtBeginning = controlSpace,
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStyle = this.FontStyle
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

        private void OnTextEntering(object sender, TextCompositionEventArgs textCompositionEventArgs)
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

        private FoldingManager foldingManager;
        private object foldingStrategy;
        private bool _IsExponentMode;


        private void UpdateFoldings()
        {
            if (foldingStrategy is BraceFoldingStrategy)
            {
                ((BraceFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager, Document);
            }
            if (foldingStrategy is XmlFoldingStrategy)
            {
                ((XmlFoldingStrategy) foldingStrategy).UpdateFoldings(foldingManager, Document);
            }
        }

        #endregion
    }
}