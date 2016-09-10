using System.Collections.Generic;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.CommandsWithOptions;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.ToolsCommands;

namespace Computator.NET.UI.Menus
{
    class ToolsCommand : DummyCommand
    {
        public ToolsCommand(IMainForm view) : base(MenuStrings.toolsToolStripMenuItem_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                new OptionsCommand(),
                new LanguageCommand(),
                new FullScreenCommand(view),
                null,
                new BenchmarkCommand(),
                null,
                new LogsCommand()
            };
        }
    }
}