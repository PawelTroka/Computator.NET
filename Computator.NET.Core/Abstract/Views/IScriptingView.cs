using System;
using Computator.NET.Core.Abstract.Controls;

namespace Computator.NET.Core.Abstract.Views
{
    public interface IScriptingView
    {
        ICodeDocumentsEditor CodeEditorView { get; }
        ISolutionExplorerView SolutionExplorerView { get; }

        string ConsoleOutput { set; }

        event EventHandler ProcessClicked;
        event EventHandler Load;
        void AppendToConsole(string output);
    }
}