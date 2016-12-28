using Computator.NET.UI.Controls;

namespace Computator.NET.UI.Interfaces
{
    public interface ISolutionExplorerView
    {
        IDirectoryTree DirectoryTree { get; }
        event DirectorySelectedDelegate DirectoryChanged;
    }
}