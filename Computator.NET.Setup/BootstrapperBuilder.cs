using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using WixSharp;
using WixSharp.Bootstrapper;

namespace Computator.NET.Setup
{
    public class BootstrapperBuilder
    {

        public void Build()
        {
            var projectBuilder = new ProjectBuilder();
            var productMsi = projectBuilder.BuildMsi();
            


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

            if (projectBuilder.CurrentHighestVersion.RealVersion == new Version(4, 0))
            {
                var patchesForNet40 = new PatchKnowledgeBase2468871().Build();
                bootstrapper.Chain.InsertRange(1,patchesForNet40.Select(v => v.Package));
            }

            //bootstrapper.SplashScreenSource = @"..\Graphics\computator.net-icon.png";//@"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";          
            //bootstrapper.PreserveTempFiles = true;

            bootstrapper.Build(Path.Combine(SharedProperties.OutDir, "Computator.NET.Setup.exe"));
        }
    }
}
