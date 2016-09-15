using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Controls
{
    public interface IDirectoryTree
    {
        string Path { get; set; }
        IDocumentsEditor CodeEditorWrapper { get; set; }
        event DirectoryTree.DirectorySelectedDelegate DirectorySelected;
    }
}