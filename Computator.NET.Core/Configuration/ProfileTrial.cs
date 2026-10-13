using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Computator.NET.Core.Configuration
{
    /// <summary>Data-only preparation of a private, backed-up profile for manual acceptance.</summary>
    public static class ProfileTrial
    {
        public const long MaximumBytes = 128 * 1024 * 1024;
        public const int MaximumFiles = 10000;

        // Returns hashes of relative input names, never writes an input file and
        // never executes imported scripts. The destination must be a new directory.
        public static IReadOnlyDictionary<string, string> Prepare(string source, string destination)
        {
            source = Path.TrimEndingDirectorySeparator(Path.GetFullPath(source));
            destination = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination));
            RejectLinks(source);
            RejectLinks(Path.GetDirectoryName(destination));
            if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
            if (Directory.Exists(destination) || File.Exists(destination)) throw new IOException("Trial destination must not exist.");
            if (Within(source, destination) || Within(destination, source)) throw new IOException("Source and destination must not overlap.");
            bool IsRootSettings(string file) => string.Equals(Path.GetDirectoryName(file), source, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(Path.GetFileName(file), "settings.json", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(Path.GetFileName(file), "settings.dat", StringComparison.OrdinalIgnoreCase));
            // A data-directory override alone is unsafe: old preferences can still
            // point at the original working/scripts directories. Refuse external roots.
            string Rebase(string path)
            {
                if (!Path.IsPathFullyQualified(path)) throw new InvalidDataException("Trial settings directories must be absolute.");
                var full = Path.GetFullPath(path);
                if (!Within(source, full)) throw new InvalidDataException("A settings directory is outside the supplied backup. Include it in a dedicated acceptance corpus first.");
                RejectLinks(full);
                return Path.GetFullPath(Path.Combine(destination, Path.GetRelativePath(source, full)));
            }
            var files = new List<string>();
            var pending = new Stack<string>(); pending.Push(source);
            long total = 0;
            while (pending.Count != 0)
            {
                var directory = pending.Pop();
                foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
                {
                    if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0) throw new IOException("Profile links/junctions are not accepted.");
                    if (Directory.Exists(entry)) { pending.Push(entry); continue; }
                    var extension = Path.GetExtension(entry);
                    if (!extension.Equals(".tsl", StringComparison.OrdinalIgnoreCase) &&
                        !extension.Equals(".tslf", StringComparison.OrdinalIgnoreCase) &&
                        !IsRootSettings(entry)) continue;
                    total = checked(total + new FileInfo(entry).Length);
                    files.Add(entry);
                    if (total > MaximumBytes || files.Count > MaximumFiles) throw new IOException("Acceptance corpus exceeds its size/file limit.");
                }
            }
            // Validate file links as well as directory ancestors before reading
            // settings. Windows also recognizes mixed-case settings filenames.
            var document = SettingsPersistence.Load(Path.Combine(source, "settings.dat"))
                ?? throw new InvalidDataException("The source has no recognized settings document.");
            document.WorkingDirectory = Rebase(document.WorkingDirectory);
            document.ScriptingDirectory = Rebase(document.ScriptingDirectory);
            document.CustomFunctionsDirectory = Rebase(document.CustomFunctionsDirectory);
            var originals = files.ToDictionary(file => Path.GetRelativePath(source, file), Hash, StringComparer.OrdinalIgnoreCase);
            Directory.CreateDirectory(destination);
            try
            {
                foreach (var file in files)
                {
                    var relative = Path.GetRelativePath(source, file);
                    // Keep both original settings formats as evidence, outside the
                    // live settings names. The rebased JSON below is the only active input.
                    var target = Path.Combine(destination, relative);
                    if (IsRootSettings(file)) target += ".original";
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(file, target, false);
                    if (Hash(target) != originals[relative]) throw new IOException("A profile input changed during copying.");
                }
                SettingsPersistence.Save(Path.Combine(destination, "settings.dat"), document);
                foreach (var file in files)
                    if (Hash(file) != originals[Path.GetRelativePath(source, file)]) throw new IOException("A source input changed during preparation; discard this trial.");
                return originals;
            }
            catch
            {
                // Do not recursively delete paths after a failure: another actor may
                // have changed them. Leave an explicitly incomplete, private trial.
                throw;
            }
        }

        private static bool Within(string root, string path) => string.Equals(root, path, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        private static string Hash(string file)
        {
            using (var input = File.OpenRead(file)) return Convert.ToHexString(SHA256.HashData(input));
        }
        private static void RejectLinks(string path)
        {
            for (var current = new DirectoryInfo(path); current != null; current = current.Parent)
                if (current.Exists && (current.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Profile paths must not traverse links or junctions.");
        }
    }
}
