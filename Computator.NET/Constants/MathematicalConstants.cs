// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation
namespace Computator.NET.Constants
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    internal static class MathematicalConstants
    {
        #region Complex math constant

        public static readonly System.Numerics.Complex PowerTowerOfI = 0.438282936727032111626975 +
                                                                       i*0.360592471871385485952940;

        public static System.Numerics.Complex i
        {
            get { return System.Numerics.Complex.ImaginaryOne; }
        }

        #endregion

        #region Basic math constants 

        public static readonly double PI = System.Math.PI;


        public static readonly double e = System.Math.E;


        public static readonly double EulerMascheroni = 0.57721566490153286060651209008240243104215933593992;


        public static readonly double GoldenRatio = (1 + System.Math.Sqrt(5))/2.0;

        public static readonly double InverseGoldenRatio = 1/GoldenRatio;

        public static readonly double SilverRatio = 1 + System.Math.Sqrt(2);

        #endregion

        #region Classical, named math constants 

        public static readonly double AperysConstant = 1.202056903159594285399738161511449990764986292;

        public static readonly double ConwaysConstant = 1.30357;

        public static readonly double KhinchinsConstant = 2.6854520010;

        public static readonly double ApéryConstantζ3 = 1.202056903159594285399738;
        public static readonly double ArtinConstant = 0.373955813619202288054728;
        public static readonly double BernsteinConstantβ = 0.280169499023869133036436;
        public static readonly double BlazysConstant = 2.566543832171388844467529;
        public static readonly double BrunConstantforTwinPrimesB4 = 1.902160583104;
        public static readonly double BrunConstantforPrimeCousinsB4 = 1.1970449;
        public static readonly double BrunConstantforPrimeQuadruplesB4 = 0.870588380;
        public static readonly double CahenConstantC = 0.643410546288338026182254;
        public static readonly double CatalanConstantC = 0.915965594177219015054603;
        public static readonly double ChampernowneConstantC10 = 0.123456789101112131415;
        public static readonly double ContinuedFractionsConstant = 1.030640834100712935881776;
        public static readonly double ConwayConstantλ3 = 1.303577269034296391257099;
        public static readonly double DelianConstant = 1.259921049894873164767210;
        public static readonly double EmbreeTrefethenConstantβ = 0.70258;
        public static readonly double ErdösBorweinConstant = 1.606695152415291763783301;
        public static readonly double FeigenbaumReductionParameterα = -2.502907875095892822283902;
        public static readonly double FeigenbaumBifurcationVelocityδ = 4.669201609102990671853203;
        public static readonly double FransénRobinsonConstant = 2.807770242028519365221501;
        public static readonly double GaussConstantG = 0.834626841674073186814297;
        public static readonly double GaussKuzminWirsingConstantλ1 = 0.303663002898732658597448;
        public static readonly double GelfondConstant = 23.140692632779269005729086;
        public static readonly double GelfondSchneiderConstant = 2.665144142690225188650297;
        public static readonly double GerverMovingSofaConstant = 2.21953166887197;
        public static readonly double HammersleyUpperBoundOnGerverConstant = 2.207416099162477962306856;
        public static readonly double GibbsConstantG = 1.851937051982466170361053;
        public static readonly double WilbrahamGibbsConstantG = 1.178979744472167270232028;
        public static readonly double GlaisherKinkelinConstantA = 1.282427129100622636875342;
        public static readonly double GolombDickmanConstantλ = 0.624329988543550870992936;
        public static readonly double GompertzConstantG = 0.596347362323194074341078;
        public static readonly double GrossmannConstant = 0.73733830336929;
        public static readonly double KhinchinConstantK_K0 = 2.685452001065306445309714;
        public static readonly double KhinchinLévyConstantγ = 3.275822918721811159787681;
        public static readonly double KnuthRandomgeneratorsConstant = 0.211324865405187117745425;
        public static readonly double KolakoskiConstantγ = 0.794507192779479276240362;
        public static readonly double LandauRamanujanConstant = 0.764223653589220662990698;
        public static readonly double LaplaceLimitConstantλ = 0.662743419349181580974742;
        public static readonly double LemniscateConstantL = 2.622057554292119810464839;
        public static readonly double FirstLemniscateConstantLA = 1.311028777146059905232419;
        public static readonly double SecondLemniscateConstantLB = 0.599070117367796103337484;
        public static readonly double LévyConstantγ = 1.186569110415625452821722;
        public static readonly double LiouvilleConstant = 0.110001000000000000000001;
        public static readonly double LochConstant = 0.970270114392033925740256;
        public static readonly double MadelungConstantM3 = -1.747564594633182190636212;
        public static readonly double MeisselMertensConstantB1 = 0.261497212847642783755426;
        public static readonly double MillsConstantA = 1.306377883863080690468614;
        public static readonly double MKBconstant = 0.6876523689276943698093;
        public static readonly double MRBconstant = 0.187859642462067120248517;
        public static readonly double OmegaConstantW1 = 0.567143290409783872999968;
        public static readonly double OtterConstantα = 2.955765285651994974714817;
        public static readonly double OtterAsymptoticConstantβuForUnrootedTrees = 0.5349496061;
        public static readonly double OtterAsymptoticConstantβrForRootedTrees = 0.439924012571;
        public static readonly double PlasticNumberρ = 1.324717957244746025960908;
        public static readonly double SilverConstant = 1.324717957244746025960908;
        public static readonly double PogsonRatio = 2.511886431509580111085032;
        public static readonly double PolyaRandomwalkConstantp3 = 0.340537329550999142826273;
        public static readonly double PrévostConstant = 3.359885666243177553172011;
        public static readonly double ReciprocalEvenFibonacciConstant = 1.535370508836252985029852;
        public static readonly double ReciprocalOddFibonacciConstant = 1.824515157406924568142158;
        public static readonly double RényiParkingConstantm = 0.747597920253411435178730;
        public static readonly double SalemNumberσ1 = 1.176280818259917506544070;
        public static readonly double SierpinskiConstantK = 2.584981759579253217065893;
        public static readonly double SoldnerConstantμ = 1.451369234883381050283968;
        public static readonly double SomosQuadraticRecurrenceConstant = 1.661687949633594121295818;
        public static readonly double ShallWilsonOrTwinPrimesConstantC2 = 0.660161815846869573927812;
        public static readonly double TheodorusConstant = 1.732050807568877293527446;
        public static readonly double ViswanathConstant = 1.1319882487943;

        #endregion

        #region Geometry constants

        public static readonly double MagicAngle = 0.955316618124509278163857;
        public static readonly double ComplementaryMagicAngle = System.Math.PI/2 - MagicAngle;
        public static readonly double TetrahedralAngle = 2*MagicAngle;
        public static readonly double ComplementaryTetrahedralAngle = System.Math.PI - TetrahedralAngle;

        public static readonly double GravitoidConstant = 1.240806478802799465254958;
        public static readonly double MinimumAreaOfAConstantWidthFigure = 0.704770923010457972467598;
        public static readonly double MoserWormConstant = 0.232239210;
        public static readonly double SquareDrillConstant = 0.987700390736053460131999;
        public static readonly double UniversalParabolicConstant = 2.295587149392638074034298;

        #endregion


        #region utils
        public static void parseConstantsFromNIST()
        {
            var sr = new System.IO.StreamReader("math_const_input.txt");
            var line = "";
            var output = new System.Text.StringBuilder();

            while ((line = sr.ReadLine()) != null)
            {
                var columns = line.Split(new[] { "  ", "\t" }, System.StringSplitOptions.RemoveEmptyEntries);
                var name =
                    ToTitleCase(
                        columns[0].Replace("mag.", "magnetic")
                            .Replace("mom.", "moment")
                            .Replace(@"'s", "")
                            .Replace(@"'", ""))
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace(".", "")
                        .Replace("/", "per")
                        .Replace(",", "_")
                        .Replace("{", "_")
                        .Replace("}", "")
                        .Replace(")", "")
                        .Replace("(", "");
                var value = columns[1].Replace(" ", "").Replace("...", "");

                output.AppendLine(string.Format("public static readonly double {0}={1};", name, value));
            }
            sr.Close();
            var sw = new System.IO.StreamWriter("math_const.txt");
            sw.Write(output);
            sw.Close();
        }

        private static string ToTitleCase(string v)
        {
            var chars = v.ToCharArray();
            for (var i = 0; i < v.Length; i++)
            {
                if (i > 0 && v[i - 1] == ' ')
                    chars[i] = char.ToUpperInvariant(chars[i]);
            }
            return new string(chars);
        }

        public const string ToCode =
            @"
            #region Complex math constant

        public static readonly System.Numerics.Complex PowerTowerOfI = 0.438282936727032111626975 +
                                                                       i*0.360592471871385485952940;

        public static System.Numerics.Complex i
        {
            get { return System.Numerics.Complex.ImaginaryOne; }
        }

        #endregion

        #region Basic math constants 

        public static readonly double PI = System.Math.PI;


        public static readonly double e = System.Math.E;


        public static readonly double EulerMascheroni = 0.57721566490153286060651209008240243104215933593992;


        public static readonly double GoldenRatio = (1 + System.Math.Sqrt(5))/2.0;

        public static readonly double InverseGoldenRatio = 1/GoldenRatio;

        public static readonly double SilverRatio = 1 + System.Math.Sqrt(2);

        #endregion

        #region Classical, named math constants 

        public static readonly double AperysConstant = 1.202056903159594285399738161511449990764986292;

        public static readonly double ConwaysConstant = 1.30357;

        public static readonly double KhinchinsConstant = 2.6854520010;

        public static readonly double ApéryConstantζ3 = 1.202056903159594285399738;
        public static readonly double ArtinConstant = 0.373955813619202288054728;
        public static readonly double BernsteinConstantβ = 0.280169499023869133036436;
        public static readonly double BlazysConstant = 2.566543832171388844467529;
        public static readonly double BrunConstantforTwinPrimesB4 = 1.902160583104;
        public static readonly double BrunConstantforPrimeCousinsB4 = 1.1970449;
        public static readonly double BrunConstantforPrimeQuadruplesB4 = 0.870588380;
        public static readonly double CahenConstantC = 0.643410546288338026182254;
        public static readonly double CatalanConstantC = 0.915965594177219015054603;
        public static readonly double ChampernowneConstantC10 = 0.123456789101112131415;
        public static readonly double ContinuedFractionsConstant = 1.030640834100712935881776;
        public static readonly double ConwayConstantλ3 = 1.303577269034296391257099;
        public static readonly double DelianConstant = 1.259921049894873164767210;
        public static readonly double EmbreeTrefethenConstantβ = 0.70258;
        public static readonly double ErdösBorweinConstant = 1.606695152415291763783301;
        public static readonly double FeigenbaumReductionParameterα = -2.502907875095892822283902;
        public static readonly double FeigenbaumBifurcationVelocityδ = 4.669201609102990671853203;
        public static readonly double FransénRobinsonConstant = 2.807770242028519365221501;
        public static readonly double GaussConstantG = 0.834626841674073186814297;
        public static readonly double GaussKuzminWirsingConstantλ1 = 0.303663002898732658597448;
        public static readonly double GelfondConstant = 23.140692632779269005729086;
        public static readonly double GelfondSchneiderConstant = 2.665144142690225188650297;
        public static readonly double GerverMovingSofaConstant = 2.21953166887197;
        public static readonly double HammersleyUpperBoundOnGerverConstant = 2.207416099162477962306856;
        public static readonly double GibbsConstantG = 1.851937051982466170361053;
        public static readonly double WilbrahamGibbsConstantG = 1.178979744472167270232028;
        public static readonly double GlaisherKinkelinConstantA = 1.282427129100622636875342;
        public static readonly double GolombDickmanConstantλ = 0.624329988543550870992936;
        public static readonly double GompertzConstantG = 0.596347362323194074341078;
        public static readonly double GrossmannConstant = 0.73733830336929;
        public static readonly double KhinchinConstantK_K0 = 2.685452001065306445309714;
        public static readonly double KhinchinLévyConstantγ = 3.275822918721811159787681;
        public static readonly double KnuthRandomgeneratorsConstant = 0.211324865405187117745425;
        public static readonly double KolakoskiConstantγ = 0.794507192779479276240362;
        public static readonly double LandauRamanujanConstant = 0.764223653589220662990698;
        public static readonly double LaplaceLimitConstantλ = 0.662743419349181580974742;
        public static readonly double LemniscateConstantL = 2.622057554292119810464839;
        public static readonly double FirstLemniscateConstantLA = 1.311028777146059905232419;
        public static readonly double SecondLemniscateConstantLB = 0.599070117367796103337484;
        public static readonly double LévyConstantγ = 1.186569110415625452821722;
        public static readonly double LiouvilleConstant = 0.110001000000000000000001;
        public static readonly double LochConstant = 0.970270114392033925740256;
        public static readonly double MadelungConstantM3 = -1.747564594633182190636212;
        public static readonly double MeisselMertensConstantB1 = 0.261497212847642783755426;
        public static readonly double MillsConstantA = 1.306377883863080690468614;
        public static readonly double MKBconstant = 0.6876523689276943698093;
        public static readonly double MRBconstant = 0.187859642462067120248517;
        public static readonly double OmegaConstantW1 = 0.567143290409783872999968;
        public static readonly double OtterConstantα = 2.955765285651994974714817;
        public static readonly double OtterAsymptoticConstantβuForUnrootedTrees = 0.5349496061;
        public static readonly double OtterAsymptoticConstantβrForRootedTrees = 0.439924012571;
        public static readonly double PlasticNumberρ = 1.324717957244746025960908;
        public static readonly double SilverConstant = 1.324717957244746025960908;
        public static readonly double PogsonRatio = 2.511886431509580111085032;
        public static readonly double PolyaRandomwalkConstantp3 = 0.340537329550999142826273;
        public static readonly double PrévostConstant = 3.359885666243177553172011;
        public static readonly double ReciprocalEvenFibonacciConstant = 1.535370508836252985029852;
        public static readonly double ReciprocalOddFibonacciConstant = 1.824515157406924568142158;
        public static readonly double RényiParkingConstantm = 0.747597920253411435178730;
        public static readonly double SalemNumberσ1 = 1.176280818259917506544070;
        public static readonly double SierpinskiConstantK = 2.584981759579253217065893;
        public static readonly double SoldnerConstantμ = 1.451369234883381050283968;
        public static readonly double SomosQuadraticRecurrenceConstant = 1.661687949633594121295818;
        public static readonly double ShallWilsonOrTwinPrimesConstantC2 = 0.660161815846869573927812;
        public static readonly double TheodorusConstant = 1.732050807568877293527446;
        public static readonly double ViswanathConstant = 1.1319882487943;

        #endregion

        #region Geometry constants

        public static readonly double MagicAngle = 0.955316618124509278163857;
        public static readonly double ComplementaryMagicAngle = System.Math.PI/2 - MagicAngle;
        public static readonly double TetrahedralAngle = 2*MagicAngle;
        public static readonly double ComplementaryTetrahedralAngle = System.Math.PI - TetrahedralAngle;

        public static readonly double GravitoidConstant = 1.240806478802799465254958;
        public static readonly double MinimumAreaOfAConstantWidthFigure = 0.704770923010457972467598;
        public static readonly double MoserWormConstant = 0.232239210;
        public static readonly double SquareDrillConstant = 0.987700390736053460131999;
        public static readonly double UniversalParabolicConstant = 2.295587149392638074034298;

        #endregion


";


        #endregion
    }
}