using System;
using System.IO;
using System.Reflection;
using Computator.NET.Properties;

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
                            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
                        else
                            AppDomain.CurrentDomain.AssemblyResolve -= Resolver;
                        _isEnabled = value;
                    }
                }
            }
        }

        /// Will attempt to load missing assembly from either x86 or x64 subdir
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            var assemblyName = args.Name.Split(new[] {','}, 2)[0] + ".dll";
            if (assemblyName.Contains("resources.dll"))
                return null;

            if (assemblyName.Contains("cblas"))
            {
                return Environment.Is64BitProcess ? Assembly.Load(Resources.cblas_x64) : Assembly.Load(Resources.cblas_x86);
            }

            if (assemblyName.Contains("gsl"))
            {
                return Environment.Is64BitProcess ? Assembly.Load(Resources.gsl_x64) : Assembly.Load(Resources.gsl_x86);
            }

            return null;
        }
    }
}