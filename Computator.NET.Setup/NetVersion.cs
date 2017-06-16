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
                    Console.WriteLine(attribute.FrameworkName);
                    var displayVersion = Regex.Replace(attribute.FrameworkName, @"(\d+.\d+(?:.\d+)?)", "$1");
                    return new NetVersion()
                    {
                        DisplayVersion = displayVersion,
                        RealVersion = new Version(displayVersion)
                    };

                }
            }
            catch
            {
                //ignored
            }
            return new NetVersion() { DisplayVersion = "0.0.0", RealVersion = new Version("0.0.0") };
        }
        public Version RealVersion { get; set; }
        public string DisplayVersion { get; set; }
    }
}
