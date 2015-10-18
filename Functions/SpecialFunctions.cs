using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Accord.Math;
using MathNet.Numerics;
using Meta.Numerics.Functions;

//using Meta.Numerics.Spin;

namespace Computator.NET.Functions
{
    internal static class SpecialFunctions
    {
        public const string ToCode =
            @"
     

        public static double findRoot(Func<double,double> f, double a, double b)
        {
            double ret = double.NaN;

            try
            {
                ret = MathNet.Numerics.FindRoots.OfFunction(f, a, b,1e-10,1000000);
            }
            catch (Exception ex)
            {

            }
            return ret;
        }


        #region signal processing

        public static double Gabor(double x, double mean, double amplitude, double position, double width, double phase,
            double frequency)
        {
            return Accord.Math.Gabor.Function1D(x, mean, amplitude, position, width, phase, frequency);
        }

        public static Complex Gabor(Complex z, double λ, double θ, double ψ, double σ, double γ)
        {
            AForge.Math.Complex z2 = Accord.Math.Gabor.Function2D((int)z.Real, (int)z.Imaginary, λ, θ, ψ, σ, γ);

            return new Complex(z2.Re, z2.Im);
        }

        #endregion

        #region test functions

        public static double Ackley(params double[] xi)
        {
            return TestFunctions.Ackley(xi);
        }

        public static double Rastrigin(params double[] xi)
        {
            return TestFunctions.Rastrigin(xi);
        }

        public static double Bohachevsky1(double x, double y)
        {
            return TestFunctions.Bohachevsky1(x, y);
        }

        public static double dropWave(double x, double y)
        {
            return TestFunctions.DropWave(x, y);
        }

        public static double Himmelblau(double x, double y)
        {
            return TestFunctions.Himmelblau(x, y);
        }

        public static double Matyas(double x, double y)
        {
            return TestFunctions.Matyas(x, y);
        }

        public static double sixHumpCamel(double x, double y)
        {
            return TestFunctions.SixHumpCamel(x, y);
        }

        public static double Rosenbrock(double x, double y)
        {
            return TestFunctions.Rosenbrock(x, y);
        }

        public static double Rosenbrock(params double[] xi)
        {
            return TestFunctions.Rosenbrock(xi);
        }

        #endregion

        #region Gamma and related functions

        public static Func<double, double, double> leftRegularizedGamma =
             (a, x) => (a <= 0 || x < 0) ? double.NaN : AdvancedMath.LeftRegularizedGamma(a, x);


        public static Func<double, double, double> rightRegularizedGamma =
             (a, x) => (a <= 0 || x < 0) ? double.NaN : AdvancedMath.RightRegularizedGamma(a, x);

        public static Func<int, double, double> ψn = AdvancedMath.Psi;

        public static Func<int, double, double> polyGamma = AdvancedMath.Psi;

        public static Func<int, double, double> ψⁿ = AdvancedMath.Psi;


        public static double gamma(double value)
        {
            return AdvancedMath.Gamma((value));
        }


        public static double Γ(double value)
        {
            return AdvancedMath.Gamma((value));
        }



        public static double logGamma(double value)
        {
            if ((value) <= 0.0)
                return double.NaN;
            return AdvancedMath.LogGamma((value));
        }


        public static double logΓ(double value)
        {
            if ((value) <= 0.0)
                return double.NaN;
            return AdvancedMath.LogGamma((value));
        }


        public static double psi(double value)
        {
            return AdvancedMath.Psi((value));
        }


        public static double digamma(double value)
        {
            return AdvancedMath.Psi((value));
        }


        public static double ψ(double value)
        {
            return AdvancedMath.Psi((value));
        }

        //COMPLEX:

        public static Complex gamma(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Gamma(cmplxToMeta(value)));
        }


        public static Complex Γ(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Gamma(cmplxToMeta(value)));
        }



        public static Complex logGamma(Complex value)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(value.Real, value.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val * Complex.ImaginaryOne);
        }


        public static Complex logΓ(Complex value)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(value.Real, value.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val * Complex.ImaginaryOne);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_lngamma_complex_e(double zr, double zi, out gsl_sf_result lnr,
            out gsl_sf_result arg);



        public static Complex psi(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Psi(cmplxToMeta(value)));
        }


        public static Complex digamma(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Psi(cmplxToMeta(value)));
        }


        public static Complex ψ(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Psi(cmplxToMeta(value)));
        }

        //non complex type compatible gamma-like functions:


        public static double gamma(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return AdvancedMath.Gamma(a, x);
        }

        public static double Γ(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return AdvancedMath.Gamma(a, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_Q(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_P(double a, double x);

        // public static double gammaQ(double a, double x) { if (x < 0 || a <= 0) return double.NaN; return gsl_sf_gamma_inc_Q(a, x); }

        public static double gammaQ(double a, double x)
        {
            return MathNet.Numerics.SpecialFunctions.GammaUpperRegularized(a, x);
        }


        public static double gammaP(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return gsl_sf_gamma_inc_P(a, x);
        }


        public static double Beta(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(x, a, b);
        }


        public static double BetaNorm(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_beta_inc(x, a, b);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_beta_inc(double a, double b, double x);


        public static double Beta(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(a, b);
        }


        public static double β(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(x, a, b);
        }


        public static double β(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(a, b);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_lnbeta(double a, double b);


        public static double logβ(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_lnbeta(a, b);
        }

        public static double logBeta(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_lnbeta(a, b);
        }

        #endregion

        #region coefficients and special values

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_poch(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_pochrel(double a, double x);

        public static double Pochhammer(double a, double x)
        {
            if (((int)(x) == x && x <= 0) || ((int)(a + x) == a + x && a + x <= 0)) return double.NaN;
            return gsl_sf_poch(a, x);
        }

        public static double PochhammerRelative(double a, double x)
        {
            if (((int)(x) == x && x <= 0) || ((int)(a + x) == a + x && a + x <= 0)) return double.NaN;
            return gsl_sf_pochrel(a, x);
        }

        #endregion

        #region logarithm derrived functions


        public static double PolyLog(int n, double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.PolyLog(n, x);
        }


        public static double diLogarithm(double value)
        {
            if (value > 1.0)
                return (double.NaN);
            return AdvancedMath.DiLog((value));
        }

        public static double diLog(double value)
        {
            if (value > 1.0)
                return (double.NaN);
            return AdvancedMath.DiLog((value));
        }


        public static double SpencesIntegral(double value)
        {
            if (value < 0.0)
                return (double.NaN);
            return AdvancedMath.DiLog(1 - (value));
        }



        public static Complex diLogarithm(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.DiLog(cmplxToMeta(value)));
        }

        public static Complex diLog(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.DiLog(cmplxToMeta(value)));
        }


        public static Complex SpencesIntegral(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.DiLog(1 - cmplxToMeta(value)));
        }

        #endregion

        #region Wave functions


        public static double CoulombG(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return AdvancedMath.CoulombG(L, η, ρ);
        }


        public static double CoulombF(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return AdvancedMath.CoulombF(L, η, ρ);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_CL_e(double L, double eta, out gsl_sf_result result);


        public static double CoulombC(int L, double η)
        {
            if (L < 0) return double.NaN;
            gsl_sf_coulomb_CL_e(L, η, out sfResult);
            return sfResult.val;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hydrogenicR(int n, int l, double Z, double r);

        public static double HydrogenicR(int n, int l, double Z, double r)
        {
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }

        public static double Rnl(int n, int l, double Z, double r)
        {
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }


        public static double CoulombW(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return CoulombC(1, η) * CoulombF(L, η, ρ) + CoulombC(2, η) * CoulombG(L, η, ρ);
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_wave_FG_e(double eta, double x,
            double lam_F,
            int k_lam_G,
            out gsl_sf_result F, out gsl_sf_result Fp,
            out gsl_sf_result G, out gsl_sf_result Gp,
            out double exp_F, out double exp_G);


        public static double CoulombGprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out r2, out r3, out sfResult, out d1, out d2);
            return sfResult.val;
        }


        public static double CoulombFprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out sfResult, out r3, out r2, out d1, out d2);
            return sfResult.val;
        }

        #endregion

        #region Fermi–Dirac complete&incomplete integral

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_int(int j, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_inc_0(double x, double b);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_0(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_mhalf(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_half(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_3half(double x);

        public static double FermiDiracFmhalf(double x)
        {
            return gsl_sf_fermi_dirac_mhalf(x);
        }

        public static double FermiDiracFhalf(double x)
        {
            return gsl_sf_fermi_dirac_half(x);
        }

        public static double FermiDiracF3half(double x)
        {
            return gsl_sf_fermi_dirac_3half(x);
        }

        public static double FermiDiracF0(double x)
        {
            return gsl_sf_fermi_dirac_0(x);
        }

        public static double FermiDiracF0(double x, double b)
        {
            return gsl_sf_fermi_dirac_inc_0(x, b);
        }

        public static double FermiDiracFj(int j, double x)
        {
            return gsl_sf_fermi_dirac_int(j, x);
        }

        #endregion

        #region lambert W functions

        public static Func<double, double> LambertW0 =
            x => (x <= -1 / Math.E) ? double.NaN : gsl_sf_lambert_W0(x);

        public static Func<double, double> LambertWm1 =
              x => (x <= -1 / Math.E) ? double.NaN : gsl_sf_lambert_Wm1(x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_W0(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_Wm1(double x);

        #endregion

        #region polynomials

        public static Func<double, double, double>
           Gegenbauer1 = gsl_sf_gegenpoly_1;

        public static Func<double, double, double>
           Gegenbauer2 = gsl_sf_gegenpoly_1;

        public static Func<double, double, double>
           Gegenbauer3 = gsl_sf_gegenpoly_1;

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_n(int n, double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_1(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_2(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_3(double lambda, double x);


        public static double Gegenbauer(int n, double α, double x)
        {
            if (α <= -0.5 || n < 0) return double.NaN;
            return gsl_sf_gegenpoly_n(n, α, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_n(int n, double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_1(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_2(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_3(double a, double x);


        public static double Laguerre(int n, double α, double x)
        {
            if (n < 0 || α <= -1.0) return double.NaN;
            return gsl_sf_laguerre_n(n, α, x);
        }


        public static double Laguerre(int n, double x)
        {
            if (n < 0) return double.NaN;
            return gsl_sf_laguerre_n(n, 0, x);
        }


        public static double LegendreP(int l, double x)
        {
            if (x > 1.0 || x < -1.0) return double.NaN;
            return OrthogonalPolynomials.LegendreP(l, x);
        }

        public static double LegendreP(int l, int m, double x)
        {
            if (x > 1.0 || x < -1.0) return double.NaN;
            return OrthogonalPolynomials.LegendreP(l, m, x);
        }

        //add legendre Q


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_Ql(int l, double x);

        // Q_l(x), x > -1, x != 1, l >= 0
        public static double LegendreQ(int l, double x)
        {
            if (x == 1.0 || x <= -1.0 || l < 0) return double.NaN;
            return gsl_sf_legendre_Ql(l, x);
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_sphPlm(int l, int m, double x);

        public static double SphericalLegendreP(int l, int m, double x)
        {
            if (x > 1.0 || x < -1.0) return double.NaN;
            return gsl_sf_legendre_sphPlm(l, m, x);
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_half(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_mhalf(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_0(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_1(double lambda, double x);


        public static double ConicalP(double μ, double λ, double x)
        {
            if (x <= -1.0) return double.NaN;
            if (μ == (int)μ)
                return ConicalP((int)μ, λ, x);
            if (μ == -0.5)
                return gsl_sf_conicalP_mhalf(λ, x);
            if (μ == 0.5)
                return gsl_sf_conicalP_half(λ, x);
            return double.NaN;
        }


        private static double ConicalP(int μ, double λ, double x)
        {
            if (x <= -1.0) return double.NaN;

            switch (μ)
            {
                case 0:
                    return gsl_sf_conicalP_0(λ, x);
                case 1:
                    return gsl_sf_conicalP_1(λ, x); //fixed...

                default:
                    return double.NaN;
            }
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_sph_reg(int l, double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_cyl_reg(int m, double lambda, double x);

        public static double SphericalConicalP(int l, double λ, double x)
        {
            if (x <= -1.0 || l < -1 || x == 0.0) return double.NaN;
            return gsl_sf_conicalP_sph_reg(l, λ, x);
        }

        public static double CylindricalConicalP(int m, double λ, double x)
        {
            if (x <= -1.0 || m < -1 || x == 0.0) return double.NaN;
            return gsl_sf_conicalP_cyl_reg(m, λ, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_H3d(int l, double lambda, double eta);

        public static double LegendreH3D(int l, double λ, double η)
        {
            if (η < 0 || l < 0 || (l >= 2 && λ == 0.0)) return double.NaN;
            return gsl_sf_legendre_H3d(l, λ, η);
        }

        public static double ChebyshevT(int n, double x)
        {
            return OrthogonalPolynomials.ChebyshevT(n, x);
        }

        //add ChebyshevU

        public static double HermiteH(int n, double x)
        {
            return OrthogonalPolynomials.HermiteH(n, x);
        }

        public static double HermiteHe(int n, double x)
        {
            return OrthogonalPolynomials.HermiteHe(n, x);
        }

        public static double ZernikeR(int n, int m, double ρ)
        {
            if (n < m || ρ < 0.0 || ρ > 1.0) return double.NaN;
            return OrthogonalPolynomials.ZernikeR(n, m, ρ);
        }

        //TODO: add Zernike Z 

        #endregion

        #region transport functions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_2(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_3(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_4(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_5(double x);

        public static double TransportJ(int n, double x)
        {
            if (x < 0.0) return double.NaN;
            switch (n)
            {
                case 2:
                    return gsl_sf_transport_2(x);
                case 3:
                    return gsl_sf_transport_3(x);
                case 4:
                    return gsl_sf_transport_4(x);
                case 5:
                    return gsl_sf_transport_5(x);
            }
            return double.NaN;
        }

        #endregion

        #region synchrotron functions

        public static Func<double, double> SynchrotronF = x => (x < 0) ? double.NaN : gsl_sf_synchrotron_1(x);
        public static Func<double, double> SynchrotronG = x => (x < 0) ? double.NaN : gsl_sf_synchrotron_2(x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_1(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_2(double x);

        #endregion

        #region coupling 3,6,9-j symbols

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_3j(int two_ja, int two_jb, int two_jc,
            int two_ma, int two_mb, int two_mc);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_6j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_RacahW(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_9j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf,
            int two_jg, int two_jh, int two_ji);


        public static double Coupling3j(int ja, int jb, int jc, int ma, int mb, int mc)
        {
            return gsl_sf_coupling_3j(ja, jb, jc, ma, mb, mc);
        }

        public static double Coupling6j(int ja, int jb, int jc, int jd, int je, int jf)
        {
            return gsl_sf_coupling_6j(ja, jb, jc, jd, je, jf);
        }

        public static double CouplingRacahW(int ja, int jb, int jc, int jd, int je, int jf)
        {
            return gsl_sf_coupling_RacahW(ja, jb, jc, jd, je, jf);
        }

        public static double Coupling9j(int ja, int jb, int jc, int jd, int je, int jf, int jg, int jh, int ji)
        {
            return gsl_sf_coupling_9j(ja, jb, jc, jd, je, jf, jg, jh, jf);
        }

        public static double Coupling3j(double j1, double j2, double j3, double m1, double m2, double m3)
        {
            return SpinMath.ThreeJ(new SpinState(j1, m1), new SpinState(j2, m2), new SpinState(j3, m3));
        }

        public static double Coupling6j(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            return SpinMath.SixJ(new Spin(j1), new Spin(j2), new Spin(j3), new Spin(j4), new Spin(j5), new Spin(j6));
        }

        public static double ClebschGordan(double j1, double j2, double j, double m1, double m2, double m)
        {
            return SpinMath.ClebschGodron(new SpinState(j1, m1), new SpinState(j2, m2), new SpinState(j, m));
        }

        #endregion

        #region Hypergeometric functions


        public static Complex SphericalHarmonic(int l, int m, double θ, double φ)
        {
            return cmplxFromMeta(AdvancedMath.SphericalHarmonic(l, m, θ, φ));
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_0F1(double c, double x);

        //Hypergeometric function related to Bessel functions 0F1[c,x]

        public static double Hypergeometric0F1(double c, double x)
        {
            if (c > 0.0 || (c != (int)(c))) return gsl_sf_hyperg_0F1(c, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1_int(int m, int n, double x);

        //Confluent hypergeometric function  for integer parameters. 1F1[m,n,x] = M(m,n,x)

        public static double Hypergeometric1F1(int m, int n, double x)
        {
            return gsl_sf_hyperg_1F1_int(m, n, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1(double a, double b, double x);

        //Confluent hypergeometric function. 1F1[a,b,x] = M(a,b,x)

        public static double Hypergeometric1F1(double a, double b, double x)
        {
            return gsl_sf_hyperg_1F1(a, b, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U_int(int m, int n, double x);

        //Confluent hypergeometric function for integer parameters. U(m,n,x)

        public static double HypergeometricU(int m, int n, double x)
        {
            if (x > 0.0 || (x != (int)(x))) return gsl_sf_hyperg_U_int(m, n, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U(double a, double b, double x);

        //Confluent hypergeometric function. U(a,b,x)
        public static double HypergeometricU(double a, double b, double x)
        {
            if (x > 0.0 || (x != (int)(x))) return gsl_sf_hyperg_U(a, b, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1(double a, double b, double c, double x);

        //Gauss hypergeometric function 2F1[a,b,c,x]

        public static double Hypergeometric2F1(double a, double b, double c, double x)
        {
            if ((c > 0.0 || (c != (int)(c))) && Math.Abs(x) < 1) return gsl_sf_hyperg_2F1(a, b, c, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj(double aR, double aI, double c, double x);

        //Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x]

        public static double Hypergeometric2F1(Complex a, double c, double x)
        {
            if ((c > 0.0 || (c != (int)(c))) && Math.Abs(x) < 1)
                return gsl_sf_hyperg_2F1_conj(a.Real, a.Imaginary, c, x);
            return double.NaN;
        } //TODO: better name for a parameter

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_renorm(double a, double b, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[a,b,c,x] / Gamma[c]

        public static double Hypergeometric2F1renorm(double a, double b, double c, double x)
        {
            if ((c > 0.0 || (c != (int)(c))) && Math.Abs(x) < 1) return gsl_sf_hyperg_2F1_renorm(a, b, c, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj_renorm(double aR, double aI, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x] / Gamma[c]

        public static double Hypergeometric2F1renorm(Complex a, double c, double x)
        {
            if ((c > 0.0 || (c != (int)(c))) && Math.Abs(x) < 1)
                return gsl_sf_hyperg_2F1_conj_renorm(a.Real, a.Imaginary, c, x);
            return double.NaN;
        } //TODO: better name for a parameter
        //2F1 sometimes returns an exceptions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F0(double a, double b, double x);

        /* Mysterious hypergeometric function. The series representation
         * is a divergent hypergeometric series. However, for x < 0 we
         * have 2F0(a,b,x) = (-1/x)^a U(a,1+a-b,-1/x)*/


        public static double Hypergeometric2F0(double a, double b, double x)
        {
            if (x <= 0) return gsl_sf_hyperg_2F0(a, b, x);
            return double.NaN;
        }

        #endregion

        #region integral functions
        public static double Ti(double x)
        {
            return AdvancedMath.IntegralTi(x);
        }


        public static Func<double, double>
           Dawson = AdvancedMath.Dawson;

        public static Func<double, double> Clausen = gsl_sf_clausen;

        public static Func<double, double> Si =
           AdvancedMath.IntegralSi; //sine integral

        public static Func<double, double> Ci =
           x => (x < 0.0) ? double.NaN : AdvancedMath.IntegralCi(x); //cosine integral

        //Generalized Exponential Integral
        public static Func<int, double, double> En =
            (n, x) => (x == 0.0 || n < 0 || n > 2) ? AdvancedMath.IntegralE(n, x) : gsl_sf_expint_En(n, x);


        //   public static double EnNEW(int n, double x)//Generalized Exponential Integral
        //  {
        //     return MathNet.Numerics.SpecialFunctions.ExponentialIntegral(x, n);
        //     return 1.0;
        //}

        public static Func<double, double> Ei = x => (x == 0.0) ? AdvancedMath.IntegralEi(x) : gsl_sf_expint_Ei(x);

        public static Func<double, double> Shi =
           gsl_sf_Shi; //hyperbolic sine integral


        public static Func<double, double> Chi =
           x => (x == 0.0) ? double.NaN : gsl_sf_Chi(x); //hyperbolic cosine integral

        public static Func<double, double> Tai =
           gsl_sf_atanint; //arcus tangent integral

        //Integrals in optics
        public static Func<double, Complex> Fresnel =
           x => cmplxFromMeta(AdvancedMath.Fresnel(x));

        public static Func<double, double> FresnelS =
           AdvancedMath.FresnelS;

        public static Func<double, double> FresnelC =
           AdvancedMath.FresnelC;

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_1(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_2(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_3(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_4(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_5(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_6(double x);

        /*
       
        */


        public static double Debye(int n, double x)
        {
            if (n == 0)
                return 0;
            if (x < 0.0)
                return double.NaN;
            switch (n)
            {
                case 1:
                    return gsl_sf_debye_1(x);
                case 2:
                    return gsl_sf_debye_2(x);
                case 3:
                    return gsl_sf_debye_3(x);
                case 4:
                    return gsl_sf_debye_4(x);
                case 5:
                    return gsl_sf_debye_5(x);
                case 6:
                    return gsl_sf_debye_6(x);
            }
            if (x > 9) // x >> 1
                return Γ(n + 1.0) * Riemannζ(n + 1.0);
            return double.NaN;
        }

        //dawnson integral

        /* Calculate the Clausen integral:
         *   Cl_2(x) := Integrate[-Log[2 Sin[t/2]], {t,0,x}]
         *
         * Relation to dilogarithm:
         *   Cl_2(theta) = Im[ Li_2(e^(i theta)) ]
         */

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_clausen(double x);


        public static Complex Ein(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.Ein(cmplxToMeta(z)));
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_En(int n, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_Ei(double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_Shi(double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_Chi(double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_atanint(double x);

        #endregion

        #region Elliptic integrals

        //In mathematics, the Carlson symmetric forms of elliptic integrals are a small canonical set of elliptic integrals to which all others may be reduced. They are a modern alternative to the Legendre forms. The Legendre forms may be expressed in terms of the Carlson forms and vice versa.
        public static Func<double, double> EllipticK =
                         x =>
                             (x < 0 || x >= 1.0)
                                 ? ((x < 0 && x > -1.0) ? gsl_sf_ellint_Kcomp(x, 0) : double.NaN)
                                 : AdvancedMath.EllipticK(x);


        public static double CarlsonD(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return AdvancedMath.CarlsonD(x, y, z);
        }



        public static double CarlsonF(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return AdvancedMath.CarlsonF(x, y, z);
        }

        public static double CarlsonC(double x, double y)
        {
            if (x <= 0 || y <= 0)
                return double.NaN;
            return gsl_sf_ellint_RC(x, y, 0);
        }

        public static double CarlsonJ(double x, double y, double z, double p)
        {
            if (x <= 0 || y <= 0 || z <= 0 || p <= 0)
                return double.NaN;
            return gsl_sf_ellint_RJ(x, y, z, p, 0);
        }


        public static double EllipticF(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI / 2.0 || φ > Math.PI / 2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_F(φ, x, 0);
            return AdvancedMath.EllipticF(φ, x);
        }


        public static double EllipticE(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI / 2.0 || φ > Math.PI / 2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_E(φ, x, 0);
            return AdvancedMath.EllipticE(φ, x);
        }


        public static double EllipticΠ(double φ, double x, int n)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI / 2.0 || φ > Math.PI / 2.0)
                return double.NaN;
            return gsl_sf_ellint_P(φ, x, n, 0);
        }

        public static double EllipticD(double φ, double x, int n)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI / 2.0 || φ > Math.PI / 2.0)
                return double.NaN;
            return gsl_sf_ellint_D(φ, x, n, 0);
        }

        public static double EllipticE(double x)
        {
            if (x < 0 || x > 1.0)
                if (x < 0 && x > -1.0)
                    return gsl_sf_ellint_Ecomp(x, 0);
                else
                    return double.NaN;
            return AdvancedMath.EllipticE(x);
        }

        public static double EllipticD(double x)
        {
            if (x <= -1 || x >= 1.0)
                return double.NaN;

            return gsl_sf_ellint_Dcomp(x, 0);
        }

        public static double EllipticΠ(double x, int n)
        {
            if (x <= -1 || x >= 1.0)
                return double.NaN;
            return gsl_sf_ellint_Pcomp(x, n, 0);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Ecomp(double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Kcomp(double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Pcomp(double k, double n, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Dcomp(double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_F(double phi, double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_E(double phi, double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_P(double phi, double k, double n, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_D(double phi, double k, double n, uint mode);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RC(double x, double y, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RD(double x, double y, double z, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RF(double x, double y, double z, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RJ(double x, double y, double z, double p, uint mode);

        #endregion

        #region  Jacobian elliptic functions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_elljac_e(double u, double m, out double sn, out double cn, out double dn);

        public static double JacobiEllipticSn(double u, double m)
        {
            if (m < -1.0 || m > 1.0) return double.NaN;
            double Sn = 0, Cn = 0, Dn = 0;
            gsl_sf_elljac_e(u, m, out Sn, out Cn, out Dn);
            return Sn;
        }

        public static double JacobiEllipticCn(double u, double m)
        {
            if (m < -1.0 || m > 1.0) return double.NaN;
            double Sn = 0, Cn = 0, Dn = 0;
            gsl_sf_elljac_e(u, m, out Sn, out Cn, out Dn);
            return Cn;
        }

        public static double JacobiEllipticDn(double u, double m)
        {
            if (m < -1.0 || m > 1.0) return double.NaN;
            double Sn = 0, Cn = 0, Dn = 0;
            gsl_sf_elljac_e(u, m, out Sn, out Cn, out Dn);
            return Dn;
        }

        #endregion

        #region error functions

        public static Func<Complex, Complex> erfi =
                  z => -Complex.ImaginaryOne * erf(Complex.ImaginaryOne * z);

        public static Func<double, double> inverseErf = AdvancedMath.InverseErf;

        public static Func<double, double> inverseErfc =
                AdvancedMath.InverseErfc;

        public static Func<double, double> logErfc = gsl_sf_log_erfc;

        public static Func<double, double> erfZ = gsl_sf_erf_Z;
        public static Func<double, double> erfQ = gsl_sf_erf_Q;

        public static Func<double, double> hazard = gsl_sf_hazard;

        public static double OwenT(double h, double a)
        {
            return Accord.Math.OwensT.Function(h, a);
        }

        public static double OwenT(double h, double a, double ah)
        {
            return Accord.Math.OwensT.Function(h, a, ah);
        }

        //checked and they work ok

        public static double erf(double value)
        {
            return AdvancedMath.Erf((value));
        }


        public static double erfc(double value)
        {
            return AdvancedMath.Erfc((value));
        }


        public static double erfcx(double value)
        {
            return ((erfc(value)) / Math.Exp(-((value)) * (value)));
        }


        public static Complex erf(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Erf(cmplxToMeta(value)));
        }


        public static Complex erfc(Complex value)
        {
            return (1 - cmplxFromMeta(AdvancedComplexMath.Erf(cmplxToMeta(value))));
        }


        public static Complex erfcx(Complex value)
        {
            return ((erfc(value)) / Complex.Exp(-((value)) * (value)));
        }


        public static Complex faddeeva(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.Faddeeva(cmplxToMeta(z)));
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_log_erfc(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Z(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Q(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hazard(double x);

        #endregion

        #region Bessel functions

        //unfortunetelly only for real arguments :( cant find any implementation for complex args
        //TODO: check

        //  Description(""angielski opis funkcji"")]
        //public static Func<double, double, double> BesselJν2 = (ν, x) => gsl_sf_bessel_Jnu(ν, x);

        // Description(""angielski opis funkcji"")]
        // public static Func<double, double, double> BesselYν2 = (ν, x) => gsl_sf_bessel_Ynu(ν, x);

        public static Func<double, double> BesselJ0 =
               x => AdvancedMath.BesselJ(0, x);

        public static Func<double, double> BesselJ1 =
            x => AdvancedMath.BesselJ(1, x);

        public static Func<double, double> BesselJ2 =
             x => AdvancedMath.BesselJ(2, x);

        public static Func<double, double> BesselJ3 =
                    x => AdvancedMath.BesselJ(3, x);

        public static Func<int, double, double> BesselJnPrime =
              (n, x) => 0.5 * (AdvancedMath.BesselJ(n - 1, x) - AdvancedMath.BesselJ(n + 1, x));


        public static Func<int, double, double> BesselJn =
                (n, x) => AdvancedMath.BesselJ(n, x);

        public static Func<double, double, double> BesselJν =
                  (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.BesselJ(ν, x);

        // Description(""angielski opis funkcji"")]
        //public static Func<double, double, double> BesselJa = BesselJν;

        public static Func<double, double> BesselY0 =
                    x => AdvancedMath.BesselY(0, x);

        public static Func<double, double> BesselY1 =
                 x => AdvancedMath.BesselY(1, x);

        public static Func<double, double> BesselY2 =
                     x => AdvancedMath.BesselY(2, x);

        public static Func<double, double> BesselY3 =
                x => AdvancedMath.BesselY(3, x);

        public static Func<int, double, double> BesselYn =
                    (n, x) => AdvancedMath.BesselY(n, x);

        public static Func<double, double, double> BesselYν =
                   (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.BesselY(ν, x);

        // Description(""angielski opis funkcji"")]
        //public static Func<double, double, double> BesselYa = BesselYν;

        public static Func<int, double, double> ModifiedBesselIn =
                    (n, x) => gsl_sf_bessel_In(n, x);


        public static Func<double, double, double> ModifiedBesselIν =
                 (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.ModifiedBesselI(ν, x);

        // Description(""angielski opis funkcji"")]
        //public static Func<double, double, double> ModifiedBesselIa = ModifiedBesselIν;

        public static Func<double, double, double> ModifiedBesselKν =
                  (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.ModifiedBesselK(ν, x);
        public static Func<int, double, double> ModifiedBesselKn =
                    (n, x) => (x <= 0.0) ? double.NaN : gsl_sf_bessel_Kn(n, x);

        // Description(""angielski opis funkcji"")]
        //public static Func<double, double, double> ModifiedBesselKa = ModifiedBesselKν;


        public static Func<double, double> SphericalBesselJ0 =
              x => AdvancedMath.SphericalBesselJ(0, x);

        public static Func<double, double> SphericalBesselJ1 =
                 x => AdvancedMath.SphericalBesselJ(1, x);

        public static Func<double, double> SphericalBesselJ2 =
               x => AdvancedMath.SphericalBesselJ(2, x);

        public static Func<double, double> SphericalBesselJ3 =
                x => AdvancedMath.SphericalBesselJ(3, x);

        public static Func<int, double, double> SphericalBesselJn =
                (n, x) => AdvancedMath.SphericalBesselJ(n, x);



        public static Func<double, double> SphericalBesselY0 =
            x => AdvancedMath.SphericalBesselY(0, x);


        public static Func<double, double> SphericalBesselY1 =
            x => AdvancedMath.SphericalBesselY(1, x);


        public static Func<double, double> SphericalBesselY2 =
            x => (x == 0) ? double.NaN : AdvancedMath.SphericalBesselY(2, x);


        public static Func<double, double> SphericalBesselY3 =
            x => AdvancedMath.SphericalBesselY(3, x);


        public static Func<int, double, double> SphericalBesselYn =
            (n, x) => AdvancedMath.SphericalBesselY(n, x);


        public static Func<int, double, double> ModifiedSphericalBesselIn =
            (n, x) => gsl_sf_bessel_il_scaled(n, x);


        public static Func<int, double, double> ModifiedSphericalBesselKn =
            (n, x) => (x <= 0) ? double.NaN : gsl_sf_bessel_kl_scaled(n, x);

        public static Func<double, double, double> logBesselKν =
            (ν, x) => (x <= 0.0 || ν < 0) ? double.NaN : gsl_sf_bessel_lnKnu(ν, x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Jnu(double nu, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Ynu(double nu, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_In(int n, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Kn(int n, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_il_scaled(int l, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_kl_scaled(int l, double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_lnKnu(double nu, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_zero_Jnu(double nu, uint s);

        public static double BesselJνZeros(double ν, double s)
        {
            if (s < 0.5 || ν < 0.0) return double.NaN;
            return gsl_sf_bessel_zero_Jnu(ν, (uint)Math.Round(s, MidpointRounding.AwayFromZero));
        }

        public static Complex Hankel1(double α, double x)
        {
            return BesselJν(α, x) + Complex.ImaginaryOne * BesselYν(α, x);
        }

        public static Complex Hankel2(double α, double x)
        {
            return BesselJν(α, x) - Complex.ImaginaryOne * BesselYν(α, x);
        }

        public static Complex SphericalHankel1(int n, double x)
        {
            return SphericalBesselJn(n, x) + Complex.ImaginaryOne * SphericalBesselYn(n, x);
        }

        public static Complex SphericalHankel2(int n, double x)
        {
            return SphericalBesselJn(n, x) - Complex.ImaginaryOne * SphericalBesselYn(n, x);
        }

        public static double RiccatiBesselS(int n, double x)
        {
            return x * SphericalBesselJn(n, x);
        }

        public static double RiccatiBesselC(int n, double x)
        {
            return -x * SphericalBesselYn(n, x);
        }

        public static double RiccatiBesselψ(int n, double x)
        {
            return x * SphericalBesselJn(n, x);
        }

        public static double RiccatiBesselχ(int n, double x)
        {
            return -x * SphericalBesselYn(n, x);
        }

        public static Complex RiccatiBesselξ(int n, double x)
        {
            return x * Hankel1(n, x);
        }

        public static Complex RiccatiBesselζ(int n, double x)
        {
            return x * Hankel2(n, x);
        }

        #endregion

        #region Airy functions


        public static Func<double, double> AiryAi = AdvancedMath.AiryAi;


        public static Func<double, double> AiryBi = AdvancedMath.AiryBi;


        public static Func<double, double> Ai = AiryAi;


        public static Func<double, double> Bi = AiryBi;

        public static Func<double, double> AiPrime = x => gsl_sf_airy_Ai_deriv(x, 0);
        public static Func<double, double> BiPrime = x => gsl_sf_airy_Bi_deriv(x, 0);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Ai_deriv(double x, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Bi_deriv(double x, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai(uint s);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi(uint s);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai_deriv(uint s);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi_deriv(uint s);

        //AiryZetaAi //ZerosOfAi// ZerosOfBi
        public static double AiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai((uint)Math.Round(x, MidpointRounding.AwayFromZero));
        }

        public static double BiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi((uint)Math.Round(x, MidpointRounding.AwayFromZero));
        }

        public static double AiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai_deriv((uint)Math.Round(x, MidpointRounding.AwayFromZero));
        }

        public static double BiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi_deriv((uint)Math.Round(x, MidpointRounding.AwayFromZero));
        }

        #endregion

        #region zeta and eta functions



        public static Func<double, double> DirichletEta =
            x => x >= 0 ? AdvancedMath.DirichletEta(x) : (1 - Math.Pow(2, 1 - x)) * RiemannZeta(x);


        public static Func<double, double> η = DirichletEta;

        public static double RiemannZeta(double x)
        {
            return AdvancedMath.RiemannZeta(x);
        }

        public static double Riemannζ(double x)
        {
            return AdvancedMath.RiemannZeta(x);
        }

        public static Complex Riemannζ(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        public static Complex RiemannZeta(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hzeta(double s, double q);

        public static double HurwitzZeta(double x, double q)
        {
            if (x <= 1.0 || q <= 0) return double.NaN;
            return gsl_sf_hzeta(x, q);
        }

        public static double ζ(double x, double q)
        {
            if (x <= 1.0) return double.NaN;
            return gsl_sf_hzeta(x, q);
        }

        #endregion

        #region mathieu functions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_a([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_b([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_ce([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_se([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_Mc([MarshalAs(UnmanagedType.I4)] int kind,
            [MarshalAs(UnmanagedType.I4)] int order, [MarshalAs(UnmanagedType.R8)] double qq,
            [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_Ms([MarshalAs(UnmanagedType.I4)] int kind,
            [MarshalAs(UnmanagedType.I4)] int order, [MarshalAs(UnmanagedType.R8)] double qq,
            [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);

        public static double MathieuSE(int n, double q, double x)
        {
            int error = gsl_sf_mathieu_se(n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuCE(int n, double q, double x)
        {
            int error = gsl_sf_mathieu_ce(n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuAn(int n, double q)
        {
            int error = gsl_sf_mathieu_a(n, q, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuBn(int n, double q)
        {
            int error = gsl_sf_mathieu_b(n, q, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuMc(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            int error = gsl_sf_mathieu_Mc(j, n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuMs(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            int error = gsl_sf_mathieu_Ms(j, n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        [StructLayout(LayoutKind.Sequential, Size = 16), Serializable]
        private struct gsl_sf_result
        {
            [MarshalAs(UnmanagedType.R8)]
            public readonly double val;
            [MarshalAs(UnmanagedType.R8)]
            public readonly double err;
        }

        #endregion

        #region logistic functions

        public static Func<double, double> Logistic = MathNet.Numerics.SpecialFunctions.Logistic;

        public static Func<double, double> Logit =
            x => (x < 0 || x > 1) ? double.NaN : MathNet.Numerics.SpecialFunctions.Logit(x);

        public static Func<double, double> StruveL1 = MathNet.Numerics.SpecialFunctions.StruveL1;
        public static Func<double, double> StruveL0 = MathNet.Numerics.SpecialFunctions.StruveL0;

        #endregion

        #region utils

        private const string gslSfLibDir = ""gsl.dll"";//""libgsl-0.dll"";
        private static gsl_sf_result sfResult;

        private static Exception gslExceptions(int error_code)
        {
            switch (error_code)
            {
                case -1:
                    return new Exception(""general failure"");
                case -2:
                    return new Exception(""iteration has not converged"");
                case 1:
                    return new Exception(""input domain error: e.g sqrt(-1)"");
                case 2:
                    return new Exception(""output range error: e.g. exp(1e100)"");
                case 3:
                    return new Exception(""invalid pointer"");
                case 4:
                    return new Exception(""invalid argument supplied by user"");
                case 5:
                    return new Exception(""generic failure"");
                case 6:
                    return new Exception(""factorization failed"");
                case 7:
                    return new Exception(""sanity check failed - shouldn't happen"");
                case 8:
                    return new Exception(""malloc failed"");
                case 9:
                    return new Exception(""problem with user-supplied function"");
                case 10:
                    return new Exception(""iterative process is out of control"");
                case 11:
                    return new Exception(""exceeded max number of iterations"");
                case 12:
                    return new Exception(""tried to divide by zero"");
                case 13:
                    return new Exception(""user specified an invalid tolerance"");
                case 14:
                    return new Exception(""failed to reach the specified tolerance"");
                case 15:
                    return new Exception(""underflow"");
                case 16:
                    return new Exception(""overflow "");
                case 17:
                    return new Exception(""loss of accuracy"");
                case 18:
                    return new Exception(""failed because of roundoff error"");
                case 19:
                    return new Exception(""matrix: vector lengths are not conformant"");
                case 20:
                    return new Exception(""matrix not square"");
                case 21:
                    return new Exception(""apparent singularity detected"");
                case 22:
                    return new Exception(""integral or series is divergent"");
                case 23:
                    return new Exception(""requested feature is not supported by the hardware"");
                case 24:
                    return new Exception(""requested feature not (yet) implemented"");
                case 25:
                    return new Exception(""cache limit exceeded"");
                case 26:
                    return new Exception(""table limit exceeded"");
                case 27:
                    return new Exception(""iteration is not making progress towards solution"");
                case 28:
                    return new Exception(""jacobian evaluations are not improving the solution"");
                case 29:
                    return new Exception(""cannot reach the specified tolerance in F"");
                case 30:
                    return new Exception(""cannot reach the specified tolerance in X"");
                case 31:
                    return new Exception(""cannot reach the specified tolerance in gradient"");
                case 32:
                    return new Exception(""end of file"");
                default:
                    return new Exception(""unknown exception"");
            }
        }

        private static Complex cmplxFromMeta(Meta.Numerics.Complex c)
        {
            return new Complex(c.Re, c.Im);
        }

        private static Meta.Numerics.Complex cmplxToMeta(Complex c)
        {
            return new Meta.Numerics.Complex(c.Real, c.Imaginary);
        }

        #endregion


";

        public static double findRoot(Func<double,double> f, double a, double b)
        {
            double ret = double.NaN;

            try
            {
                ret = MathNet.Numerics.FindRoots.OfFunction(f, a, b,1e-2,10000);
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        #region signal processing

        public static double Gabor(double x, double mean, double amplitude, double position, double width, double phase,
            double frequency)
        {
            return Accord.Math.Gabor.Function1D(x, mean, amplitude, position, width, phase, frequency);
        }

        public static Complex Gabor(Complex z, double λ, double θ, double ψ, double σ, double γ)
        {
            var z2 = Accord.Math.Gabor.Function2D((int) z.Real, (int) z.Imaginary, λ, θ, ψ, σ, γ);

            return new Complex(z2.Re, z2.Im);
        }

        #endregion

        #region test functions

        public static double Ackley(params double[] xi)
        {
            return TestFunctions.Ackley(xi);
        }

        public static double Rastrigin(params double[] xi)
        {
            return TestFunctions.Rastrigin(xi);
        }

        public static double Bohachevsky1(double x, double y)
        {
            return TestFunctions.Bohachevsky1(x, y);
        }

        public static double dropWave(double x, double y)
        {
            return TestFunctions.DropWave(x, y);
        }

        public static double Himmelblau(double x, double y)
        {
            return TestFunctions.Himmelblau(x, y);
        }

        public static double Matyas(double x, double y)
        {
            return TestFunctions.Matyas(x, y);
        }

        public static double sixHumpCamel(double x, double y)
        {
            return TestFunctions.SixHumpCamel(x, y);
        }

        public static double Rosenbrock(double x, double y)
        {
            return TestFunctions.Rosenbrock(x, y);
        }

        public static double Rosenbrock(params double[] xi)
        {
            return TestFunctions.Rosenbrock(xi);
        }

        #endregion

        #region Gamma and related functions

        public static Func<double, double, double> leftRegularizedGamma =
            (a, x) => (a <= 0 || x < 0) ? double.NaN : AdvancedMath.LeftRegularizedGamma(a, x);


        public static Func<double, double, double> rightRegularizedGamma =
            (a, x) => (a <= 0 || x < 0) ? double.NaN : AdvancedMath.RightRegularizedGamma(a, x);

        public static Func<int, double, double> ψn = AdvancedMath.Psi;

        public static Func<int, double, double> polyGamma = AdvancedMath.Psi;

        public static Func<int, double, double> ψⁿ = AdvancedMath.Psi;


        public static double gamma(double value)
        {
            return AdvancedMath.Gamma((value));
        }


        public static double Γ(double value)
        {
            return AdvancedMath.Gamma((value));
        }


        public static double logGamma(double value)
        {
            if ((value) <= 0.0)
                return double.NaN;
            return AdvancedMath.LogGamma((value));
        }


        public static double logΓ(double value)
        {
            if ((value) <= 0.0)
                return double.NaN;
            return AdvancedMath.LogGamma((value));
        }


        public static double psi(double value)
        {
            return AdvancedMath.Psi((value));
        }


        public static double digamma(double value)
        {
            return AdvancedMath.Psi((value));
        }


        public static double ψ(double value)
        {
            return AdvancedMath.Psi((value));
        }

        //COMPLEX:

        public static Complex gamma(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Gamma(cmplxToMeta(value)));
        }


        public static Complex Γ(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Gamma(cmplxToMeta(value)));
        }


        public static Complex logGamma(Complex value)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(value.Real, value.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val*Complex.ImaginaryOne);
        }


        public static Complex logΓ(Complex value)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(value.Real, value.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val*Complex.ImaginaryOne);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_lngamma_complex_e(double zr, double zi, out gsl_sf_result lnr,
            out gsl_sf_result arg);


        public static Complex psi(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Psi(cmplxToMeta(value)));
        }


        public static Complex digamma(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Psi(cmplxToMeta(value)));
        }


        public static Complex ψ(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Psi(cmplxToMeta(value)));
        }

        //non complex type compatible gamma-like functions:


        public static double gamma(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return AdvancedMath.Gamma(a, x);
        }

        public static double Γ(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return AdvancedMath.Gamma(a, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_Q(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_P(double a, double x);

        // public static double gammaQ(double a, double x) { if (x < 0 || a <= 0) return double.NaN; return gsl_sf_gamma_inc_Q(a, x); }

        public static double gammaQ(double a, double x)
        {
            return MathNet.Numerics.SpecialFunctions.GammaUpperRegularized(a, x);
        }


        public static double gammaP(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return gsl_sf_gamma_inc_P(a, x);
        }


        public static double Beta(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(x, a, b);
        }


        public static double BetaNorm(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_beta_inc(x, a, b);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_beta_inc(double a, double b, double x);


        public static double Beta(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(a, b);
        }


        public static double β(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(x, a, b);
        }


        public static double β(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return AdvancedMath.Beta(a, b);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_lnbeta(double a, double b);


        public static double logβ(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_lnbeta(a, b);
        }

        public static double logBeta(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_lnbeta(a, b);
        }

        #endregion

        #region coefficients and special values

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_poch(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_pochrel(double a, double x);

        public static double Pochhammer(double a, double x)
        {
            if (((int) (x) == x && x <= 0) || ((int) (a + x) == a + x && a + x <= 0)) return double.NaN;
            return gsl_sf_poch(a, x);
        }

        public static double PochhammerRelative(double a, double x)
        {
            if (((int) (x) == x && x <= 0) || ((int) (a + x) == a + x && a + x <= 0)) return double.NaN;
            return gsl_sf_pochrel(a, x);
        }

        #endregion

        #region logarithm derrived functions

        public static double PolyLog(int n, double x)
        {
            return AdvancedMath.PolyLog(n, x);
        }


        public static double diLogarithm(double value)
        {
            if (value > 1.0)
                return (double.NaN);
            return AdvancedMath.DiLog((value));
        }

        public static double diLog(double value)
        {
            if (value > 1.0)
                return (double.NaN);
            return AdvancedMath.DiLog((value));
        }


        public static double SpencesIntegral(double value)
        {
            if (value < 0.0)
                return (double.NaN);
            return AdvancedMath.DiLog(1 - (value));
        }


        public static Complex diLogarithm(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.DiLog(cmplxToMeta(value)));
        }

        public static Complex diLog(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.DiLog(cmplxToMeta(value)));
        }


        public static Complex SpencesIntegral(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.DiLog(1 - cmplxToMeta(value)));
        }

        #endregion

        #region Wave functions

        public static double CoulombG(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return AdvancedMath.CoulombG(L, η, ρ);
        }


        public static double CoulombF(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return AdvancedMath.CoulombF(L, η, ρ);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_CL_e(double L, double eta, out gsl_sf_result result);


        public static double CoulombC(int L, double η)
        {
            if (L < 0) return double.NaN;
            gsl_sf_coulomb_CL_e(L, η, out sfResult);
            return sfResult.val;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hydrogenicR(int n, int l, double Z, double r);

        public static double HydrogenicR(int n, int l, double Z, double r)
        {
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }

        public static double Rnl(int n, int l, double Z, double r)
        {
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }


        public static double CoulombW(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return CoulombC(1, η)*CoulombF(L, η, ρ) + CoulombC(2, η)*CoulombG(L, η, ρ);
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_wave_FG_e(double eta, double x,
            double lam_F,
            int k_lam_G,
            out gsl_sf_result F, out gsl_sf_result Fp,
            out gsl_sf_result G, out gsl_sf_result Gp,
            out double exp_F, out double exp_G);


        public static double CoulombGprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out r2, out r3, out sfResult, out d1, out d2);
            return sfResult.val;
        }


        public static double CoulombFprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out sfResult, out r3, out r2, out d1, out d2);
            return sfResult.val;
        }

        #endregion

        #region Fermi–Dirac complete&incomplete integral

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_int(int j, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_inc_0(double x, double b);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_0(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_mhalf(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_half(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_3half(double x);

        public static double FermiDiracFmhalf(double x)
        {
            return gsl_sf_fermi_dirac_mhalf(x);
        }

        public static double FermiDiracFhalf(double x)
        {
            return gsl_sf_fermi_dirac_half(x);
        }

        public static double FermiDiracF3half(double x)
        {
            return gsl_sf_fermi_dirac_3half(x);
        }

        public static double FermiDiracF0(double x)
        {
            return gsl_sf_fermi_dirac_0(x);
        }

        public static double FermiDiracF0(double x, double b)
        {
            return gsl_sf_fermi_dirac_inc_0(x, b);
        }

        public static double FermiDiracFj(int j, double x)
        {
            return gsl_sf_fermi_dirac_int(j, x);
        }

        #endregion

        #region lambert W functions

        public static Func<double, double> LambertW0 =
            x => (x <= -1/Math.E) ? double.NaN : gsl_sf_lambert_W0(x);

        public static Func<double, double> LambertWm1 =
            x => (x <= -1/Math.E) ? double.NaN : gsl_sf_lambert_Wm1(x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_W0(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_Wm1(double x);

        #endregion

        #region polynomials

        public static Func<double, double, double>
            Gegenbauer1 = gsl_sf_gegenpoly_1;

        public static Func<double, double, double>
            Gegenbauer2 = gsl_sf_gegenpoly_1;

        public static Func<double, double, double>
            Gegenbauer3 = gsl_sf_gegenpoly_1;

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_n(int n, double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_1(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_2(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_3(double lambda, double x);


        public static double Gegenbauer(int n, double α, double x)
        {
            if (α <= -0.5 || n < 0) return double.NaN;
            return gsl_sf_gegenpoly_n(n, α, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_n(int n, double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_1(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_2(double a, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_3(double a, double x);


        public static double Laguerre(int n, double α, double x)
        {
            if (n < 0 || α <= -1.0) return double.NaN;
            return gsl_sf_laguerre_n(n, α, x);
        }


        public static double Laguerre(int n, double x)
        {
            if (n < 0) return double.NaN;
            return gsl_sf_laguerre_n(n, 0, x);
        }


        public static double LegendreP(int l, double x)
        {
            if (x > 1.0 || x < -1.0) return double.NaN;
            return OrthogonalPolynomials.LegendreP(l, x);
        }

        public static double LegendreP(int l, int m, double x)
        {
            if (x > 1.0 || x < -1.0) return double.NaN;
            return OrthogonalPolynomials.LegendreP(l, m, x);
        }

        //add legendre Q


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_Ql(int l, double x);

        // Q_l(x), x > -1, x != 1, l >= 0
        public static double LegendreQ(int l, double x)
        {
            if (x == 1.0 || x <= -1.0 || l < 0) return double.NaN;
            return gsl_sf_legendre_Ql(l, x);
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_sphPlm(int l, int m, double x);

        public static double SphericalLegendreP(int l, int m, double x)
        {
            if (x > 1.0 || x < -1.0) return double.NaN;
            return gsl_sf_legendre_sphPlm(l, m, x);
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_half(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_mhalf(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_0(double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_1(double lambda, double x);


        public static double ConicalP(double μ, double λ, double x)
        {
            if (x <= -1.0) return double.NaN;
            if (μ == (int) μ)
                return ConicalP((int) μ, λ, x);
            if (μ == -0.5)
                return gsl_sf_conicalP_mhalf(λ, x);
            if (μ == 0.5)
                return gsl_sf_conicalP_half(λ, x);
            return double.NaN;
        }


        private static double ConicalP(int μ, double λ, double x)
        {
            if (x <= -1.0) return double.NaN;

            switch (μ)
            {
                case 0:
                    return gsl_sf_conicalP_0(λ, x);
                case 1:
                    return gsl_sf_conicalP_1(λ, x); //fixed...

                default:
                    return double.NaN;
            }
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_sph_reg(int l, double lambda, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_cyl_reg(int m, double lambda, double x);

        public static double SphericalConicalP(int l, double λ, double x)
        {
            if (x <= -1.0 || l < -1 || x == 0.0) return double.NaN;
            return gsl_sf_conicalP_sph_reg(l, λ, x);
        }

        public static double CylindricalConicalP(int m, double λ, double x)
        {
            if (x <= -1.0 || m < -1 || x == 0.0) return double.NaN;
            return gsl_sf_conicalP_cyl_reg(m, λ, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_H3d(int l, double lambda, double eta);

        public static double LegendreH3D(int l, double λ, double η)
        {
            if (η < 0 || l < 0 || (l >= 2 && λ == 0.0)) return double.NaN;
            return gsl_sf_legendre_H3d(l, λ, η);
        }

        public static double ChebyshevT(int n, double x)
        {
            return OrthogonalPolynomials.ChebyshevT(n, x);
        }

        //add ChebyshevU

        public static double HermiteH(int n, double x)
        {
            return OrthogonalPolynomials.HermiteH(n, x);
        }

        public static double HermiteHe(int n, double x)
        {
            return OrthogonalPolynomials.HermiteHe(n, x);
        }

        public static double ZernikeR(int n, int m, double ρ)
        {
            if (n < m || ρ < 0.0 || ρ > 1.0) return double.NaN;
            return OrthogonalPolynomials.ZernikeR(n, m, ρ);
        }

        //TODO: add Zernike Z 

        #endregion

        #region transport functions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_2(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_3(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_4(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_5(double x);

        public static double TransportJ(int n, double x)
        {
            if (x < 0.0) return double.NaN;
            switch (n)
            {
                case 2:
                    return gsl_sf_transport_2(x);
                case 3:
                    return gsl_sf_transport_3(x);
                case 4:
                    return gsl_sf_transport_4(x);
                case 5:
                    return gsl_sf_transport_5(x);
            }
            return double.NaN;
        }

        #endregion

        #region synchrotron functions

        public static Func<double, double> SynchrotronF = x => (x < 0) ? double.NaN : gsl_sf_synchrotron_1(x);
        public static Func<double, double> SynchrotronG = x => (x < 0) ? double.NaN : gsl_sf_synchrotron_2(x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_1(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_2(double x);

        #endregion

        #region coupling 3,6,9-j symbols

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_3j(int two_ja, int two_jb, int two_jc,
            int two_ma, int two_mb, int two_mc);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_6j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_RacahW(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_9j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf,
            int two_jg, int two_jh, int two_ji);


        public static double Coupling3j(int ja, int jb, int jc, int ma, int mb, int mc)
        {
            return gsl_sf_coupling_3j(ja, jb, jc, ma, mb, mc);
        }

        public static double Coupling6j(int ja, int jb, int jc, int jd, int je, int jf)
        {
            return gsl_sf_coupling_6j(ja, jb, jc, jd, je, jf);
        }

        public static double CouplingRacahW(int ja, int jb, int jc, int jd, int je, int jf)
        {
            return gsl_sf_coupling_RacahW(ja, jb, jc, jd, je, jf);
        }

        public static double Coupling9j(int ja, int jb, int jc, int jd, int je, int jf, int jg, int jh, int ji)
        {
            return gsl_sf_coupling_9j(ja, jb, jc, jd, je, jf, jg, jh, jf);
        }

        public static double Coupling3j(double j1, double j2, double j3, double m1, double m2, double m3)
        {
            return SpinMath.ThreeJ(new SpinState(j1, m1), new SpinState(j2, m2), new SpinState(j3, m3));
        }

        public static double Coupling6j(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            return SpinMath.SixJ(new Spin(j1), new Spin(j2), new Spin(j3), new Spin(j4), new Spin(j5), new Spin(j6));
        }

        public static double ClebschGordan(double j1, double j2, double j, double m1, double m2, double m)
        {
            return SpinMath.ClebschGodron(new SpinState(j1, m1), new SpinState(j2, m2), new SpinState(j, m));
        }

        #endregion

        #region Hypergeometric functions

        public static Complex SphericalHarmonic(int l, int m, double θ, double φ)
        {
            return cmplxFromMeta(AdvancedMath.SphericalHarmonic(l, m, θ, φ));
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_0F1(double c, double x);

        //Hypergeometric function related to Bessel functions 0F1[c,x]

        public static double Hypergeometric0F1(double c, double x)
        {
            if (c > 0.0 || (c != (int) (c))) return gsl_sf_hyperg_0F1(c, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1_int(int m, int n, double x);

        //Confluent hypergeometric function  for integer parameters. 1F1[m,n,x] = M(m,n,x)

        public static double Hypergeometric1F1(int m, int n, double x)
        {
            return gsl_sf_hyperg_1F1_int(m, n, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1(double a, double b, double x);

        //Confluent hypergeometric function. 1F1[a,b,x] = M(a,b,x)

        public static double Hypergeometric1F1(double a, double b, double x)
        {
            return gsl_sf_hyperg_1F1(a, b, x);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U_int(int m, int n, double x);

        //Confluent hypergeometric function for integer parameters. U(m,n,x)

        public static double HypergeometricU(int m, int n, double x)
        {
            if (x > 0.0 || (x != (int) (x))) return gsl_sf_hyperg_U_int(m, n, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U(double a, double b, double x);

        //Confluent hypergeometric function. U(a,b,x)
        public static double HypergeometricU(double a, double b, double x)
        {
            if (x > 0.0 || (x != (int) (x))) return gsl_sf_hyperg_U(a, b, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1(double a, double b, double c, double x);

        //Gauss hypergeometric function 2F1[a,b,c,x]

        public static double Hypergeometric2F1(double a, double b, double c, double x)
        {
            if ((c > 0.0 || (c != (int) (c))) && Math.Abs(x) < 1) return gsl_sf_hyperg_2F1(a, b, c, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj(double aR, double aI, double c, double x);

        //Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x]

        public static double Hypergeometric2F1(Complex a, double c, double x)
        {
            if ((c > 0.0 || (c != (int) (c))) && Math.Abs(x) < 1)
                return gsl_sf_hyperg_2F1_conj(a.Real, a.Imaginary, c, x);
            return double.NaN;
        } //TODO: better name for a parameter

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_renorm(double a, double b, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[a,b,c,x] / Gamma[c]

        public static double Hypergeometric2F1renorm(double a, double b, double c, double x)
        {
            if ((c > 0.0 || (c != (int) (c))) && Math.Abs(x) < 1) return gsl_sf_hyperg_2F1_renorm(a, b, c, x);
            return double.NaN;
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj_renorm(double aR, double aI, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x] / Gamma[c]

        public static double Hypergeometric2F1renorm(Complex a, double c, double x)
        {
            if ((c > 0.0 || (c != (int) (c))) && Math.Abs(x) < 1)
                return gsl_sf_hyperg_2F1_conj_renorm(a.Real, a.Imaginary, c, x);
            return double.NaN;
        } //TODO: better name for a parameter
        //2F1 sometimes returns an exceptions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F0(double a, double b, double x);

        /* Mysterious hypergeometric function. The series representation
         * is a divergent hypergeometric series. However, for x < 0 we
         * have 2F0(a,b,x) = (-1/x)^a U(a,1+a-b,-1/x)*/


        public static double Hypergeometric2F0(double a, double b, double x)
        {
            if (x <= 0) return gsl_sf_hyperg_2F0(a, b, x);
            return double.NaN;
        }

        #endregion

        #region integral functions

        public static double Ti(double x)
        {
            return AdvancedMath.IntegralTi(x);
        }


        public static Func<double, double>
            Dawson = AdvancedMath.Dawson;

        public static Func<double, double> Clausen = gsl_sf_clausen;

        public static Func<double, double> Si =
            AdvancedMath.IntegralSi; //sine integral

        public static Func<double, double> Ci =
            x => (x < 0.0) ? double.NaN : AdvancedMath.IntegralCi(x); //cosine integral

        //Generalized Exponential Integral
        public static Func<int, double, double> En =
            (n, x) => (x == 0.0 || n < 0 || n > 2) ? AdvancedMath.IntegralE(n, x) : gsl_sf_expint_En(n, x);


        //   public static double EnNEW(int n, double x)//Generalized Exponential Integral
        //  {
        //     return MathNet.Numerics.SpecialFunctions.ExponentialIntegral(x, n);
        //     return 1.0;
        //}

        public static Func<double, double> Ei = x => (x == 0.0) ? AdvancedMath.IntegralEi(x) : gsl_sf_expint_Ei(x);

        public static Func<double, double> Shi =
            gsl_sf_Shi; //hyperbolic sine integral


        public static Func<double, double> Chi =
            x => (x == 0.0) ? double.NaN : gsl_sf_Chi(x); //hyperbolic cosine integral

        public static Func<double, double> Tai =
            gsl_sf_atanint; //arcus tangent integral

        //Integrals in optics
        public static Func<double, Complex> Fresnel =
            x => cmplxFromMeta(AdvancedMath.Fresnel(x));

        public static Func<double, double> FresnelS =
            AdvancedMath.FresnelS;

        public static Func<double, double> FresnelC =
            AdvancedMath.FresnelC;

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_1(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_2(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_3(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_4(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_5(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_6(double x);

        /*
       
        */


        public static double Debye(int n, double x)
        {
            if (n == 0)
                return 0;
            if (x < 0.0)
                return double.NaN;
            switch (n)
            {
                case 1:
                    return gsl_sf_debye_1(x);
                case 2:
                    return gsl_sf_debye_2(x);
                case 3:
                    return gsl_sf_debye_3(x);
                case 4:
                    return gsl_sf_debye_4(x);
                case 5:
                    return gsl_sf_debye_5(x);
                case 6:
                    return gsl_sf_debye_6(x);
            }
            if (x > 9) // x >> 1
                return Γ(n + 1.0)*Riemannζ(n + 1.0);
            return double.NaN;
        }

        //dawnson integral

        /* Calculate the Clausen integral:
         *   Cl_2(x) := Integrate[-Log[2 Sin[t/2]], {t,0,x}]
         *
         * Relation to dilogarithm:
         *   Cl_2(theta) = Im[ Li_2(e^(i theta)) ]
         */

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_clausen(double x);


        public static Complex Ein(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.Ein(cmplxToMeta(z)));
        }


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_En(int n, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_Ei(double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_Shi(double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_Chi(double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_atanint(double x);

        #endregion

        #region Elliptic integrals

        //In mathematics, the Carlson symmetric forms of elliptic integrals are a small canonical set of elliptic integrals to which all others may be reduced. They are a modern alternative to the Legendre forms. The Legendre forms may be expressed in terms of the Carlson forms and vice versa.
        public static Func<double, double> EllipticK =
            x =>
                (x < 0 || x >= 1.0)
                    ? ((x < 0 && x > -1.0) ? gsl_sf_ellint_Kcomp(x, 0) : double.NaN)
                    : AdvancedMath.EllipticK(x);


        public static double CarlsonD(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return AdvancedMath.CarlsonD(x, y, z);
        }


        public static double CarlsonF(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return AdvancedMath.CarlsonF(x, y, z);
        }

        public static double CarlsonC(double x, double y)
        {
            if (x <= 0 || y <= 0)
                return double.NaN;
            return gsl_sf_ellint_RC(x, y, 0);
        }

        public static double CarlsonJ(double x, double y, double z, double p)
        {
            if (x <= 0 || y <= 0 || z <= 0 || p <= 0)
                return double.NaN;
            return gsl_sf_ellint_RJ(x, y, z, p, 0);
        }


        public static double EllipticF(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI/2.0 || φ > Math.PI/2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_F(φ, x, 0);
            return AdvancedMath.EllipticF(φ, x);
        }


        public static double EllipticE(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI/2.0 || φ > Math.PI/2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_E(φ, x, 0);
            return AdvancedMath.EllipticE(φ, x);
        }


        public static double EllipticΠ(double φ, double x, int n)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI/2.0 || φ > Math.PI/2.0)
                return double.NaN;
            return gsl_sf_ellint_P(φ, x, n, 0);
        }

        public static double EllipticD(double φ, double x, int n)
        {
            if (x < -1.0 || x > 1.0 || φ < -Math.PI/2.0 || φ > Math.PI/2.0)
                return double.NaN;
            return gsl_sf_ellint_D(φ, x, n, 0);
        }

        public static double EllipticE(double x)
        {
            if (x < 0 || x > 1.0)
                if (x < 0 && x > -1.0)
                    return gsl_sf_ellint_Ecomp(x, 0);
                else
                    return double.NaN;
            return AdvancedMath.EllipticE(x);
        }

        public static double EllipticD(double x)
        {
            if (x <= -1 || x >= 1.0)
                return double.NaN;

            return gsl_sf_ellint_Dcomp(x, 0);
        }

        public static double EllipticΠ(double x, int n)
        {
            if (x <= -1 || x >= 1.0)
                return double.NaN;
            return gsl_sf_ellint_Pcomp(x, n, 0);
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Ecomp(double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Kcomp(double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Pcomp(double k, double n, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Dcomp(double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_F(double phi, double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_E(double phi, double k, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_P(double phi, double k, double n, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_D(double phi, double k, double n, uint mode);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RC(double x, double y, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RD(double x, double y, double z, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RF(double x, double y, double z, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RJ(double x, double y, double z, double p, uint mode);

        #endregion

        #region  Jacobian elliptic functions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gsl_sf_elljac_e(double u, double m, out double sn, out double cn, out double dn);

        public static double JacobiEllipticSn(double u, double m)
        {
            if (m < -1.0 || m > 1.0) return double.NaN;
            double Sn = 0, Cn = 0, Dn = 0;
            gsl_sf_elljac_e(u, m, out Sn, out Cn, out Dn);
            return Sn;
        }

        public static double JacobiEllipticCn(double u, double m)
        {
            if (m < -1.0 || m > 1.0) return double.NaN;
            double Sn = 0, Cn = 0, Dn = 0;
            gsl_sf_elljac_e(u, m, out Sn, out Cn, out Dn);
            return Cn;
        }

        public static double JacobiEllipticDn(double u, double m)
        {
            if (m < -1.0 || m > 1.0) return double.NaN;
            double Sn = 0, Cn = 0, Dn = 0;
            gsl_sf_elljac_e(u, m, out Sn, out Cn, out Dn);
            return Dn;
        }

        #endregion

        #region error functions

        public static Func<Complex, Complex> erfi =
            z => -Complex.ImaginaryOne*erf(Complex.ImaginaryOne*z);

        public static Func<double, double> inverseErf = AdvancedMath.InverseErf;

        public static Func<double, double> inverseErfc =
            AdvancedMath.InverseErfc;

        public static Func<double, double> logErfc = gsl_sf_log_erfc;

        public static Func<double, double> erfZ = gsl_sf_erf_Z;
        public static Func<double, double> erfQ = gsl_sf_erf_Q;

        public static Func<double, double> hazard = gsl_sf_hazard;

        public static double OwenT(double h, double a)
        {
            return OwensT.Function(h, a);
        }

        public static double OwenT(double h, double a, double ah)
        {
            return OwensT.Function(h, a, ah);
        }

        //checked and they work ok

        public static double erf(double value)
        {
            return AdvancedMath.Erf((value));
        }


        public static double erfc(double value)
        {
            return AdvancedMath.Erfc((value));
        }


        public static double erfcx(double value)
        {
            return ((erfc(value))/Math.Exp(-((value))*(value)));
        }


        public static Complex erf(Complex value)
        {
            return cmplxFromMeta(AdvancedComplexMath.Erf(cmplxToMeta(value)));
        }


        public static Complex erfc(Complex value)
        {
            return (1 - cmplxFromMeta(AdvancedComplexMath.Erf(cmplxToMeta(value))));
        }


        public static Complex erfcx(Complex value)
        {
            return ((erfc(value))/Complex.Exp(-((value))*(value)));
        }


        public static Complex faddeeva(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.Faddeeva(cmplxToMeta(z)));
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_log_erfc(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Z(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Q(double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hazard(double x);

        #endregion

        #region Bessel functions

        //unfortunetelly only for real arguments :( cant find any implementation for complex args
        //TODO: check

        //  Description("angielski opis funkcji")]
        //public static Func<double, double, double> BesselJν2 = (ν, x) => gsl_sf_bessel_Jnu(ν, x);

        // Description("angielski opis funkcji")]
        // public static Func<double, double, double> BesselYν2 = (ν, x) => gsl_sf_bessel_Ynu(ν, x);

        public static Func<double, double> BesselJ0 =
            x => AdvancedMath.BesselJ(0, x);

        public static Func<double, double> BesselJ1 =
            x => AdvancedMath.BesselJ(1, x);

        public static Func<double, double> BesselJ2 =
            x => AdvancedMath.BesselJ(2, x);

        public static Func<double, double> BesselJ3 =
            x => AdvancedMath.BesselJ(3, x);

        public static Func<int, double, double> BesselJnPrime =
            (n, x) => 0.5*(AdvancedMath.BesselJ(n - 1, x) - AdvancedMath.BesselJ(n + 1, x));


        public static Func<int, double, double> BesselJn =
            (n, x) => AdvancedMath.BesselJ(n, x);

        public static Func<double, double, double> BesselJν =
            (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.BesselJ(ν, x);

        // Description("angielski opis funkcji")]
        //public static Func<double, double, double> BesselJa = BesselJν;

        public static Func<double, double> BesselY0 =
            x => AdvancedMath.BesselY(0, x);

        public static Func<double, double> BesselY1 =
            x => AdvancedMath.BesselY(1, x);

        public static Func<double, double> BesselY2 =
            x => AdvancedMath.BesselY(2, x);

        public static Func<double, double> BesselY3 =
            x => AdvancedMath.BesselY(3, x);

        public static Func<int, double, double> BesselYn =
            (n, x) => AdvancedMath.BesselY(n, x);

        public static Func<double, double, double> BesselYν =
            (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.BesselY(ν, x);

        // Description("angielski opis funkcji")]
        //public static Func<double, double, double> BesselYa = BesselYν;

        public static Func<int, double, double> ModifiedBesselIn =
            (n, x) => gsl_sf_bessel_In(n, x);


        public static Func<double, double, double> ModifiedBesselIν =
            (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.ModifiedBesselI(ν, x);

        // Description("angielski opis funkcji")]
        //public static Func<double, double, double> ModifiedBesselIa = ModifiedBesselIν;

        public static Func<double, double, double> ModifiedBesselKν =
            (ν, x) => (x < 0.0) ? double.NaN : AdvancedMath.ModifiedBesselK(ν, x);

        public static Func<int, double, double> ModifiedBesselKn =
            (n, x) => (x <= 0.0) ? double.NaN : gsl_sf_bessel_Kn(n, x);

        // Description("angielski opis funkcji")]
        //public static Func<double, double, double> ModifiedBesselKa = ModifiedBesselKν;


        public static Func<double, double> SphericalBesselJ0 =
            x => AdvancedMath.SphericalBesselJ(0, x);

        public static Func<double, double> SphericalBesselJ1 =
            x => AdvancedMath.SphericalBesselJ(1, x);

        public static Func<double, double> SphericalBesselJ2 =
            x => AdvancedMath.SphericalBesselJ(2, x);

        public static Func<double, double> SphericalBesselJ3 =
            x => AdvancedMath.SphericalBesselJ(3, x);

        public static Func<int, double, double> SphericalBesselJn =
            (n, x) => AdvancedMath.SphericalBesselJ(n, x);


        public static Func<double, double> SphericalBesselY0 =
            x => AdvancedMath.SphericalBesselY(0, x);


        public static Func<double, double> SphericalBesselY1 =
            x => AdvancedMath.SphericalBesselY(1, x);


        public static Func<double, double> SphericalBesselY2 =
            x => (x == 0) ? double.NaN : AdvancedMath.SphericalBesselY(2, x);


        public static Func<double, double> SphericalBesselY3 =
            x => AdvancedMath.SphericalBesselY(3, x);


        public static Func<int, double, double> SphericalBesselYn =
            (n, x) => AdvancedMath.SphericalBesselY(n, x);


        public static Func<int, double, double> ModifiedSphericalBesselIn =
            (n, x) => gsl_sf_bessel_il_scaled(n, x);


        public static Func<int, double, double> ModifiedSphericalBesselKn =
            (n, x) => (x <= 0) ? double.NaN : gsl_sf_bessel_kl_scaled(n, x);

        public static Func<double, double, double> logBesselKν =
            (ν, x) => (x <= 0.0 || ν < 0) ? double.NaN : gsl_sf_bessel_lnKnu(ν, x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Jnu(double nu, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Ynu(double nu, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_In(int n, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Kn(int n, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_il_scaled(int l, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_kl_scaled(int l, double x);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_lnKnu(double nu, double x);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_zero_Jnu(double nu, uint s);

        public static double BesselJνZeros(double ν, double s)
        {
            if (s < 0.5 || ν < 0.0) return double.NaN;
            return gsl_sf_bessel_zero_Jnu(ν, (uint) Math.Round(s, MidpointRounding.AwayFromZero));
        }

        public static Complex Hankel1(double α, double x)
        {
            return BesselJν(α, x) + Complex.ImaginaryOne*BesselYν(α, x);
        }

        public static Complex Hankel2(double α, double x)
        {
            return BesselJν(α, x) - Complex.ImaginaryOne*BesselYν(α, x);
        }

        public static Complex SphericalHankel1(int n, double x)
        {
            return SphericalBesselJn(n, x) + Complex.ImaginaryOne*SphericalBesselYn(n, x);
        }

        public static Complex SphericalHankel2(int n, double x)
        {
            return SphericalBesselJn(n, x) - Complex.ImaginaryOne*SphericalBesselYn(n, x);
        }

        public static double RiccatiBesselS(int n, double x)
        {
            return x*SphericalBesselJn(n, x);
        }

        public static double RiccatiBesselC(int n, double x)
        {
            return -x*SphericalBesselYn(n, x);
        }

        public static double RiccatiBesselψ(int n, double x)
        {
            return x*SphericalBesselJn(n, x);
        }

        public static double RiccatiBesselχ(int n, double x)
        {
            return -x*SphericalBesselYn(n, x);
        }

        public static Complex RiccatiBesselξ(int n, double x)
        {
            return x*Hankel1(n, x);
        }

        public static Complex RiccatiBesselζ(int n, double x)
        {
            return x*Hankel2(n, x);
        }

        #endregion

        #region Airy functions

        public static Func<double, double> AiryAi = AdvancedMath.AiryAi;


        public static Func<double, double> AiryBi = AdvancedMath.AiryBi;


        public static Func<double, double> Ai = AiryAi;


        public static Func<double, double> Bi = AiryBi;

        public static Func<double, double> AiPrime = x => gsl_sf_airy_Ai_deriv(x, 0);
        public static Func<double, double> BiPrime = x => gsl_sf_airy_Bi_deriv(x, 0);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Ai_deriv(double x, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Bi_deriv(double x, uint mode);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai(uint s);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi(uint s);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai_deriv(uint s);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi_deriv(uint s);

        //AiryZetaAi //ZerosOfAi// ZerosOfBi
        public static double AiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai((uint) Math.Round(x, MidpointRounding.AwayFromZero));
        }

        public static double BiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi((uint) Math.Round(x, MidpointRounding.AwayFromZero));
        }

        public static double AiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai_deriv((uint) Math.Round(x, MidpointRounding.AwayFromZero));
        }

        public static double BiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi_deriv((uint) Math.Round(x, MidpointRounding.AwayFromZero));
        }

        #endregion

        #region zeta and eta functions

        public static Func<double, double> DirichletEta =
            x => x >= 0 ? AdvancedMath.DirichletEta(x) : (1 - Math.Pow(2, 1 - x))*RiemannZeta(x);


        public static Func<double, double> η = DirichletEta;

        public static double RiemannZeta(double x)
        {
            return AdvancedMath.RiemannZeta(x);
        }

        public static double Riemannζ(double x)
        {
            return AdvancedMath.RiemannZeta(x);
        }

        public static Complex Riemannζ(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        public static Complex RiemannZeta(Complex z)
        {
            return cmplxFromMeta(AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        private static extern double gsl_sf_hzeta(double s, double q);

        public static double HurwitzZeta(double x, double q)
        {
            if (x <= 1.0 || q <= 0) return double.NaN;
            return gsl_sf_hzeta(x, q);
        }

        public static double ζ(double x, double q)
        {
            if (x <= 1.0) return double.NaN;
            return gsl_sf_hzeta(x, q);
        }

        #endregion

        #region mathieu functions

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_a([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_b([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_ce([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);


        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_se([MarshalAs(UnmanagedType.I4)] int order,
            [MarshalAs(UnmanagedType.R8)] double qq, [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_Mc([MarshalAs(UnmanagedType.I4)] int kind,
            [MarshalAs(UnmanagedType.I4)] int order, [MarshalAs(UnmanagedType.R8)] double qq,
            [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);

        [DllImport(gslSfLibDir, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int gsl_sf_mathieu_Ms([MarshalAs(UnmanagedType.I4)] int kind,
            [MarshalAs(UnmanagedType.I4)] int order, [MarshalAs(UnmanagedType.R8)] double qq,
            [MarshalAs(UnmanagedType.R8)] double zz, out gsl_sf_result result);

        public static double MathieuSE(int n, double q, double x)
        {
            var error = gsl_sf_mathieu_se(n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuCE(int n, double q, double x)
        {
            var error = gsl_sf_mathieu_ce(n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuAn(int n, double q)
        {
            var error = gsl_sf_mathieu_a(n, q, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuBn(int n, double q)
        {
            var error = gsl_sf_mathieu_b(n, q, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuMc(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            var error = gsl_sf_mathieu_Mc(j, n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        public static double MathieuMs(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            var error = gsl_sf_mathieu_Ms(j, n, q, x, out sfResult);
            if (error == 0)
                return sfResult.val;
            throw gslExceptions(error);
        }

        [StructLayout(LayoutKind.Sequential, Size = 16), Serializable]
        private struct gsl_sf_result
        {
            [MarshalAs(UnmanagedType.R8)] public readonly double val;
            [MarshalAs(UnmanagedType.R8)] public readonly double err;
        }

        #endregion

        #region logistic functions

        public static Func<double, double> Logistic = MathNet.Numerics.SpecialFunctions.Logistic;

        public static Func<double, double> Logit =
            x => (x < 0 || x > 1) ? double.NaN : MathNet.Numerics.SpecialFunctions.Logit(x);

        public static Func<double, double> StruveL1 = MathNet.Numerics.SpecialFunctions.StruveL1;
        public static Func<double, double> StruveL0 = MathNet.Numerics.SpecialFunctions.StruveL0;

        #endregion

        #region utils

        private const string gslSfLibDir = "gsl.dll"; //"libgsl-0.dll";
        private static gsl_sf_result sfResult;

        private static Exception gslExceptions(int error_code)
        {
            switch (error_code)
            {
                case -1:
                    return new Exception("general failure");
                case -2:
                    return new Exception("iteration has not converged");
                case 1:
                    return new Exception("input domain error: e.g sqrt(-1)");
                case 2:
                    return new Exception("output range error: e.g. exp(1e100)");
                case 3:
                    return new Exception("invalid pointer");
                case 4:
                    return new Exception("invalid argument supplied by user");
                case 5:
                    return new Exception("generic failure");
                case 6:
                    return new Exception("factorization failed");
                case 7:
                    return new Exception("sanity check failed - shouldn't happen");
                case 8:
                    return new Exception("malloc failed");
                case 9:
                    return new Exception("problem with user-supplied function");
                case 10:
                    return new Exception("iterative process is out of control");
                case 11:
                    return new Exception("exceeded max number of iterations");
                case 12:
                    return new Exception("tried to divide by zero");
                case 13:
                    return new Exception("user specified an invalid tolerance");
                case 14:
                    return new Exception("failed to reach the specified tolerance");
                case 15:
                    return new Exception("underflow");
                case 16:
                    return new Exception("overflow ");
                case 17:
                    return new Exception("loss of accuracy");
                case 18:
                    return new Exception("failed because of roundoff error");
                case 19:
                    return new Exception("matrix: vector lengths are not conformant");
                case 20:
                    return new Exception("matrix not square");
                case 21:
                    return new Exception("apparent singularity detected");
                case 22:
                    return new Exception("integral or series is divergent");
                case 23:
                    return new Exception("requested feature is not supported by the hardware");
                case 24:
                    return new Exception("requested feature not (yet) implemented");
                case 25:
                    return new Exception("cache limit exceeded");
                case 26:
                    return new Exception("table limit exceeded");
                case 27:
                    return new Exception("iteration is not making progress towards solution");
                case 28:
                    return new Exception("jacobian evaluations are not improving the solution");
                case 29:
                    return new Exception("cannot reach the specified tolerance in F");
                case 30:
                    return new Exception("cannot reach the specified tolerance in X");
                case 31:
                    return new Exception("cannot reach the specified tolerance in gradient");
                case 32:
                    return new Exception("end of file");
                default:
                    return new Exception("unknown exception");
            }
        }

        private static Complex cmplxFromMeta(Meta.Numerics.Complex c)
        {
            return new Complex(c.Re, c.Im);
        }

        private static Meta.Numerics.Complex cmplxToMeta(Complex c)
        {
            return new Meta.Numerics.Complex(c.Real, c.Imaginary);
        }

        #endregion
    }
}