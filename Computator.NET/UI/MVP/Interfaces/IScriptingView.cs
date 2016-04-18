using System;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public interface IScriptingView
    {
        ICodeDocumentsEditor CodeEditorView { get; }
        ISolutionExplorerView SolutionExplorerView { get; }

        string ConsoleOutput { set; }

        event EventHandler ProcessClicked;
        void AppendToConsole(string output);
    }
}