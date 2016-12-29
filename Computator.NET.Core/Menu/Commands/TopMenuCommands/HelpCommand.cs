using System.Collections.Generic;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Menu.Commands.HelpCommands;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
{
    public class HelpCommand : DummyCommand
    {
        public HelpCommand(FeaturesCommand featuresCommand, ChangelogCommand changelogCommand, AboutCommand aboutCommand, ThanksToCommand thanksToCommand, BugReportingCommand bugReportingCommand) : base(MenuStrings.helpToolStripMenuItem1_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                featuresCommand,
                changelogCommand,
                null,
                aboutCommand,
                null,
                thanksToCommand,
                bugReportingCommand
            };
        }
    }
}