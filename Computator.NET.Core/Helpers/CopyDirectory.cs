using System;
using System.IO;

namespace Computator.NET.Core.Helpers
{
    internal class CopyDirectory
    {
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var sourceRoot = Path.GetFullPath(sourceDirectory);
            var targetRoot = Path.GetFullPath(targetDirectory);
            if (!Directory.Exists(sourceRoot))
                throw new DirectoryNotFoundException("Source directory not found: " + sourceRoot);

            // Copying a directory into itself (or its descendant) would either
            // overwrite the source or recursively copy the new destination.
            var sourcePrefix = sourceRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var targetPrefix = targetRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (string.Equals(sourcePrefix, targetPrefix, comparison) ||
                targetPrefix.StartsWith(sourcePrefix + Path.DirectorySeparatorChar, comparison))
                throw new ArgumentException("The destination cannot be the source or a child of it.", nameof(targetDirectory));

            Directory.CreateDirectory(targetRoot);

            foreach (var directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            {
                var relative = directory.Substring(sourceRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(targetRoot, relative));
            }

            // Use "*" to preserve files without extensions (including Unix dotfiles).
            // Do not overwrite any existing user documents in the destination.
            foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                var relative = file.Substring(sourceRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                File.Copy(file, Path.Combine(targetRoot, relative), false);
            }
        }
    }
}
