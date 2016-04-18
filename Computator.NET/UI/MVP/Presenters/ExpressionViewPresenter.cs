using System;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.MVP.Views
{
    public class ExpressionViewPresenter
    {
        private readonly ModeDeterminer modeDeterminer = new ModeDeterminer();
        private readonly IExpressionView _view;


        public ExpressionViewPresenter(IExpressionView view)
        {
            _view = view;
            _view.ExpressionTextBox.TextChanged += ExpressionTextBox_TextChanged;
            _view.ExpressionTextBox.KeyPress += ExpressionTextBox_KeyPress;
        }

        private void ExpressionTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) //enter
                SharedViewState.Instance.CurrentAction?.Invoke(this, e);
        }

        private void ExpressionTextBox_TextChanged(object sender, EventArgs e)
        {
            var mode = modeDeterminer.DetermineMode(SharedViewState.Instance.ExpressionText);
            if (mode == SharedViewState.Instance.CalculationsMode) return;
            SharedViewState.Instance.CalculationsMode = mode;
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
        }
    }
}