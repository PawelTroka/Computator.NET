using System;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public interface IScriptingView
    {
        ICodeEditorView CodeEditorView { get; }
        IDirectoryTree DirectoryTree { get; }

        event EventHandler ProcessClicked;
        event DirectoryTree.DirectorySelectedDelegate DirectoryChanged;
        string ConsoleOutput { set; }
        void AppendToConsole(string output);
    }
}