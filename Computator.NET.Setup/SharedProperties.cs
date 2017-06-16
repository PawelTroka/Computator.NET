using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Computator.NET.Setup
{
    public class SharedProperties
    {
        public const string UpgradeCode = "86D89770-B31B-4467-BC5E-B54B0FF263A1";
        public const string IconLocation = @"..\Graphics\computator.net-icon.ico";
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
        public const string SetupGif = @"..\Graphics\Installer\InstallShield Computator.NET Theme\setup.gif";
        public const string Logo = @"..\Graphics\computator.net-icon.png";
        public const string TslIcon = @"..\Graphics\tsl.ico";
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
