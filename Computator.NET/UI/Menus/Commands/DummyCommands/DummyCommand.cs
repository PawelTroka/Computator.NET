namespace Computator.NET.UI.Commands
{
    class DummyCommand : CommandBase
    {
        public DummyCommand(string text, string toolTip=null)
        {
            //this.Icon = Resources.runToolStripButtonImage;
            this.Text = text;
            this.ToolTip = toolTip ?? text;
        }
        
        public override void Execute()
        {
            
        }
    }
}