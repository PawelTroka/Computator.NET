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
            var errorMessage = $"requires .NET Framework {netVersion.DisplayVersion} or higher.";

            if(netVersion.RealVersion <= new Version(4,0))
                return new Prerequisite(){ErrorMessage = errorMessage,WixPrerequisite = "NETFRAMEWORK40FULL='#1'" };
            
            if(netVersion.RealVersion == new Version(4,6,1))
                return new Prerequisite(){ErrorMessage = errorMessage,WixPrerequisite = "WIX_IS_NETFRAMEWORK_461_OR_LATER_INSTALLED >= '#394254'" };

            return new Prerequisite(){ErrorMessage = errorMessage,WixPrerequisite = $"WIX_IS_NETFRAMEWORK_{netVersion.DisplayVersion.Replace(".",string.Empty)}_OR_LATER_INSTALLED='#1'" };
        }

        public static string GetPackegeRef(NetVersion netVersion)
        {
            return $"NetFx{netVersion.DisplayVersion.Replace(".", string.Empty)}Web";
        }
    }
}
