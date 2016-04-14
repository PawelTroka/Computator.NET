using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using Computator.NET.NumericalCalculations;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Controls;

namespace Computator.NET.UI.Views
{
    public class NumericalCalculationsPresenter
    {
        private readonly INumericalCalculationsView _view;


        private Type integrationType = typeof (Func<Func<double, double>, double, double, double, double>);

        private Type derivationType = typeof(Func<Func<double, double>, double, uint, double, double>);

        private Type functionRootType = typeof(Func<Func<double, double>, double, double, double, int, double>);

        private readonly ReadOnlyDictionary<string,ReadOnlyDictionary<string, Delegate>> _methods;

        private readonly IErrorHandler errorHandler;
        private readonly ITextProvider expressionView;

        private CalculationsMode _calculationsMode;
        private ITextProvider _customFunctionsCodeEditor;

        public NumericalCalculationsPresenter(INumericalCalculationsView view, IErrorHandler errorHandler, ITextProvider expressionView, ITextProvider customFunctionsCodeEditor)
        {
            _methods = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, Delegate>>(
                new Dictionary<string, ReadOnlyDictionary<string, Delegate>>
                {
                    {Strings.Integral, new ReadOnlyDictionary<string, Delegate>(
                        new Dictionary<string, Delegate>
                        {
                            {Strings.trapezoidal_method, (Func<Func<double,double>,double,double, double, double>)Integral.trapezoidalMethod },
                            {Strings.rectangle_method,(Func<Func<double,double>,double,double, double, double>)Integral.trapezoidalMethod  },
                            {Strings.Simpson_s_method,(Func<Func<double,double>,double,double, double, double>)Integral.simpsonMethod  },
                            {Strings.double_exponential_transformation,(Func<Func<double,double>,double,double, double, double>)Integral.doubleExponentialTransformation  },
                            {Strings.non_adaptive_Gauss_Kronrod_method,(Func<Func<double,double>,double,double, double, double>)Integral.nonAdaptiveGaussKronrodMethod  },
                            {Strings.infinity_adaptive_Gauss_Kronrod_method,(Func<Func<double,double>,double,double, double, double>)Integral.infiniteAdaptiveGaussKronrodMethod  },
                            {Strings.Monte_Carlo_method,(Func<Func<double,double>,double,double, double, double>)Integral.monteCarloMethod  },
                            {Strings.Romberg_s_method,(Func<Func<double,double>,double,double, double, double>)Integral.rombergMethod  }
                        }) },

                    {Strings.Derivative, new ReadOnlyDictionary<string, Delegate>(
                        new Dictionary<string, Delegate>
                        {
                            { Strings.centered_order_point_method, (Func<Func<double,double>,double,uint, double, double>)Derivative.derivativeAtPoint },
                            {Strings.finite_difference_formula, (Func<Func<double,double>,double,uint, double, double>)Derivative.finiteDifferenceFormula },
                            {Strings.two_point_finite_difference_formula, (Func<Func<double,double>,double,uint, double, double>)Derivative.twoPointfiniteDifferenceFormula },
                            {Strings.stable_finite_difference_formula, (Func<Func<double,double>,double,uint, double, double>)Derivative.stableFiniteDifferenceFormula },
                            {Strings.centered_five_point_method, (Func<Func<double,double>,double,uint, double, double>)Derivative.centeredFivePointMethod }
                        }) },

                    {Strings.Function_root, new ReadOnlyDictionary<string, Delegate>(
                        new Dictionary<string, Delegate>
                        {
                            {Strings.bisection_method, (Func<Func<double,double>,double,double, double, int,double>)FunctionRoot.bisectionMethod },
                            {Strings.secant_method, (Func<Func<double,double>,double,double, double, int,double>)FunctionRoot.secantMethod },
                            {Strings.Brent_s_method, (Func<Func<double,double>,double,double, double, int,double>)FunctionRoot.BrentMethod },
                            {Strings.Broyden_s_method, (Func<Func<double,double>,double,double, double, int,double>)FunctionRoot.BroydenMethod },
                            {Strings.secant_Newton_Raphson_method, (Func<Func<double,double>,double,double, double, int,double>)FunctionRoot.secantNewtonRaphsonMethod }
                        }) }
                });


            _view = view;
            this.errorHandler = errorHandler;
            this.expressionView = expressionView;
            _customFunctionsCodeEditor = customFunctionsCodeEditor;
            _view.SetOperations(_methods.Keys.ToArray());
            _view.SelectedOperation = _methods.Keys.First();
            _view.OperationChanged += _view_OperationChanged;

            _view_OperationChanged(null, null);

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(c => _calculationsMode=c.CalculationsMode);

            _view.ComputeClicked += _view_ComputeClicked;
        }

        private readonly ExpressionsEvaluator expressionsEvaluator = new ExpressionsEvaluator();

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
                    var function = expressionsEvaluator.Evaluate(expressionView.Text,
                        _customFunctionsCodeEditor.Text, _calculationsMode);

                    Func<double, double> fx = (double x) => function.Evaluate(x);

                    var result = double.NaN;
                    var eps = _view.Epsilon;

                    if (double.IsNaN(eps))
                    {
                        errorHandler.DispalyError(Strings.GivenΕIsNotValid, Strings.Error);
                        return;
                    }
                    if (!(eps > 0.0) || !(eps < 1))
                    {
                        errorHandler.DispalyError(
                            Strings.GivenΕIsNotValidΕShouldBeSmallPositiveNumber, Strings.Error);
                        return;
                    }

                    var parametersStr = "";


                    if (_view.SelectedOperation == Strings.Integral)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(_methods[_view.SelectedOperation][_view.SelectedMethod],
                                    integrationType))
                                (fx, _view.A, _view.B, _view.N);
                        parametersStr = $"a={_view.A.ToMathString()}; b={_view.B.ToMathString()}; N={_view.N}";
                    }
                    else if (_view.SelectedOperation == Strings.Derivative)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(_methods[_view.SelectedOperation][_view.SelectedMethod],
                                    derivationType))
                                (fx, _view.X, _view.Order, eps);
                        parametersStr = $"n={_view.Order}; x={_view.X.ToMathString()}; ε={eps.ToMathString()}";
                    }
                    else if (_view.SelectedOperation == Strings.Function_root)
                    {
                        result =
                            ((dynamic)
                                Convert.ChangeType(_methods[_view.SelectedOperation][_view.SelectedMethod],
                                    functionRootType))
                                (fx, _view.A, _view.B, eps, _view.N);
                        parametersStr =
                            $"a={_view.A.ToMathString()}; b={_view.B.ToMathString()}; ε={eps.ToMathString()}; N={_view.N}";
                    }

                    _view.AddResult(expressionView.Text,
                        _view.SelectedOperation,
                        _view.SelectedMethod,
                        parametersStr,
                        result.ToMathString());
                }
                catch (Exception ex)//TODO: introduce real exception handling just like in MainFormPresenter for Calculations or add fuction to chart
                {
                    ExceptionsHandler.Instance.HandleException(ex,errorHandler);
                }
            }
            else
            {
                errorHandler.DispalyError(
                    Strings
                        .GUI_numericalOperationButton_Click_Only_Real_mode_is_supported_in_Numerical_calculations_right_now__more_to_come_in_next_versions_ +
                    Environment.NewLine +
                    Strings.GUI_numericalOperationButton_Click__Check__Real___f_x___mode,
                    Strings.GUI_numericalOperationButton_Click_Warning_);
            }
        }

        private void _view_OperationChanged(object sender, EventArgs e)
        {
            _view.SetMethods(_methods[_view.SelectedOperation].Keys.ToArray());
            _view.SelectedMethod = _methods[_view.SelectedOperation].Keys.First();

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