using Computator.NET.Core.Model;

namespace Computator.NET.Core.Abstract.Controls
{

    public delegate void DirectorySelectedDelegate(object sender, DirectorySelectedEventArgs e);
    public interface IDirectoryTree
    {
        string Path { get; set; }
        IDocumentsEditor CodeEditorWrapper { get; set; }
        event DirectorySelectedDelegate DirectorySelected;
    }
}