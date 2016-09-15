using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.FileCommands;
using Computator.NET.UI.Models;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Menus
{
    public class FileCommand : DummyCommand
    {

        public FileCommand(ICanFileEdit codeEditorView, ICanFileEdit customFunctionsEditor, ISharedViewState sharedViewState, ApplicationManager applicationManager, ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.fileToolStripMenuItem_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                new NewCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                new OpenCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                null,
                new SaveCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                new SaveAsCommand(codeEditorView, customFunctionsEditor, sharedViewState),
                null,
                new PrintCommand(codeEditorView, customFunctionsEditor, sharedViewState, applicationManager,charts),
                new PrintPreviewCommand(codeEditorView,
                    customFunctionsEditor, sharedViewState,applicationManager,charts),
                null,
                new ExitCommand()
            };
        }
    }
}