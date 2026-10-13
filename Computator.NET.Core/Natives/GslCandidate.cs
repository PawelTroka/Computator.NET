using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace Computator.NET.Core.Natives
{
    internal static class GslCandidate
    {
        internal static IntPtr Load(string directory)
        {
            if (!Path.IsPathRooted(directory)) throw new ArgumentException("Candidate GSL directory must be absolute.");
            using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "gsl-provenance.json")));
            var root = document.RootElement;
            if (root.GetProperty("schemaVersion").GetInt32() != 1 || root.GetProperty("version").GetString() != "2.8" ||
                root.GetProperty("rid").GetString() != "win-x64" || RuntimeInformation.ProcessArchitecture != Architecture.X64)
                throw new InvalidDataException("Candidate GSL version/architecture has not been accepted by this build.");
            var names = root.GetProperty("files").EnumerateObject().ToArray();
            foreach (var file in names)
            {
                if (Path.GetFileName(file.Name) != file.Name) throw new InvalidDataException("Invalid native file path.");
                var bytes = File.ReadAllBytes(Path.Combine(directory, file.Name));
                if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(file.Value.GetString(), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Candidate GSL hash mismatch: " + file.Name);
            }
            var entry = root.GetProperty("entryPoint").GetString();
            if (entry != "gsl.dll" || !names.Any(file => file.Name == entry) || !names.Any(file => file.Name == "gslcblas.dll"))
                throw new InvalidDataException("The GSL/CBLAS runtime payload is incomplete.");
            NativeLibrary.Load(Path.Combine(directory, "gslcblas.dll"));
            var handle = NativeLibrary.Load(Path.Combine(directory, entry));
            var version = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(NativeLibrary.GetExport(handle, "gsl_version")));
            if (version != "2.8") throw new InvalidDataException("Native GSL version does not match its inventory.");
            return handle; // Intentionally process-lifetime: delegates may remain in generated assemblies.
        }
    }
}
