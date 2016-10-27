using System.Windows.Forms;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class ExpressionView : UserControl, IExpressionView
    {
        public ExpressionView()
        {
            InitializeComponent();
        }

        public IExpressionTextBox ExpressionTextBox
        {
            get { return expressionTextBox; }
        }
    }
}