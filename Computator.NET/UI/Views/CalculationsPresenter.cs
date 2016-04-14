// Computator.NET Copyright © 2016 - 2016 Pawel Troka

using System;
using System.Numerics;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public class CalculationsPresenter
    {
        private readonly ICalculationsView _view;
        private readonly ITextProvider _expression;
        private readonly ITextProvider _customFunctions;
        private readonly IErrorHandler _errorHandler;
        private CalculationsMode _calculationsMode;

        public CalculationsPresenter(ICalculationsView view, ITextProvider expression, ITextProvider customFunctions, IErrorHandler errorHandler)
        {
            _view = view;
            _expression = expression;
            _customFunctions = customFunctions;
            _errorHandler = errorHandler;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(_ModeChanged);
            _view.CalculateClicked += _view_CalculateClicked;
        }

        private readonly ExpressionsEvaluator _expressionsEvaluator = new ExpressionsEvaluator();

        private void _view_CalculateClicked(object sender, System.EventArgs e)
        {
            if (_expression.Text != "")
            {
                try
                {
                    var function = _expressionsEvaluator.Evaluate(_expression.Text, _customFunctions.Text, _calculationsMode);

                    var x = _view.X;
                    var y = _view.Y;
                    var z = new Complex(x, y);

                    dynamic result = function.EvaluateDynamic(x, y);

                    var resultStr = ScriptingExtensions.ToMathString(result);

                    _view.AddResult(_expression.Text, _calculationsMode == CalculationsMode.Complex ? z.ToMathString() : (_calculationsMode == CalculationsMode.Fxy ? $"{x.ToMathString()}, {y.ToMathString()}" : x.ToMathString()), resultStr);
                }
                catch (Exception ex)
                {
                   ExceptionsHandler.Instance.HandleException(ex,_errorHandler);
                }
            }
            else
                _errorHandler.DispalyError(Strings.GUI_addToChartButton_Click_Expression_should_not_be_empty_, Strings.GUI_numericalOperationButton_Click_Warning_);
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