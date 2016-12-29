using System;
using Computator.NET.Core.Abstract.Controls;

namespace Computator.NET.Core.Abstract.Views
{
    public interface ICustomFunctionsView
    {
        ISolutionExplorerView SolutionExplorerView { get; }
        ICodeDocumentsEditor CustomFunctionsEditor { get; }
        event EventHandler Load;
    }
}