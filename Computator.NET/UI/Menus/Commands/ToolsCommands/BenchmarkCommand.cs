using Computator.NET.Benchmarking;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    public class BenchmarkCommand : CommandBase
    {
        public BenchmarkCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Benchmark_Text;
            ToolTip = MenuStrings.Benchmark_Text;
        }


        public override void Execute()
        {
            new BenchmarkForm().ShowDialog( /*this*/);
        }
    }
}