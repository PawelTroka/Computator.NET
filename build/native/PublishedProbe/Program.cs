using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Pass the published application directory and a JSON report path.");
var published = Path.GetFullPath(args[0]);
var report = Path.GetFullPath(args[1]);
var profile = Path.Combine(Path.GetDirectoryName(report)!, "profile");
Directory.CreateDirectory(profile);
var expectedLibrary = Path.Combine(published, "native", "gsl", "gsl.dll");
string? selectedLibrary = null, nativeVersion = null, nativeHash = null, failure = null;
var checks = new Dictionary<string, double>();
var passed = false;
try
{
    foreach (var name in new[] { "gsl.dll", "gslcblas.dll", "gsl-provenance.json", "COPYING.GSL" })
        if (!File.Exists(Path.Combine(published, "native", "gsl", name)))
            throw new FileNotFoundException("The packaged GSL payload is incomplete: " + name);
    // Exercise the deployed Core assembly and its normal resolver. Never use a
    // project reference, an opt-in native override, or the developer's profile.
    Environment.SetEnvironmentVariable("COMPUTATOR_GSL_DIRECTORY", null);
    Environment.SetEnvironmentVariable("COMPUTATOR_DATA_DIRECTORY", profile);
    AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", published + Path.DirectorySeparatorChar);
    AssemblyLoadContext.Default.Resolving += (_, name) =>
    {
        var path = Path.Combine(published, name.Name + ".dll");
        return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
    };
    var core = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(published, "Computator.NET.Core.dll"));
    core.GetType("Computator.NET.Core.Natives.GSLInitializer", true)!.GetMethod("Initialize")!.Invoke(null, null);
    using var process = Process.GetCurrentProcess();
    var native = process.Modules.Cast<ProcessModule>().Single(module =>
        module.ModuleName.Equals("gsl.dll", StringComparison.OrdinalIgnoreCase));
    selectedLibrary = Path.GetFullPath(native.FileName);
    nativeHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(selectedLibrary)));
    nativeVersion = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(NativeLibrary.GetExport(native.BaseAddress, "gsl_version")));

    var functions = core.GetType("Computator.NET.Core.Functions.SpecialFunctions", true)!;
    void Check(string name, double expected, params double[] values)
    {
        var method = functions.GetMethod(name, values.Select(_ => typeof(double)).ToArray())!;
        var actual = (double)method.Invoke(null, values.Cast<object>().ToArray())!;
        checks[name] = actual;
        if (!double.IsFinite(actual) || Math.Abs(actual - expected) > 1e-12)
            throw new InvalidDataException($"Published {name} returned {actual:R}; expected {expected:R}.");
    }
    Check("gamma", 2, 3);
    Check("gammaP", 1 - 2 / Math.E, 2, 1);
    Check("AiPrime", -0.2588194037928068, 0);
    Check("BesselJ0", 1, 0);
    if (!selectedLibrary.Equals(expectedLibrary, StringComparison.OrdinalIgnoreCase) || nativeVersion != "2.8")
        throw new InvalidDataException($"Published resolver selected GSL {nativeVersion} from {selectedLibrary}; expected packaged GSL 2.8.");
    passed = true;
}
catch (Exception error)
{
    failure = (error is TargetInvocationException { InnerException: not null } invocation ? invocation.InnerException! : error).ToString();
}
finally
{
    File.WriteAllText(report, JsonSerializer.Serialize(new
    {
        schemaVersion = 1, completedUtc = DateTimeOffset.UtcNow, passed,
        publishedDirectory = published, expectedLibrary, selectedLibrary, nativeVersion, nativeHash, checks, failure
    }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
}
if (!passed)
{
    Console.Error.WriteLine("Published GSL validation failed. " + failure);
    return 1;
}
Console.WriteLine($"Published GSL 2.8 resolver and {checks.Count} numerical checks passed: {report}");
return 0;
