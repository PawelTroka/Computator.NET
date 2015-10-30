namespace Computator.NET.Config
{
    public static class MultiplatformDllLoader
    {
        private static bool _isEnabled;

        public static bool Enable
        {
            get { return _isEnabled; }
            set
            {
                lock (typeof (MultiplatformDllLoader))
                {
                    if (_isEnabled != value)
                    {
                        if (value)
                            System.AppDomain.CurrentDomain.AssemblyResolve += Resolver;
                        else
                            System.AppDomain.CurrentDomain.AssemblyResolve -= Resolver;
                        _isEnabled = value;
                    }
                }
            }
        }

        /// Will attempt to load missing assembly from either x86 or x64 subdir
        private static System.Reflection.Assembly Resolver(object sender, System.ResolveEventArgs args)
        {
            var assemblyName = args.Name.Split(new[] {','}, 2)[0] + ".dll";
            if (assemblyName.Contains("resources.dll"))
                return null;

            var archSpecificPath =
                System.IO.Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Special",
                    System.Environment.Is64BitProcess ? "x64" : "x86",
                    assemblyName);

            return System.IO.File.Exists(archSpecificPath)
                ? System.Reflection.Assembly.LoadFile(archSpecificPath)
                : null;
        }
    }
}