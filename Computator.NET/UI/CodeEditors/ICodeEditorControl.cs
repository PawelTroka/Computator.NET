using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;

namespace Computator.NET.UI.CodeEditors
{

    public interface ISupportsExceptionHighliting
    {
        void ClearHighlightedErrors();
        void HighlightErrors(IEnumerable<CompilerError> compilerErrors);
    }

    public interface ICodeEditorView : ITextProvider, ISupportsExceptionHighliting
    {

    }


    public interface ITextProvider
    {
        string Text { get; set; }
    }

    public interface IDocumentsEditor
    {
        void RemoveTab(string fullPath);
        bool ContainsDocument(string oldPath);
        void RenameDocument(string oldPath, string fullPath);
        void NewDocument(string fullPath);
        string CurrentFileName { get; set; }
        void SwitchTab(string fullPath);
    }

    public interface ICodeDocumentsEditor : IDocumentsEditor, ICodeEditorView
    {
        
    }

    public interface ICodeEditorControl
    {
        string Text { get; set; }
        bool ExponentMode { get; set; }
        IEnumerable<string> Documents { get; }
        void Undo();
        void Redo();
        void Cut();
        void Paste();
        void Copy();
        void SelectAll();
        void AppendText(string text);
        void SwitchDocument(string filename);
        void CloseDocument(string filename);
        void NewDocument(string text);
        void RenameDocument(string filename, string newFilename);
        void HighlightErrors(IEnumerable<CompilerError> errors);
        bool ContainsDocument(string filename);
        void ClearHighlightedErrors();
        void SetFont(Font font);
    }
}