using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Controls
{

    public delegate void DirectorySelectedDelegate(object sender, DirectorySelectedEventArgs e);
    public interface IDirectoryTree
    {
        string Path { get; set; }
        IDocumentsEditor CodeEditorWrapper { get; set; }
        event DirectorySelectedDelegate DirectorySelected;
    }
}