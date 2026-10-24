using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.SettingsTypes;

namespace Computator.NET.Core.Natives
{
    public class GSLInitializer
    {
        private static readonly object Gate = new object();
        private static readonly gsl_error_handler_t UnmanagedHandler = HandleUnmanagedError;
#if __MonoCS__
        private static bool _initialized;
#else
        private static IntPtr _library;
#endif
        public static void Initialize()
        {
            lock (Gate)
            {
#if __MonoCS__
                if (!_initialized)
                {
                    // Use Mono's native loader and the system GSL installation.
                    // The shared GslDllName is also used by generated programs, so
                    // their P/Invoke calls use the same library and error handler.
                    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY")))
                        throw new PlatformNotSupportedException("COMPUTATOR_GSL_DIRECTORY selects a Windows GSL package. Mono uses the system GSL installation.");
#else
                if (_library == IntPtr.Zero)
                {
                    var candidate = Environment.GetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY");
                    var packaged = Path.Combine(AppContext.BaseDirectory, "native", "gsl");
                    if (!string.IsNullOrWhiteSpace(candidate)) _library = GslCandidate.Load(candidate);
                    else if (Directory.Exists(packaged)) _library = GslCandidate.Load(packaged);
                    else
                    {
                        var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
                        var bytes = architecture == Architecture.X64 ? Resources.gsl_x64 : architecture == Architecture.X86 ? Resources.gsl_x86
                            : throw new PlatformNotSupportedException("The bundled GSL binaries support Windows x64 and x86 only.");
                        Directory.CreateDirectory(GslConfig.Location);
                        var path = Path.Combine(GslConfig.Location, GslConfig.GslLibraryName);
                        if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
                        {
                            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                            try { File.WriteAllBytes(temporary, bytes); File.Move(temporary, path, true); }
                            finally { if (File.Exists(temporary)) File.Delete(temporary); }
                        }
                        _library = NativeLibrary.Load(path);
                    }
                    NativeLibrary.SetDllImportResolver(typeof(GSLInitializer).Assembly, Resolve);
#endif
                    // Install exactly once; do not race process-wide native handler
                    // changes when requests initialize their own evaluators.
                    NativeMethods.gsl_set_error_handler(UnmanagedHandler);
                    // The settings editor updates the singleton in place. Keep
                    // its mode live without replacing the native callback.
                    Settings.Default.PropertyChanged += OnSettingsChanged;
#if __MonoCS__
                    _initialized = true;
#endif
                }
                UpdateErrorMode();
            }
        }
        private static void OnSettingsChanged(object sender, PropertyChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(Settings.CalculationsErrors))
                UpdateErrorMode();
        }
        private static void UpdateErrorMode() =>
            GslErrorBoundary.ThrowOnError = Settings.Default.CalculationsErrors != CalculationsErrors.ReturnNAN;
#if !__MonoCS__
        private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath) =>
            string.Equals(name, GslConfig.GslDllName, StringComparison.OrdinalIgnoreCase) ? _library : IntPtr.Zero;
#endif
        private static void HandleUnmanagedError(string reason, string file, int line, int status) =>
            GslErrorBoundary.Record(reason, status);
    }
}
