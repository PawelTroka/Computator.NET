using Computator.NET.Core.Abstract.Controls;

namespace Computator.NET.Core.Abstract.Views
{
    public interface IExpressionView
    {
        IExpressionTextBox ExpressionTextBox { get; }
        bool Visible { set; }
    }
}