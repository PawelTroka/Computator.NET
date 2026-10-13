using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Computator.NET.Core.Compilation
{
    /// <summary>
    /// The returned assembly and its delegates own this program's lifetime. There
    /// is deliberately no process-wide collection of generated assemblies.
    /// </summary>
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
}
