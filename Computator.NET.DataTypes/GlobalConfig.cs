using System.IO;
using System.Windows.Forms;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.DataTypes
{
    public static class GlobalConfig
    {
        public const string AppName = "Computator.NET";
        public const string version = "v2.1.0ß";
        public const string authorWithEmail = author + " (pawel.troka@outlook.com)";
        public const string author = "Paweł Troka";
        //
        public const string tslFilesFIlter =
            @"Troka Scripting Language(*.tsl)|*.tsl|Troka Scripting Language Functions(*.tslf)|*.tslf";

        public const string gslDllName = "gsl.dll"; //"libgsl-0.dll";
        public const string gslCblasDllName = "cblas.dll"; //"libgslcblas-0.dll";

        public static readonly string betatesters = Strings.betaTesters +
                                                    ":\n - Kordian Czyżewski (kordiancz25@wp.pl)\n - Vojtech Mańkowski (vojtaman@gmail.com)\n - Marcin Piwowarski (marcpiwowarski@gmail.com)";

        public static readonly string translators = Strings.translators +
                                                    ":\n - Paweł Troka (pawel.troka@outlook.com) - English&Polish versions\n - Vojtech Mańkowski (vojtaman@gmail.com) - Czech version\n - Athena Hristanas (athena@fizyka.dk) - Deutsch version";

        public static readonly string libraries = Strings.librariesUsed +
                                                  ":\n - Meta.Numerics v3.0.0 | © David Wright | Microsoft Public License (Ms-PL)\n - GNU Scientific Library v2.1 | GNU General Public License (GNU GPL)\n - Math.NET Numerics v3.11.1 | © Math.NET Team | The MIT License (MIT)\n - Autocomplete Menu rev.35 | © Pavel Torgashov | LGPLv3\n - ScintillaNET v3.5.6 | © Garrett Serack | The MIT License (MIT)\n - Accord.Math v3.0.0 | © César Roberto de Souza | GNU LGPL v2.1\n - AvalonEdit v5.0.3 | © Daniel Grunwald | The MIT License (MIT)";

        public static readonly string others = Strings.otherContributors +
                                               ":\n - Jianzhong Zhang (" +
                                               Strings
                                                   .GlobalConfig_others_Chart3D_classes_are_based_on_code_from_High_performance_WPF_3D_Chart_rev_6_application_on +
                                               " Code Project Open License (CPOL) 1.02)\n - Claudio Rocchini (" +
                                               Strings
                                                   .GlobalConfig_others_standard_algorithm_for_complex_domain_coloring +
                                               ")";


        public static readonly string issuesUrl = "https://github.com/PawelTroka/Computator.NET/issues";

        // public static readonly string features = Strings.featuresInclude;

        //public static readonly string assemblyDescription = Strings.GlobalConfig_assemblyDescription_Computator_NET_is_a_special_kind_of_numerical_software_that_is_fast_and_easy_to_use_but_not_worse_than_others_feature_wise_;


        //public static readonly string basePath =
        //    Path.GetDirectoryName(Application.ExecutablePath) +
        //   Path.DirectorySeparatorChar;

        //  public static readonly string gslLibPath = (Environment.Is64BitOperatingSystem)
        //     ? FullPath("Special", "x64", gslDllName)
        //      : FullPath("Special", "x86", gslDllName);

        // public static readonly string gslBlasPath = (Environment.Is64BitOperatingSystem)
        //     ? FullPath("Special", "x64", gslCblasDllName)
        //    : FullPath("Special", "x86", gslCblasDllName);

        //public static readonly FunctionsDetails functionsDetails = new FunctionsDetails();

        public static string FullPath(params string[] foldersAndFile)
        {
            return Path.GetDirectoryName(Application.ExecutablePath) +
                   Path.DirectorySeparatorChar +
                   Path.Combine(foldersAndFile);
        }
    }
}