namespace Computator.NET.UI.Views
{
    public interface ISolutionExplorerView
    {
        IDirectoryTree DirectoryTree { get; }
        event DirectoryTree.DirectorySelectedDelegate DirectoryChanged;
    }
}