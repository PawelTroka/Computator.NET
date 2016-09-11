using System.Collections.Generic;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.EditCommands;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Menus
{
    public class EditCommand : DummyCommand
    {
        public EditCommand(ICanFileEdit codeEditorView, ICanFileEdit customFunctionsEditor, ISharedViewState sharedViewState, IApplicationManager applicationManager) : base(MenuStrings.editToolStripMenuItem1_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                new UndoCommand(codeEditorView, customFunctionsEditor,sharedViewState,applicationManager),
                new RedoCommand(codeEditorView, customFunctionsEditor,sharedViewState,applicationManager),
                null,
                new CutCommand(codeEditorView, customFunctionsEditor,sharedViewState,applicationManager),
                new CopyCommand(codeEditorView, customFunctionsEditor,
                     sharedViewState,applicationManager),
                new PasteCommand(codeEditorView, customFunctionsEditor, sharedViewState,applicationManager),
                null,
                new SelectAllCommand(codeEditorView,
                    customFunctionsEditor,sharedViewState,applicationManager),
                null,
                new ExponentCommand(sharedViewState)
            };
        }
    }
}