using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Computator.NET.Setup
{
    public class SharedProperties
    {
        private const string BigIcon = @"..\Graphics\computator.net-icon.ico";
        private const string HighlyCompatibleIcon = @"..\Graphics\computator.net-icon-16x16-to-256x256.ico";

        public const string UpgradeCode = "86D89770-B31B-4467-BC5E-B54B0FF263A1";

        public const string IconLocation = HighlyCompatibleIcon;//BigIcon;
        //computator.net-icon-16x16-to-256x256.ico
        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public const string SetupGif = @"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";
        public const string Logo = @"..\Graphics\computator.net-icon.png";

        private const string TslIconBig = @"..\Graphics\tsl.ico";
        private const string TslIconCompatible = @"..\Graphics\tsl-16x16-to-256x256.ico";

        public const string TslIcon = TslIconCompatible;//TslIconBig;
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
}
