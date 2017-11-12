using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;

// ReSharper disable LocalizableElement

namespace Computator.NET.Core.IntegrationTests
{
    [TestFixture]
    [Category("LongRunningTests")]
    public class MathematicalFunctionsTests
    {
        [SetUp]
        public void Init()
        {
            _c = (from d1 in _x from d2 in _x select new Complex(d1, d2)).ToArray();
            Settings.Default.CalculationsErrors = CalculationsErrors.ReturnNAN;
            GSLInitializer.Initialize();
        }

        private const int StepsSmall = 6;
        private const int Steps = 11;
        private const double Min = -10;
        private const double Max = 10;
        private readonly int[] _a = Enumerable.Range(-10, 21).ToArray(); // {-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5};
        private readonly int[] _aSmall = Enumerable.Range(-5, 11).ToArray();

        private readonly int[] _aVerySmall = Enumerable.Range(-3, 7).ToArray();

        private readonly double[] _x = Enumerable.Range(0, Steps)
            .Select(i => Min + (Max - Min)*((double) i/(Steps - 1))).ToArray();

        private readonly double[] _xSmall = Enumerable.Range(0, StepsSmall)
            .Select(i => Min + (Max - Min)*((double) i/(Steps - 1))).ToArray();

        private Complex[] _c;

        private void TestFunctions(List<MethodInfo> methodsToTest)
        {
            object ret = null;

            foreach (var specialMethod in methodsToTest)
            {
                Debug.WriteLine("Testing: " + specialMethod);
                var parameters = specialMethod.GetParameters();
                var testIt = true;


                for (var index = 0;
                    index < _a.Length && (parameters.Any(p => p.ParameterType.IsInteger()) || index == 0);
                    index++)
                {
                    var i = _a[index];
                    for (var index1 = 0;
                        index1 < _x.Length && (parameters.Any(p => p.ParameterType.IsDouble()) || index1 == 0);
                        index1++)
                    {
                        var d = _x[index1];
                        for (var i1 = 0;
                            i1 < _c.Length && parameters.Any(p => p.ParameterType.IsComplex() || i1 == 0);
                            i1++)
                        {
                            var c = _c[i1];
                            var invokeList = new List<object>();
                            foreach (var parameterInfo in parameters)
                            {
                                if (parameterInfo.ParameterType.IsDouble())
                                {
                                    invokeList.Add(d);
                                }
                                else if (parameterInfo.ParameterType.IsInteger())
                                {
                                    invokeList.Add(i);
                                }
                                else if (parameterInfo.ParameterType.IsComplex())
                                {
                                    invokeList.Add(c);
                                }
                                else if (parameterInfo.ParameterType.IsClass)
                                {
                                    testIt = false;
                                    break;
                                }
                                else
                                {
                                    invokeList.Add(parameterInfo.DefaultValue);
                                }
                            }

                            try
                            {
                                ret = testIt ? specialMethod.Invoke(null, invokeList.ToArray()) : 1;
                            }
                            catch (Exception ex)
                            {
                                Assert.Fail("Exception occured: \n"
                                            + specialMethod.Name + " : parameters: " + " a=" + i + " x=" + d + " z=" + c +
                                            ": " + ex.Message + "\n" + ex.StackTrace + "\n" +
                                            "ORIGINAL EXCEPTION:\n" +
                                            ex?.InnerException);
                            }

                            Assert.IsNotNull(ret);
                        }
                    }
                }
            }
        }

        public void MathieuSETest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)
                    foreach (var d2 in _x)

                    {
                        try
                        {
                            ret = SpecialFunctions.MathieuSE(i, d1, d2);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }

        public void LegendreH3DTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)
                    foreach (var d2 in _x)

                    {
                        try
                        {
                            ret = SpecialFunctions.LegendreH3D(i, d1, d2);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }

        [Test]
        public void ClebschGordanTest()
        {
            object ret = null;

            foreach (var d1 in _a)
                foreach (var d2 in _a)
                    foreach (var d3 in _a)
                        foreach (var d4 in _a)
                            foreach (var d5 in _a)
                                foreach (var d6 in _a)
                                {
                                    try
                                    {
                                        ret = SpecialFunctions.ClebschGordan(d1, d2, d3, d4, d5, d6);
                                    }
                                    catch (Exception ex)
                                    {
                                        Assert.Fail("Exception occured: " + ex);
                                    }
                                    Assert.IsNotNull(ret);
                                }
        }

        [Test]
        public void Coupling3jIntTest()
        {
            object ret = null;

            foreach (var d1 in _a)
                foreach (var d2 in _a)
                    foreach (var d3 in _a)
                        foreach (var d4 in _a)
                            foreach (var d5 in _a)
                                foreach (var d6 in _a)
                                {
                                    try
                                    {
                                        ret = SpecialFunctions.Coupling3j(d1, d2, d3, d4, d5, d6);
                                    }
                                    catch (Exception ex)
                                    {
                                        Assert.Fail("Exception occured: " + ex);
                                    }
                                    Assert.IsNotNull(ret);
                                }
        }

        [Test]
        public void Coupling3jTest()
        {
            object ret = null;

            foreach (var d1 in _xSmall)
                foreach (var d2 in _xSmall)
                    foreach (var d3 in _xSmall)
                        foreach (var d4 in _xSmall)
                            foreach (var d5 in _xSmall)
                                foreach (var d6 in _xSmall)
                                {
                                    try
                                    {
                                        ret = SpecialFunctions.Coupling3j(d1, d2, d3, d4, d5, d6);
                                    }
                                    catch (Exception ex)
                                    {
                                        Assert.Fail("Exception occured: " + ex);
                                    }
                                    Assert.IsNotNull(ret);
                                }
        }

        [Test]
        public void Coupling6jIntTest()
        {
            object ret = null;

            foreach (var d1 in _a)
                foreach (var d2 in _a)
                    foreach (var d3 in _a)
                        foreach (var d4 in _a)
                            foreach (var d5 in _a)
                                foreach (var d6 in _a)
                                {
                                    try
                                    {
                                        ret = SpecialFunctions.Coupling6j(d1, d2, d3, d4, d5, d6);
                                    }
                                    catch (Exception ex)
                                    {
                                        Assert.Fail("Exception occured: " + ex);
                                    }
                                    Assert.IsNotNull(ret);
                                }
        }

        [Test]
        public void Coupling6jTest()
        {
            object ret = null;

            foreach (var d1 in _xSmall)
                foreach (var d2 in _xSmall)
                    foreach (var d3 in _xSmall)
                        foreach (var d4 in _xSmall)
                            foreach (var d5 in _xSmall)
                                foreach (var d6 in _xSmall)
                                {
                                    try
                                    {
                                        ret = SpecialFunctions.Coupling6j(d1, d2, d3, d4, d5, d6);
                                    }
                                    catch (Exception ex)
                                    {
                                        Assert.Fail("Exception occured: " + ex);
                                    }
                                    Assert.IsNotNull(ret);
                                }
        }

        [Test]
        public void Coupling9jIntTest()
        {
            object ret = null;

            foreach (var d1 in _aVerySmall)
                foreach (var d2 in _aVerySmall)
                    foreach (var d3 in _aVerySmall)
                        foreach (var d4 in _aVerySmall)
                            foreach (var d5 in _aVerySmall)
                                foreach (var d6 in _aVerySmall)
                                    foreach (var d7 in _aVerySmall)
                                        foreach (var d8 in _aVerySmall)
                                            foreach (var d9 in _aVerySmall)
                                            {
                                                try
                                                {
                                                    ret = SpecialFunctions.Coupling9j(d1, d2, d3, d4, d5, d6, d7, d8, d9);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Assert.Fail("Exception occured: " + ex);
                                                }
                                                Assert.IsNotNull(ret);
                                            }
        }

        [Test]
        public void CouplingRacahW()
        {
            object ret = null;

            foreach (var d1 in _a)
                foreach (var d2 in _a)
                    foreach (var d3 in _a)
                        foreach (var d4 in _a)
                            foreach (var d5 in _a)
                                foreach (var d6 in _a)
                                {
                                    try
                                    {
                                        ret = SpecialFunctions.CouplingRacahW(d1, d2, d3, d4, d5, d6);
                                    }
                                    catch (Exception ex)
                                    {
                                        Assert.Fail("Exception occured: " + ex);
                                    }
                                    Assert.IsNotNull(ret);
                                }
        }

        [Test]
        public void ElementaryFunctionsTest()
        {
            var elementaryFunctions = new List<MethodInfo>();
            elementaryFunctions.AddRange(
                typeof(ElementaryFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            //methodsToTest.AddRange(typeof(ElementaryFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            // methodsToTest.AddRange(typeof(StatisticFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            TestFunctions(elementaryFunctions);
        }

        [Test]
        public void EllipticΠIncompleteTest()
        {
            object ret = null;
            foreach (var a in _a)
                foreach (var d1 in _x)
                    foreach (var d2 in _x)

                    {
                        try
                        {
                            ret = SpecialFunctions.EllipticΠ(d1, d2, a);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }

        [Test]
        public void EllipticΠTest()
        {
            object ret = null;

            foreach (var d1 in _x)
                foreach (var d2 in _x)

                {
                    try
                    {
                        ret = SpecialFunctions.EllipticΠ(d1, d2);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }


        [Test]
        public void EnTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)

                {
                    Trace.WriteLine($"Testing {nameof(EnTest)}, parameters: {i}; {d1}");
                    try
                    {
                        ret = SpecialFunctions.En(i, d1);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }


        [Test]
        public void Hypergeometric1F1IntTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var j in _a)
                    foreach (var d1 in _x)

                    {
                        Trace.WriteLine($"Testing {nameof(Hypergeometric1F1IntTest)}, parameters: {i}; {j}; {d1}");
                        try
                        {
                            ret = SpecialFunctions.Hypergeometric1F1(i, j, d1);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }


        [Test]
        public void Hypergeometric1F1Test()
        {
            object ret = null;
            foreach (var d1 in _x)
                foreach (var d2 in _x)
                    foreach (var d3 in _x)

                    {
                        Trace.WriteLine($"Testing {nameof(Hypergeometric1F1Test)}, parameters: {d1}; {d2}; {d3}");
                        try
                        {
                            ret = SpecialFunctions.Hypergeometric1F1(d1, d2, d3);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }


        [Test]
        public void Hypergeometric2F1IntTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var j in _a)
                    foreach (var d1 in _x)

                    {
                        Trace.WriteLine($"Testing {nameof(Hypergeometric2F1IntTest)}, parameters: {i}; {j}; {d1}");
                        try
                        {
                            ret = SpecialFunctions.Hypergeometric2F1(i, j, d1);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }


        [Test]
        public void Hypergeometric2F1Test()
        {
            object ret = null;
            foreach (var d1 in _x)
                foreach (var d2 in _x)
                    foreach (var d3 in _x)

                    {
                        Trace.WriteLine($"Testing {nameof(Hypergeometric2F1Test)}, parameters: {d1}; {d2}; {d3}");
                        try
                        {
                            ret = SpecialFunctions.Hypergeometric2F1(d1, d2, d3);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }


        [Test]
        public void HypergeometricUIntTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var j in _a)
                    foreach (var d1 in _x)

                    {
                        Trace.WriteLine($"Testing {nameof(HypergeometricUIntTest)}, parameters: {i}; {j}; {d1}");
                        try
                        {
                            ret = SpecialFunctions.HypergeometricU(i, j, d1);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }


        [Test]
        public void HypergeometricUTest()
        {
            object ret = null;
            foreach (var d1 in _x)
                foreach (var d2 in _x)
                    foreach (var d3 in _x)

                    {
                        Trace.WriteLine($"Testing {nameof(HypergeometricUTest)}, parameters: {d1}; {d2}; {d3}");
                        try
                        {
                            ret = SpecialFunctions.HypergeometricU(d1, d2, d3);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }


        [Test]
        public void MathieuBnTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)

                {
                    try
                    {
                        ret = SpecialFunctions.MathieuBn(i, d1);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }

        [Test]
        public void MathieuCETest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)
                    foreach (var d2 in _x)

                    {
                        try
                        {
                            ret = SpecialFunctions.MathieuCE(i, d1, d2);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Exception occured: " + ex);
                        }
                        Assert.IsNotNull(ret);
                    }
        }

        //MathieuMc

        [Test]
        public void MathieuMcTest()
        {
            object ret = null;

            foreach (var a1 in _a)
                foreach (var a2 in _a)
                    foreach (var d1 in _x)
                        foreach (var d2 in _x)

                        {
                            try
                            {
                                Debug.WriteLine(@"MathieuMc({0},{1},{2},{3}", a1, a2, d1, d2);
                                ret = SpecialFunctions.MathieuMc(a1, a2, d1, d2);
                            }
                            catch (Exception ex)
                            {
                                Assert.Fail("Exception occured: " + ex);
                            }
                            Assert.IsNotNull(ret);
                        }
        }

        [Test]
        public void MathieuMsTest()
        {
            object ret = null;

            foreach (var a1 in _a)
                foreach (var a2 in _a)
                    foreach (var d1 in _x)
                        foreach (var d2 in _x)
                        {
                            try
                            {
                                Debug.WriteLine(@"MathieuMs({0},{1},{2},{3}", a1, a2, d1, d2);
                                ret = SpecialFunctions.MathieuMs(a1, a2, d1, d2);
                            }
                            catch (Exception ex)
                            {
                                Assert.Fail("Exception occured: " + ex);
                            }
                            Assert.IsNotNull(ret);
                        }
        }

        [Test]
        public void ModifiedSphericalBesselInTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)

                {
                    try
                    {
                        ret = SpecialFunctions.ModifiedSphericalBesselIn(i, d1);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }

        [Test]
        public void PolyLogTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)

                {
                    try
                    {
                        ret = SpecialFunctions.PolyLog(i, d1);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }

        [Test]
        public void SpecialFunctionsTest()
        {
            var specialFunctions = new List<MethodInfo>();
            specialFunctions.AddRange(typeof(SpecialFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            //methodsToTest.AddRange(typeof(ElementaryFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            // methodsToTest.AddRange(typeof(StatisticFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            TestFunctions(specialFunctions);
        }

        [Test]
        public void SphericalBesselJnTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)

                {
                    try
                    {
                        ret = SpecialFunctions.SphericalBesselJn(i, d1);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }

        [Test]
        public void SphericalBesselYnTest()
        {
            object ret = null;

            foreach (var i in _a)
                foreach (var d1 in _x)

                {
                    try
                    {
                        ret = SpecialFunctions.SphericalBesselYn(i, d1);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Exception occured: " + ex);
                    }
                    Assert.IsNotNull(ret);
                }
        }

        [Test]
        public void StatisticsFunctionsTest()
        {
            var sstatisticsFunctions = new List<MethodInfo>();
            sstatisticsFunctions.AddRange(
                typeof(StatisticsFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            //methodsToTest.AddRange(typeof(ElementaryFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            // methodsToTest.AddRange(typeof(StatisticFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static));
            TestFunctions(sstatisticsFunctions);
        }

        [TestCase(1)]
        [TestCase(1e5)]
        [TestCase(1e10)]
        [TestCase(1e30)]
        [TestCase(1e100)]
        [TestCase(1e-5)]
        [TestCase(1e-10)]
        [TestCase(1e-30)]
        [TestCase(1e-100)]
        [TestCase(-1e5)]
        [TestCase(-1e10)]
        [TestCase(-1e30)]
        [TestCase(-1e100)]
        [TestCase(-1e-5)]
        [TestCase(-1e-10)]
        [TestCase(-1e-30)]
        [TestCase(-1e-100)]
        public void ComplexLogGammaTests(double multiplier)
        {
            object ret = null;
            foreach (var c in _c)
            {
                try
                {
                    ret = SpecialFunctions.logGamma(c);
                    ret = SpecialFunctions.logGamma(new Complex(multiplier * c.Real, c.Imaginary));
                    ret = SpecialFunctions.logGamma(new Complex(c.Real, multiplier * c.Imaginary));
                    ret = SpecialFunctions.logGamma(new Complex(multiplier * c.Real, multiplier * c.Imaginary));
                }
                catch (Exception ex)
                {
                    Assert.Fail("Exception occured: " + ex);
                }
                Assert.IsNotNull(ret);
            }
        }

        [TestCase(1)]
        [TestCase(1e5)]
        [TestCase(1e10)]
        [TestCase(1e30)]
        [TestCase(1e100)]
        [TestCase(1e-5)]
        [TestCase(1e-10)]
        [TestCase(1e-30)]
        [TestCase(1e-100)]
        [TestCase(-1e5)]
        [TestCase(-1e10)]
        [TestCase(-1e30)]
        [TestCase(-1e100)]
        [TestCase(-1e-5)]
        [TestCase(-1e-10)]
        [TestCase(-1e-30)]
        [TestCase(-1e-100)]
        public void LogGammaTests(double multiplier)
        {
            object ret = null;
            foreach (var x in _x)
            {
                try
                {
                    ret = SpecialFunctions.logGamma(multiplier* x);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Exception occured: " + ex);
                }
                Assert.IsNotNull(ret);
            }
        }
    }
}