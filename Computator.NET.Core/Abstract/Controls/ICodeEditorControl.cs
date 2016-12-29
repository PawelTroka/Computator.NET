using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;

namespace Computator.NET.Core.Abstract.Controls
{
    public interface ISupportsExceptionHighliting
    {
        void ClearHighlightedErrors();
        void HighlightErrors(IEnumerable<CompilerError> compilerErrors);
    }

    public interface ICodeEditorView : IScriptProvider, ISupportsExceptionHighliting
    {
    }

    public interface IScriptProvider
    {
        string Text { get; set; }
    }

    public interface ICanOpenFiles
    {
        void NewDocument(string fullPath);
    }

    public interface IDocumentsEditor : ICanOpenFiles
    {
        string CurrentFileName { get; set; }
        void RemoveTab(string fullPath);
        bool ContainsDocument(string oldPath);
        void RenameDocument(string oldPath, string fullPath);

        void SwitchTab(string fullPath);
    }


    public interface ICanFileEdit : ICanOpenFiles
    {
        bool Focused { get; }
        void NewDocument();
        void Save();
        void SaveAs();
        void Undo();
        void Redo();
        void Cut();
        void Paste();
        void Copy();
        void SelectAll();
    }

    public interface ICodeDocumentsEditor : IDocumentsEditor, ICodeEditorView, ICanFileEdit
    {
    }

    public interface ICodeEditorControl
    {
        string Text { get; set; }
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