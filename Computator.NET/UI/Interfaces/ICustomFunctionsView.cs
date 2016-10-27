using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Interfaces
{
    public interface ICustomFunctionsView
    {
        ISolutionExplorerView SolutionExplorerView { get; }
        ICodeDocumentsEditor CustomFunctionsEditor { get; }
    }
}