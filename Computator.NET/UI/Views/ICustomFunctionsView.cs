using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public interface ICustomFunctionsView
    {
        ISolutionExplorerView SolutionExplorerView { get; }
        ICodeDocumentsEditor CustomFunctionsEditor { get; }
    }
}