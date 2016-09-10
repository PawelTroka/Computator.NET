using System.Collections.Generic;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.HelpCommands;

namespace Computator.NET.UI.Menus
{
    class HelpCommand : DummyCommand
    {
        public HelpCommand() : base(MenuStrings.helpToolStripMenuItem1_Text)
        {
            ChildrenCommands = new List<IToolbarCommand>
            {
                new FeaturesCommand(),
                new ChangelogCommand(),
                null,
                new AboutCommand(),
                null,
                new ThanksToCommand(),
                new BugReportingCommand()
            };
        }
    }
}