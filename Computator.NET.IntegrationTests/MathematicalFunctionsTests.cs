using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using Moq;
using NUnit.Framework;

// ReSharper disable LocalizableElement

namespace Computator.NET.IntegrationTests
{
    [TestFixture]
    public class MathematicalFunctionsTests
    {
        [OneTimeSetUp]
        public void InitFixture()
        {
            C = (from d1 in X from d2 in X select new Complex(d1, d2)).ToArray();
        }
        
        [SetUp]
        public void Init()
        {
            Settings.Default.CalculationsErrors = CalculationsErrors.ReturnNAN;
            GSLInitializer.Initialize(new Mock<IMessagingService>().Object);
            NativeMethods.gsl_set_error_handler_off();
        }

        private const int stepsSmall = 25;
        private const int steps = 50;
        private const double min = -10;
        private const double max = 10;
        private readonly int[] A = Enumerable.Range(-10, 21).ToArray(); // {-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5};
        private readonly int[] Asmall = Enumerable.Range(-5, 11).ToArray();

        private readonly int[] Averysmall = Enumerable.Range(-3, 7).ToArray();

        private readonly double[] X = Enumerable.Range(0, steps)
            .Select(i => min + (max - min)*((double) i/(steps - 1))).ToArray();

        //  new int[] { -20, -19, -18, -17, -16, -15, -14, -13, -12, -11, -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

        private readonly double[] Xsmall = Enumerable.Range(0, stepsSmall)
            .Select(i => min + (max - min)*((double) i/(steps - 1))).ToArray();

        private Complex[] C;

        private void TestFunctions(List<MethodInfo> methodsToTest)
        {
            object ret = null;

            foreach (var specialMethod in methodsToTest)
            {
                Debug.WriteLine("Testing: " + specialMethod);
                Trace.WriteLine("Testing: " + specialMethod);
                Console.WriteLine("Testing: " + specialMethod);
                var parameters = specialMethod.GetParameters();
                var testIt = true;


                for (var index = 0;
                    index < A.Length && (parameters.Any(p => p.ParameterType.IsInteger()) || index == 0);
                    index++)
                {
                    var i = A[index];
                    for (var index1 = 0;
                        index1 < X.Length && (parameters.Any(p => p.ParameterType.IsDouble()) || index1 == 0);
                        index1++)
                    {
                        var d = X[index1];
                        for (var i1 = 0;
                            i1 < C.Length && parameters.Any(p => p.ParameterType.IsComplex() || i1 == 0);
                            i1++)
                        {
                            var c = C[i1];
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

            foreach (var i in A)
                foreach (var d1 in X)
                    foreach (var d2 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)
                    foreach (var d2 in X)

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

            foreach (var d1 in A)
                foreach (var d2 in A)
                    foreach (var d3 in A)
                        foreach (var d4 in A)
                            foreach (var d5 in A)
                                foreach (var d6 in A)
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

            foreach (var d1 in A)
                foreach (var d2 in A)
                    foreach (var d3 in A)
                        foreach (var d4 in A)
                            foreach (var d5 in A)
                                foreach (var d6 in A)
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

            foreach (var d1 in Xsmall)
                foreach (var d2 in Xsmall)
                    foreach (var d3 in Xsmall)
                        foreach (var d4 in Xsmall)
                            foreach (var d5 in Xsmall)
                                foreach (var d6 in Xsmall)
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

            foreach (var d1 in A)
                foreach (var d2 in A)
                    foreach (var d3 in A)
                        foreach (var d4 in A)
                            foreach (var d5 in A)
                                foreach (var d6 in A)
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

            foreach (var d1 in Xsmall)
                foreach (var d2 in Xsmall)
                    foreach (var d3 in Xsmall)
                        foreach (var d4 in Xsmall)
                            foreach (var d5 in Xsmall)
                                foreach (var d6 in Xsmall)
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

            foreach (var d1 in Averysmall)
                foreach (var d2 in Averysmall)
                    foreach (var d3 in Averysmall)
                        foreach (var d4 in Averysmall)
                            foreach (var d5 in Averysmall)
                                foreach (var d6 in Averysmall)
                                    foreach (var d7 in Averysmall)
                                        foreach (var d8 in Averysmall)
                                            foreach (var d9 in Averysmall)
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

            foreach (var d1 in A)
                foreach (var d2 in A)
                    foreach (var d3 in A)
                        foreach (var d4 in A)
                            foreach (var d5 in A)
                                foreach (var d6 in A)
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
            foreach (var a in A)
                foreach (var d1 in X)
                    foreach (var d2 in X)

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

            foreach (var d1 in X)
                foreach (var d2 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)

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

            foreach (var i in A)
                foreach (var j in A)
                    foreach (var d1 in X)

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
            foreach (var d1 in X)
                foreach (var d2 in X)
                    foreach (var d3 in X)

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

            foreach (var i in A)
                foreach (var j in A)
                    foreach (var d1 in X)

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
            foreach (var d1 in X)
                foreach (var d2 in X)
                    foreach (var d3 in X)

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

            foreach (var i in A)
                foreach (var j in A)
                    foreach (var d1 in X)

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
            foreach (var d1 in X)
                foreach (var d2 in X)
                    foreach (var d3 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)
                    foreach (var d2 in X)

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

            foreach (var a1 in A)
                foreach (var a2 in A)
                    foreach (var d1 in X)
                        foreach (var d2 in X)

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

            foreach (var a1 in A)
                foreach (var a2 in A)
                    foreach (var d1 in X)
                        foreach (var d2 in X)
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

            foreach (var i in A)
                foreach (var d1 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)

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

            foreach (var i in A)
                foreach (var d1 in X)

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
    }
}