using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.Distributions;

namespace Computator.NET.Functions
{
    internal static class StatisticsFunctions
    {
        public const string ToCode = @"


        private static Random m_random = new Random();
        public static double random(double x, double y)
        {
          //  if (x == y)
            //    return x;
            //else if (y < x)
                //return double.NaN;
            return x + (y-x)*m_random.NextDouble();
        }

        public static double random(int x)
        {
            return m_random.Next(x+1);
        }

        public static double random(int x, int y)
        {
            return m_random.Next(x,y+1);
        }

        public static double random(double x)
        {
            return x*m_random.NextDouble();
        }

        public static Complex random(Complex z1, Complex z2)
        {
            return z1 + (z2 - z1) * m_random.NextDouble();
        }

        public static Complex random(Complex z)
        {
            //return z * m_random.NextDouble();
            return new Complex(z.Real * m_random.NextDouble(), z.Imaginary * m_random.NextDouble());
        }

        
        public class NormalDistribution : MathNet.Numerics.Distributions.Normal
        {
            //combos
            public NormalDistribution(double mean, double stddev)
                : base(mean, stddev)
            {
            }

            //objects:

            public static NormalDistribution fromMeanStdDev(double mean, double stddev)
            {
                var normal = MathNet.Numerics.Distributions.Normal.WithMeanStdDev(mean, stddev); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            public static NormalDistribution fromMeanVariance(double mean, double var)
            {
                var normal = MathNet.Numerics.Distributions.Normal.WithMeanVariance(mean, var); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            public static NormalDistribution fromMeanPrecision(double mean, double precision)
            {
                var normal = MathNet.Numerics.Distributions.Normal.WithMeanPrecision(mean, precision); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }



            public static NormalDistribution estime(params double[] points)
            {
                var normal = MathNet.Numerics.Distributions.Normal.Estimate(points); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            public static NormalDistribution estime(IEnumerable<double> points)
            {
                var normal = MathNet.Numerics.Distributions.Normal.Estimate(points); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            //naturally static
            public static double PDF(double mean, double stddev, double x)
            {
                return MathNet.Numerics.Distributions.Normal.PDF(mean, stddev, x);
            }
            public static double lnPDF(double mean, double stddev, double x)
            {
                return MathNet.Numerics.Distributions.Normal.PDFLn(mean, stddev, x);
            }

            public static double CDF(double mean, double stddev, double x)
            {
                return MathNet.Numerics.Distributions.Normal.CDF(mean, stddev, x);
            }

            public static double invCDF(double mean, double stddev, double x)
            {
                return MathNet.Numerics.Distributions.Normal.InvCDF(mean, stddev, x);
            }
            public static double sample(double mean, double stddev) //discuss
            {
                return MathNet.Numerics.Distributions.Normal.Sample(mean, stddev);
            }

            public static double isValid(double mean, double stddev) //discuss
            {
                return MathNet.Numerics.Distributions.Normal.IsValidParameterSet(mean, stddev) ? 1.0 : 0.0;
            }


            //forced static
            public static double density(double mean, double stddev, double x)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Density(x);
            }

            public static double densityLn(double mean, double stddev, double x)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).DensityLn(x);
            }



            public static double cumulativeDistribution(double mean, double stddev, double x) //discuss
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).CumulativeDistribution(x);
            }

            public static double inverseCumulativeDistribution(double mean, double stddev, double x) //discuss
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).InverseCumulativeDistribution(x);
            }

            //forced properties
            public static double entropy(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Entropy;
            }

            public static double max(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Maximum;
            }

            public static double min(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Minimum;
            }


            public static double skewness(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Skewness;
            }

            public static double variance(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Variance;
            }

            public static double precision(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Precision;
            }

            public static double median(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Median;
            }


            public static double mode(double mean, double stddev)
            {
                return new MathNet.Numerics.Distributions.Normal(mean, stddev).Mode;
            }





            //object oriented
            public double PDF(double x)
            {
                return MathNet.Numerics.Distributions.Normal.PDF(this.Mean, this.StdDev, x);
            }
            public double lnPDF(double x)
            {
                return MathNet.Numerics.Distributions.Normal.PDFLn(this.Mean, this.StdDev, x);
            }

            public double CDF(double x)
            {
                return MathNet.Numerics.Distributions.Normal.CDF(this.Mean, this.StdDev, x);
            }

            public double invCDF(double x)
            {
                return MathNet.Numerics.Distributions.Normal.InvCDF(this.Mean, this.StdDev, x);
            }
            public double sample() //discuss
            {
                return this.sample();
            }



            //forced static
            public double density(double x)
            {
                return this.Density(x);
            }

            public double densityLn(double x)
            {
                return this.DensityLn(x);
            }



            public double cumulativeDistribution(double x) //discuss
            {
                return this.CumulativeDistribution(x);
            }

            public double inverseCumulativeDistribution(double x) //discuss
            {
                return this.InverseCumulativeDistribution(x);
            }

            //forced properties
            public double entropy()
            {
                return this.Entropy;
            }

            public double max()
            {
                return this.Maximum;
            }

            public double min()
            {
                return this.Minimum;
            }


            public double skewness()
            {
                return this.Skewness;
            }

            public double variance()
            {
                return this.Variance;
            }

            public double precision()
            {
                return this.Precision;
            }

            public double median()
            {
                return this.Median;
            }


            public double mode()
            {
                return this.Mode;
            }
        }


        ";
        private static readonly Random m_random = new Random();

        public static double random(double x, double y)
        {
            //  if (x == y)
            //    return x;
            //else if (y < x)
            //return double.NaN;
            return x + (y - x)*m_random.NextDouble();
        }

        public static double random(int x)
        {
            return m_random.Next(x + 1);
        }

        public static double random(int x, int y)
        {
            return m_random.Next(x, y + 1);
        }

        public static double random(double x)
        {
            return x*m_random.NextDouble();
        }

        public static Complex random(Complex z1, Complex z2)
        {
            return z1 + (z2 - z1)*m_random.NextDouble();
        }

        public static Complex random(Complex z)
        {
            //return z * m_random.NextDouble();
            return new Complex(z.Real*m_random.NextDouble(), z.Imaginary*m_random.NextDouble());
        }

        public class NormalDistribution : Normal
        {
            //combos
            public NormalDistribution(double mean, double stddev)
                : base(mean, stddev)
            {
            }

            //objects:

            public static NormalDistribution fromMeanStdDev(double mean, double stddev)
            {
                Normal normal = WithMeanStdDev(mean, stddev); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            public static NormalDistribution fromMeanVariance(double mean, double var)
            {
                Normal normal = WithMeanVariance(mean, var); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            public static NormalDistribution fromMeanPrecision(double mean, double precision)
            {
                Normal normal = WithMeanPrecision(mean, precision); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }


            public static NormalDistribution estime(params double[] points)
            {
                Normal normal = Estimate(points); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            public static NormalDistribution estime(IEnumerable<double> points)
            {
                Normal normal = Estimate(points); //points
                return new NormalDistribution(normal.Mean, normal.StdDev);
            }

            //naturally static
            public static double PDF(double mean, double stddev, double x)
            {
                return Normal.PDF(mean, stddev, x);
            }

            public static double lnPDF(double mean, double stddev, double x)
            {
                return PDFLn(mean, stddev, x);
            }

            public static double CDF(double mean, double stddev, double x)
            {
                return Normal.CDF(mean, stddev, x);
            }

            public static double invCDF(double mean, double stddev, double x)
            {
                return InvCDF(mean, stddev, x);
            }

            public static double sample(double mean, double stddev) //discuss
            {
                return Sample(mean, stddev);
            }

            public static double isValid(double mean, double stddev) //discuss
            {
                return IsValidParameterSet(mean, stddev) ? 1.0 : 0.0;
            }


            //forced static
            //public static double density(double mean, double stddev, double x)
            //{
            //    return new MathNet.Numerics.Distributions.Normal(mean, stddev).Density(x);
            //}

            //public static double densityLn(double mean, double stddev, double x)
            //{
            //    return new MathNet.Numerics.Distributions.Normal(mean, stddev).DensityLn(x);
            //}


            //public static double cumulativeDistribution(double mean, double stddev, double x) //discuss
            //{
            //    return new MathNet.Numerics.Distributions.Normal(mean, stddev).CumulativeDistribution(x);
            //}

            //public static double inverseCumulativeDistribution(double mean, double stddev, double x) //discuss
            //{
            //    return new MathNet.Numerics.Distributions.Normal(mean, stddev).InverseCumulativeDistribution(x);
            //}

            //forced properties
            public static double entropy(double mean, double stddev)
            {
                return new Normal(mean, stddev).Entropy;
            }

            public static double max(double mean, double stddev)
            {
                return new Normal(mean, stddev).Maximum;
            }

            public static double min(double mean, double stddev)
            {
                return new Normal(mean, stddev).Minimum;
            }


            public static double skewness(double mean, double stddev)
            {
                return new Normal(mean, stddev).Skewness;
            }

            public static double variance(double mean, double stddev)
            {
                return new Normal(mean, stddev).Variance;
            }

            public static double precision(double mean, double stddev)
            {
                return new Normal(mean, stddev).Precision;
            }

            public static double median(double mean, double stddev)
            {
                return new Normal(mean, stddev).Median;
            }


            public static double mode(double mean, double stddev)
            {
                return new Normal(mean, stddev).Mode;
            }


            //object oriented
            public double PDF(double x)
            {
                return Normal.PDF(Mean, StdDev, x);
            }

            public double lnPDF(double x)
            {
                return PDFLn(Mean, StdDev, x);
            }

            public double CDF(double x)
            {
                return Normal.CDF(Mean, StdDev, x);
            }

            public double invCDF(double x)
            {
                return InvCDF(Mean, StdDev, x);
            }

            public double sample() //discuss
            {
                return sample();
            }


            //forced static
            public double density(double x)
            {
                return Density(x);
            }

            public double densityLn(double x)
            {
                return DensityLn(x);
            }


            public double cumulativeDistribution(double x) //discuss
            {
                return CumulativeDistribution(x);
            }

            public double inverseCumulativeDistribution(double x) //discuss
            {
                return InverseCumulativeDistribution(x);
            }

            //forced properties
            public double entropy()
            {
                return Entropy;
            }

            public double max()
            {
                return Maximum;
            }

            public double min()
            {
                return Minimum;
            }


            public double skewness()
            {
                return Skewness;
            }

            public double variance()
            {
                return Variance;
            }

            public double precision()
            {
                return Precision;
            }

            public double median()
            {
                return Median;
            }


            public double mode()
            {
                return Mode;
            }
        }
    }
}