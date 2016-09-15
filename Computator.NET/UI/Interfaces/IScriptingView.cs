using System;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Interfaces
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