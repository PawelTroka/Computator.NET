using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using WixSharp;
using WixSharp.Bootstrapper;

namespace Computator.NET.Setup
{
    class ExePackageWithRegCondition
    {
        public RegValueProperty RegCondition { get; set; }
        public ExePackage Package { get; set; } 
    }
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

        RegValueProperty KB2468871x64Installed;
        RegValueProperty KB2468871x86Installed;
        private ExePackage KB2468871x64Package;
        private ExePackage KB2468871x86Package;

        public ExePackageWithRegCondition[] Build()
        {
            KB2468871x64Installed = new RegValueProperty(nameof(KB2468871x64Installed), RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2468871", "", "0") { Win64 = true };
            KB2468871x86Installed = new RegValueProperty(nameof(KB2468871x86Installed), RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2468871", "", "0") { Win64 = false };

            //TODO: those packages binaries should not be included in repo
            //DownloadUrl should make it downloadable from web during build time
            KB2468871x64Package = new ExePackage(@"..\redist\NDP40-KB2468871-v2-x64.exe")
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
                DetectCondition = $"{nameof(KB2468871x64Installed)} OR VersionNT64 >= 600",
                InstallCondition = $"VersionNT64 < 600 AND (NOT {nameof(KB2468871x64Installed)})",
            };

            KB2468871x86Package = new ExePackage(@"..\redist\NDP40-KB2468871-v2-x86.exe")
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
                DetectCondition = $"{nameof(KB2468871x86Installed)} OR VersionNT >= 600",
                InstallCondition = $"VersionNT < 600 AND (NOT {nameof(KB2468871x86Installed)}) AND (NOT VersionNT64)",
            };

            return new[]
            {
                new ExePackageWithRegCondition() {RegCondition = KB2468871x64Installed,Package = KB2468871x64Package},
                new ExePackageWithRegCondition() {RegCondition = KB2468871x86Installed, Package = KB2468871x86Package}, 
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
