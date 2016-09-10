using System.Collections.Generic;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.FileCommands;

namespace Computator.NET.UI.Menus
{
    class FileCommand : DummyCommand
    {

        public FileCommand(IMainForm view, ICanFileEdit codeEditorView, ICanFileEdit customFunctionsEditor, ISharedViewState sharedViewState) : base(MenuStrings.fileToolStripMenuItem_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                new NewCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                new OpenCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                null,
                new SaveCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                new SaveAsCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                null,
                new PrintCommand(codeEditorView, customFunctionsEditor,
                    view, sharedViewState),
                new PrintPreviewCommand(codeEditorView,
                    customFunctionsEditor, view, sharedViewState),
                null,
                new ExitCommand()
            };
        }
    }
}