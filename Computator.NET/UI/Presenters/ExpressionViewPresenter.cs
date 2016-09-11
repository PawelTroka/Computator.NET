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
        private readonly IModeDeterminer modeDeterminer;
        private ISharedViewState _sharedViewState;

        public ExpressionViewPresenter(IExpressionView view, ISharedViewState sharedViewState, IModeDeterminer modeDeterminer)
        {
            _view = view;
            _sharedViewState = sharedViewState;
            this.modeDeterminer = modeDeterminer;
            _view.ExpressionTextBox.TextChanged += ExpressionTextBox_TextChanged;
            _view.ExpressionTextBox.KeyPress += ExpressionTextBox_KeyPress;

            _sharedViewState.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(_sharedViewState.CurrentView))
                    _view.Visible =
                        !(_sharedViewState.CurrentView == ViewName.Scripting ||
                          _sharedViewState.CurrentView == ViewName.CustomFunctions);
            };
        }

        private void ExpressionTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) //enter
                _sharedViewState.CurrentAction?.Invoke(this, e);
        }

        private void ExpressionTextBox_TextChanged(object sender, EventArgs e)
        {
            var mode = modeDeterminer.DetermineMode(_view.ExpressionTextBox.Text);
            if (mode == _sharedViewState.CalculationsMode) return;
            _sharedViewState.CalculationsMode = mode;
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
        }
    }
}