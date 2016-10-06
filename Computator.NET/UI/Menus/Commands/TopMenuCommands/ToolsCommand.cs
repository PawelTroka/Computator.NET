using System;
using System.Collections.Generic;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.CommandsWithOptions;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.ToolsCommands;

namespace Computator.NET.UI.Menus
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