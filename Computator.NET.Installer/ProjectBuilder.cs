using System;
using System.Linq;
using WixSharp;
using WixSharp.CommonTasks;

namespace Computator.NET.Setup
{
    class ProjectBuilder
    {
        private readonly string _dottedNetVersion;

        public ProjectBuilder(string dottedNetVersion)
        {
            _dottedNetVersion = dottedNetVersion;
        }

        public NetVersion CurrentHighestVersion { get; private set; } = new NetVersion(){RealVersion = new Version(4, 0), DisplayVersion = "4.0"};

        public string BuildMsi()
        {
            var baseBinPath = $@"..\Computator.NET\{SharedProperties.OutDir}\v{_dottedNetVersion}";

            var binariesPath = System.IO.Path.Combine(baseBinPath,@"*.*");

            Console.WriteLine($"Analyzing binaries from path '{binariesPath}'");

            var assemblies = System.IO.Directory.GetFiles(baseBinPath).Where(f => f.EndsWith(".dll")).ToList();
            //assemblies.AddRange(System.IO.Directory.GetFiles(binariesPath.Replace("*.*", "*.exe")));
            
            foreach (var assembly in assemblies)
            {
                var version = NetVersion.FromAssembly(assembly);
                if (version.RealVersion > CurrentHighestVersion.RealVersion)
                    CurrentHighestVersion = version;
            }
            Console.WriteLine($"Highest .NET version among included assemblies is '{CurrentHighestVersion.DisplayVersion}'");

            var project = new Project("Computator.NET",
                new Dir(@"%ProgramFiles%\Computator.NET", new Files(binariesPath), new File(new Id(nameof(SharedProperties.TslIcon)), SharedProperties.TslIcon))
                ,new Dir("%Fonts%", Fonts.GetFontFiles())
                )
            {
                Version = SharedProperties.Version,
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
                OutDir = System.IO.Path.Combine(SharedProperties.OutDir,$"net{_dottedNetVersion.Replace(".","")}"),
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
                }
            };
            //project.LightOptions += @" XPath=""/wixOutput/table[@name='File']/row/field[5]"" InnerText=""65535.0.0.0""";
            //project.WixSourceGenerated += Fonts.InjectFonts;

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

            //project.LaunchConditions.Add(new LaunchCondition());

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
