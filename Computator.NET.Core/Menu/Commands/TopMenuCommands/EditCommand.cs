using System.Collections.Generic;
using Computator.NET.Core.Menu;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.EditCommands;

namespace Computator.NET.UI.Menus
{
    public class EditCommand : DummyCommand
    {
        public EditCommand(UndoCommand undoCommand, RedoCommand redoCommand, CutCommand cutCommand,
            CopyCommand copyCommand, PasteCommand pasteCommand, SelectAllCommand selectAllCommand,
            ExponentCommand exponentCommand)
            : base(MenuStrings.editToolStripMenuItem1_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                undoCommand,
                redoCommand,
                null,
                cutCommand,
                copyCommand,
                pasteCommand,
                null,
                selectAllCommand,
                null,
                exponentCommand
            };
        }
    }
}