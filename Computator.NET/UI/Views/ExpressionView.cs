using System.Windows.Forms;
using Computator.NET.Data;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class ExpressionView : UserControl, IExpressionView
    {
        private Controls.ExpressionTextBox expressionTextBox;
        public ExpressionView(ISharedViewState sharedViewState, IFunctionsDetails functionsDetails) : this()
        {
            this.expressionTextBox = new Computator.NET.UI.Controls.ExpressionTextBox(sharedViewState,functionsDetails);
            this.tableLayoutPanel1.Controls.Add(this.expressionTextBox, 1, 0);
        }

        private ExpressionView()
        {
            InitializeComponent();
        }

        public IExpressionTextBox ExpressionTextBox
        {
            get { return expressionTextBox; }
        }
    }
}