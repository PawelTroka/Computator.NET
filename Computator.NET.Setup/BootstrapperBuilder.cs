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
                    DisableModify = "yes",
                    Version = new Version(SharedProperties.Version),
                    UpgradeCode = new Guid(SharedProperties.UpgradeCode),
                    HelpUrl = SharedProperties.HelpUrl,
                    AboutUrl = SharedProperties.AboutUrl,
                    UpdateUrl = SharedProperties.UpdateUrl,
                    HelpTelephone = SharedProperties.HelpTelephone,
                    Manufacturer = SharedProperties.Company,
                    /*Application = new SilentBootstrapperApplication()
                    {
                        LogoFile = SharedProperties.Logo,
                        LicensePath = SharedProperties.License,
                    }*/
                    Application = new LicenseBootstrapperApplication()
                    {
                        LogoFile = SharedProperties.Logo,
                        LicensePath = SharedProperties.License,
                    }
                };

            if (projectBuilder.CurrentHighestVersion.RealVersion == new Version(4, 0))
            {
                Console.WriteLine($"Assemblies are from .NET 4.0 - will include {nameof(PatchKnowledgeBase2468871)} for async-wait support.");
                var patchKnowledgeBase2468871 = new PatchKnowledgeBase2468871();
                var patchesForNet40 = patchKnowledgeBase2468871.Build(bootstrapper);
                bootstrapper.Chain.InsertRange(1,patchesForNet40);
            }
            else
            {
                Console.WriteLine("Assemblies are not from .NET 4.0 - nothing more needs to be included.");
            }

            //bootstrapper.SplashScreenSource = @"..\Graphics\computator.net-icon.png";//@"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";          
            //bootstrapper.PreserveTempFiles = true;
            
            var finalPath = Path.Combine(SharedProperties.OutDir, "Computator.NET.Setup.exe");
            Console.WriteLine($"Building final bundle '{finalPath}'");
            bootstrapper.Build(finalPath);
        }
    }
}
