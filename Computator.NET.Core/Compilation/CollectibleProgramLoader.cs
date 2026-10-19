using System;
using System.IO;
using System.Reflection;
#if !NETFRAMEWORK
using System.Runtime.Loader;
#endif

namespace Computator.NET.Core.Compilation
{
    /// <summary>
    /// Modern .NET can collect a released program. Framework/Mono preserves the
    /// shared compiler and types but retains generated assemblies until exit.
    /// </summary>
#if NETFRAMEWORK
    internal static class CollectibleProgramLoader
    {
        public static Assembly LoadProgram(Stream image)
        {
            using (var buffer = new MemoryStream())
            {
                image.Position = 0;
                image.CopyTo(buffer);
                return Assembly.Load(buffer.ToArray());
            }
        }
    }
#else
    internal sealed class CollectibleProgramLoader : AssemblyLoadContext
    {
        private CollectibleProgramLoader() : base("Computator.TSL." + Guid.NewGuid().ToString("N"), isCollectible: true) { }

        public static Assembly LoadProgram(Stream image)
        {
            var context = new CollectibleProgramLoader();
            try { image.Position = 0; return context.LoadFromStream(image); }
            catch { context.Unload(); throw; }
        }

        // Resolve framework/application dependencies through the default context,
        // preserving Function/Complex and numerical-library type identities. Do not
        // subscribe this instance to Default.Resolving or store its own Assembly:
        // either can turn a collectible context into a permanent reference root.
        protected override Assembly Load(AssemblyName assemblyName) => null;
    }
#endif
}
