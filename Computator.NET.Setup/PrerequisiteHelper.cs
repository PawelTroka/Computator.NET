using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Computator.NET.Setup
{
    struct Prerequisite
    {
        public string WixPrerequisite { get; set; }
        public string ErrorMessage { get; set; }
    }
    class PrerequisiteHelper
    {
        public static Prerequisite GetPrerequisite(NetVersion netVersion)
        {
            Console.WriteLine($"Getting .NET prerequisite for .NET version {netVersion.DisplayVersion}");
            var errorMessage = $"requires .NET Framework {netVersion.DisplayVersion} or higher.";

            if(netVersion.RealVersion <= new Version(4,0))
                return new Prerequisite(){ErrorMessage = errorMessage,WixPrerequisite = "NETFRAMEWORK40FULL='#1'" };
            
            return new Prerequisite(){ErrorMessage = errorMessage,WixPrerequisite = $"WIX_IS_NETFRAMEWORK_{netVersion.DisplayVersion.Replace(".",string.Empty)}_OR_LATER_INSTALLED" };
        }

        public static string GetPackegeRef(NetVersion netVersion)
        {
            return $"NetFx{netVersion.DisplayVersion.Replace(".", string.Empty)}Web";
        }
    }
}
