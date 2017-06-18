using System;
using System.Linq;
using WixSharp;
using WixSharp.CommonTasks;

namespace Computator.NET.Setup
{
    class ProjectBuilder
    {
        public NetVersion CurrentHighestVersion = new NetVersion(){RealVersion = new Version(4, 0), DisplayVersion = "4.0"};

        public string BuildMsi()
        {
            var binariesPath = $@"..\Computator.NET\{SharedProperties.OutDir}\*.*";
            Console.WriteLine($"Analyzing binaries from path '{binariesPath}'");

            var assemblies = System.IO.Directory.GetFiles(binariesPath.Replace("*.*", string.Empty)).Where(f => f.EndsWith(".exe") || f.EndsWith(".dll"));
            
            foreach (var assembly in assemblies)
            {
                var version = NetVersion.FromAssembly(assembly);
                if (version.RealVersion > CurrentHighestVersion.RealVersion)
                    CurrentHighestVersion = version;
            }
            Console.WriteLine($"Highest .NET version among included assemblies is '{CurrentHighestVersion.DisplayVersion}'");

            var project = new Project("Computator.NET",
                new Dir(@"%ProgramFiles%\Computator.NET", new Files(binariesPath), new File(new Id(nameof(SharedProperties.TslIcon)), SharedProperties.TslIcon)),
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
                InstallScope = InstallScope.perMachine,//TODO: investigate if we could somehow go for perUser here
                //MajorUpgradeStrategy = MajorUpgradeStrategy.Default,//only MajorUpgradeStrategy or MajorUpgrade can be defined
                MajorUpgrade = new MajorUpgrade
                {
                    AllowDowngrades = false,
                    AllowSameVersionUpgrades = true,
                    Disallow = false,
                    DowngradeErrorMessage =
                        "A later version of [ProductName] is already installed. Setup will now exit.",
                    IgnoreRemoveFailure = true,
                    Schedule = UpgradeSchedule.afterInstallInitialize,
                },
            };

            var prerequisite = PrerequisiteHelper.GetPrerequisite(CurrentHighestVersion);
            Console.WriteLine($"Setting required NetFx {nameof(prerequisite)} '{prerequisite.WixPrerequisite}'");
            project.SetNetFxPrerequisite(prerequisite.WixPrerequisite, prerequisite.ErrorMessage);

            var mainExe = project.ResolveWildCards().FindFile((f) => f.Name.EndsWith("Computator.NET.exe")).Single();
            Console.WriteLine($"Main executable is '{mainExe.ToString()}'");

            Console.WriteLine("Setting shortcuts..");
            mainExe.Shortcuts = new[]
            {
                //new FileShortcut("Computator.NET", "INSTALLDIR"){IconFile = SharedProperties.IconLocation},
                new FileShortcut("Computator.NET", "%Desktop%"){IconFile = SharedProperties.IconLocation},
                new FileShortcut("Computator.NET",@"%ProgramMenu%"){IconFile = SharedProperties.IconLocation},
            };

            Console.WriteLine("Setting files associations..");
            mainExe.Associations = new[]
            {
                new FileAssociation("tsl") { Icon = nameof(SharedProperties.TslIcon), Description = "TROKA Scripting Language script file", ContentType = @"text/tsl" },
                new FileAssociation("tslf") { Icon = nameof(SharedProperties.TslIcon), Description = "TROKA Scripting Language functions file", ContentType = @"text/tslf" },
            };


            //project.ControlPanelInfo.NoRepair = true,
            //project.ControlPanelInfo.NoRemove = true,
            //project.ControlPanelInfo.SystemComponent = true, //if set will not be shown in Control Panel
            //Compiler.WixLocation
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";
            Console.WriteLine("Building main project...");
            return project.BuildMsi();
        }
    }
}
