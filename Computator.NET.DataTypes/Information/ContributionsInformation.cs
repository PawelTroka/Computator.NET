using Computator.NET.Localization;

namespace Computator.NET.DataTypes
{
    public static class ContributionsInformation
    {
        public static readonly string Betatesters = Strings.betaTesters +
                                                    ":\n - Kordian Czyżewski (kordiancz25@wp.pl)\n - Vojtech Mańkowski (vojtaman@gmail.com)\n - Marcin Piwowarski (marcpiwowarski@gmail.com)";

        public static readonly string Translators = Strings.translators +
                                                    ":\n - Paweł Troka (pawel.troka@outlook.com) - English&Polish versions\n - Vojtech Mańkowski (vojtaman@gmail.com) - Czech version\n - Athena Hristanas (athena@fizyka.dk) - Deutsch version";

        public static readonly string Libraries = Strings.librariesUsed +
                                                  ":\n - Meta.Numerics v3.1.0 | © David Wright | Microsoft Public License (Ms-PL)\n - GNU Scientific Library v2.3 | GNU General Public License (GNU GPL)\n - Math.NET Numerics v3.19.0 | © Math.NET Team | The MIT License (MIT)\n - Autocomplete Menu rev.35 | © Pavel Torgashov | LGPLv3\n - ScintillaNET v3.6.3 | © Jacob Slusser | The MIT License (MIT)\n - Accord.Math v3.4.0 | © César Roberto de Souza | GNU LGPL v2.1\n - AvalonEdit v5.0.3 | © Daniel Grunwald | The MIT License (MIT)";

        public static readonly string Others = Strings.otherContributors +
                                               ":\n - Jianzhong Zhang (" +
                                               Strings
                                                   .GlobalConfig_others_Chart3D_classes_are_based_on_code_from_High_performance_WPF_3D_Chart_rev_6_application_on +
                                               " Code Project Open License (CPOL) 1.02)\n - Claudio Rocchini (" +
                                               Strings
                                                   .GlobalConfig_others_standard_algorithm_for_complex_domain_coloring +
                                               ")";
    }
}