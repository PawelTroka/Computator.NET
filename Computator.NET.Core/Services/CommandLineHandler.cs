using System;
using System.IO;

namespace Computator.NET.Core.Services
{
    public interface ICommandLineHandler
    {
        bool TryGetScriptingDocument(out string filepath);
        bool TryGetCustomFunctionsDocument(out string filepath);
    }

    public class CommandLineHandler : ICommandLineHandler
    {
        public bool TryGetScriptingDocument(out string filepath) =>
            TryGetDocument(Environment.GetCommandLineArgs(), ".tsl", out filepath);

        public bool TryGetCustomFunctionsDocument(out string filepath) =>
            TryGetDocument(Environment.GetCommandLineArgs(), ".tslf", out filepath);

        // Shell activation supplies an already-unquoted argument. Do not split on
        // spaces, strip characters, or log private document paths/source code.
        public static bool TryGetDocument(string[] arguments, string extension, out string filepath)
        {
            filepath = null;
            if (arguments == null || arguments.Length < 2 || string.IsNullOrWhiteSpace(arguments[1])) return false;
            var candidate = arguments[1];
            if (candidate.IndexOf('\0') >= 0 || candidate.StartsWith("--", StringComparison.Ordinal)) return false;
            if (!string.Equals(Path.GetExtension(candidate), extension, StringComparison.OrdinalIgnoreCase)) return false;
            filepath = candidate;
            return true;
        }
    }
}
