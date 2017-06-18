using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

namespace Computator.NET.Setup
{
    public class NetVersion
    {
        public static NetVersion FromAssembly(string assemblyPath)
        {
            try
            {
                object[] list = Assembly.LoadFrom(assemblyPath).GetCustomAttributes(true);
                var attribute = list.OfType<TargetFrameworkAttribute>().FirstOrDefault();

                if (attribute?.FrameworkName != null)
                {
                    var displayVersion = Regex.Replace(attribute.FrameworkName, @"(\d+.\d+(?:.\d+)?)", "$1");
                    Console.WriteLine($"Assembly '{assemblyPath}' is compiled against .NET version '{displayVersion}'");
                    return new NetVersion()
                    {
                        DisplayVersion = displayVersion,
                        RealVersion = new Version(displayVersion)
                    };

                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Exception occured while processing assembly '{assemblyPath}' - details: '{exception.Message}'");
            }
            return new NetVersion() { DisplayVersion = "0.0.0", RealVersion = new Version("0.0.0") };
        }
        public Version RealVersion { get; set; }
        public string DisplayVersion { get; set; }
    }
}
