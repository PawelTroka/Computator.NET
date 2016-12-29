using Computator.NET.Core.Abstract.Controls;

namespace Computator.NET.Core.Abstract.Views
{
    public interface ISolutionExplorerView
    {
        IDirectoryTree DirectoryTree { get; }
        event DirectorySelectedDelegate DirectoryChanged;
    }
}