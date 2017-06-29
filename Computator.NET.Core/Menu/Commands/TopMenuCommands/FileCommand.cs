using System.Collections.Generic;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
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