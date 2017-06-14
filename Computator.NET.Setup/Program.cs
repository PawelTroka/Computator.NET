using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
//using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.Bootstrapper;
using Assembly = System.Reflection.Assembly;

namespace Computator.NET.Setup
{
    class SharedProperties
    {
        public const string UpgradeCode = "86D89770-B31B-4467-BC5E-B54B0FF263A1";
        public const string IconLocation = @"..\Graphics\computator.net-icon.ico";
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        public const string SetupGif = @"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";
        public const string Logo = @"..\Graphics\computator.net-icon.png";
        public const string License = @"..\docs\eula.rtf";
        public const string Company = "TROKA Software";
        public const string HelpTelephone = "+48-725-656-424";
#if DEBUG
        public const string OutDir = @"bin\Debug";
#else
        public const string OutDir = @"bin\Release";
#endif
    }

    class Program
    {
        public static bool IsPatchAlreadyInstalled(string productCode, string patchCode)
        {
            //var patches =
                //PatchInstallation.GetPatches(null, productCode, null, UserContexts.Machine, PatchStates.Applied);

            return true;//patches.Any(patch => patch.DisplayName == patchCode);
        }
        static void Main()
        {
            Compiler.WixSourceGenerated += InjectImages;
            string productMsi = BuildMsi();

            IsPatchAlreadyInstalled("{F5B09CFD-F0B2-36AF-8DF4-1DF6B63FC7B4}", "KB2468871");// .NET Framework 4 Client Profile 64-bit
            IsPatchAlreadyInstalled("{8E34682C-8118-31F1-BC4C-98CD9675E1C2}", "KB2468871");// .NET Framework 4 Extended 64-bit
            IsPatchAlreadyInstalled("{3C3901C5-3455-3E0A-A214-0B093A5070A6}", "KB2468871");// .NET Framework 4 Client Profile 32-bit
            IsPatchAlreadyInstalled("{0A0CADCF-78DA-33C4-A350-CD51849B9702}", "KB2468871");// .NET Framework 4 Extended 32-bit

            var bootstrapper =
              new Bundle("Computator.NET",
                  //new PackageGroupRef("NetFx40Web"),
                  new PackageGroupRef("NetFx461Web"),
                  new MsiPackage(productMsi) { DisplayInternalUI = true });
            
            bootstrapper.IconFile = SharedProperties.IconLocation;
            bootstrapper.Version = new Version(SharedProperties.Version);
            bootstrapper.UpgradeCode = new Guid(SharedProperties.UpgradeCode);
            

            //bootstrapper.SplashScreenSource = @"..\Graphics\computator.net-icon.png";//@"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";
            bootstrapper.HelpTelephone = SharedProperties.HelpTelephone;
            bootstrapper.Manufacturer = SharedProperties.Company;
            

            bootstrapper.Application = new LicenseBootstrapperApplication(){LogoFile = SharedProperties.Logo, LicensePath = SharedProperties.License};
            // bootstrapper.PreserveTempFiles = true;

            bootstrapper.Build(System.IO.Path.Combine(SharedProperties.OutDir,"Computator.NET.Setup.exe"));
        }

        private static void InjectImages(XDocument document)
        {
            //var productElement = document.Root.Select("Product");
            //productElement.Add(new XElement("WixVariable",
            //    new XAttribute("Id", "WixUIBannerBmp"),
            //    new XAttribute("Value", @"..\Graphics\Installer\InstallShield Computator.NET Theme\banner.jpg")));

            //productElement.Add(new XElement("WixVariable",
            //    new XAttribute("Id", "WixUIDialogBmp"),
            //    new XAttribute("Value", @"..\Graphics\Installer\InstallShield Computator.NET Theme\welcome.jpg")));
        }

        static string BuildMsi()
        {
            var project = new Project("Computator.NET",
                new Dir(@"%ProgramFiles%\Computator.NET", new Files($@"..\Computator.NET\{SharedProperties.OutDir}\*.*"))
            )
            {
                Version = new Version(SharedProperties.Version),
                GUID = new Guid(SharedProperties.UpgradeCode),
                UI = WUI.WixUI_Minimal,
                LicenceFile = SharedProperties.License,
                ControlPanelInfo =
                {
                    Comments =
                        "Computator.NET is a special kind of numerical software that is fast and easy to use but not worse than others feature-wise",
                    Readme = "https://github.com/PawelTroka/Computator.NET",
                    HelpLink = "https://github.com/PawelTroka/Computator.NET/issues",
                    HelpTelephone = SharedProperties.HelpTelephone,
                    UrlInfoAbout = "https://github.com/PawelTroka/Computator.NET",
                    UrlUpdateInfo = "https://github.com/PawelTroka/Computator.NET/releases",
                    ProductIcon = SharedProperties.IconLocation,
                    Contact = "pawel@troka.software",
                    Manufacturer = SharedProperties.Company,
                    InstallLocation = "[INSTALLDIR]",
                    NoModify = true
                },
                BackgroundImage = @"..\Graphics\Installer\InstallShield Computator.NET Theme\welcome.jpg",
                BannerImage = @"..\Graphics\Installer\InstallShield Computator.NET Theme\banner.jpg",
                OutDir = SharedProperties.OutDir,
                OutFileName = "Computator.NET",
            };
            project.InstallScope=InstallScope.perUser;
            //project.ControlPanelInfo.NoRepair = true,
            //project.ControlPanelInfo.NoRemove = true,
            //project.ControlPanelInfo.SystemComponent = true, //if set will not be shown in Control Panel
            //Compiler.WixLocation
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            return project.BuildMsi();
        }
    }
}