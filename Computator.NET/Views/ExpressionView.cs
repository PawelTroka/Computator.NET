using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;

namespace Computator.NET.Views
{
    public partial class ExpressionView : UserControl, IExpressionView
    {
        private readonly Controls.ExpressionTextBox _expressionTextBox;
        public ExpressionView(Controls.ExpressionTextBox expressionTextBox) : this()
        {
            this._expressionTextBox = expressionTextBox;
            this._expressionTextBox.Dock = DockStyle.Fill;
            this._expressionTextBox.AutoSize = true;

            this.tableLayoutPanel1.Controls.Add(this._expressionTextBox, 1, 0);
        }

        private ExpressionView()
        {
            InitializeComponent();
        }

        public IExpressionTextBox ExpressionTextBox
        {
            get { return _expressionTextBox; }
        }
    }
}