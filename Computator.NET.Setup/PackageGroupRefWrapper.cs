using System;
using System.IO;
using WixSharp.Bootstrapper;

namespace Computator.NET.Setup
{
    /// <summary>
    /// Wraps PackageGroupRef in Exe Bundle so it can be used in final bootstrapper along with other PackageGroupRefs
    /// Inspired by: https://stackoverflow.com/a/24557280/2583080
    /// </summary>
    internal class PackageGroupRefWrapper
    {
        private readonly NetVersion _netVersion;

        public PackageGroupRefWrapper(NetVersion netVersion)
        {
            _netVersion = netVersion;
        }

        public string Build()
        {
            var packegeGroupRef = PrerequisiteHelper.GetPackegeRef(_netVersion);
            var packegeGroupRefPath = Path.Combine(SharedProperties.OutDir, $"{packegeGroupRef}.exe");
            
            var wrapperBundle = new Bundle(packegeGroupRef, new PackageGroupRef(packegeGroupRef))
            {
                UpgradeCode = new Guid("4E11B932-46CF-48AE-BF37-FEAB24F5FB7B"),
                Version = _netVersion.RealVersion,
                //Application = new SilentBootstrapperApplication(),
            };

            Console.WriteLine($"Building {nameof(PackageGroupRefWrapper)} for {nameof(PackageGroupRef)} '{packegeGroupRef}' for .NET version '{_netVersion}'");
            var wrapperPath = wrapperBundle.Build(packegeGroupRefPath);
            Console.WriteLine($"Build successful, path is '{wrapperPath}'");

            return wrapperPath;
        }
    }
}