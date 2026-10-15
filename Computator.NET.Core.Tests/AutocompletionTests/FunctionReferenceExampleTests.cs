using System;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class FunctionReferenceExampleTests
    {
        [OneTimeSetUp]
        public void InitializeNativeExamples() => GSLInitializer.Initialize();

        // Simple published examples/conventions with independent known answers.
        // Existing numerical tests and their tolerances are not changed.
        [TestCase("sinc(0)", 1.0)]
        [TestCase("sinc(1)", 0.8414709848078965)]
        [TestCase("Heaviside(0)", 0.5)]
        [TestCase("TaylorCoeff(3,2)", 1.3333333333333333)]
        [TestCase("gamma(5)", 24.0)]
        [TestCase("gamma(1,2)", 0.1353352832366127)]
        [TestCase("Beta(1,1)", 1.0)]
        [TestCase("Beta(0.5,1,1)", 0.5)]
        [TestCase("BesselJ0(0)", 1.0)]
        [TestCase("EllipticK(0)", 1.5707963267948966)]
        [TestCase("ModifiedSphericalBesselKn(0,1)", 1.5707963267948966)]
        [TestCase("FermiDiracF0(0)", 0.6931471805599453)]
        [TestCase("Normal.CDF(0,1,0)", 0.5)]
        [TestCase("Normal.PDF(0,1,0)", 0.3989422804014327)]
        [TestCase("Geometric.PMF(0.5,1)", 0.5)]
        [TestCase("NegativeBinomial.PMF(1,0.5,0)", 0.5)]
        [TestCase("Gamma.PDF(1,2,0.5)", 0.7357588823428847)]
        [TestCase("Poisson.PMF(1,0)", 0.3678794411714423)]
        public void DocumentedExampleAgreesWithTheActualImplementation(string example, double expected)
        {
            var actual = example switch
            {
                "sinc(0)" => ElementaryFunctions.sinc(0),
                "sinc(1)" => ElementaryFunctions.sinc(1),
                "Heaviside(0)" => ElementaryFunctions.Heaviside(0),
                "TaylorCoeff(3,2)" => ElementaryFunctions.TaylorCoeff(3, 2),
                "gamma(5)" => SpecialFunctions.gamma(5),
                "gamma(1,2)" => SpecialFunctions.gamma(1, 2),
                "Beta(1,1)" => SpecialFunctions.Beta(1, 1),
                "Beta(0.5,1,1)" => SpecialFunctions.Beta(0.5, 1, 1),
                "BesselJ0(0)" => SpecialFunctions.BesselJ0(0),
                "EllipticK(0)" => SpecialFunctions.EllipticK(0),
                "ModifiedSphericalBesselKn(0,1)" => SpecialFunctions.ModifiedSphericalBesselKn(0, 1),
                "FermiDiracF0(0)" => SpecialFunctions.FermiDiracF0(0),
                "Normal.CDF(0,1,0)" => MathNet.Numerics.Distributions.Normal.CDF(0, 1, 0),
                "Normal.PDF(0,1,0)" => MathNet.Numerics.Distributions.Normal.PDF(0, 1, 0),
                "Geometric.PMF(0.5,1)" => MathNet.Numerics.Distributions.Geometric.PMF(0.5, 1),
                "NegativeBinomial.PMF(1,0.5,0)" => MathNet.Numerics.Distributions.NegativeBinomial.PMF(1, 0.5, 0),
                "Gamma.PDF(1,2,0.5)" => MathNet.Numerics.Distributions.Gamma.PDF(1, 2, 0.5),
                "Poisson.PMF(1,0)" => MathNet.Numerics.Distributions.Poisson.PMF(1, 0),
                _ => throw new ArgumentException(example)
            };
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(1e-11));
        }
    }
}
