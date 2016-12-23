namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    public class DummyCommand : CommandBase
    {
        public DummyCommand(string text, string toolTip = null)
        {
            //this.Icon = Resources.runToolStripButtonImage;
            Text = text;
            ToolTip = toolTip ?? text;
        }

        public override void Execute()
        {
        }
    }
}