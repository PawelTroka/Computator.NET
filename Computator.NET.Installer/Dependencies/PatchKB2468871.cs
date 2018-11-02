using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.Bootstrapper;

namespace Computator.NET.Installer
{
    /// <summary>
    /// Microsoft .NET Framework 4 KB2468871
    /// Update to .NET Framework 4.
    /// https://www.microsoft.com/en-us/download/details.aspx?id=3556
    /// http://support.microsoft.com/kb/2468871
    /// Version: KB2468871
    /// File Name: NDP40-KB2468871-v2-x86.exe NDP40-KB2468871-v2-IA64.exe NDP40-KB2468871-v2-x64.exe
    /// Date Published: 6/8/2011
    /// File Size: 18.7 MB 29.0 MB 27.3 MB
    /// </summary>
    class PatchKnowledgeBase2468871
    {
        public static bool IsPatchAlreadyInstalled(string productCode, string patchCode)
        {
            var patches =
                PatchInstallation.GetPatches(null, productCode, null, UserContexts.Machine, PatchStates.Applied);

            return patches.Any(patch => patch.DisplayName == patchCode);
        }
        
        private ExePackage _kb2468871X64Package;
        private ExePackage _kb2468871X86Package;

        public ExePackage[] Build(Bundle bootstrapper)
        {
            const string variableNameForX86 = "KB2468871x86Installed";
            const string variableNameForX64 = "KB2468871x64Installed";

            bootstrapper.IncludeWixExtension(WixExtension.Util);

            var kb468871X86RegistrySearch = new UtilRegistrySearch
            {
                Root = RegistryHive.LocalMachine,
                Key = @"SOFTWARE\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2468871",
                Result = SearchResult.exists,
                Format = SearchFormat.raw,
                Win64 = false,
                Variable = variableNameForX86,
            };
            var kb2468871X64RegistrySearch = new UtilRegistrySearch
            {
                Root = RegistryHive.LocalMachine,
                Key = @"SOFTWARE\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2468871",//SOFTWARE\wow6432node\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2468871
                Result = SearchResult.exists,
                Format = SearchFormat.raw,
                Win64 = true,
                Variable = variableNameForX64,
            };
            bootstrapper.AddWixFragment("Wix/Bundle", kb468871X86RegistrySearch, kb2468871X64RegistrySearch);

            //TODO: those packages binaries should not be included in repo
            //DownloadUrl should make it downloadable from web during build time
            _kb2468871X64Package = new ExePackage(@"..\redist\NDP40-KB2468871-v2-x64.exe")
            {
                Description =
                    "This prerequisite installs the .NET Framework 4.0 full profile update provided in Microsoft KB article 2468871.",
                Compressed = false,
                DownloadUrl =
                    "http://download.microsoft.com/download/2/B/F/2BF4D7D1-E781-4EE0-9E4F-FDD44A2F8934/NDP40-KB2468871-v2-x64.exe",
                Name = "NDP40-KB2468871-v2-x64.exe",
                //SourceFile = "NDP40-KB2468871-v2-x64.exe",
                Vital = true,
                PerMachine = true,
                Permanent = true,
                //ExitCodes = new List<ExitCode>() { new ExitCode(){Value = "1641",Behavior = BehaviorValues.scheduleReboot}, new ExitCode() { Value = "3010", Behavior = BehaviorValues.scheduleReboot } },
                InstallCommand = "/q /norestart",
                DetectCondition = $"({variableNameForX64}) OR (VersionNT64 >= v6.0)",
                InstallCondition = $"(VersionNT64 < v6.0) AND (NOT {variableNameForX64})",
            };

            _kb2468871X86Package = new ExePackage(@"..\redist\NDP40-KB2468871-v2-x86.exe")
            {
                Description =
                    "This prerequisite installs the .NET Framework 4.0 full profile update provided in Microsoft KB article 2468871.",
                Compressed = false,
                DownloadUrl =
                    "http://download.microsoft.com/download/2/B/F/2BF4D7D1-E781-4EE0-9E4F-FDD44A2F8934/NDP40-KB2468871-v2-x86.exe",
                Name = "NDP40-KB2468871-v2-x86.exe",
                //SourceFile = "NDP40-KB2468871-v2-x86.exe",
                Vital = true,
                PerMachine = true,
                Permanent = true,
                //ExitCodes = new List<ExitCode>() { new ExitCode(){Value = "1641",Behavior = BehaviorValues.scheduleReboot}, new ExitCode() { Value = "3010", Behavior = BehaviorValues.scheduleReboot } },
                InstallCommand = "/q /norestart",
                DetectCondition = $"({variableNameForX86}) OR (VersionNT >= v6.0)",
                InstallCondition = $"(VersionNT < v6.0) AND (NOT {variableNameForX86}) AND (NOT VersionNT64)",
            };

            return new[]
            {
                _kb2468871X64Package,
                _kb2468871X86Package, 
            };
        }

        private void Check()
        {
            IsPatchAlreadyInstalled("{F5B09CFD-F0B2-36AF-8DF4-1DF6B63FC7B4}",
                "KB2468871"); // .NET Framework 4 Client Profile 64-bit
            IsPatchAlreadyInstalled("{8E34682C-8118-31F1-BC4C-98CD9675E1C2}",
                "KB2468871"); // .NET Framework 4 Extended 64-bit
            IsPatchAlreadyInstalled("{3C3901C5-3455-3E0A-A214-0B093A5070A6}",
                "KB2468871"); // .NET Framework 4 Client Profile 32-bit
            IsPatchAlreadyInstalled("{0A0CADCF-78DA-33C4-A350-CD51849B9702}",
                "KB2468871"); // .NET Framework 4 Extended 32-bit
        }
    }
}
