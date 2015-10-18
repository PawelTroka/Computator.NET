using System.Windows.Forms;

namespace Computator.NET.UI.CodeEditors
{
    internal interface ICodeEditorControl
    {
        string Text { get; set; }

        void Undo();
        void Redo();
        void Cut();
        void Paste();
        void Copy();

        void SelectAll();

    }
}