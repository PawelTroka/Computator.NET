// Computator.NET Copyright © 2016 - 2016 Pawel Troka

using System;
using System.Numerics;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.UI.ErrorHandling;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class CalculationsPresenter
    {
        private readonly IErrorHandler _errorHandler;
        private readonly ISharedViewState _sharedViewState;
        private readonly ExpressionsEvaluator _expressionsEvaluator = new ExpressionsEvaluator();
        private readonly ICalculationsView _view;
        private CalculationsMode _calculationsMode;

        public CalculationsPresenter(ICalculationsView view, IErrorHandler errorHandler, ISharedViewState sharedViewState)
        {
            _view = view;

            _errorHandler = errorHandler;
            _sharedViewState = sharedViewState;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(_ModeChanged);
            _view.CalculateClicked += _view_CalculateClicked;
            _sharedViewState.DefaultActions[ViewName.Calculations] = _view_CalculateClicked;
        }

        private void _view_CalculateClicked(object sender, EventArgs e)
        {
            if (_sharedViewState.ExpressionText != "")
            {
                try
                {
                    _sharedViewState.CustomFunctionsEditor.ClearHighlightedErrors();
                    var function = _expressionsEvaluator.Evaluate(_sharedViewState.ExpressionText,
                        _sharedViewState.CustomFunctionsText, _calculationsMode);

                    var x = _view.X;
                    var y = _view.Y;
                    var z = new Complex(x, y);

                    dynamic result = function.EvaluateDynamic(x, y);

                    var resultStr = ScriptingExtensions.ToMathString(result);

                    _view.AddResult(_sharedViewState.ExpressionText,
                        _calculationsMode == CalculationsMode.Complex
                            ? z.ToMathString()
                            : (_calculationsMode == CalculationsMode.Fxy
                                ? $"{x.ToMathString()}, {y.ToMathString()}"
                                : x.ToMathString()), resultStr);
                }
                catch (Exception ex)
                {
                    ExceptionsHandler.Instance.HandleException(ex, _errorHandler);
                }
            }
            else
                _errorHandler.DispalyError(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_,
                    Strings.GUI_numericalOperationButton_Click_Warning_);
        }

        private void _ModeChanged(CalculationsModeChangedEvent calculationsModeChangedEvent)
        {
            if (calculationsModeChangedEvent.CalculationsMode == _calculationsMode) return;

            _view.YVisible = calculationsModeChangedEvent.CalculationsMode == CalculationsMode.Complex ||
                             calculationsModeChangedEvent.CalculationsMode == CalculationsMode.Fxy;

            switch (calculationsModeChangedEvent.CalculationsMode)
            {
                case CalculationsMode.Complex:
                    _view.XLabel = "Re(z) =";
                    _view.YLabel = "Im(z) =";
                    break;
                case CalculationsMode.Fxy:
                    _view.XLabel = "       x =";
                    _view.YLabel = "       y =";
                    break;
                case CalculationsMode.Real:
                    _view.XLabel = "       x =";
                    break;
            }

            _calculationsMode = calculationsModeChangedEvent.CalculationsMode;
        }
    }
}