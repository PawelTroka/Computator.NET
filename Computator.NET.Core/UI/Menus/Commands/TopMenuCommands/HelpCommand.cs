using System.Collections.Generic;
using Computator.NET.Core.UI.Menus;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.HelpCommands;

namespace Computator.NET.UI.Menus
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