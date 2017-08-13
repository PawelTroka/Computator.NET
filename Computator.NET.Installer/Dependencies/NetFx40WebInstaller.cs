using Microsoft.Win32;
using WixSharp;
using WixSharp.Bootstrapper;

namespace Computator.NET.Installer
{
    /// <summary>
    /// Microsoft .NET Framework 4 (Web Installer)
    /// The Microsoft .NET Framework 4 web installer package downloads and installs the .NET Framework components required to run on the target machine architecture and OS.
    /// An Internet connection is required during the installation. .NET Framework 4 is required to run and develop applications to target the .NET Framework 4.
    /// https://www.microsoft.com/en-us/download/details.aspx?id=17851
    /// https://download.microsoft.com/download/1/B/E/1BE39E79-7E39-46A3-96FF-047F95396215/dotNetFx40_Full_setup.exe
    /// based on: https://github.com/wixtoolset/wix3/blob/develop/src/ext/NetFxExtension/wixlib/NetFx4.wxs
    /// </summary>
    class NetFx40WebInstaller
    {
        public ExePackage Build(Bundle bootstrapper)
        {
            const string variableNameForNetFx40 = "NETFRAMEWORK40";

            bootstrapper.IncludeWixExtension(WixExtension.Util);

            var netFx40RegistrySearch = new UtilRegistrySearch
            {
                Root = RegistryHive.LocalMachine,
                Key = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full",
                Value = "Install",
                Result = SearchResult.value,
                Format = SearchFormat.raw,
                Variable = variableNameForNetFx40,
            };

            bootstrapper.AddWixFragment("Wix/Bundle", netFx40RegistrySearch);
            
            var package = new ExePackage(@"..\redist\dotNetFx40_Full_setup.exe")
            {
                Name= "Microsoft .NET Framework 4",
                Description= "Microsoft .NET Framework 4 Setup",
                Compressed = true,
                DownloadUrl =
                    "https://download.microsoft.com/download/1/B/E/1BE39E79-7E39-46A3-96FF-047F95396215/dotNetFx40_Full_setup.exe",
                //Name = "dotNetFx40_Full_setup.exe",
                //SourceFile = "dotNetFx40_Full_setup.exe",
                Vital = true,
                PerMachine = true,
                Permanent = true,
                //ExitCodes = new List<ExitCode>() { new ExitCode(){Value = "1641",Behavior = BehaviorValues.scheduleReboot}, new ExitCode() { Value = "3010", Behavior = BehaviorValues.scheduleReboot } },
                InstallCommand = "/q /norestart",
                DetectCondition = $"({variableNameForNetFx40}) OR (VersionNT >= v6.0)",
                InstallCondition = $"(VersionNT < v6.0) AND (NOT {variableNameForNetFx40})",
            };

            return package;
        }
    }
}