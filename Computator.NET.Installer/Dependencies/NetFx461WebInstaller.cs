using WixSharp;
using WixSharp.Bootstrapper;

namespace Computator.NET.Installer
{
    /// <summary>
    /// Microsoft .NET Framework 4.6.1 (Web Installer)
    /// The Microsoft .NET Framework 4.6.1 is a highly compatible, in-place update to the Microsoft .NET Framework 4, Microsoft .NET Framework 4.5, Microsoft .NET Framework 4.5.1, Microsoft .NET Framework 4.5.2 and Microsoft .NET Framework 4.6.
    /// The web installer is a small package that automatically determines and downloads only the components applicable for a particular platform.
    /// https://www.microsoft.com/en-us/download/details.aspx?id=49981
    /// https://download.microsoft.com/download/3/5/9/35980F81-60F4-4DE3-88FC-8F962B97253B/NDP461-KB3102438-Web.exe
    /// based on: https://github.com/wixtoolset/wix3/blob/develop/src/ext/NetFxExtension/wixlib/NetFx461.wxs
    /// </summary>
    class NetFx461WebInstaller
    {
        public ExePackage Build(Bundle bootstrapper)
        {
            const string nameForNetFx45Release = "NETFRAMEWORK45";

            bootstrapper.IncludeWixExtension(WixExtension.Util);

            var netFx40RelaseRegistrySearch = new UtilRegistrySearch
            {
                Root = RegistryHive.LocalMachine,
                Key = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full",
                Value = "Release",
                Result = SearchResult.value,
                Format = SearchFormat.raw,
                Variable = nameForNetFx45Release,
            };

            bootstrapper.AddWixFragment("Wix/Bundle", netFx40RelaseRegistrySearch);

            var isInstalledCondition = $"{nameForNetFx45Release} >= 394254";

            var package = new ExePackage(@"..\redist\NDP461-KB3102438-Web.exe")
            {
                Name = "Microsoft .NET Framework 4.6.1",
                Description = "Microsoft .NET Framework 4.6.1 Setup",
                Compressed = true,
                DownloadUrl =
                    "https://download.microsoft.com/download/3/5/9/35980F81-60F4-4DE3-88FC-8F962B97253B/NDP461-KB3102438-Web.exe",
                //Name = "NDP461-KB3102438-Web.exe",
                //SourceFile = "NDP461-KB3102438-Web.exe",
                Vital = true,
                PerMachine = true,
                Permanent = true,
                //ExitCodes = new List<ExitCode>() { new ExitCode(){Value = "1641",Behavior = BehaviorValues.scheduleReboot}, new ExitCode() { Value = "3010", Behavior = BehaviorValues.scheduleReboot } },
                InstallCommand = "/q /norestart",
                DetectCondition = $"{isInstalledCondition}",
                InstallCondition = $"(VersionNT >= v6.0) AND (NOT ({isInstalledCondition}))",
            };

            return package;
        }
    }
}