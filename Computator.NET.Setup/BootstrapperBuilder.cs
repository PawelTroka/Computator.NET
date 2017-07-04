using System;
using System.IO;
using WixSharp;
using WixSharp.Bootstrapper;

namespace Computator.NET.Setup
{
    public class BootstrapperBuilder
    {
        public void Build()
        {
            var projectBuilder = new ProjectBuilder("4.6.1");
            var projectBuilderNet40 = new ProjectBuilder("4.0");

            Console.WriteLine($"Building {nameof(MsiPackage)}s...");
            var productMsi = projectBuilder.BuildMsi();
            var productMsiNet40 = projectBuilderNet40.BuildMsi();
            var productMsiPackage = new MsiPackage(productMsi) {DisplayInternalUI = true, InstallCondition = "VersionNT >= v6.0" };
            var productMsiPackageNet40 = new MsiPackage(productMsiNet40) { Id = "Computator.NET__Windows_XP", DisplayInternalUI = true, InstallCondition = "VersionNT < v6.0" };

            Console.WriteLine($"Building {nameof(PackageGroupRefWrapper)}s...");
            var packageGroupRefWrapperNet40 = new PackageGroupRefWrapper(projectBuilderNet40.CurrentHighestVersion);
            var packageGroupRefWrapperNet = new PackageGroupRefWrapper(projectBuilder.CurrentHighestVersion);
            var packegeGroupRefNet40Path = packageGroupRefWrapperNet40.Build();
            var packegeGroupRefNetPath = packageGroupRefWrapperNet.Build();

            Console.WriteLine($"Creating final {nameof(Bundle)} object");
            var bootstrapper =
                new Bundle("Computator.NET",
                    new ExePackage(packegeGroupRefNet40Path)
                    {
                        InstallCondition = "VersionNT < v6.0",
                        InstallCommand = "-q",
                        Compressed = true
                    },
                    new ExePackage(packegeGroupRefNetPath)
                    {
                        InstallCondition = "VersionNT >= v6.0",
                        InstallCommand = "-q",
                        Compressed = true
                    },
                    productMsiPackageNet40,
                    productMsiPackage)
                {
                    IconFile = SharedProperties.IconLocation,
                    DisableModify = "yes",
                    Version = SharedProperties.Version,
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
                        LogoFile = @"../Graphics/Installer/BootstrapperLogoFile65.png", //SharedProperties.LogoBmp,
                        LicensePath = SharedProperties.License, //@"https://github.com/PawelTroka/Computator.NET/blob/master/LICENSE"
                    },
                    SplashScreenSource = @"../Graphics/Installer/BootstrapperSplashScreen.bmp",//@"..\Graphics\computator.net-icon.png",//@"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif",
                    //PreserveTempFiles = true,
                };

            Console.WriteLine($"Adding {nameof(PatchKnowledgeBase2468871)} for async-await support on Windows XP.");
            var patchKnowledgeBase2468871 = new PatchKnowledgeBase2468871();
            var patchesForNet40 = patchKnowledgeBase2468871.Build(bootstrapper);
            bootstrapper.Chain.InsertRange(1, patchesForNet40);

            var finalPath = Path.Combine(SharedProperties.OutDir, "Computator.NET.Setup.exe");
            Console.WriteLine($"Building final bundle '{finalPath}'");
            bootstrapper.Build(finalPath);
        }
    }
}
