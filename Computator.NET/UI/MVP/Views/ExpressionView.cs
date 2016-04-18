using System.Windows.Forms;
using Computator.NET.UI.Controls;

namespace Computator.NET.UI.MVP.Views
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