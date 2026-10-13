using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 1) throw new ArgumentException("Pass the staged native directory.");
var directory = Path.GetFullPath(args[0]);
using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "gsl-provenance.json")));
foreach (var item in document.RootElement.GetProperty("files").EnumerateObject())
{
    if (Path.GetFileName(item.Name) != item.Name) throw new InvalidDataException("Invalid runtime path.");
    var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory, item.Name))));
    if (!hash.Equals(item.Value.GetString(), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Hash mismatch: " + item.Name);
}
// Preload the pinned BLAS dependency, not anything found in a user-controlled PATH.
var blas = Directory.GetFiles(directory).First(path => Path.GetFileName(path).Contains("gslcblas") &&
    (path.EndsWith(".dll") || path.EndsWith(".dylib") || path.EndsWith(".so")));
NativeLibrary.Load(blas);
var handle = NativeLibrary.Load(Path.Combine(directory, document.RootElement.GetProperty("entryPoint").GetString()!));
foreach (var symbol in new[] { "gsl_set_error_handler_off", "gsl_sf_gamma_e", "gsl_sf_bessel_J0_e", "gsl_cdf_ugaussian_P", "gsl_version" })
    if (!NativeLibrary.TryGetExport(handle, symbol, out _)) throw new EntryPointNotFoundException(symbol);
var version = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(NativeLibrary.GetExport(handle, "gsl_version")));
if (version != "2.8") throw new InvalidDataException("Unexpected native version: " + version);
Marshal.GetDelegateForFunctionPointer<DisableErrors>(NativeLibrary.GetExport(handle, "gsl_set_error_handler_off"))();
var gamma = Marshal.GetDelegateForFunctionPointer<UnaryResult>(NativeLibrary.GetExport(handle, "gsl_sf_gamma_e"));
if (Marshal.SizeOf<Result>() != 16 || Marshal.OffsetOf<Result>(nameof(Result.Error)).ToInt32() != 8)
    throw new InvalidDataException("Managed/native result layout differs.");
if (gamma(3, out var value) != 0 || Math.Abs(value.Value - 2) > 1e-13) throw new InvalidDataException("Managed gamma mismatch.");
if (gamma(0, out value) != 1 || !double.IsNaN(value.Value)) throw new InvalidDataException("Native domain status mismatch.");
Console.WriteLine($"Managed ABI probe passed on {RuntimeInformation.RuntimeIdentifier}, GSL {version}.");

[StructLayout(LayoutKind.Sequential)]
struct Result { public double Value; public double Error; }
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate IntPtr DisableErrors();
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate int UnaryResult(double input, out Result result);
