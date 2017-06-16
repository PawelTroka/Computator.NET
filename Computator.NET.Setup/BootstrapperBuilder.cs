using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp.Bootstrapper;

namespace Computator.NET.Setup
{
    public class BootstrapperBuilder
    {
        public static bool IsPatchAlreadyInstalled(string productCode, string patchCode)
        {
            var patches =
                PatchInstallation.GetPatches(null, productCode, null, UserContexts.Machine, PatchStates.Applied);

            return patches.Any(patch => patch.DisplayName == patchCode);
        }
        public void Build()
        {
            var projectBuilder = new ProjectBuilder();
            var productMsi = projectBuilder.BuildMsi();

            IsPatchAlreadyInstalled("{F5B09CFD-F0B2-36AF-8DF4-1DF6B63FC7B4}",
                "KB2468871"); // .NET Framework 4 Client Profile 64-bit
            IsPatchAlreadyInstalled("{8E34682C-8118-31F1-BC4C-98CD9675E1C2}",
                "KB2468871"); // .NET Framework 4 Extended 64-bit
            IsPatchAlreadyInstalled("{3C3901C5-3455-3E0A-A214-0B093A5070A6}",
                "KB2468871"); // .NET Framework 4 Client Profile 32-bit
            IsPatchAlreadyInstalled("{0A0CADCF-78DA-33C4-A350-CD51849B9702}",
                "KB2468871"); // .NET Framework 4 Extended 32-bit

            var bootstrapper =
                new Bundle("Computator.NET",
                    new PackageGroupRef(PrerequisiteHelper.GetPackegeRef(projectBuilder.CurrentHighestVersion)),
                    new MsiPackage(productMsi) { DisplayInternalUI = true })
                {
                    IconFile = SharedProperties.IconLocation,
                    Version = new Version(SharedProperties.Version),
                    UpgradeCode = new Guid(SharedProperties.UpgradeCode),
                    HelpUrl = SharedProperties.HelpUrl,
                    AboutUrl = SharedProperties.AboutUrl,
                    UpdateUrl = SharedProperties.UpdateUrl,
                    HelpTelephone = SharedProperties.HelpTelephone,
                    Manufacturer = SharedProperties.Company,
                    Application = new LicenseBootstrapperApplication
                    {
                        LogoFile = SharedProperties.Logo,
                        LicensePath = SharedProperties.License
                    }
                };
            
            //bootstrapper.SplashScreenSource = @"..\Graphics\computator.net-icon.png";//@"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";          
            // bootstrapper.PreserveTempFiles = true;

            bootstrapper.Build(Path.Combine(SharedProperties.OutDir, "Computator.NET.Setup.exe"));
        }
    }
}
