namespace Computator.NET.UI.CodeEditors
{
    internal interface ICodeEditorControl
    {
        string Text { get; set; }
        bool ExponentMode { get; set; }
        string SaveAs(string filename);
        void Undo();
        void Redo();
        void Cut();
        void Paste();
        void Copy();
        void SelectAll();
        void AppendText(string text);
        string SaveDocument(string filename);
        bool ContainsDocument(string filename);
        void NewDocument(string filename);
        void SwitchDocument(string filename);
        void CloseDocument(string filename);
    }
}