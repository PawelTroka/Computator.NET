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
            var comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var sourcePrefix = sourceRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            var normalizedTarget = targetRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (normalizedTarget.Equals(sourceRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), comparison) ||
                normalizedTarget.StartsWith(sourcePrefix, comparison))
                throw new IOException("The copy destination must be outside the source directory.");

            // The target may have been deleted since settings initialization.
            Directory.CreateDirectory(targetRoot);
            foreach (var directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Destination(directory));

            // Include extensionless files (for example README), and do not
            // overwrite existing user documents during an examples restore.
            foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                var destination = Destination(file);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, false);
            }

            string Destination(string path)
            {
                // Replace only the leading root, not arbitrary matching text in
                // nested names. This also works with an input ending in a separator.
                var relative = path.Substring(sourceRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return Path.Combine(targetRoot, relative);
            }
        }
    }
}
