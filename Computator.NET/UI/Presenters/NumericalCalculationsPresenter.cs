using System;
using System.Linq;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.NumericalCalculations;
using Computator.NET.UI.ErrorHandling;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class NumericalCalculationsPresenter
    {
        private readonly Type _derivationType = typeof(Func<Func<double, double>, double, uint, double, double>);

        private readonly IErrorHandler _errorHandler;

        private readonly Type _functionRootType =
            typeof(Func<Func<double, double>, double, double, double, uint, double>);


        private readonly Type _integrationType = typeof(Func<Func<double, double>, double, double, double, double>);
        private readonly NumericalMethodsInfo _numericalMethodsInfo;
        private readonly INumericalCalculationsView _view;


        private readonly ExpressionsEvaluator expressionsEvaluator = new ExpressionsEvaluator();


        private CalculationsMode _calculationsMode;


        public NumericalCalculationsPresenter(INumericalCalculationsView view, IErrorHandler errorHandler)
        {
            _view = view;
            _errorHandler = errorHandler;
            _view.SetOperations(NumericalMethodsInfo.Instance._methods.Keys.ToArray());
            _view.SelectedOperation = NumericalMethodsInfo.Instance._methods.Keys.First();
            _view.OperationChanged += _view_OperationChanged;

            _view_OperationChanged(null, null);

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(c => _calculationsMode = c.CalculationsMode);

            SharedViewState.Instance.DefaultActions[ViewName.NumericalCalculations] = _view_ComputeClicked;
            _view.ComputeClicked += _view_ComputeClicked;
        }

        public T Cast<T>(object input)
        {
            return (T) input;
        }

        private void _view_ComputeClicked(object sender, EventArgs e)
        {
            if (_calculationsMode == CalculationsMode.Real)
            {
                try
                {
                    SharedViewState.Instance.CustomFunctionsEditor.ClearHighlightedErrors();
                    var function = expressionsEvaluator.Evaluate(SharedViewState.Instance.ExpressionText,
                        SharedViewState.Instance.CustomFunctionsText, _calculationsMode);

                    Func<double, double> fx = x => function.Evaluate(x);

                    var result = double.NaN;
                    var eps = _view.Epsilon;

                    if (_view.SelectedOperation == Strings.Derivative ||
                        _view.SelectedOperation == Strings.Function_root)
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
                                Convert.ChangeType(
                                    NumericalMethodsInfo.Instance._methods[_view.SelectedOperation][_view.SelectedMethod
                                        ],
                                    _integrationType))
                                (fx, _view.A, _view.B, _view.N);
                        parametersStr = $"a={_view.A.ToMathString()}; b={_view.B.ToMathString()}; N={_view.N}";
                    }
                    else if (_view.SelectedOperation == Strings.Derivative)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(
                                    NumericalMethodsInfo.Instance._methods[_view.SelectedOperation][_view.SelectedMethod
                                        ],
                                    _derivationType))
                                (fx, _view.X, _view.Order, eps);
                        parametersStr = $"n={_view.Order}; x={_view.X.ToMathString()}; ε={eps.ToMathString()}";
                    }
                    else if (_view.SelectedOperation == Strings.Function_root)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(
                                    NumericalMethodsInfo.Instance._methods[_view.SelectedOperation][_view.SelectedMethod
                                        ],
                                    _functionRootType))
                                (fx, _view.A, _view.B, eps, _view.N);
                        parametersStr =
                            $"a={_view.A.ToMathString()}; b={_view.B.ToMathString()}; ε={eps.ToMathString()}; N={_view.N}";
                    }

                    _view.AddResult(SharedViewState.Instance.ExpressionText,
                        _view.SelectedOperation,
                        _view.SelectedMethod,
                        parametersStr,
                        result.ToMathString());
                }
                catch (Exception ex)
                {
                    ExceptionsHandler.Instance.HandleException(ex, _errorHandler);
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