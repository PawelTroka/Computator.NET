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
using Computator.NET.DataTypes.Text;
using Computator.NET.DataTypes.Utility;
using ICSharpCode.TextEditor.Document;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    public class TextEditorCodeEditor : ICSharpCode.TextEditor.TextEditorControl, INotifyPropertyChanged, ICodeEditorControl
    {
        private ISharedViewState _sharedViewState;
        private readonly Dictionary<string, IDocument>
            _documents;
        private readonly HashSet<IDocument> _highlightedDocuments = new HashSet<IDocument>();

        private sealed class CompilationErrorMarker : TextMarker
        {
            public CompilationErrorMarker(int offset, int length, string message)
                : base(offset, length, TextMarkerType.SolidBlock, Color.MistyRose)
            {
                ToolTip = message;
            }
        }

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

            var text = File.Exists(filename) ? File.ReadAllText(filename) : string.Empty;
            NewDocument(filename, text);
        }

        public void NewDocument(string filename, string text)
        {
            if (_documents.ContainsKey(filename))
                return;

            var document = new DocumentFactory().CreateDocument();
            document.TextContent = text;
            document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("VS-C#");
            Document = document;

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
            ClearHighlightedErrors(doc);
            _documents.Remove(filename);
            //this.ReleaseDocument(doc);
            if (_documents.Count > 0)
                SwitchDocument(_documents.Keys.Last());
        }

        public void HighlightErrors(IEnumerable<CompilerError> errors)
        {
            ClearHighlightedErrors();
            if (errors == null)
                return;

            var document = Document;
            var errorsByLine = errors
                .Where(error => error != null && error.Line > 0 && error.Line <= document.TotalNumberOfLines)
                .GroupBy(error => error.Line)
                .ToArray();
            foreach (var lineErrors in errorsByLine)
            {
                // Compiler diagnostics use one-based lines; the editor uses zero-based lines.
                var line = document.GetLineSegment(lineErrors.Key - 1);
                if (line.Length == 0)
                    continue;

                var message = string.Join(Environment.NewLine, lineErrors.Select(error =>
                    string.IsNullOrEmpty(error.ErrorNumber) ? error.ErrorText : error.ErrorNumber + ": " + error.ErrorText)
                    .Distinct());
                document.MarkerStrategy.AddMarker(new CompilationErrorMarker(line.Offset, line.Length, message));
                if (_highlightedDocuments.Add(document))
                    document.DocumentChanged += ClearStaleErrorHighlights;
            }

            ActiveTextAreaControl.TextArea.Invalidate();
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
            ClearHighlightedErrors(Document);
        }

        private void ClearStaleErrorHighlights(object sender, DocumentEventArgs e)
        {
            // A diagnostic describes the source that was compiled, not a subsequently edited buffer.
            ClearHighlightedErrors((IDocument)sender);
        }

        private void ClearHighlightedErrors(IDocument document, bool redraw = true)
        {
            if (!_highlightedDocuments.Remove(document))
                return;

            document.DocumentChanged -= ClearStaleErrorHighlights;
            // Search results and other editor adornments belong to their own features.
            document.MarkerStrategy.RemoveAll(marker => marker is CompilationErrorMarker);
            if (redraw && ReferenceEquals(Document, document) && !IsDisposed && !Disposing)
                ActiveTextAreaControl.TextArea.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (var document in _highlightedDocuments.ToArray())
                    ClearHighlightedErrors(document, false);

            base.Dispose(disposing);
        }

    }
}
