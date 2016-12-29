using System.Collections.Generic;
using Computator.NET.Core.Menu.Commands.CommandsWithOptions;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Menu.Commands.ToolsCommands;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
{
    public class ToolsCommand : DummyCommand
    {
        public ToolsCommand(OptionsCommand optionsCommand, LanguageCommand languageCommand, FullScreenCommand fullScreenCommand, BenchmarkCommand benchmarkCommand, LogsCommand logsCommand) : base(MenuStrings.toolsToolStripMenuItem_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                optionsCommand,
                languageCommand,
                fullScreenCommand,
                null,
                benchmarkCommand,
                null,
                logsCommand
            };
        }
    }
}