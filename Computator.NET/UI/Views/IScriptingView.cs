using System;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public interface IScriptingView
    {
        ICodeDocumentsEditor CodeEditorView { get; }
        ISolutionExplorerView SolutionExplorerView { get; }

        event EventHandler ProcessClicked;

        string ConsoleOutput { set; }
        void AppendToConsole(string output);
    }
}