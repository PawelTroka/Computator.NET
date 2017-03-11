namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    internal abstract class ChartOption : CommandBase
    {
        protected ChartOption(object value)
        {
            IsOption = true;
            CheckOnClick = true;
            Text = value.ToString();
            ToolTip = value.ToString();
        }
    }
}