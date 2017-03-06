// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

// ReSharper disable InconsistentNaming
namespace Computator.NET.Core.Functions
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public static class SpecialFunctions
    {
        [System.ComponentModel.Category("Root finding")]
        public static double findRoot(System.Func<double, double> f, double a, double b)
        {
            var ret = double.NaN;

            try
            {
                ret = MathNet.Numerics.FindRoots.OfFunction(f, a, b, 1e-2, 10000);
            }
            catch
            {
                // ignored
            }
            return ret;
        }

        #region signal processing
        [System.ComponentModel.Category("Signal processing")]
        public static double Gabor(double x, double mean, double amplitude, double position, double width, double phase,
            double frequency)
        {
            return Accord.Math.Gabor.Function1D(x, mean, amplitude, position, width, phase, frequency);
        }
        [System.ComponentModel.Category("Signal processing")]
        public static System.Numerics.Complex Gabor(System.Numerics.Complex z, double λ, double θ, double ψ, double σ,
            double γ)
        {
            var z2 = Accord.Math.Gabor.Function2D((int) z.Real, (int) z.Imaginary, λ, θ, ψ, σ, γ);

            return new System.Numerics.Complex(z2.Real, z2.Imaginary);
        }

        #endregion

        #region test functions

        public static double Ackley(params double[] xi)
        {
            return MathNet.Numerics.TestFunctions.Ackley(xi);
        }

        public static double Rastrigin(params double[] xi)
        {
            return MathNet.Numerics.TestFunctions.Rastrigin(xi);
        }

        public static double Bohachevsky1(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Bohachevsky1(x, y);
        }

        public static double dropWave(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.DropWave(x, y);
        }

        public static double Himmelblau(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Himmelblau(x, y);
        }

        public static double Matyas(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Matyas(x, y);
        }

        public static double sixHumpCamel(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.SixHumpCamel(x, y);
        }

        public static double Rosenbrock(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Rosenbrock(x, y);
        }

        public static double Rosenbrock(params double[] xi)
        {
            return MathNet.Numerics.TestFunctions.Rosenbrock(xi);
        }

        #endregion

        #region Gamma and related functions

        public static double leftRegularizedGamma(double a, double x)
        {
            return (a <= 0 || x < 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.LeftRegularizedGamma(a, x);
        }


        public static double rightRegularizedGamma(double a, double x)
        {
            return (a <= 0 || x < 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.RightRegularizedGamma(a, x);
        }
        [System.ComponentModel.Category("Gamma and related functions")]
        public static double polyGamma(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi(x);
        }
        [System.ComponentModel.Category("Gamma and related functions")]
        public static double ψn(double x)
        {
            return polyGamma(x);
        }

        [System.ComponentModel.Category("Gamma and related functions")]
        public static double ψⁿ(double x)
        {
            return polyGamma(x);
        }


        public static double polyGamma(int n, double x)
        {
            if (n < 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Psi(n,x);
        }

        public static double ψn(int n,double x)
        {
            return polyGamma(n,x);
        }


        public static double ψⁿ(int n, double x)
        {
            return polyGamma(n,x);
        }


        public static double gamma(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Gamma((x));
        }


        public static double Γ(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Gamma((x));
        }

        //[System.ComponentModel.Category("Gamma and related functions")]
        public static double logGamma(double x)
        {
            if ((x) <= 0.0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.LogGamma((x));
        }


        public static double logΓ(double x)
        {
            if ((x) <= 0.0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.LogGamma((x));
        }


        public static double psi(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi((x));
        }


        public static double digamma(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi((x));
        }


        public static double ψ(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi((x));
        }

        //COMPLEX:

        public static System.Numerics.Complex gamma(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Gamma(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex Γ(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Gamma(cmplxToMeta(z)));
        }

        /*
        public static Complex logGamma(Complex z)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(z.Real, z.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val*Complex.ImaginaryOne);
        }

        public static Complex logΓ(Complex z)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(z.Real, z.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val*Complex.ImaginaryOne);
        }
        */

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int gsl_sf_lngamma_complex_e(double zr, double zi, out gsl_sf_result lnr,
            out gsl_sf_result arg);


        public static System.Numerics.Complex psi(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Psi(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex digamma(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Psi(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex ψ(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Psi(cmplxToMeta(z)));
        }

        //non complex type compatible gamma-like functions:


        public static double gamma(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Gamma(a, x);
        }

        public static double Γ(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Gamma(a, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_Q(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_P(double a, double x);

        // public static double gammaQ(double a, double x) { if (x < 0 || a <= 0) return double.NaN; return gsl_sf_gamma_inc_Q(a, x); }

        public static double gammaQ(double a, double x)
        {
            if (a < 0) return double.NaN;
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
            return Meta.Numerics.Functions.AdvancedMath.Beta(x, a, b);
        }


        public static double BetaNorm(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_beta_inc(x, a, b);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_beta_inc(double a, double b, double x);


        public static double Beta(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Beta(a, b);
        }


        public static double β(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Beta(x, a, b);
        }


        public static double β(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Beta(a, b);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_poch(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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
        [System.ComponentModel.Category("Zeta and L-functions")]
        public static double PolyLog(int n, double x)
        {
            if (x > 1.0 || n < 0) return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.PolyLog(n, x);
        }


        public static double diLogarithm(double x)
        {
            if (x > 1.0)
                return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.DiLog((x));
        }
        [System.ComponentModel.Category("Zeta and L-functions")]
        public static double diLog(double x)
        {
            if (x > 1.0)
                return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.DiLog((x));
        }


        public static double SpencesIntegral(double x)
        {
            if (x < 0.0)
                return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.DiLog(1 - (x));
        }


        public static System.Numerics.Complex diLogarithm(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.DiLog(cmplxToMeta(z)));
        }
        [System.ComponentModel.Category("Zeta and L-functions")]
        public static System.Numerics.Complex diLog(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.DiLog(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex SpencesIntegral(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.DiLog(1 - cmplxToMeta(z)));
        }

        #endregion

        #region Wave functions

        public static double CoulombG(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CoulombG(L, η, ρ);
        }


        public static double CoulombF(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CoulombF(L, η, ρ);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_CL_e(double L, double eta, out gsl_sf_result result);


        public static double CoulombC(int L, double η)
        {
            if (L < 0) return double.NaN;
            gsl_sf_coulomb_CL_e(L, η, out sfResult);
            return sfResult.val;
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hydrogenicR(int n, int l, double Z, double r);

        public static double HydrogenicR(int n, int l, double Z, double r)
        {
            if (n < 1 || l > n - 1 || Z <= 0.0 || r < 0.0)
            {
                return double.NaN;
            }
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }

        public static double Rnl(int n, int l, double Z, double r)
        {
            if (n < 1 || l > n - 1 || Z <= 0.0 || r < 0.0)
            {
                return double.NaN;
            }
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }


        public static double CoulombW(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return CoulombC(1, η)*CoulombF(L, η, ρ) + CoulombC(2, η)*CoulombG(L, η, ρ);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_wave_FG_e(double eta, double x,
            double lam_F,
            int k_lam_G,
            out gsl_sf_result F, out gsl_sf_result Fp,
            out gsl_sf_result G, out gsl_sf_result Gp,
            out double exp_F, out double exp_G);


        public static double CoulombGprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ <= 0.0) return double.NaN;


            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out r2, out r3, out sfResult, out d1, out d2);
            return sfResult.val;
        }


        public static double CoulombFprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ <= 0.0) return double.NaN;
            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out sfResult, out r3, out r2, out d1, out d2);
            return sfResult.val;
        }

        #endregion

        #region Fermi–Dirac complete&incomplete integral

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_int(int j, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_inc_0(double x, double b);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_0(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_mhalf(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_half(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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
            if (b < 0.0)
            {
                return double.NaN;
            }
            return gsl_sf_fermi_dirac_inc_0(x, b);
        }

        public static double FermiDiracFj(int j, double x)
        {
            return gsl_sf_fermi_dirac_int(j, x);
        }

        #endregion

        #region lambert W functions

        public static double LambertW0(double x)
        {
            return (x <= -1/System.Math.E) ? double.NaN : gsl_sf_lambert_W0(x);
        }

        public static double LambertWm1(double x)
        {
            return (x <= -1/System.Math.E) ? double.NaN : gsl_sf_lambert_Wm1(x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_W0(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_Wm1(double x);

        #endregion

        #region polynomials

        public static double Gegenbauer1(double α, double x)
        {
            return gsl_sf_gegenpoly_1(α, x);
        }

        public static double
            Gegenbauer2(double α, double x)
        {
            return gsl_sf_gegenpoly_2(α, x);
        }

        public static double
            Gegenbauer3(double α, double x)
        {
            return gsl_sf_gegenpoly_3(α, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_n(int n, double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_1(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_2(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_3(double lambda, double x);


        public static double Gegenbauer(int n, double α, double x)
        {
            if (α <= -0.5 || n < 0) return double.NaN;
            return gsl_sf_gegenpoly_n(n, α, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_n(int n, double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_1(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_2(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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
            if (l < 0) return double.NaN;
            if (System.Math.Abs(x) > 1.0) return double.NaN;
            return Meta.Numerics.Functions.OrthogonalPolynomials.LegendreP(l, x);
        }

        public static double LegendreP(int l, int m, double x)
        {
            if (l < 0) return double.NaN;
            if (System.Math.Abs(m) > l) return double.NaN;
            if (System.Math.Abs(x) > 1.0) return double.NaN;
            return Meta.Numerics.Functions.OrthogonalPolynomials.LegendreP(l, m, x);
        }

        //add legendre Q


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_Ql(int l, double x);

        // Q_l(x), x > -1, x != 1, l >= 0
        public static double LegendreQ(int l, double x)
        {
            if (x == 1.0 || x <= -1.0 || l < 0) return double.NaN;
            return gsl_sf_legendre_Ql(l, x);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_sphPlm(int l, int m, double x);

        public static double SphericalLegendreP(int l, int m, double x)
        {
            if (m < 0 || l < m || x < -1.0 || x > 1.0)
            {
                return double.NaN;
            }

            return gsl_sf_legendre_sphPlm(l, m, x);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_half(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_mhalf(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_0(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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


        public static double ConicalP(int μ, double λ, double x)
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


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_sph_reg(int l, double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_H3d(int l, double lambda, double eta);

        public static double LegendreH3D(int l, double λ, double η)
        {
            if (η < 0 || l < 0 || (l >= 2 && λ == 0.0)) return double.NaN;
            return gsl_sf_legendre_H3d(l, λ, η);
        }

        public static double ChebyshevT(int n, double x)
        {
            if (System.Math.Abs(x) > 1.0) return double.NaN;
            if (n < 0) return double.NaN;

            return Meta.Numerics.Functions.OrthogonalPolynomials.ChebyshevT(n, x);
        }

        //add ChebyshevU

        public static double HermiteH(int n, double x)
        {
            if (n < 0)
            {
                return double.NaN;
            }
            return Meta.Numerics.Functions.OrthogonalPolynomials.HermiteH(n, x);
        }

        public static double HermiteHe(int n, double x)
        {
            if (n < 0)
            {
                return double.NaN;
            }
            return Meta.Numerics.Functions.OrthogonalPolynomials.HermiteHe(n, x);
        }

        public static double ZernikeR(int n, int m, double ρ)
        {
            if (n < 0) return double.NaN;
            if ((m < 0) || (m > n)) return double.NaN;
            if ((ρ < 0.0) || (ρ > 1.0)) return double.NaN;
            return Meta.Numerics.Functions.OrthogonalPolynomials.ZernikeR(n, m, ρ);
        }

        //TODO: add Zernike Z 

        #endregion

        #region transport functions

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_2(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_3(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_4(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        public static double SynchrotronF(double x)
        {
            return (x < 0) ? double.NaN : gsl_sf_synchrotron_1(x);
        }

        public static double SynchrotronG(double x)
        {
            return (x < 0) ? double.NaN : gsl_sf_synchrotron_2(x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_1(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_2(double x);

        #endregion

        #region coupling 3,6,9-j symbols

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_3j(int two_ja, int two_jb, int two_jc,
            int two_ma, int two_mb, int two_mc);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_6j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_RacahW(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_9j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf,
            int two_jg, int two_jh, int two_ji);


        public static double Coupling3j(int ja, int jb, int jc, int ma, int mb, int mc)
        {
            if (ja < 0 || jb < 0 || jc < 0)
                return double.NaN;
            return gsl_sf_coupling_3j(ja, jb, jc, ma, mb, mc);
        }

        public static double Coupling6j(int ja, int jb, int jc, int jd, int je, int jf)
        {
            if (ja < 0 || jb < 0 || jc < 0 || jd < 0 || je < 0 || jf < 0)
                return double.NaN;
            return gsl_sf_coupling_6j(ja, jb, jc, jd, je, jf);
        }

        public static double CouplingRacahW(int ja, int jb, int jc, int jd, int je, int jf)
        {
            if (ja < 0 || jb < 0 || jc < 0 || jd < 0 || je < 0 || jf < 0)
                return double.NaN;
            return gsl_sf_coupling_RacahW(ja, jb, jc, jd, je, jf);
        }

        public static double Coupling9j(int ja, int jb, int jc, int jd, int je, int jf, int jg, int jh, int ji)
        {
            if (ja < 0 || jb < 0 || jc < 0 || jd < 0 || je < 0 || jf < 0 || jg < 0 || jh < 0 || ji < 0)
                return double.NaN;
            return gsl_sf_coupling_9j(ja, jb, jc, jd, je, jf, jg, jh, jf);
        }

        public static double Coupling3j(double j1, double j2, double j3, double m1, double m2, double m3)
        {
            // no negative, spin must be integer or half-integer
            if (j1 < 0 || j2 < 0 || j3 < 0 ||
                System.Math.Floor(2*j1) != 2*j1 || System.Math.Floor(2*j2) != 2*j2 || System.Math.Floor(2*j3) != 2*j3)
                return double.NaN;


            // -J <= M <= J
            if (m1 < -System.Math.Abs(j1) || m1 > System.Math.Abs(j1) ||
                m2 < -System.Math.Abs(j2) || m2 > System.Math.Abs(j2) ||
                m3 < -System.Math.Abs(j3) || m3 > System.Math.Abs(j3))
                return double.NaN;


            // 2M must be an integer
            if (System.Math.Floor(2*m1) != 2*m1 || System.Math.Floor(2*m2) != 2*m2 || System.Math.Floor(2*m3) != 2*m3)
                return double.NaN;


            // half-integer J requires half-integer M; integer J requires integer M
            if (((2*j1)%2) != System.Math.Abs((2*m1)%2) || ((2*j2)%2) != System.Math.Abs((2*m2)%2) ||
                ((2*j3)%2) != System.Math.Abs((2*m3)%2))
                return double.NaN;


            return Meta.Numerics.Functions.SpinMath.ThreeJ(new Meta.Numerics.Functions.SpinState(j1, m1),
                new Meta.Numerics.Functions.SpinState(j2, m2), new Meta.Numerics.Functions.SpinState(j3, m3));
        }

        public static double Coupling6j(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            if (j1 < 0 || j2 < 0 || j3 < 0 || j4 < 0 || j5 < 0 || j6 < 0 ||
                System.Math.Floor(2*j1) != 2*j1 || System.Math.Floor(2*j2) != 2*j2 || System.Math.Floor(2*j3) != 2*j3 ||
                System.Math.Floor(2*j4) != 2*j4 || System.Math.Floor(2*j5) != 2*j5 || System.Math.Floor(2*j6) != 2*j6)
                return double.NaN;

            return Meta.Numerics.Functions.SpinMath.SixJ(new Meta.Numerics.Functions.Spin(j1),
                new Meta.Numerics.Functions.Spin(j2), new Meta.Numerics.Functions.Spin(j3),
                new Meta.Numerics.Functions.Spin(j4), new Meta.Numerics.Functions.Spin(j5),
                new Meta.Numerics.Functions.Spin(j6));
        }

        public static double ClebschGordan(double j1, double j2, double j, double m1, double m2, double m)
        {
            // no negative, spin must be integer or half-integer
            if (j1 < 0 || j2 < 0 || j < 0 ||
                System.Math.Floor(2*j1) != 2*j1 || System.Math.Floor(2*j2) != 2*j2 || System.Math.Floor(2*j) != 2*j)
                return double.NaN;

            // -J <= M <= J
            if (m1 < -System.Math.Abs(j1) || m1 > System.Math.Abs(j1) ||
                m2 < -System.Math.Abs(j2) || m2 > System.Math.Abs(j2) ||
                m < -System.Math.Abs(j) || m > System.Math.Abs(j))
                return double.NaN;


            // 2M must be an integer
            if (System.Math.Floor(2*m1) != 2*m1 || System.Math.Floor(2*m2) != 2*m2 || System.Math.Floor(2*m) != 2*m)
                return double.NaN;


            // half-integer J requires half-integer M; integer J requires integer M
            if (((2*j1)%2) != System.Math.Abs((2*m1)%2) || ((2*j2)%2) != System.Math.Abs((2*m2)%2) ||
                ((2*j)%2) != System.Math.Abs((2*m)%2))
                return double.NaN;

            return Meta.Numerics.Functions.SpinMath.ClebschGodron(new Meta.Numerics.Functions.SpinState(j1, m1),
                new Meta.Numerics.Functions.SpinState(j2, m2), new Meta.Numerics.Functions.SpinState(j, m));
        }

        #endregion

        #region Hypergeometric functions

        public static System.Numerics.Complex SphericalHarmonic(int l, int m, double θ, double φ)
        {
            if (l < 0) return double.NaN;
            if ((m > l) || (m < -l)) return double.NaN;

            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedMath.SphericalHarmonic(l, m, θ, φ));
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_0F1(double c, double x);

        //Hypergeometric function related to Bessel functions 0F1[c,x]

        public static double Hypergeometric0F1(double c, double x)
        {
            return gsl_sf_hyperg_0F1(c, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1_int(int m, int n, double x);

        //Confluent hypergeometric function  for integer parameters. 1F1[m,n,x] = M(m,n,x)

        public static double Hypergeometric1F1(int m, int n, double x)
        {
            return gsl_sf_hyperg_1F1_int(m, n, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1(double a, double b, double x);

        //Confluent hypergeometric function. 1F1[a,b,x] = M(a,b,x)

        public static double Hypergeometric1F1(double a, double b, double x)
        {
            return gsl_sf_hyperg_1F1(a, b, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U_int(int m, int n, double x);

        //Confluent hypergeometric function for integer parameters. U(m,n,x)

         public static double HypergeometricU(int m, int n, double x)
         {
            return gsl_sf_hyperg_U_int(m, n, x);
        }

        //Confluent hypergeometric function. U(a,b,x)
        public static double HypergeometricU(double a, double b, double x)
        {
            return gsl_sf_hyperg_U(a, b, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U(double a, double b, double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1(double a, double b, double c, double x);

        //Gauss hypergeometric function 2F1[a,b,c,x]

        public static double Hypergeometric2F1(double a, double b, double c, double x)
        {
            return gsl_sf_hyperg_2F1(a, b, c, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj(double aR, double aI, double c, double x);

        //Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x]

        public static double Hypergeometric2F1(System.Numerics.Complex a, double c, double x)
        {
            return gsl_sf_hyperg_2F1_conj(a.Real, a.Imaginary, c, x);
        } //TODO: better name for a parameter

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_renorm(double a, double b, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[a,b,c,x] / Gamma[c]

        public static double Hypergeometric2F1renorm(double a, double b, double c, double x)
        {
            return gsl_sf_hyperg_2F1_renorm(a, b, c, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj_renorm(double aR, double aI, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x] / Gamma[c]

        public static double Hypergeometric2F1renorm(System.Numerics.Complex a, double c, double x)
        {
            return gsl_sf_hyperg_2F1_conj_renorm(a.Real, a.Imaginary, c, x);
        } //TODO: better name for a parameter

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F0(double a, double b, double x);

        /* Mysterious hypergeometric function. The series representation
         * is a divergent hypergeometric series. However, for x < 0 we
         * have 2F0(a,b,x) = (-1/x)^a U(a,1+a-b,-1/x)*/


        public static double Hypergeometric2F0(double a, double b, double x)
        {
            return gsl_sf_hyperg_2F0(a, b, x);
        }

        #endregion

        #region integral functions

        public static double Ti(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.IntegralTi(x);
        }

        public static double Dawson(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Dawson(x);
        }

        public static double Clausen(double x) { return gsl_sf_clausen(x); }

        public static double Si(double x) { return Meta.Numerics.Functions.AdvancedMath.IntegralSi(x); } //sine integral

        public static double Ci(double x)//cosine integral
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.IntegralCi(x);
        }

        //Generalized Exponential Integral
        public static double En(int n, double x)
        {
            if (x < 0)
            {
                if (n < 0)
                {
                    return double.NaN; //DOMAIN_ERROR(result);
                }
                if (n == 0)
                {
                    if (x == 0)
                    {
                        return double.NaN; //DOMAIN_ERROR(result);
                    }
                    //   result->val = (scale ? 1.0 : exp(-x)) / x;
                    // result->err = 2 * GSL_DBL_EPSILON * fabs(result->val);
                    //CHECK_UNDERFLOW(result);
                    return gsl_sf_expint_En(n, x);//GSL_SUCCESS;
                }
                if (n == 1)
                {
                    return gsl_sf_expint_En(n, x);//expint_E1_impl(x, result, scale);
                }
                if (n == 2)
                {
                    return gsl_sf_expint_En(n, x);//expint_E2_impl(x, result, scale);
                }
                if (x < 0)
                {
                    return double.NaN;//DOMAIN_ERROR(result);
                }
                if (x == 0)
                {
                    // result->val = (scale ? exp(x) : 1) * (1 / (n - 1.0));
                    // result->err = 2 * GSL_DBL_EPSILON * fabs(result->val);
                    //CHECK_UNDERFLOW(result);
                    return gsl_sf_expint_En(n, x);//GSL_SUCCESS;
                }
                //gsl_sf_result result_g;
                //double prefactor = pow(x, n - 1);
                //int status = gsl_sf_gamma_inc_e(1 - n, x, &result_g);
                //double scale_factor = (scale ? exp(x) : 1.0);
                //result->val = scale_factor * prefactor * result_g.val;
                //result->err = 2 * GSL_DBL_EPSILON * fabs(result->val);
                //result->err += 2 * fabs(scale_factor * prefactor * result_g.err);
                //if (status == GSL_SUCCESS) CHECK_UNDERFLOW(result);
                return gsl_sf_expint_En(n, x);//status;
            }
            return Meta.Numerics.Functions.AdvancedMath.IntegralE(n, x);
        }


        //   public static double EnNEW(int n, double x)//Generalized Exponential Integral
        //  {
        //     return MathNet.Numerics.SpecialFunctions.ExponentialIntegral(x, n);
        //     return 1.0;
        //}

        public static double Ei(double x)
            {
                return (x == 0.0) ? Meta.Numerics.Functions.AdvancedMath.IntegralEi(x) : gsl_sf_expint_Ei(x);
            }

        public static double Shi(double x) { return gsl_sf_Shi(x); } //hyperbolic sine integral




        //hyperbolic cosine integral

        public static double Chi(double x)
        {
            return (x == 0.0) ? double.NaN : gsl_sf_Chi(x);
        }

        public static double Tai(double x) { return gsl_sf_atanint(x); } //arcus tangent integral


        //Integrals in optics
        public static System.Numerics.Complex Fresnel(double x)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedMath.Fresnel(x));
        }

        public static double FresnelS(double x)
        {
           return Meta.Numerics.Functions.AdvancedMath.FresnelS(x);
        }

        public static double FresnelC(double x)
        {
           return Meta.Numerics.Functions.AdvancedMath.FresnelC(x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_1(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_2(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_3(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_4(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_5(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_clausen(double x);


        public static System.Numerics.Complex Ein(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Ein(cmplxToMeta(z)));
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_En(int n, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_Ei(double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_Shi(double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_Chi(double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_atanint(double x);

        #endregion

        #region Elliptic integrals

        //In mathematics, the Carlson symmetric forms of elliptic integrals are a small canonical set of elliptic integrals to which all others may be reduced. They are a modern alternative to the Legendre forms. The Legendre forms may be expressed in terms of the Carlson forms and vice versa.
        public static double EllipticK(double x)
        {
            return (x < 0 || x >= 1.0) ? ((x < 0 && x > -1.0) ? gsl_sf_ellint_Kcomp(x, 0) : double.NaN) : Meta.Numerics.Functions.AdvancedMath.EllipticK(x);
        }


        public static double CarlsonD(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CarlsonD(x, y, z);
        }


        public static double CarlsonF(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CarlsonF(x, y, z);
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
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_F(φ, x, 0);
            return Meta.Numerics.Functions.AdvancedMath.EllipticF(φ, x);
        }

        public static double EllipticE(double x)
        {
            if (x < 0 || x > 1.0)
                if (x < 0 && x > -1.0)
                    return gsl_sf_ellint_Ecomp(x, 0);
                else
                    return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.EllipticE(x);
        }

        public static double EllipticE(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_E(φ, x, 0);
            return Meta.Numerics.Functions.AdvancedMath.EllipticE(φ, x);
        }

        public static double EllipticΠ(double x, double n)
        {
            if (x*x >= 1.0 || n <= -1.0)
                return double.NaN;
            return gsl_sf_ellint_Pcomp(x, n, 0);
        }

        public static double EllipticΠ(double φ, double x, int n)
        {
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0 || n <= -2)
                //TODO: try to relax on these conditions
                return double.NaN;
            return gsl_sf_ellint_P(φ, x, n, 0);
        }


        public static double EllipticD(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0)
                return double.NaN;
            return gsl_sf_ellint_D(φ, x, 0);
        }


        public static double EllipticD(double x)
        {
            if (x <= -1 || x >= 1.0)
                return double.NaN;

            return gsl_sf_ellint_Dcomp(x, 0);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Ecomp(double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Kcomp(double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Pcomp(double k, double n, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Dcomp(double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_F(double phi, double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_E(double phi, double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_P(double phi, double k, double n, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        //private static extern double gsl_sf_ellint_D(double phi, double k, double n, uint mode);
        private static extern double gsl_sf_ellint_D(double phi, double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RC(double x, double y, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RD(double x, double y, double z, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RF(double x, double y, double z, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RJ(double x, double y, double z, double p, uint mode);

        #endregion

        #region  Jacobian elliptic functions

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        public static System.Numerics.Complex erfi(System.Numerics.Complex z)
        {
            return -System.Numerics.Complex.ImaginaryOne*erf(System.Numerics.Complex.ImaginaryOne*z);
        }

        public static double inverseErf(double x)
        {
            if (x > 1 || x < 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.InverseErf(x);
        }
        public static double inverseErfc(double x)
        {
            if (x > 1 || x<0)
                return double.NaN;
            return
                Meta.Numerics.Functions.AdvancedMath.InverseErfc(x);
        }

        public static double logErfc(double x)
        {
            return gsl_sf_log_erfc(x);
        }

        public static double erfZ(double x)
        {
            return gsl_sf_erf_Z(x);
        }

        public static double erfQ(double x)
        {
            return 
            gsl_sf_erf_Q(x);
        }

        public static double hazard(double x)
        {
            return 
            gsl_sf_hazard(x);
        }
        [System.ComponentModel.Category("Functions related to probability distributions")]
        public static double OwenT(double h, double a)
        {
            return Accord.Math.OwensT.Function(h, a);
        }
        [System.ComponentModel.Category("Functions related to probability distributions")]
        public static double OwenT(double h, double a, double ah)
        {
            return Accord.Math.OwensT.Function(h, a, ah);
        }

        //checked and they work ok

        public static double erf(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Erf((x));
        }


        public static double erfc(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Erfc((x));
        }


        public static double erfcx(double x)
        {
            return ((erfc(x))/System.Math.Exp(-((x))*(x)));
        }


        public static System.Numerics.Complex erf(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Erf(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex erfc(System.Numerics.Complex z)
        {
            return (1 - cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Erf(cmplxToMeta(z))));
        }


        public static System.Numerics.Complex erfcx(System.Numerics.Complex z)
        {
            return ((erfc(z))/System.Numerics.Complex.Exp(-((z))*(z)));
        }


        public static System.Numerics.Complex faddeeva(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Faddeeva(cmplxToMeta(z)));
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_log_erfc(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Z(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Q(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hazard(double x);

        #endregion

        #region Bessel functions

        //unfortunetelly only for real arguments :( cant find any implementation for complex args
        //TODO: check

        //  Description("angielski opis funkcji")]
        //public static Func<double, double, double> BesselJν2 = (ν, x) => gsl_sf_bessel_Jnu(ν, x);

        // Description("angielski opis funkcji")]
        // public static Func<double, double, double> BesselYν2 = (ν, x) => gsl_sf_bessel_Ynu(ν, x);

        public static double BesselJ0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(0, x);
        }

        public static double BesselJ1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(1, x);
        }

        public static double BesselJ2(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(2, x);
        }

        public static double BesselJ3(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(3, x);
        }

        public static double BesselJnPrime(int n, double x)
        {
            return 0.5*
                   (Meta.Numerics.Functions.AdvancedMath.BesselJ(n - 1, x) -
                    Meta.Numerics.Functions.AdvancedMath.BesselJ(n + 1, x));
        }


        public static double BesselJn(int n, double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(n, x);
        }

        public static double BesselJν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.BesselJ(ν, x);
        }

        // Description("angielski opis funkcji")]
        //public static double BesselJa = BesselJν;

        public static double BesselY0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(0, x);
        }

        public static double BesselY1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(1, x);
        }

        public static double BesselY2(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(2, x);
        }

        public static double BesselY3(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(3, x);
        }

        public static double BesselYn(int n, double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(n, x);
        }

        public static double BesselYν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.BesselY(ν, x);
        }

        // Description("angielski opis funkcji")]
        //public static double BesselYa = BesselYν;

        public static double ModifiedBesselIn(int n, double x)
        {
            return gsl_sf_bessel_In(n, x);
        }


        public static double ModifiedBesselIν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.ModifiedBesselI(ν, x);
        }

        // Description("angielski opis funkcji")]
        //public static double ModifiedBesselIa = ModifiedBesselIν;

        public static double ModifiedBesselKν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.ModifiedBesselK(ν, x);
        }

        public static double ModifiedBesselKn(int n, double x)
        {
            return (x <= 0.0) ? double.NaN : gsl_sf_bessel_Kn(n, x);
        }

        // Description("angielski opis funkcji")]
        //public static double ModifiedBesselKa = ModifiedBesselKν;


        public static double SphericalBesselJ0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(0, x);
        }

        public static double SphericalBesselJ1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(1, x);
        }

        public static double SphericalBesselJ2(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(2, x);
        }

        public static double SphericalBesselJ3(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(3, x);
        }

        public static double SphericalBesselJn(int n, double x)
        {
            if (x == 0 && n <= -3)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(n, x);
        }


        public static double SphericalBesselY0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(0, x);
        }


        public static double SphericalBesselY1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(1, x);
        }


        public static double SphericalBesselY2(double x)
        {
            return (x == 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(2, x);
        }


        public static double SphericalBesselY3(double x)
        {
            return (x == 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(3, x);
        }


        public static double SphericalBesselYn(int n, double x)
        {
            if (x == 0 & n >= 2)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(n, x);
        }


        public static double ModifiedSphericalBesselIn(int n, double x)
        {
            if (n < 0)
                return double.NaN;
            return gsl_sf_bessel_il_scaled(n, x);
        }


        public static double ModifiedSphericalBesselKn(int n, double x)
        {
            return (x <= 0 || n < 0) ? double.NaN : gsl_sf_bessel_kl_scaled(n, x);
        }

        public static double logBesselKν(double ν, double x)
        {
            return (x <= 0.0 || ν < 0) ? double.NaN : gsl_sf_bessel_lnKnu(ν, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Jnu(double nu, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Ynu(double nu, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_In(int n, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Kn(int n, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_il_scaled(int l, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_kl_scaled(int l, double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_lnKnu(double nu, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_zero_Jnu(double nu, uint s);

        public static double BesselJνZeros(double ν, double s)
        {
            if (s < 0.5 || ν < 0.0) return double.NaN;
            return gsl_sf_bessel_zero_Jnu(ν, (uint) System.Math.Round(s, System.MidpointRounding.AwayFromZero));
        }

        public static System.Numerics.Complex Hankel1(double α, double x)
        {
            return BesselJν(α, x) + System.Numerics.Complex.ImaginaryOne*BesselYν(α, x);
        }

        public static System.Numerics.Complex Hankel2(double α, double x)
        {
            return BesselJν(α, x) - System.Numerics.Complex.ImaginaryOne*BesselYν(α, x);
        }

        public static System.Numerics.Complex SphericalHankel1(int n, double x)
        {
            return SphericalBesselJn(n, x) + System.Numerics.Complex.ImaginaryOne*SphericalBesselYn(n, x);
        }

        public static System.Numerics.Complex SphericalHankel2(int n, double x)
        {
            return SphericalBesselJn(n, x) - System.Numerics.Complex.ImaginaryOne*SphericalBesselYn(n, x);
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

        public static System.Numerics.Complex RiccatiBesselξ(int n, double x)
        {
            return x*Hankel1(n, x);
        }

        public static System.Numerics.Complex RiccatiBesselζ(int n, double x)
        {
            return x*Hankel2(n, x);
        }

        #endregion

        #region Airy functions

        public static double AiryAi(double x)
        {
              return Meta.Numerics.Functions.AdvancedMath.AiryAi(x); 
        }

        public static double AiryBi(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.AiryBi(x);
        }


        public static double Ai(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.AiryAi(x);
        }

        public static double Bi(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.AiryBi(x);
        }

        public static double AiPrime(double x)
        {
            return gsl_sf_airy_Ai_deriv(x,0);
        }


        public static double BiPrime(double x)
        {
            return gsl_sf_airy_Bi_deriv(x, 0);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Ai_deriv(double x, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Bi_deriv(double x, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai(uint s);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi(uint s);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai_deriv(uint s);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi_deriv(uint s);

        //AiryZetaAi //ZerosOfAi// ZerosOfBi
        public static double AiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        public static double BiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        public static double AiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai_deriv((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        public static double BiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi_deriv((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        #endregion

        #region zeta and eta functions

        public static double DirichletEta(double x)
        {
            return x >= 0 ? Meta.Numerics.Functions.AdvancedMath.DirichletEta(x) : (1 - System.Math.Pow(2, 1 - x))*RiemannZeta(x);
        }

        public static double η(double x)
        {
            return DirichletEta(x);
        }


        public static double RiemannZeta(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.RiemannZeta(x);
        }

        public static double Riemannζ(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.RiemannZeta(x);
        }

        public static System.Numerics.Complex Riemannζ(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        public static System.Numerics.Complex RiemannZeta(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_a(int order, double qq);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
    CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_b(int order, double qq);



        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_ce(int order, double qq, double zz);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_se(int order, double qq, double zz);



        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_Mc(int kind, int order, double qq, double zz);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_Ms(int kind, int order, double qq, double zz);


        public static double MathieuSE(int n, double q, double x)
        {
            return gsl_sf_mathieu_se(n, q, x);
        }

        public static double MathieuCE(int n, double q, double x)
        {
            return gsl_sf_mathieu_ce(n, q, x);
        }

        public static double MathieuAn(int n, double q)
        {
            return gsl_sf_mathieu_a(n, q);
        }

        public static double MathieuBn(int n, double q)
        {
            if (n == 0)
                return double.NaN;
            return gsl_sf_mathieu_b(n, q);
        }

        public static double MathieuMc(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            return gsl_sf_mathieu_Mc(j,n, q,x);
        }

        public static double MathieuMs(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            return gsl_sf_mathieu_Ms(j, n, q, x);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 16),
         System.Serializable]
        private struct gsl_sf_result
        {
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.R8)] public readonly
                double val;

            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.R8)] public readonly
                double err;
        }

        #endregion

        #region logistic functions

        public static double Logistic(double x)
        {
            return MathNet.Numerics.SpecialFunctions.Logistic(x);
        }

        public static double Logit(double x)
        {
            return (x < 0 || x > 1) ? double.NaN : MathNet.Numerics.SpecialFunctions.Logit(x);
        }


        public static double StruveL1(double x)
        {
            return MathNet.Numerics.SpecialFunctions.StruveL1(x);
        }

        public static double StruveL0(double x)
        {
            return MathNet.Numerics.SpecialFunctions.StruveL0(x);
        }

        #endregion

        #region utils
        

        private static gsl_sf_result sfResult;

        private static System.Exception gslExceptions(int error_code)
        {
            switch (error_code)
            {
                case -1:
                    return new System.Exception("general failure");
                case -2:
                    return new System.Exception("iteration has not converged");
                case 1:
                    return new System.Exception("input domain error: e.g sqrt(-1)");
                case 2:
                    return new System.Exception("output range error: e.g. exp(1e100)");
                case 3:
                    return new System.Exception("invalid pointer");
                case 4:
                    return new System.Exception("invalid argument supplied by user");
                case 5:
                    return new System.Exception("generic failure");
                case 6:
                    return new System.Exception("factorization failed");
                case 7:
                    return new System.Exception("sanity check failed - shouldn't happen");
                case 8:
                    return new System.Exception("malloc failed");
                case 9:
                    return new System.Exception("problem with user-supplied function");
                case 10:
                    return new System.Exception("iterative process is out of control");
                case 11:
                    return new System.Exception("exceeded max number of iterations");
                case 12:
                    return new System.Exception("tried to divide by zero");
                case 13:
                    return new System.Exception("user specified an invalid tolerance");
                case 14:
                    return new System.Exception("failed to reach the specified tolerance");
                case 15:
                    return new System.Exception("underflow");
                case 16:
                    return new System.Exception("overflow ");
                case 17:
                    return new System.Exception("loss of accuracy");
                case 18:
                    return new System.Exception("failed because of roundoff error");
                case 19:
                    return new System.Exception("matrix: vector lengths are not conformant");
                case 20:
                    return new System.Exception("matrix not square");
                case 21:
                    return new System.Exception("apparent singularity detected");
                case 22:
                    return new System.Exception("integral or series is divergent");
                case 23:
                    return new System.Exception("requested feature is not supported by the hardware");
                case 24:
                    return new System.Exception("requested feature not (yet) implemented");
                case 25:
                    return new System.Exception("cache limit exceeded");
                case 26:
                    return new System.Exception("table limit exceeded");
                case 27:
                    return new System.Exception("iteration is not making progress towards solution");
                case 28:
                    return new System.Exception("jacobian evaluations are not improving the solution");
                case 29:
                    return new System.Exception("cannot reach the specified tolerance in F");
                case 30:
                    return new System.Exception("cannot reach the specified tolerance in X");
                case 31:
                    return new System.Exception("cannot reach the specified tolerance in gradient");
                case 32:
                    return new System.Exception("end of file");
                default:
                    return new System.Exception("unknown exception");
            }
        }

        private static System.Numerics.Complex cmplxFromMeta(Meta.Numerics.Complex c)
        {
            return new System.Numerics.Complex(c.Re, c.Im);
        }

        private static Meta.Numerics.Complex cmplxToMeta(System.Numerics.Complex c)
        {
            return new Meta.Numerics.Complex(c.Real, c.Imaginary);
        }

        public const string ToCode =
            @"
     public static double findRoot(System.Func<double, double> f, double a, double b)
        {
            var ret = double.NaN;

            try
            {
                ret = MathNet.Numerics.FindRoots.OfFunction(f, a, b, 1e-2, 10000);
            }
            catch (System.Exception ex)
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

        public static System.Numerics.Complex Gabor(System.Numerics.Complex z, double λ, double θ, double ψ, double σ,
            double γ)
        {
            var z2 = Accord.Math.Gabor.Function2D((int) z.Real, (int) z.Imaginary, λ, θ, ψ, σ, γ);

            return new System.Numerics.Complex(z2.Real, z2.Imaginary);
        }

        #endregion

        #region test functions

        public static double Ackley(params double[] xi)
        {
            return MathNet.Numerics.TestFunctions.Ackley(xi);
        }

        public static double Rastrigin(params double[] xi)
        {
            return MathNet.Numerics.TestFunctions.Rastrigin(xi);
        }

        public static double Bohachevsky1(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Bohachevsky1(x, y);
        }

        public static double dropWave(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.DropWave(x, y);
        }

        public static double Himmelblau(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Himmelblau(x, y);
        }

        public static double Matyas(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Matyas(x, y);
        }

        public static double sixHumpCamel(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.SixHumpCamel(x, y);
        }

        public static double Rosenbrock(double x, double y)
        {
            return MathNet.Numerics.TestFunctions.Rosenbrock(x, y);
        }

        public static double Rosenbrock(params double[] xi)
        {
            return MathNet.Numerics.TestFunctions.Rosenbrock(xi);
        }

        #endregion

        #region Gamma and related functions

        public static double leftRegularizedGamma(double a, double x)
        {
            return (a <= 0 || x < 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.LeftRegularizedGamma(a, x);
        }


        public static double rightRegularizedGamma(double a, double x)
        {
            return (a <= 0 || x < 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.RightRegularizedGamma(a, x);
        }

        public static double polyGamma(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi(x);
        }

        public static double ψn(double x)
        {
            return polyGamma(x);
        }


        public static double ψⁿ(double x)
        {
            return polyGamma(x);
        }


        public static double polyGamma(int n, double x)
        {
            if (n < 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Psi(n,x);
        }

        public static double ψn(int n,double x)
        {
            return polyGamma(n,x);
        }


        public static double ψⁿ(int n, double x)
        {
            return polyGamma(n,x);
        }


        public static double gamma(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Gamma((x));
        }


        public static double Γ(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Gamma((x));
        }


        public static double logGamma(double x)
        {
            if ((x) <= 0.0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.LogGamma((x));
        }


        public static double logΓ(double x)
        {
            if ((x) <= 0.0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.LogGamma((x));
        }


        public static double psi(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi((x));
        }


        public static double digamma(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi((x));
        }


        public static double ψ(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Psi((x));
        }

        //COMPLEX:

        public static System.Numerics.Complex gamma(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Gamma(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex Γ(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Gamma(cmplxToMeta(z)));
        }

        /*
        public static Complex logGamma(Complex z)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(z.Real, z.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val*Complex.ImaginaryOne);
        }

        public static Complex logΓ(Complex z)
        {
            gsl_sf_result lnr = new gsl_sf_result(), arg = new gsl_sf_result();
            gsl_sf_lngamma_complex_e(z.Real, z.Imaginary, out lnr, out arg);
            return (lnr.val + arg.val*Complex.ImaginaryOne);
        }
        */

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int gsl_sf_lngamma_complex_e(double zr, double zi, out gsl_sf_result lnr,
            out gsl_sf_result arg);


        public static System.Numerics.Complex psi(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Psi(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex digamma(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Psi(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex ψ(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Psi(cmplxToMeta(z)));
        }

        //non complex type compatible gamma-like functions:


        public static double gamma(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Gamma(a, x);
        }

        public static double Γ(double a, double x)
        {
            if (x < 0 || a <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Gamma(a, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_Q(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gamma_inc_P(double a, double x);

        // public static double gammaQ(double a, double x) { if (x < 0 || a <= 0) return double.NaN; return gsl_sf_gamma_inc_Q(a, x); }

        public static double gammaQ(double a, double x)
        {
            if (a < 0) return double.NaN;
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
            return Meta.Numerics.Functions.AdvancedMath.Beta(x, a, b);
        }


        public static double BetaNorm(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return gsl_sf_beta_inc(x, a, b);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_beta_inc(double a, double b, double x);


        public static double Beta(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Beta(a, b);
        }


        public static double β(double x, double a, double b)
        {
            if (x > 1 || x < 0 || a <= 0 || b <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Beta(x, a, b);
        }


        public static double β(double a, double b)
        {
            if (a <= 0 || b <= 0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.Beta(a, b);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_poch(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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
            if (x > 1.0 || n < 0) return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.PolyLog(n, x);
        }


        public static double diLogarithm(double x)
        {
            if (x > 1.0)
                return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.DiLog((x));
        }

        public static double diLog(double x)
        {
            if (x > 1.0)
                return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.DiLog((x));
        }


        public static double SpencesIntegral(double x)
        {
            if (x < 0.0)
                return (double.NaN);
            return Meta.Numerics.Functions.AdvancedMath.DiLog(1 - (x));
        }


        public static System.Numerics.Complex diLogarithm(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.DiLog(cmplxToMeta(z)));
        }

        public static System.Numerics.Complex diLog(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.DiLog(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex SpencesIntegral(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.DiLog(1 - cmplxToMeta(z)));
        }

        #endregion

        #region Wave functions

        public static double CoulombG(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CoulombG(L, η, ρ);
        }


        public static double CoulombF(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CoulombF(L, η, ρ);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_CL_e(double L, double eta, out gsl_sf_result result);


        public static double CoulombC(int L, double η)
        {
            if (L < 0) return double.NaN;
            gsl_sf_coulomb_CL_e(L, η, out sfResult);
            return sfResult.val;
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hydrogenicR(int n, int l, double Z, double r);

        public static double HydrogenicR(int n, int l, double Z, double r)
        {
            if (n < 1 || l > n - 1 || Z <= 0.0 || r < 0.0)
            {
                return double.NaN;
            }
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }

        public static double Rnl(int n, int l, double Z, double r)
        {
            if (n < 1 || l > n - 1 || Z <= 0.0 || r < 0.0)
            {
                return double.NaN;
            }
            return gsl_sf_hydrogenicR(n, l, Z, r);
        }


        public static double CoulombW(int L, double η, double ρ)
        {
            if (L < 0 || ρ < 0.0) return double.NaN;
            return CoulombC(1, η)*CoulombF(L, η, ρ) + CoulombC(2, η)*CoulombG(L, η, ρ);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int gsl_sf_coulomb_wave_FG_e(double eta, double x,
            double lam_F,
            int k_lam_G,
            out gsl_sf_result F, out gsl_sf_result Fp,
            out gsl_sf_result G, out gsl_sf_result Gp,
            out double exp_F, out double exp_G);


        public static double CoulombGprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ <= 0.0) return double.NaN;


            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out r2, out r3, out sfResult, out d1, out d2);
            return sfResult.val;
        }


        public static double CoulombFprime(int L, double η, double ρ)
        {
            if (L < 0 || ρ <= 0.0) return double.NaN;
            double d1, d2;
            var r1 = new gsl_sf_result();
            var r2 = new gsl_sf_result();
            var r3 = new gsl_sf_result();
            gsl_sf_coulomb_wave_FG_e(η, ρ, L, L, out r1, out sfResult, out r3, out r2, out d1, out d2);
            return sfResult.val;
        }

        #endregion

        #region Fermi–Dirac complete&incomplete integral

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_int(int j, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_inc_0(double x, double b);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_0(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_mhalf(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_fermi_dirac_half(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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
            if (b < 0.0)
            {
                return double.NaN;
            }
            return gsl_sf_fermi_dirac_inc_0(x, b);
        }

        public static double FermiDiracFj(int j, double x)
        {
            return gsl_sf_fermi_dirac_int(j, x);
        }

        #endregion

        #region lambert W functions

        public static double LambertW0(double x)
        {
            return (x <= -1/System.Math.E) ? double.NaN : gsl_sf_lambert_W0(x);
        }

        public static double LambertWm1(double x)
        {
            return (x <= -1/System.Math.E) ? double.NaN : gsl_sf_lambert_Wm1(x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_W0(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_lambert_Wm1(double x);

        #endregion

        #region polynomials

        public static double Gegenbauer1(double α, double x)
        {
            return gsl_sf_gegenpoly_1(α, x);
        }

        public static double
            Gegenbauer2(double α, double x)
        {
            return gsl_sf_gegenpoly_2(α, x);
        }

        public static double
            Gegenbauer3(double α, double x)
        {
            return gsl_sf_gegenpoly_3(α, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_n(int n, double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_1(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_2(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_gegenpoly_3(double lambda, double x);


        public static double Gegenbauer(int n, double α, double x)
        {
            if (α <= -0.5 || n < 0) return double.NaN;
            return gsl_sf_gegenpoly_n(n, α, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_n(int n, double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_1(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_laguerre_2(double a, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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
            if (l < 0) return double.NaN;
            if (System.Math.Abs(x) > 1.0) return double.NaN;
            return Meta.Numerics.Functions.OrthogonalPolynomials.LegendreP(l, x);
        }

        public static double LegendreP(int l, int m, double x)
        {
            if (l < 0) return double.NaN;
            if (System.Math.Abs(m) > l) return double.NaN;
            if (System.Math.Abs(x) > 1.0) return double.NaN;
            return Meta.Numerics.Functions.OrthogonalPolynomials.LegendreP(l, m, x);
        }

        //add legendre Q


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_Ql(int l, double x);

        // Q_l(x), x > -1, x != 1, l >= 0
        public static double LegendreQ(int l, double x)
        {
            if (x == 1.0 || x <= -1.0 || l < 0) return double.NaN;
            return gsl_sf_legendre_Ql(l, x);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_sphPlm(int l, int m, double x);

        public static double SphericalLegendreP(int l, int m, double x)
        {
            if (m < 0 || l < m || x < -1.0 || x > 1.0)
            {
                return double.NaN;
            }

            return gsl_sf_legendre_sphPlm(l, m, x);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_half(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_mhalf(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_0(double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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


        public static double ConicalP(int μ, double λ, double x)
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


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_conicalP_sph_reg(int l, double lambda, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_legendre_H3d(int l, double lambda, double eta);

        public static double LegendreH3D(int l, double λ, double η)
        {
            if (η < 0 || l < 0 || (l >= 2 && λ == 0.0)) return double.NaN;
            return gsl_sf_legendre_H3d(l, λ, η);
        }

        public static double ChebyshevT(int n, double x)
        {
            if (System.Math.Abs(x) > 1.0) return double.NaN;
            if (n < 0) return double.NaN;

            return Meta.Numerics.Functions.OrthogonalPolynomials.ChebyshevT(n, x);
        }

        //add ChebyshevU

        public static double HermiteH(int n, double x)
        {
            if (n < 0)
            {
                return double.NaN;
            }
            return Meta.Numerics.Functions.OrthogonalPolynomials.HermiteH(n, x);
        }

        public static double HermiteHe(int n, double x)
        {
            if (n < 0)
            {
                return double.NaN;
            }
            return Meta.Numerics.Functions.OrthogonalPolynomials.HermiteHe(n, x);
        }

        public static double ZernikeR(int n, int m, double ρ)
        {
            if (n < 0) return double.NaN;
            if ((m < 0) || (m > n)) return double.NaN;
            if ((ρ < 0.0) || (ρ > 1.0)) return double.NaN;
            return Meta.Numerics.Functions.OrthogonalPolynomials.ZernikeR(n, m, ρ);
        }

        //TODO: add Zernike Z 

        #endregion

        #region transport functions

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_2(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_3(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_transport_4(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        public static double SynchrotronF(double x)
        {
            return (x < 0) ? double.NaN : gsl_sf_synchrotron_1(x);
        }

        public static double SynchrotronG(double x)
        {
            return (x < 0) ? double.NaN : gsl_sf_synchrotron_2(x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_1(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_synchrotron_2(double x);

        #endregion

        #region coupling 3,6,9-j symbols

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_3j(int two_ja, int two_jb, int two_jc,
            int two_ma, int two_mb, int two_mc);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_6j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_RacahW(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_coupling_9j(int two_ja, int two_jb, int two_jc,
            int two_jd, int two_je, int two_jf,
            int two_jg, int two_jh, int two_ji);


        public static double Coupling3j(int ja, int jb, int jc, int ma, int mb, int mc)
        {
            if (ja < 0 || jb < 0 || jc < 0)
                return double.NaN;
            return gsl_sf_coupling_3j(ja, jb, jc, ma, mb, mc);
        }

        public static double Coupling6j(int ja, int jb, int jc, int jd, int je, int jf)
        {
            if (ja < 0 || jb < 0 || jc < 0 || jd < 0 || je < 0 || jf < 0)
                return double.NaN;
            return gsl_sf_coupling_6j(ja, jb, jc, jd, je, jf);
        }

        public static double CouplingRacahW(int ja, int jb, int jc, int jd, int je, int jf)
        {
            if (ja < 0 || jb < 0 || jc < 0 || jd < 0 || je < 0 || jf < 0)
                return double.NaN;
            return gsl_sf_coupling_RacahW(ja, jb, jc, jd, je, jf);
        }

        public static double Coupling9j(int ja, int jb, int jc, int jd, int je, int jf, int jg, int jh, int ji)
        {
            if (ja < 0 || jb < 0 || jc < 0 || jd < 0 || je < 0 || jf < 0 || jg < 0 || jh < 0 || ji < 0)
                return double.NaN;
            return gsl_sf_coupling_9j(ja, jb, jc, jd, je, jf, jg, jh, jf);
        }

        public static double Coupling3j(double j1, double j2, double j3, double m1, double m2, double m3)
        {
            // no negative, spin must be integer or half-integer
            if (j1 < 0 || j2 < 0 || j3 < 0 ||
                System.Math.Floor(2*j1) != 2*j1 || System.Math.Floor(2*j2) != 2*j2 || System.Math.Floor(2*j3) != 2*j3)
                return double.NaN;


            // -J <= M <= J
            if (m1 < -System.Math.Abs(j1) || m1 > System.Math.Abs(j1) ||
                m2 < -System.Math.Abs(j2) || m2 > System.Math.Abs(j2) ||
                m3 < -System.Math.Abs(j3) || m3 > System.Math.Abs(j3))
                return double.NaN;


            // 2M must be an integer
            if (System.Math.Floor(2*m1) != 2*m1 || System.Math.Floor(2*m2) != 2*m2 || System.Math.Floor(2*m3) != 2*m3)
                return double.NaN;


            // half-integer J requires half-integer M; integer J requires integer M
            if (((2*j1)%2) != System.Math.Abs((2*m1)%2) || ((2*j2)%2) != System.Math.Abs((2*m2)%2) ||
                ((2*j3)%2) != System.Math.Abs((2*m3)%2))
                return double.NaN;


            return Meta.Numerics.Functions.SpinMath.ThreeJ(new Meta.Numerics.Functions.SpinState(j1, m1),
                new Meta.Numerics.Functions.SpinState(j2, m2), new Meta.Numerics.Functions.SpinState(j3, m3));
        }

        public static double Coupling6j(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            if (j1 < 0 || j2 < 0 || j3 < 0 || j4 < 0 || j5 < 0 || j6 < 0 ||
                System.Math.Floor(2*j1) != 2*j1 || System.Math.Floor(2*j2) != 2*j2 || System.Math.Floor(2*j3) != 2*j3 ||
                System.Math.Floor(2*j4) != 2*j4 || System.Math.Floor(2*j5) != 2*j5 || System.Math.Floor(2*j6) != 2*j6)
                return double.NaN;

            return Meta.Numerics.Functions.SpinMath.SixJ(new Meta.Numerics.Functions.Spin(j1),
                new Meta.Numerics.Functions.Spin(j2), new Meta.Numerics.Functions.Spin(j3),
                new Meta.Numerics.Functions.Spin(j4), new Meta.Numerics.Functions.Spin(j5),
                new Meta.Numerics.Functions.Spin(j6));
        }

        public static double ClebschGordan(double j1, double j2, double j, double m1, double m2, double m)
        {
            // no negative, spin must be integer or half-integer
            if (j1 < 0 || j2 < 0 || j < 0 ||
                System.Math.Floor(2*j1) != 2*j1 || System.Math.Floor(2*j2) != 2*j2 || System.Math.Floor(2*j) != 2*j)
                return double.NaN;

            // -J <= M <= J
            if (m1 < -System.Math.Abs(j1) || m1 > System.Math.Abs(j1) ||
                m2 < -System.Math.Abs(j2) || m2 > System.Math.Abs(j2) ||
                m < -System.Math.Abs(j) || m > System.Math.Abs(j))
                return double.NaN;


            // 2M must be an integer
            if (System.Math.Floor(2*m1) != 2*m1 || System.Math.Floor(2*m2) != 2*m2 || System.Math.Floor(2*m) != 2*m)
                return double.NaN;


            // half-integer J requires half-integer M; integer J requires integer M
            if (((2*j1)%2) != System.Math.Abs((2*m1)%2) || ((2*j2)%2) != System.Math.Abs((2*m2)%2) ||
                ((2*j)%2) != System.Math.Abs((2*m)%2))
                return double.NaN;

            return Meta.Numerics.Functions.SpinMath.ClebschGodron(new Meta.Numerics.Functions.SpinState(j1, m1),
                new Meta.Numerics.Functions.SpinState(j2, m2), new Meta.Numerics.Functions.SpinState(j, m));
        }

        #endregion


        #region Hypergeometric functions

        public static System.Numerics.Complex SphericalHarmonic(int l, int m, double θ, double φ)
        {
            if (l < 0) return double.NaN;
            if ((m > l) || (m < -l)) return double.NaN;

            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedMath.SphericalHarmonic(l, m, θ, φ));
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_0F1(double c, double x);

        //Hypergeometric function related to Bessel functions 0F1[c,x]

        public static double Hypergeometric0F1(double c, double x)
        {
            return gsl_sf_hyperg_0F1(c, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1_int(int m, int n, double x);

        //Confluent hypergeometric function  for integer parameters. 1F1[m,n,x] = M(m,n,x)

        public static double Hypergeometric1F1(int m, int n, double x)
        {
            return gsl_sf_hyperg_1F1_int(m, n, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_1F1(double a, double b, double x);

        //Confluent hypergeometric function. 1F1[a,b,x] = M(a,b,x)

        public static double Hypergeometric1F1(double a, double b, double x)
        {
            return gsl_sf_hyperg_1F1(a, b, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U_int(int m, int n, double x);

        //Confluent hypergeometric function for integer parameters. U(m,n,x)

         public static double HypergeometricU(int m, int n, double x)
         {
            return gsl_sf_hyperg_U_int(m, n, x);
        }

        //Confluent hypergeometric function. U(a,b,x)
        public static double HypergeometricU(double a, double b, double x)
        {
            return gsl_sf_hyperg_U(a, b, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_U(double a, double b, double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1(double a, double b, double c, double x);

        //Gauss hypergeometric function 2F1[a,b,c,x]

        public static double Hypergeometric2F1(double a, double b, double c, double x)
        {
            return gsl_sf_hyperg_2F1(a, b, c, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj(double aR, double aI, double c, double x);

        //Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x]

        public static double Hypergeometric2F1(System.Numerics.Complex a, double c, double x)
        {
            return gsl_sf_hyperg_2F1_conj(a.Real, a.Imaginary, c, x);
        } //TODO: better name for a parameter

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_renorm(double a, double b, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[a,b,c,x] / Gamma[c]

        public static double Hypergeometric2F1renorm(double a, double b, double c, double x)
        {
            return gsl_sf_hyperg_2F1_renorm(a, b, c, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F1_conj_renorm(double aR, double aI, double c, double x);

        //Renormalized Gauss hypergeometric function 2F1[aR + I aI, aR - I aI, c, x] / Gamma[c]

        public static double Hypergeometric2F1renorm(System.Numerics.Complex a, double c, double x)
        {
            return gsl_sf_hyperg_2F1_conj_renorm(a.Real, a.Imaginary, c, x);
        } //TODO: better name for a parameter

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hyperg_2F0(double a, double b, double x);

        /* Mysterious hypergeometric function. The series representation
         * is a divergent hypergeometric series. However, for x < 0 we
         * have 2F0(a,b,x) = (-1/x)^a U(a,1+a-b,-1/x)*/


        public static double Hypergeometric2F0(double a, double b, double x)
        {
            return gsl_sf_hyperg_2F0(a, b, x);
        }

        #endregion


        #region integral functions

        public static double Ti(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.IntegralTi(x);
        }

        public static double Dawson(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Dawson(x);
        }

        public static double Clausen(double d) { return gsl_sf_clausen(d); }

        public static double Si(double d) { return Meta.Numerics.Functions.AdvancedMath.IntegralSi(d); } //sine integral

        public static double Ci(double x)//cosine integral
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.IntegralCi(x);
        }

        //Generalized Exponential Integral
        public static double En(int n, double x)
        {
            if (x < 0)
            {
                if (n < 0)
                {
                    return double.NaN; //DOMAIN_ERROR(result);
                }
                if (n == 0)
                {
                    if (x == 0)
                    {
                        return double.NaN; //DOMAIN_ERROR(result);
                    }
                    //   result->val = (scale ? 1.0 : exp(-x)) / x;
                    // result->err = 2 * GSL_DBL_EPSILON * fabs(result->val);
                    //CHECK_UNDERFLOW(result);
                    return gsl_sf_expint_En(n, x);//GSL_SUCCESS;
                }
                if (n == 1)
                {
                    return gsl_sf_expint_En(n, x);//expint_E1_impl(x, result, scale);
                }
                if (n == 2)
                {
                    return gsl_sf_expint_En(n, x);//expint_E2_impl(x, result, scale);
                }
                if (x < 0)
                {
                    return double.NaN;//DOMAIN_ERROR(result);
                }
                if (x == 0)
                {
                    // result->val = (scale ? exp(x) : 1) * (1 / (n - 1.0));
                    // result->err = 2 * GSL_DBL_EPSILON * fabs(result->val);
                    //CHECK_UNDERFLOW(result);
                    return gsl_sf_expint_En(n, x);//GSL_SUCCESS;
                }
                //gsl_sf_result result_g;
                //double prefactor = pow(x, n - 1);
                //int status = gsl_sf_gamma_inc_e(1 - n, x, &result_g);
                //double scale_factor = (scale ? exp(x) : 1.0);
                //result->val = scale_factor * prefactor * result_g.val;
                //result->err = 2 * GSL_DBL_EPSILON * fabs(result->val);
                //result->err += 2 * fabs(scale_factor * prefactor * result_g.err);
                //if (status == GSL_SUCCESS) CHECK_UNDERFLOW(result);
                return gsl_sf_expint_En(n, x);//status;
            }
            return Meta.Numerics.Functions.AdvancedMath.IntegralE(n, x);
        }




        //   public static double EnNEW(int n, double x)//Generalized Exponential Integral
        //  {
        //     return MathNet.Numerics.SpecialFunctions.ExponentialIntegral(x, n);
        //     return 1.0;
        //}

        public static double Ei(double x)
            {
                return (x == 0.0) ? Meta.Numerics.Functions.AdvancedMath.IntegralEi(x) : gsl_sf_expint_Ei(x);
            }

        public static double Shi(double d) { return gsl_sf_Shi(d); } //hyperbolic sine integral




        //hyperbolic cosine integral

        public static double Chi(double x)
        {
            return (x == 0.0) ? double.NaN : gsl_sf_Chi(x);
        }

        public static double Tai(double d) { return gsl_sf_atanint(d); } //arcus tangent integral


        //Integrals in optics
        public static System.Numerics.Complex Fresnel(double x)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedMath.Fresnel(x));
        }

        public static double FresnelS(double x)
        {
           return Meta.Numerics.Functions.AdvancedMath.FresnelS(x);
        }

        public static double FresnelC(double x)
        {
           return Meta.Numerics.Functions.AdvancedMath.FresnelC(x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_1(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_2(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_3(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_4(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_debye_5(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_clausen(double x);


        public static System.Numerics.Complex Ein(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Ein(cmplxToMeta(z)));
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_En(int n, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_expint_Ei(double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_Shi(double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_Chi(double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_atanint(double x);

        #endregion

        #region Elliptic integrals

        //In mathematics, the Carlson symmetric forms of elliptic integrals are a small canonical set of elliptic integrals to which all others may be reduced. They are a modern alternative to the Legendre forms. The Legendre forms may be expressed in terms of the Carlson forms and vice versa.
        public static double EllipticK(double x)
        {
            return (x < 0 || x >= 1.0) ? ((x < 0 && x > -1.0) ? gsl_sf_ellint_Kcomp(x, 0) : double.NaN) : Meta.Numerics.Functions.AdvancedMath.EllipticK(x);
        }


        public static double CarlsonD(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CarlsonD(x, y, z);
        }


        public static double CarlsonF(double x, double y, double z)
        {
            if (x <= 0 || y <= 0 || z <= 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.CarlsonF(x, y, z);
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
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_F(φ, x, 0);
            return Meta.Numerics.Functions.AdvancedMath.EllipticF(φ, x);
        }

        public static double EllipticE(double x)
        {
            if (x < 0 || x > 1.0)
                if (x < 0 && x > -1.0)
                    return gsl_sf_ellint_Ecomp(x, 0);
                else
                    return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.EllipticE(x);
        }

        public static double EllipticE(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0)
                return double.NaN;

            if (x < 0.0)
                return gsl_sf_ellint_E(φ, x, 0);
            return Meta.Numerics.Functions.AdvancedMath.EllipticE(φ, x);
        }

        public static double EllipticΠ(double k, double n)
        {
            if (k*k >= 1.0 || n <= -1.0)
                return double.NaN;
            return gsl_sf_ellint_Pcomp(k, n, 0);
        }

        public static double EllipticΠ(double φ, double x, int n)
        {
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0 || n <= -2)
                //TODO: try to relax on these conditions
                return double.NaN;
            return gsl_sf_ellint_P(φ, x, n, 0);
        }



        public static double EllipticD(double φ, double x)
        {
            if (x < -1.0 || x > 1.0 || φ < -System.Math.PI/2.0 || φ > System.Math.PI/2.0)
                return double.NaN;
            return gsl_sf_ellint_D(φ, x, 0);
        }


        public static double EllipticD(double x)
        {
            if (x <= -1 || x >= 1.0)
                return double.NaN;

            return gsl_sf_ellint_Dcomp(x, 0);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Ecomp(double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Kcomp(double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Pcomp(double k, double n, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_D(double phi, double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_F(double phi, double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_E(double phi, double k, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_P(double phi, double k, double n, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_Dcomp(double k, uint mode);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RC(double x, double y, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RD(double x, double y, double z, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RF(double x, double y, double z, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_ellint_RJ(double x, double y, double z, double p, uint mode);

        #endregion

        #region  Jacobian elliptic functions

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        public static System.Numerics.Complex erfi(System.Numerics.Complex z)
        {
            return -System.Numerics.Complex.ImaginaryOne*erf(System.Numerics.Complex.ImaginaryOne*z);
        }

        public static double inverseErf(double x)
        {
            if (x > 1 || x < 0)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.InverseErf(x);
        }
        public static double inverseErfc(double x)
        {
            if (x > 1 || x<0)
                return double.NaN;
            return
                Meta.Numerics.Functions.AdvancedMath.InverseErfc(x);
        }

        public static double logErfc(double x)
        {
            return gsl_sf_log_erfc(x);
        }

        public static double erfZ(double x)
        {
            return gsl_sf_erf_Z(x);
        }

        public static double erfQ(double x)
        {
            return 
            gsl_sf_erf_Q(x);
        }

        public static double hazard(double x)
        {
            return 
            gsl_sf_hazard(x);
        }

        public static double OwenT(double h, double a)
        {
            return Accord.Math.OwensT.Function(h, a);
        }

        public static double OwenT(double h, double a, double ah)
        {
            return Accord.Math.OwensT.Function(h, a, ah);
        }

        //checked and they work ok

        public static double erf(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Erf((x));
        }


        public static double erfc(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.Erfc((x));
        }


        public static double erfcx(double x)
        {
            return ((erfc(x))/System.Math.Exp(-((x))*(x)));
        }


        public static System.Numerics.Complex erf(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Erf(cmplxToMeta(z)));
        }


        public static System.Numerics.Complex erfc(System.Numerics.Complex z)
        {
            return (1 - cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Erf(cmplxToMeta(z))));
        }


        public static System.Numerics.Complex erfcx(System.Numerics.Complex z)
        {
            return ((erfc(z))/System.Numerics.Complex.Exp(-((z))*(z)));
        }


        public static System.Numerics.Complex faddeeva(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.Faddeeva(cmplxToMeta(z)));
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_log_erfc(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Z(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_erf_Q(double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_hazard(double x);

        #endregion

        #region Bessel functions

        //unfortunetelly only for real arguments :( cant find any implementation for complex args
        //TODO: check

        //  Description(""angielski opis funkcji"")]
        //public static Func<double, double, double> BesselJν2 = (ν, x) => gsl_sf_bessel_Jnu(ν, x);

        // Description(""angielski opis funkcji"")]
        // public static Func<double, double, double> BesselYν2 = (ν, x) => gsl_sf_bessel_Ynu(ν, x);

        public static double BesselJ0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(0, x);
        }

        public static double BesselJ1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(1, x);
        }

        public static double BesselJ2(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(2, x);
        }

        public static double BesselJ3(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(3, x);
        }

        public static double BesselJnPrime(int n, double x)
        {
            return 0.5*
                   (Meta.Numerics.Functions.AdvancedMath.BesselJ(n - 1, x) -
                    Meta.Numerics.Functions.AdvancedMath.BesselJ(n + 1, x));
        }


        public static double BesselJn(int n, double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselJ(n, x);
        }

        public static double BesselJν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.BesselJ(ν, x);
        }

        // Description(""angielski opis funkcji"")]
        //public static double BesselJa = BesselJν;

        public static double BesselY0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(0, x);
        }

        public static double BesselY1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(1, x);
        }

        public static double BesselY2(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(2, x);
        }

        public static double BesselY3(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(3, x);
        }

        public static double BesselYn(int n, double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.BesselY(n, x);
        }

        public static double BesselYν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.BesselY(ν, x);
        }

        // Description(""angielski opis funkcji"")]
        //public static double BesselYa = BesselYν;

        public static double ModifiedBesselIn(int n, double x)
        {
            return gsl_sf_bessel_In(n, x);
        }


        public static double ModifiedBesselIν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.ModifiedBesselI(ν, x);
        }

        // Description(""angielski opis funkcji"")]
        //public static double ModifiedBesselIa = ModifiedBesselIν;

        public static double ModifiedBesselKν(double ν, double x)
        {
            return (x < 0.0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.ModifiedBesselK(ν, x);
        }

        public static double ModifiedBesselKn(int n, double x)
        {
            return (x <= 0.0) ? double.NaN : gsl_sf_bessel_Kn(n, x);
        }

        // Description(""angielski opis funkcji"")]
        //public static double ModifiedBesselKa = ModifiedBesselKν;


        public static double SphericalBesselJ0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(0, x);
        }

        public static double SphericalBesselJ1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(1, x);
        }

        public static double SphericalBesselJ2(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(2, x);
        }

        public static double SphericalBesselJ3(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(3, x);
        }

        public static double SphericalBesselJn(int n, double x)
        {
            if (x == 0 && n <= -3)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselJ(n, x);
        }


        public static double SphericalBesselY0(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(0, x);
        }


        public static double SphericalBesselY1(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(1, x);
        }


        public static double SphericalBesselY2(double x)
        {
            return (x == 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(2, x);
        }


        public static double SphericalBesselY3(double x)
        {
            return (x == 0) ? double.NaN : Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(3, x);
        }


        public static double SphericalBesselYn(int n, double x)
        {
            if (x == 0 & n >= 2)
                return double.NaN;
            return Meta.Numerics.Functions.AdvancedMath.SphericalBesselY(n, x);
        }


        public static double ModifiedSphericalBesselIn(int n, double x)
        {
            if (n < 0)
                return double.NaN;
            return gsl_sf_bessel_il_scaled(n, x);
        }


        public static double ModifiedSphericalBesselKn(int n, double x)
        {
            return (x <= 0 || n < 0) ? double.NaN : gsl_sf_bessel_kl_scaled(n, x);
        }

        public static double logBesselKν(double ν, double x)
        {
            return (x <= 0.0 || ν < 0) ? double.NaN : gsl_sf_bessel_lnKnu(ν, x);
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Jnu(double nu, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Ynu(double nu, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_In(int n, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_Kn(int n, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_il_scaled(int l, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_kl_scaled(int l, double x);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_lnKnu(double nu, double x);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_bessel_zero_Jnu(double nu, uint s);

        public static double BesselJνZeros(double ν, double s)
        {
            if (s < 0.5 || ν < 0.0) return double.NaN;
            return gsl_sf_bessel_zero_Jnu(ν, (uint) System.Math.Round(s, System.MidpointRounding.AwayFromZero));
        }

        public static System.Numerics.Complex Hankel1(double α, double x)
        {
            return BesselJν(α, x) + System.Numerics.Complex.ImaginaryOne*BesselYν(α, x);
        }

        public static System.Numerics.Complex Hankel2(double α, double x)
        {
            return BesselJν(α, x) - System.Numerics.Complex.ImaginaryOne*BesselYν(α, x);
        }

        public static System.Numerics.Complex SphericalHankel1(int n, double x)
        {
            return SphericalBesselJn(n, x) + System.Numerics.Complex.ImaginaryOne*SphericalBesselYn(n, x);
        }

        public static System.Numerics.Complex SphericalHankel2(int n, double x)
        {
            return SphericalBesselJn(n, x) - System.Numerics.Complex.ImaginaryOne*SphericalBesselYn(n, x);
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

        public static System.Numerics.Complex RiccatiBesselξ(int n, double x)
        {
            return x*Hankel1(n, x);
        }

        public static System.Numerics.Complex RiccatiBesselζ(int n, double x)
        {
            return x*Hankel2(n, x);
        }

        #endregion

        #region Airy functions

        public static double AiryAi(double x)
        {
              return Meta.Numerics.Functions.AdvancedMath.AiryAi(x); 
        }

        public static double AiryBi(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.AiryBi(x);
        }


        public static double Ai(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.AiryAi(x);
        }

        public static double Bi(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.AiryBi(x);
        }

        public static double AiPrime(double x)
        {
            return gsl_sf_airy_Ai_deriv(x,0);
        }


        public static double BiPrime(double x)
        {
            return gsl_sf_airy_Bi_deriv(x, 0);
        }


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Ai_deriv(double x, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_Bi_deriv(double x, uint mode);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai(uint s);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi(uint s);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Ai_deriv(uint s);

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_airy_zero_Bi_deriv(uint s);

        //AiryZetaAi //ZerosOfAi// ZerosOfBi
        public static double AiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        public static double BiZeros(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        public static double AiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Ai_deriv((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        public static double BiZerosPrime(double x)
        {
            if (x < 0.5) return double.NaN;
            return gsl_sf_airy_zero_Bi_deriv((uint) System.Math.Round(x, System.MidpointRounding.AwayFromZero));
        }

        #endregion

        #region zeta and eta functions

        public static double DirichletEta(double x)
        {
            return x >= 0 ? Meta.Numerics.Functions.AdvancedMath.DirichletEta(x) : (1 - System.Math.Pow(2, 1 - x))*RiemannZeta(x);
        }

        public static double η(double x)
        {
            return DirichletEta(x);
        }


        public static double RiemannZeta(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.RiemannZeta(x);
        }

        public static double Riemannζ(double x)
        {
            return Meta.Numerics.Functions.AdvancedMath.RiemannZeta(x);
        }

        public static System.Numerics.Complex Riemannζ(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        public static System.Numerics.Complex RiemannZeta(System.Numerics.Complex z)
        {
            return cmplxFromMeta(Meta.Numerics.Functions.AdvancedComplexMath.RiemannZeta(cmplxToMeta(z)));
        }

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
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

        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_a(int order, double qq);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
    CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_b(int order, double qq);



        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_ce(int order, double qq, double zz);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_se(int order, double qq, double zz);



        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_Mc(int kind, int order, double qq, double zz);


        [System.Runtime.InteropServices.DllImport(Computator.NET.DataTypes.GslConfig.GslDllName,
CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern double gsl_sf_mathieu_Ms(int kind, int order, double qq, double zz);


        public static double MathieuSE(int n, double q, double x)
        {
            return gsl_sf_mathieu_se(n, q, x);
        }

        public static double MathieuCE(int n, double q, double x)
        {
            return gsl_sf_mathieu_ce(n, q, x);
        }

        public static double MathieuAn(int n, double q)
        {
            return gsl_sf_mathieu_a(n, q);
        }

        public static double MathieuBn(int n, double q)
        {
            if (n == 0)
                return double.NaN;
            return gsl_sf_mathieu_b(n, q);
        }

        public static double MathieuMc(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            return gsl_sf_mathieu_Mc(j,n, q,x);
        }

        public static double MathieuMs(int j, int n, double q, double x)
        {
            if (q <= 0 || j > 2 || j < 1)
                return double.NaN;
            return gsl_sf_mathieu_Ms(j, n, q, x);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 16),
         System.Serializable]
        private struct gsl_sf_result
        {
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.R8)] public readonly
                double val;

            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.R8)] public readonly
                double err;
        }

        #endregion


        #region logistic functions

        public static double Logistic(double x)
        {
            return MathNet.Numerics.SpecialFunctions.Logistic(x);
        }

        public static double Logit(double x)
        {
            return (x < 0 || x > 1) ? double.NaN : MathNet.Numerics.SpecialFunctions.Logit(x);
        }


        public static double StruveL1(double x)
        {
            return MathNet.Numerics.SpecialFunctions.StruveL1(x);
        }

        public static double StruveL0(double x)
        {
            return MathNet.Numerics.SpecialFunctions.StruveL0(x);
        }

        #endregion

        #region utils

        private static gsl_sf_result sfResult;

        private static System.Exception gslExceptions(int error_code)
        {
            switch (error_code)
            {
                case -1:
                    return new System.Exception(""general failure"");
                case -2:
                    return new System.Exception(""iteration has not converged"");
                case 1:
                    return new System.Exception(""input domain error: e.g sqrt(-1)"");
                case 2:
                    return new System.Exception(""output range error: e.g. exp(1e100)"");
                case 3:
                    return new System.Exception(""invalid pointer"");
                case 4:
                    return new System.Exception(""invalid argument supplied by user"");
                case 5:
                    return new System.Exception(""generic failure"");
                case 6:
                    return new System.Exception(""factorization failed"");
                case 7:
                    return new System.Exception(""sanity check failed - shouldn't happen"");
                case 8:
                    return new System.Exception(""malloc failed"");
                case 9:
                    return new System.Exception(""problem with user-supplied function"");
                case 10:
                    return new System.Exception(""iterative process is out of control"");
                case 11:
                    return new System.Exception(""exceeded max number of iterations"");
                case 12:
                    return new System.Exception(""tried to divide by zero"");
                case 13:
                    return new System.Exception(""user specified an invalid tolerance"");
                case 14:
                    return new System.Exception(""failed to reach the specified tolerance"");
                case 15:
                    return new System.Exception(""underflow"");
                case 16:
                    return new System.Exception(""overflow "");
                case 17:
                    return new System.Exception(""loss of accuracy"");
                case 18:
                    return new System.Exception(""failed because of roundoff error"");
                case 19:
                    return new System.Exception(""matrix: vector lengths are not conformant"");
                case 20:
                    return new System.Exception(""matrix not square"");
                case 21:
                    return new System.Exception(""apparent singularity detected"");
                case 22:
                    return new System.Exception(""integral or series is divergent"");
                case 23:
                    return new System.Exception(""requested feature is not supported by the hardware"");
                case 24:
                    return new System.Exception(""requested feature not (yet) implemented"");
                case 25:
                    return new System.Exception(""cache limit exceeded"");
                case 26:
                    return new System.Exception(""table limit exceeded"");
                case 27:
                    return new System.Exception(""iteration is not making progress towards solution"");
                case 28:
                    return new System.Exception(""jacobian evaluations are not improving the solution"");
                case 29:
                    return new System.Exception(""cannot reach the specified tolerance in F"");
                case 30:
                    return new System.Exception(""cannot reach the specified tolerance in X"");
                case 31:
                    return new System.Exception(""cannot reach the specified tolerance in gradient"");
                case 32:
                    return new System.Exception(""end of file"");
                default:
                    return new System.Exception(""unknown exception"");
            }
        }

        private static System.Numerics.Complex cmplxFromMeta(Meta.Numerics.Complex c)
        {
            return new System.Numerics.Complex(c.Re, c.Im);
        }

        private static Meta.Numerics.Complex cmplxToMeta(System.Numerics.Complex c)
        {
            return new Meta.Numerics.Complex(c.Real, c.Imaginary);
        }
        #endregion

";

        #endregion
    }
}