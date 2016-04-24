using Computator.NET.Benchmarking;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class BenchmarkCommand : CommandBase
    {
        public BenchmarkCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.Benchmark_Text;
            this.ToolTip = MenuStrings.Benchmark_Text;
        }


        public override void Execute()
        {
            new BenchmarkForm().ShowDialog(/*this*/);
        }
    }
}