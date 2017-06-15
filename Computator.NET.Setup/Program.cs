using System;
using System.IO;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.Bootstrapper;
using WixSharp.CommonTasks;
using Assembly = System.Reflection.Assembly;

namespace Computator.NET.Setup
{
    internal class SharedProperties
    {
        public const string UpgradeCode = "86D89770-B31B-4467-BC5E-B54B0FF263A1";
        public const string IconLocation = @"..\Graphics\computator.net-icon.ico";
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        public const string SetupGif = @"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";
        public const string Logo = @"..\Graphics\computator.net-icon.png";
        public const string License = @"..\docs\eula.rtf";
        public const string Company = "TROKA Software";
        public const string HelpTelephone = "+48-725-656-424";
        public const string HelpUrl = "https://github.com/PawelTroka/Computator.NET/issues";
        public const string AboutUrl = "https://github.com/PawelTroka/Computator.NET";
        public const string UpdateUrl = "https://github.com/PawelTroka/Computator.NET/releases";
#if DEBUG
        public const string OutDir = @"bin\Debug";
#else
        public const string OutDir = @"bin\Release";
#endif
    }

    internal class Program
    {
        public static bool IsPatchAlreadyInstalled(string productCode, string patchCode)
        {
            var patches =
                PatchInstallation.GetPatches(null, productCode, null, UserContexts.Machine, PatchStates.Applied);

            return patches.Any(patch => patch.DisplayName == patchCode);
        }

        private static void Main()
        {
            var productMsi = BuildMsi();

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
                    //new PackageGroupRef("NetFx40Web"),
                    new PackageGroupRef("NetFx461Web"),
                    new MsiPackage(productMsi) {DisplayInternalUI = true})
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

        private static string BuildMsi()
        {
            var project = new Project("Computator.NET",
                new Dir(@"%ProgramFiles%\Computator.NET", new Files($@"..\Computator.NET\{SharedProperties.OutDir}\*.*")),
                //new Dir("%Fonts%", new FontFile(@"..\Computator.NET.Core\Static\fonts\*.*")),
                new Dir("%Fonts%", new Files(@"..\Computator.NET.Core\Static\fonts\*.*")
                {
                    AttributesDefinition = "TrueType=yes",
                    Filter = f => f.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase) ||
                                  f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                                  f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)
                }))
            {
                Version = new Version(SharedProperties.Version),
                GUID = new Guid(SharedProperties.UpgradeCode),
                UpgradeCode = new Guid(SharedProperties.UpgradeCode),
                ProductId = Guid.NewGuid(),
                UI = WUI.WixUI_Minimal,
                LicenceFile = SharedProperties.License,
                ControlPanelInfo =
                {
                    Comments =
                        "Computator.NET is a special kind of numerical software that is fast and easy to use but not worse than others feature-wise",
                    Readme = "https://github.com/PawelTroka/Computator.NET",
                    HelpLink = SharedProperties.HelpUrl,
                    HelpTelephone = SharedProperties.HelpTelephone,
                    UrlInfoAbout = SharedProperties.AboutUrl,
                    UrlUpdateInfo = SharedProperties.UpdateUrl,
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
                InstallScope = InstallScope.perMachine,
                //MajorUpgradeStrategy = MajorUpgradeStrategy.Default,//only MajorUpgradeStrategy or MajorUpgrade can be defined
                MajorUpgrade = new MajorUpgrade
                {
                    AllowDowngrades = false,
                    AllowSameVersionUpgrades = true,
                    Disallow = false,
                    DowngradeErrorMessage =
                        "A later version of [ProductName] is already installed. Setup will now exit.",
                    IgnoreRemoveFailure = true
                }
            };

            //project.SetNetFxPrerequisite("WIX_IS_NETFRAMEWORK_40_OR_LATER_INSTALLED='#1'"); //project.SetNetFxPrerequisite("NETFRAMEWORK40FULL='#1'");
            project.SetNetFxPrerequisite("WIX_IS_NETFRAMEWORK_461_OR_LATER_INSTALLED >= '#394254'","requires .NET Framework 4.6.1 or higher.");

            project.ResolveWildCards()
                .FindFile((f) => f.Name.EndsWith("Computator.NET.exe"))
                .First()
                .Shortcuts = new[] {
                new FileShortcut("Computator.NET", "INSTALLDIR"){IconFile = SharedProperties.IconLocation},
                new FileShortcut("Computator.NET", "%Desktop%"){IconFile = SharedProperties.IconLocation},
                new FileShortcut("Computator.NET",@"%ProgramMenu%"){IconFile = SharedProperties.IconLocation}, 
            };

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