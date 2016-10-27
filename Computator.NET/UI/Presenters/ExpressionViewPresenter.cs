using System;
using System.Windows.Forms;
using Computator.NET.DataTypes.Events;
using Computator.NET.Evaluation;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class ExpressionViewPresenter
    {
        private readonly IExpressionView _view;
        private readonly ModeDeterminer modeDeterminer = new ModeDeterminer();


        public ExpressionViewPresenter(IExpressionView view)
        {
            _view = view;
            _view.ExpressionTextBox.TextChanged += ExpressionTextBox_TextChanged;
            _view.ExpressionTextBox.KeyPress += ExpressionTextBox_KeyPress;

            SharedViewState.Instance.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(SharedViewState.Instance.CurrentView))
                    _view.Visible =
                        !(SharedViewState.Instance.CurrentView == ViewName.Scripting ||
                          SharedViewState.Instance.CurrentView == ViewName.CustomFunctions);
            };
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