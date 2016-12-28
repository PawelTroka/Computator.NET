using System.Collections.Generic;
using Computator.NET.Core.Menu;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.FileCommands;

namespace Computator.NET.UI.Menus
{
    public class FileCommand : DummyCommand
    {

        public FileCommand(NewCommand newCommand, OpenCommand openCommand, SaveCommand saveCommand, SaveAsCommand saveAsCommand,
            PrintCommand printCommand, PrintPreviewCommand printPreviewCommand, ExitCommand exitCommand) : base(MenuStrings.fileToolStripMenuItem_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                newCommand,
                openCommand,
                null,
                saveCommand,
                saveAsCommand,
                null,
                printCommand,
                printPreviewCommand,
                null,
                exitCommand
            };
        }
    }
}