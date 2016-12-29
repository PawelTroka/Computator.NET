using System.Windows.Forms;
using Computator.NET.Data;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
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

            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionView));
            resources.ApplyResources(this._expressionTextBox, nameof(_expressionTextBox));
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