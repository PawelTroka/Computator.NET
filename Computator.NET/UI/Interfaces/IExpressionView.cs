using Computator.NET.UI.Controls;

namespace Computator.NET.UI.Interfaces
{
    public interface IExpressionView
    {
        IExpressionTextBox ExpressionTextBox { get; }
        bool Visible { set; }
    }
}