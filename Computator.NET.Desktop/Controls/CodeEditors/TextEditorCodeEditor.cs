using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using ICSharpCode.TextEditor.Document;

namespace Computator.NET.Controls.CodeEditors
{
    public class TextEditorCodeEditor : ICSharpCode.TextEditor.TextEditorControl, INotifyPropertyChanged, ICodeEditorControl
    {
        private ISharedViewState _sharedViewState;
        private readonly Dictionary<string, IDocument>
            _documents;

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public TextEditorCodeEditor(ISharedViewState sharedViewState)
        {
            _sharedViewState = sharedViewState;
            this.Dock=DockStyle.Fill;

            _documents =
                new Dictionary<string, IDocument>();


            string dir = PathUtility.GetFullPath("Static"); // Insert the path to your xshd-files.
            FileSyntaxModeProvider fsmProvider; // Provider
            if (Directory.Exists(dir))
            {
                fsmProvider = new FileSyntaxModeProvider(dir); // Create new provider with the highlighting directory.
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmProvider); // Attach to the text editor.
                base.SetHighlighting("VS-C#"); // Activate the highlighting, use the name from the SyntaxDefinition node.
            }

        }

        public void SetFont(Font font)
        {
            if (font.FontFamily.Name == "Cambria")
            {
                Font = CustomFonts.GetMathFont(font.Size);
            }
            else if (font.FontFamily.Name == "Consolas")
            {
                Font = CustomFonts.GetScriptingFont(font.Size);
            }
            else
            {
                Font = font;
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


            if (File.Exists(filename))
            {
                Document = new DocumentFactory().CreateFromFile(filename);
                /////Text = File.ReadAllText(filename);

                Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("VS-C#");

            }
            else
            {
                Document = new DocumentFactory().CreateDocument();

                Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("VS-C#");
                
            }

            _documents.Add(filename, Document);

            ///// InitDocument();
            UltraSuperRefreshMethod();
        }

        public void AppendText(string text)
        {
            //TODO: implement
        }

        public void SwitchDocument(string filename)
        {
            if (!_documents.ContainsKey(filename))
                return;

            var prevDocument = Document;

            Document = _documents[filename];
            UltraSuperRefreshMethod();
        }

        private void UltraSuperRefreshMethod()
        {
            this.Update();
            this.ResizeRedraw = true;
            this.UpdateStyles();
            this.Refresh();
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
            //TODO: implement
            // _offsetColorizer.LinesWithErrors.Clear();
            foreach (var error in errors)
            {
                // _offsetColorizer.LinesWithErrors.Add(error.Line);
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


        public void Cut()
        {
            ActiveTextAreaControl.TextArea.ClipboardHandler.Cut(this, EventArgs.Empty);
        }

        public void Paste()
        {
            ActiveTextAreaControl.TextArea.ClipboardHandler.Paste(this, EventArgs.Empty);
        }

        public void Copy()
        {
            ActiveTextAreaControl.TextArea.ClipboardHandler.Copy(this, EventArgs.Empty);
        }

        public void SelectAll()
        {
            ActiveTextAreaControl.TextArea.ClipboardHandler.SelectAll(this, EventArgs.Empty);
        }

        public void ClearHighlightedErrors()
        {
            //TODO: implement
            //  _offsetColorizer.LinesWithErrors.Clear();
            //  TextArea.TextView.Redraw();
        }

    }
}