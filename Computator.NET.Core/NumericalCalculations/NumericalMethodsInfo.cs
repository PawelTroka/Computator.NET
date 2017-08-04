using System;
using System.Collections.Generic;
using Computator.NET.Localization;

namespace Computator.NET.Core.NumericalCalculations
{
    public class NumericalMethodsInfo
    {
        public readonly IDictionary<string, IDictionary<string, Delegate>> _methods;

        private NumericalMethodsInfo()
        {
            _methods = new Dictionary<string, IDictionary<string, Delegate>>(
                new Dictionary<string, IDictionary<string, Delegate>>
                {
                    {
                        Strings.Integral, new Dictionary<string, Delegate>(
                            new Dictionary<string, Delegate>
                            {
                                {
                                    Strings.trapezoidal_method,
                                    (Func<Func<double, double>, double, double, double, double>)
                                        Integral.trapezoidalMethod
                                },
                                {
                                    Strings.rectangle_method,
                                    (Func<Func<double, double>, double, double, double, double>)
                                        Integral.rectangleMethod
                                },
                                {
                                    Strings.Simpson_s_method,
                                    (Func<Func<double, double>, double, double, double, double>) Integral.simpsonMethod
                                },
                                {
                                    Strings.double_exponential_transformation,
                                    (Func<Func<double, double>, double, double, double, double>)
                                        Integral.doubleExponentialTransformation
                                },
                                {
                                    Strings.non_adaptive_Gauss_Kronrod_method,
                                    (Func<Func<double, double>, double, double, double, double>)
                                        Integral.nonAdaptiveGaussKronrodMethod
                                },
                                {
                                    Strings.infinity_adaptive_Gauss_Kronrod_method,
                                    (Func<Func<double, double>, double, double, double, double>)
                                        Integral.infiniteAdaptiveGaussKronrodMethod
                                },
                                {
                                    Strings.Monte_Carlo_method,
                                    (Func<Func<double, double>, double, double, double, double>)
                                        Integral.monteCarloMethod
                                },
                                {
                                    Strings.Romberg_s_method,
                                    (Func<Func<double, double>, double, double, double, double>) Integral.rombergMethod
                                }
                            })
                    },

                    {
                        Strings.Derivative, new Dictionary<string, Delegate>(
                            new Dictionary<string, Delegate>
                            {
                                {
                                    Strings.centered_order_point_method,
                                    (Func<Func<double, double>, double, uint, double, double>)
                                        Derivative.derivativeAtPoint
                                },
                                {
                                    Strings.finite_difference_formula,
                                    (Func<Func<double, double>, double, uint, double, double>)
                                        Derivative.finiteDifferenceFormula
                                },
                                {
                                    Strings.two_point_finite_difference_formula,
                                    (Func<Func<double, double>, double, uint, double, double>)
                                        Derivative.twoPointfiniteDifferenceFormula
                                },
                                {
                                    Strings.stable_finite_difference_formula,
                                    (Func<Func<double, double>, double, uint, double, double>)
                                        Derivative.stableFiniteDifferenceFormula
                                },
                                {
                                    Strings.centered_five_point_method,
                                    (Func<Func<double, double>, double, uint, double, double>)
                                        Derivative.centeredFivePointMethod
                                }
                            })
                    },

                    {
                        Strings.Function_root, new Dictionary<string, Delegate>(
                            new Dictionary<string, Delegate>
                            {
                                {
                                    Strings.bisection_method,
                                    (Func<Func<double, double>, double, double, double, uint, double>)
                                        FunctionRoot.bisectionMethod
                                },
                                {
                                    Strings.secant_method,
                                    (Func<Func<double, double>, double, double, double, uint, double>)
                                        FunctionRoot.secantMethod
                                },
                                {
                                    Strings.Brent_s_method,
                                    (Func<Func<double, double>, double, double, double, uint, double>)
                                        FunctionRoot.BrentMethod
                                },
                                {
                                    Strings.Broyden_s_method,
                                    (Func<Func<double, double>, double, double, double, uint, double>)
                                        FunctionRoot.BroydenMethod
                                },
                                {
                                    Strings.secant_Newton_Raphson_method,
                                    (Func<Func<double, double>, double, double, double, uint, double>)
                                        FunctionRoot.secantNewtonRaphsonMethod
                                }
                            })
                    }
                });
        }

        public IDictionary<string, Func<Func<double, double>, double, double, double, uint, double>>
            FunctionRootMethods
        {
            get
            {
                var dict = _methods[Strings.Function_root];

                var newDict = new Dictionary<string, Func<Func<double, double>, double, double, double, uint, double>>();
                foreach (var aaaa in dict)
                {
                    newDict.Add(aaaa.Key, aaaa.Value as Func<Func<double, double>, double, double, double, uint, double>);
                }
                return
                    new Dictionary<string, Func<Func<double, double>, double, double, double, uint, double>>(
                        newDict);
            }
        }

        public IDictionary<string, Func<Func<double, double>, double, double, double, double>> IntegrationMethods
        {
            get
            {
                var dict = _methods[Strings.Integral];

                var newDict = new Dictionary<string, Func<Func<double, double>, double, double, double, double>>();
                foreach (var aaaa in dict)
                {
                    newDict.Add(aaaa.Key, aaaa.Value as Func<Func<double, double>, double, double, double, double>);
                }
                return
                    new Dictionary<string, Func<Func<double, double>, double, double, double, double>>(newDict);
            }
        }

        public IDictionary<string, Func<Func<double, double>, double, uint, double, double>> DerrivationMethods
        {
            get
            {
                var dict = _methods[Strings.Derivative];

                var newDict = new Dictionary<string, Func<Func<double, double>, double, uint, double, double>>();
                foreach (var aaaa in dict)
                {
                    newDict.Add(aaaa.Key, aaaa.Value as Func<Func<double, double>, double, uint, double, double>);
                }
                return new Dictionary<string, Func<Func<double, double>, double, uint, double, double>>(newDict);
            }
        }

        public static NumericalMethodsInfo Instance { get; } = new NumericalMethodsInfo();
    }
}