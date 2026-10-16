using System;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion.DataSource;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture]
    public class FunctionReferenceDistributionTests
    {
        private static string Reference(string signature) =>
            XElement.Parse(new FunctionsDetailsFileSource().Details[signature].Description).Value;

        [Test]
        public void CategoricalQuantileWarningDistinguishesTheLowerQuantileFromATiedBinarySearch()
        {
            var weights = new[] { 1.0, 0.0, 0.0, 1.0 };
            // Independent lower-quantile definition: index zero already reaches p=0.5.
            NUnit.Framework.Assert.That(Categorical.CDF(weights, 0), Is.EqualTo(0.5));
            NUnit.Framework.Assert.That(Categorical.InvCDF(weights, 0.5), Is.EqualTo(1));
            NUnit.Framework.Assert.That(Categorical.InvCDFWithCumulativeDistribution(new[] { 1.0, 1.0, 1.0, 2.0 }, 0.5), Is.EqualTo(1));
            foreach (var signature in new[] { "Categorical.InvCDF(probabilityMass,probability)", "Categorical.InvCDFWithCumulativeDistribution(cdfUnnormalized,probability)" })
            {
                var text = Reference(signature);
                NUnit.Framework.Assert.That(text, Does.Contain("binary search"));
                NUnit.Framework.Assert.That(text, Does.Contain("not guaranteed to return the first"));
            }
        }

        [Test]
        public void MatrixNormalSamplingWarningDistinguishesInverseColumnScalingFromTheCovarianceDefinition()
        {
            var mean = DenseMatrix.OfArray(new[,] { { 0.0 } });
            var unit = DenseMatrix.OfArray(new[,] { { 1.0 } });
            var columns = DenseMatrix.OfArray(new[,] { { 4.0 } });
            var standardDraw = MatrixNormal.Sample(new Random(12345), mean, unit, unit)[0, 0];
            var actual = MatrixNormal.Sample(new Random(12345), mean, unit, columns)[0, 0];
            // A column covariance of four mathematically multiplies a standard draw by two.
            // The pinned implementation instead divides it by two. Do not bless that as the definition.
            NUnit.Framework.Assert.That(Math.Abs(standardDraw), Is.GreaterThan(0.01));
            NUnit.Framework.Assert.That(actual, Is.EqualTo(standardDraw / 2).Within(1e-14));
            NUnit.Framework.Assert.That(Math.Abs(actual - standardDraw * 2), Is.GreaterThan(0.01));
            var text = Reference("MatrixNormal.Sample(rnd,m,v,k)");
            NUnit.Framework.Assert.That(text, Does.Contain("v⊗k⁻¹"));
            NUnit.Framework.Assert.That(text, Does.Contain("does not implement the stated column-covariance convention"));
        }

        [Test]
        public void LevyEvaluationWarningIdentifiesTheBoundaryAndOutsideSupportDifferences()
        {
            // A Levy law has zero density/CDF below its location and density tends to zero at it.
            NUnit.Framework.Assert.That(Stable.PDF(0.5, 1, 1, 0, 0), Is.NaN);
            NUnit.Framework.Assert.That(Stable.PDFLn(0.5, 1, 1, 0, 0), Is.NaN);
            NUnit.Framework.Assert.That(Stable.CDF(0.5, 1, 1, 0, -1), Is.NaN);
            NUnit.Framework.Assert.That(Stable.CDF(0.5, 1, 1, 0, 0), Is.Zero);
            NUnit.Framework.Assert.Throws<NotSupportedException>(() => Stable.PDF(0.5, 1, 1, 0, -1));
            foreach (var operation in new[] { "PDF", "PDFLn", "CDF" })
                NUnit.Framework.Assert.That(Reference("Stable." + operation + "(alpha,beta,scale,location,x)"),
                    Does.Contain("At x=location, PDF and PDFLn return NaN"));
        }
    }
}
