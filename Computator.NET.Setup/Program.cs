using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using WixSharp.Bootstrapper;
using WixSharp.CommonTasks;

namespace Computator.NET.Setup
{
    internal class Program
    {
        private static void Main()
        {
            var bootstrapperBuilder = new BootstrapperBuilder();
            bootstrapperBuilder.Build();
        }
    }
}