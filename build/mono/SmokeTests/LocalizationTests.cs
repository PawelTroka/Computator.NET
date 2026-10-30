using System;
using System.IO;
using Computator.NET.Localization;
using Computator.NET.TestSupport;

internal static partial class Program
{
    private static void CompiledUiTranslations(string culture, string regionalCulture)
    {
        var count = LocalizationResourceContract.Verify(typeof(Strings).Assembly,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization"), culture, regionalCulture);
        Console.WriteLine("Verified {0} compiled {1} UI strings and {2} regional fallbacks.", count, culture, regionalCulture);
    }
}
