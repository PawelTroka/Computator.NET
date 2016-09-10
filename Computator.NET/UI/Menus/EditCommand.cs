using System.Collections.Generic;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.EditCommands;

namespace Computator.NET.UI.Menus
{
    class EditCommand : DummyCommand
    {
        public EditCommand(IMainForm view, ICanFileEdit codeEditorView, ICanFileEdit customFunctionsEditor, ISharedViewState sharedViewState) : base(MenuStrings.editToolStripMenuItem1_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                new UndoCommand(codeEditorView, customFunctionsEditor,
                    view, sharedViewState),
                new RedoCommand(codeEditorView, customFunctionsEditor,
                    view, sharedViewState),
                null,
                new CutCommand(codeEditorView, customFunctionsEditor,
                    view, sharedViewState),
                new CopyCommand(codeEditorView, customFunctionsEditor,
                    view, sharedViewState),
                new PasteCommand(codeEditorView, customFunctionsEditor,
                    view, sharedViewState),
                null,
                new SelectAllCommand(codeEditorView,
                    customFunctionsEditor, view, sharedViewState),
                null,
                new ExponentCommand(sharedViewState)
            };
        }
    }
}