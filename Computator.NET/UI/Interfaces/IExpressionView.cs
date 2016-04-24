using Computator.NET.UI.Controls;

namespace Computator.NET.UI.MVP.Views
{
    public interface IExpressionView
    {
        IExpressionTextBox ExpressionTextBox { get; }
        bool Visible { set; }
    }
}