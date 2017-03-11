// Computator.NET Copyright © 2016 - 2017 Pawel Troka

using System;
using System.Numerics;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.Core.Presenters
{
    public class CalculationsPresenter
    {
        private readonly IErrorHandler _errorHandler;
        private readonly ISharedViewState _sharedViewState;
        private readonly IExpressionsEvaluator _expressionsEvaluator;
        private readonly ICalculationsView _view;
        private CalculationsMode _calculationsMode;
        private readonly ITextProvider _expressionTextProvider;

        public CalculationsPresenter(ICalculationsView view, IErrorHandler errorHandler, ISharedViewState sharedViewState, IExceptionsHandler exceptionsHandler, ITextProvider expressionTextProvider, ICodeEditorView customFunctionsEditor, IExpressionsEvaluator expressionsEvaluator)
        {
            _view = view;

            _errorHandler = errorHandler;
            _sharedViewState = sharedViewState;
            _exceptionsHandler = exceptionsHandler;
            _expressionTextProvider = expressionTextProvider;
            this._customFunctionsEditor = customFunctionsEditor;
            _expressionsEvaluator = expressionsEvaluator;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(_ModeChanged);
            _view.CalculateClicked += _view_CalculateClicked;
            _sharedViewState.DefaultActions[ViewName.Calculations] = _view_CalculateClicked;
        }

        private void _view_CalculateClicked(object sender, EventArgs e)
        {
            if (_expressionTextProvider.Text != "")
            {
                try
                {
                    _customFunctionsEditor.ClearHighlightedErrors();
                    var function = _expressionsEvaluator.Evaluate(_expressionTextProvider.Text,
                        _customFunctionsEditor.Text, _calculationsMode);

                    var x = _view.X;
                    var y = _view.Y;
                    var z = new Complex(x, y);

                    dynamic result = function.EvaluateDynamic(x, y);

                    var resultStr = ScriptingExtensions.ToMathString(result);

                    _view.AddResult(_expressionTextProvider.Text,
                        _calculationsMode == CalculationsMode.Complex
                            ? z.ToMathString()
                            : (_calculationsMode == CalculationsMode.Fxy
                                ? $"{x.ToMathString()}, {y.ToMathString()}"
                                : x.ToMathString()), resultStr);
                }
                catch (Exception ex)
                {
                    _exceptionsHandler.HandleException(ex);
                }
            }
            else
                _errorHandler.DisplayError(Strings.Expression_should_not_be_empty_,
                    Strings.Warning_);
        }

        private readonly IExceptionsHandler _exceptionsHandler;
        private readonly ICodeEditorView _customFunctionsEditor;

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