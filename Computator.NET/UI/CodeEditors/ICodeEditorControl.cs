using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Computator.NET.UI.CodeEditors
{
    public interface ICodeEditorControl
    {
        void ClearHighlightedErrors();
        string Text { get; set; }
        bool ExponentMode { get; set; }
        void Undo();
        void Redo();
        void Cut();
        void Paste();
        void Copy();
        void SelectAll();
        void AppendText(string text);
        void RenameDocument(string filename, string newFilename);
        bool ContainsDocument(string filename);
        void NewDocument(string filename);
        void SwitchDocument(string filename);
        void CloseDocument(string filename);

        void HighlightErrors(List<CompilerError> errors);
        IEnumerable<string> Documents { get; }
    }
}