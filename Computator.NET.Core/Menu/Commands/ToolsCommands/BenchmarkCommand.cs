using Computator.NET.Core.Abstract;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ToolsCommands
{
    public class BenchmarkCommand : CommandBase
    {
        public BenchmarkCommand(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Benchmark_Text;
            ToolTip = MenuStrings.Benchmark_Text;
        }

        private readonly IDialogFactory _dialogFactory;

        public override void Execute()
        {
            _dialogFactory.ShowDialog("benchmark");
        }
    }
}