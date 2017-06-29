using System.Collections.Generic;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Menu.Commands.EditCommands;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
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