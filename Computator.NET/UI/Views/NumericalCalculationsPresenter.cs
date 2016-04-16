using System;
using System.Linq;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Controls;

namespace Computator.NET.UI.Views
{
    public class NumericalCalculationsPresenter
    {
        private readonly INumericalCalculationsView _view;


        private readonly Type _integrationType = typeof (Func<Func<double, double>, double, double, double, double>);

        private readonly Type _derivationType = typeof(Func<Func<double, double>, double, uint, double, double>);

        private readonly Type _functionRootType = typeof(Func<Func<double, double>, double, double, double, uint, double>);

        private readonly IErrorHandler _errorHandler;
        private readonly ITextProvider _expressionView;

        private CalculationsMode _calculationsMode;
        private readonly ITextProvider _customFunctionsCodeEditor;

        public NumericalCalculationsPresenter(INumericalCalculationsView view, IErrorHandler errorHandler, ITextProvider expressionView, ITextProvider customFunctionsCodeEditor)
        {
            _view = view;
            this._errorHandler = errorHandler;
            this._expressionView = expressionView;
            _customFunctionsCodeEditor = customFunctionsCodeEditor;
            _view.SetOperations(NumericalMethodsInfo.Instance._methods.Keys.ToArray());
            _view.SelectedOperation = NumericalMethodsInfo.Instance._methods.Keys.First();
            _view.OperationChanged += _view_OperationChanged;

            _view_OperationChanged(null, null);

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(c => _calculationsMode=c.CalculationsMode);

            _view.ComputeClicked += _view_ComputeClicked;
        }


        private readonly ExpressionsEvaluator expressionsEvaluator = new ExpressionsEvaluator();
        private readonly NumericalMethodsInfo _numericalMethodsInfo;

        public T Cast<T>(object input)
        {
            return (T)input;
        }

        private void _view_ComputeClicked(object sender, EventArgs e)
        {
            if (_calculationsMode == CalculationsMode.Real)
            {
                try
                {
                    var function = expressionsEvaluator.Evaluate(_expressionView.Text,
                        _customFunctionsCodeEditor.Text, _calculationsMode);

                    Func<double, double> fx = (double x) => function.Evaluate(x);

                    var result = double.NaN;
                    var eps = _view.Epsilon;

                    if (_view.SelectedOperation == Strings.Derivative || _view.SelectedOperation==Strings.Function_root)
                    {
                        if (double.IsNaN(eps))
                        {
                            _errorHandler.DispalyError(Strings.GivenΕIsNotValid, Strings.Error);
                            return;
                        }
                        if (!(eps > 0.0) || !(eps < 1))
                        {
                            _errorHandler.DispalyError(
                                Strings.GivenΕIsNotValidΕShouldBeSmallPositiveNumber, Strings.Error);
                            return;
                        }
                    }

                    var parametersStr = "";


                    if (_view.SelectedOperation == Strings.Integral)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(NumericalMethodsInfo.Instance._methods[_view.SelectedOperation][_view.SelectedMethod],
                                    _integrationType))
                                (fx, _view.A, _view.B, _view.N);
                        parametersStr = $"a={_view.A.ToMathString()}; b={_view.B.ToMathString()}; N={_view.N}";
                    }
                    else if (_view.SelectedOperation == Strings.Derivative)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(NumericalMethodsInfo.Instance._methods[_view.SelectedOperation][_view.SelectedMethod],
                                    _derivationType))
                                (fx, _view.X, _view.Order, eps);
                        parametersStr = $"n={_view.Order}; x={_view.X.ToMathString()}; ε={eps.ToMathString()}";
                    }
                    else if (_view.SelectedOperation == Strings.Function_root)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(NumericalMethodsInfo.Instance._methods[_view.SelectedOperation][_view.SelectedMethod],
                                    _functionRootType))
                                (fx, _view.A, _view.B, eps, _view.N);
                        parametersStr =
                            $"a={_view.A.ToMathString()}; b={_view.B.ToMathString()}; ε={eps.ToMathString()}; N={_view.N}";
                    }

                    _view.AddResult(_expressionView.Text,
                        _view.SelectedOperation,
                        _view.SelectedMethod,
                        parametersStr,
                        result.ToMathString());
                }
                catch (Exception ex)
                {
                    ExceptionsHandler.Instance.HandleException(ex,_errorHandler);
                }
            }
            else
            {
                _errorHandler.DispalyError(
                    Strings
                        .GUI_numericalOperationButton_Click_Only_Real_mode_is_supported_in_Numerical_calculations_right_now__more_to_come_in_next_versions_ +
                    Environment.NewLine +
                    Strings.GUI_numericalOperationButton_Click__Check__Real___f_x___mode,
                    Strings.GUI_numericalOperationButton_Click_Warning_);
            }
        }

        private void _view_OperationChanged(object sender, EventArgs e)
        {
            _view.SetMethods(NumericalMethodsInfo.Instance._methods[_view.SelectedOperation].Keys.ToArray());
            _view.SelectedMethod = NumericalMethodsInfo.Instance._methods[_view.SelectedOperation].Keys.First();

            _view.StepsVisible = _view.IntervalVisible = _view.DerrivativeVisible = _view.ErrorVisible = false;
            if (_view.SelectedOperation == Strings.Integral)
            {
                _view.StepsVisible = _view.IntervalVisible = true;
            }
            else if (_view.SelectedOperation == Strings.Derivative)
            {
                _view.DerrivativeVisible = _view.ErrorVisible = true;
            }
            else if (_view.SelectedOperation == Strings.Function_root)
            {
                _view.StepsVisible = _view.IntervalVisible = _view.ErrorVisible = true;
            }
        }
    }
}