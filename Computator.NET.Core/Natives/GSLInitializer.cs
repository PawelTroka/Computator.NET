using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Localization;

namespace Computator.NET.Core.Natives
{
    public class GSLInitializer
    {
        private static readonly object Gate = new object();
        private static readonly gsl_error_handler_t UnmanagedHandler = HandleUnmanagedException;
        private static IntPtr _library;

        public static void Initialize()
        {
            lock (Gate)
            {
                if (_library == IntPtr.Zero)
                {
                    var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
                    var bytes = architecture == Architecture.X64 ? Resources.gsl_x64
                        : architecture == Architecture.X86 ? Resources.gsl_x86
                        : throw new PlatformNotSupportedException("The bundled GSL binaries support Windows x64 and x86 only.");
                    Directory.CreateDirectory(GslConfig.Location);
                    var path = Path.Combine(GslConfig.Location, GslConfig.GslLibraryName);
                    // Use an explicit library path; do not depend on mutable global
                    // PATH search rules that differ between Framework and .NET.
                    if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
                    {
                        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                        try
                        {
                            File.WriteAllBytes(temporary, bytes);
                            File.Move(temporary, path, true);
                        }
                        finally { if (File.Exists(temporary)) File.Delete(temporary); }
                    }
                    _library = NativeLibrary.Load(path);
                    NativeLibrary.SetDllImportResolver(typeof(GSLInitializer).Assembly, Resolve);
                }
                if (Settings.Default.CalculationsErrors == CalculationsErrors.ReturnNAN)
                    NativeMethods.gsl_set_error_handler_off();
                else
                    NativeMethods.gsl_set_error_handler(UnmanagedHandler);
            }
        }

        private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath) =>
            string.Equals(name, GslConfig.GslDllName, StringComparison.OrdinalIgnoreCase) ? _library : IntPtr.Zero;

        private static void HandleUnmanagedException(string reason, string file, int line, int gsl_errno)
        {
            throw new Exception($"{Strings.Exception_occcured_in} {file}\n {Strings.at_line} {line}\n" +
                $"{Strings.Reason}: {reason}\n{Strings.Error_code}: {gsl_errno}");
        }
    }
}
