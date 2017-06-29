using System.Collections.Generic;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.Localization;

namespace Computator.NET.IntegrationTests
{
    public partial class NumericalCalculationsPresenterTests
    {

        public static object[] IntegralTestCases
        {
            get
            {
                if (_integralTestCases == null) GenerateTestCases();
                return _integralTestCases;
            }
            set { _integralTestCases = value; }
        }

        public static object[] DerrivativeTestCases
        {
            get
            {
                if (_derrivativeTestCases == null) GenerateTestCases();
                return _derrivativeTestCases;
            }
            set { _derrivativeTestCases = value; }
        }

        public static object[] FunctionRootTestCases
        {
            get
            {
                if (_functionRootTestCases == null) GenerateTestCases();
                return _functionRootTestCases;
            }
            set { _functionRootTestCases = value; }
        }

        private static object[] _integralTestCases;

        private static object[] _derrivativeTestCases;

        private static object[] _functionRootTestCases;


        private const int AMax = 2;
        private const int AMin = -2;
        private const int AInc = 2;
        private const int NMax = 2;
        private const double EpsMin = 0.01;
        private const int EpsMax = 1;
        private const double EpsInc = 0.5;

        private static void GenerateTestCases()
        {
            var interalsTestCases = new List<object>();
            var functionRootTestCases = new List<object>();
            var derrivativeTestCases = new List<object>();

            foreach (var function in functions)
                for (double a = AMin; a <= AMax; a += AInc)
                {
                    for (uint n = 1000; n <= NMax * 1000; n += 1000)
                    {
                        for (var b = a; b <= AMax; b += AInc)
                        {
                            foreach (var integrationMethod in NumericalMethodsInfo.Instance.IntegrationMethods)
                                if (integrationMethod.Key != Strings.Monte_Carlo_method)
                                {
                                    interalsTestCases.Add(new object[]
                                    {
                                        integrationMethod.Value, function.Value, integrationMethod.Key, function.Key, a,
                                        b,
                                        integrationMethod.Key != Strings.Romberg_s_method
                                            ? n
                                            : n / 1000 + 2
                                    });
                                }

                            for (var eps = EpsMin; eps < EpsMax; eps += EpsInc)
                                foreach (var functionRootMethod in NumericalMethodsInfo.Instance.FunctionRootMethods)
                                    functionRootTestCases.Add(new object[]
                                    {
                                        functionRootMethod.Value, function.Value, functionRootMethod.Key, function.Key,
                                        a,
                                        b, n, eps
                                    });
                        }
                    }
                }


            foreach (var function in functions)
                for (double a = AMin; a <= AMax; a += AInc)
                for (uint n = 0; n <= NMax; n++)
                for (var eps = EpsMin; eps < EpsMax; eps += EpsInc)
                    foreach (var derrivativeMethod in NumericalMethodsInfo.Instance.DerrivationMethods)
                        derrivativeTestCases.Add(new object[]
                        {
                            derrivativeMethod.Value, function.Value, derrivativeMethod.Key, function.Key, a, n,
                            eps
                        });


            _integralTestCases = interalsTestCases.ToArray();
            _derrivativeTestCases = derrivativeTestCases.ToArray();
            _functionRootTestCases = functionRootTestCases.ToArray();
        }
    }
}