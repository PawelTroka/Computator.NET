using System;
using System.IO;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    // Document keys are fixed when a file is opened, never relative to a later
    // working directory. Ordinary Windows path aliases share one document.
    internal static class DocumentFilePath
    {
        internal static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A document file name is required.", nameof(path));

            if (Path.DirectorySeparatorChar == '\\')
            {
                var windowsPath = path.Replace('/', '\\');
                if (windowsPath.StartsWith(@"\\?\", StringComparison.Ordinal) ||
                    windowsPath.StartsWith(@"\\.\", StringComparison.Ordinal))
                    throw new IOException("Device and extended file paths cannot be used as document names.");

                // Win32 silently trims these components. Reject the ambiguous
                // spelling before Path.GetFullPath can discard its suffix.
                foreach (var component in windowsPath.Split('\\'))
                    if (component != "." && component != ".." &&
                        (component.EndsWith(".", StringComparison.Ordinal) ||
                         component.EndsWith(" ", StringComparison.Ordinal)))
                        throw new IOException("Document path components cannot end with a dot or space.");
            }

            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath);
            foreach (var component in fullPath.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                if (component.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    throw new IOException("The document path contains invalid file name characters.");

                if (Path.DirectorySeparatorChar == '\\')
                {
                    var stem = component.Split('.')[0].TrimEnd(' ');
                    if (string.Equals(stem, "CON", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(stem, "PRN", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(stem, "AUX", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(stem, "NUL", StringComparison.OrdinalIgnoreCase) ||
                        (stem.Length == 4 &&
                         (stem.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                          stem.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
                         "123456789¹²³".IndexOf(stem[3]) >= 0))
                        throw new IOException("Windows device names cannot be used as document names.");
                }
            }
            return fullPath;
        }

        internal static bool AreEquivalent(string left, string right)
        {
            return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
        }

        internal static bool TryNormalize(string path, out string fullPath)
        {
            try
            {
                fullPath = Normalize(path);
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException ||
                                               exception is IOException || exception is NotSupportedException)
            {
                fullPath = null;
                return false;
            }
        }
    }
}
