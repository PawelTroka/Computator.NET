using System;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;

namespace Computator.NET.Core.Presenters
{
    public class ExpressionViewPresenter
    {
        private readonly IExpressionView _view;
        private readonly IModeDeterminer modeDeterminer;
        private ISharedViewState _sharedViewState;
        private readonly IScriptProvider _customFunctionsEditor;

        public ExpressionViewPresenter(IExpressionView view, ISharedViewState sharedViewState, IModeDeterminer modeDeterminer,
            IScriptProvider customFunctionsEditor)
        {
            _view = view;
            _sharedViewState = sharedViewState;
            this.modeDeterminer = modeDeterminer;
            _customFunctionsEditor = customFunctionsEditor;
            _view.ExpressionTextBox.TextChanged += ExpressionTextBox_TextChanged;
            _view.ExpressionTextBox.KeyPress += ExpressionTextBox_KeyPress;

            _sharedViewState.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(_sharedViewState.CurrentView))
                {
                    var visible =
                        !(_sharedViewState.CurrentView == ViewName.Scripting ||
                          _sharedViewState.CurrentView == ViewName.CustomFunctions);
                    _view.Visible = visible;
                    // Returning from custom definitions can change whether a bare
                    // superscript is an exact name or an exponent, without changing
                    // the expression itself.
                    if (visible) ExpressionTextBox_TextChanged(this, EventArgs.Empty);
                }
            };
        }

        private void ExpressionTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) //enter
            {
                ExpressionTextBox_TextChanged(this, EventArgs.Empty);
                _sharedViewState.CurrentAction?.Invoke(this, e);
            }
        }

        private void ExpressionTextBox_TextChanged(object sender, EventArgs e)
        {
            var mode = modeDeterminer.DetermineMode(_view.ExpressionTextBox.Text, _customFunctionsEditor.Text);
            if (mode == _sharedViewState.CalculationsMode) return;
            _sharedViewState.CalculationsMode = mode;
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
        }
    }
}
