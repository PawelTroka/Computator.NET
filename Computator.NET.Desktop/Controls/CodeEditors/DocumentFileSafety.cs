using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    // The displayed text is decoded from the same exact bytes as its baseline.
    internal sealed class DocumentFileSnapshot
    {
        private readonly byte[] _bytes;
        private readonly DocumentFileIdentity? _identity;

        internal DocumentFileSnapshot(byte[] bytes, DocumentFileIdentity? identity,
            FileAttributes attributes, uint linkCount)
        {
            _bytes = bytes;
            _identity = identity;
            Attributes = attributes;
            LinkCount = linkCount;
            Text = string.Empty;
            if (bytes != null)
            {
                using (var reader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8, true))
                    Text = reader.ReadToEnd();
            }
        }

        internal bool Exists => _bytes != null;
        internal string Text { get; }
        internal string RetainedBackupPath { get; set; }
        internal FileAttributes Attributes { get; }
        internal uint LinkCount { get; }
        internal static DocumentFileSnapshot Missing => new DocumentFileSnapshot(null, null, 0, 0);

        internal bool Matches(DocumentFileSnapshot other)
        {
            if (other == null || Exists != other.Exists) return false;
            if (!Exists) return true;
            return Nullable.Equals(_identity, other._identity) &&
#if __MonoCS__
                System.Linq.Enumerable.SequenceEqual(_bytes, other._bytes);
#else
                _bytes.AsSpan().SequenceEqual(other._bytes);
#endif
        }
    }

    internal readonly struct DocumentFileIdentity : IEquatable<DocumentFileIdentity>
    {
        private readonly ulong _volume;
        private readonly ulong _low;
        private readonly ulong _high;

        internal DocumentFileIdentity(ulong volume, ulong low, ulong high)
        {
            _volume = volume;
            _low = low;
            _high = high;
        }

        public bool Equals(DocumentFileIdentity other) =>
            _volume == other._volume && _low == other._low && _high == other._high;

        public override bool Equals(object other) => other is DocumentFileIdentity identity && Equals(identity);
        public override int GetHashCode()
        {
#if __MonoCS__
            unchecked { return ((_volume.GetHashCode() * 397) ^ _low.GetHashCode()) * 397 ^ _high.GetHashCode(); }
#else
            return HashCode.Combine(_volume, _low, _high);
#endif
        }
    }

    internal static class DocumentFileSafety
    {
        private static bool IsWindows
        {
            get
            {
#if __MonoCS__
                return Environment.OSVersion.Platform == PlatformID.Win32NT;
#else
                return OperatingSystem.IsWindows();
#endif
            }
        }

        private const string ChangedMessage =
            "The destination changed outside this editor. Your unsaved document has been kept. Choose a new file name with Save As.";

        internal static DocumentFileSnapshot Capture(string path)
        {
            path = Path.GetFullPath(path);
            using (var stream = OpenExisting(path, FileShare.Read))
                return stream == null ? DocumentFileSnapshot.Missing : ReadSnapshot(path, stream);
        }

        internal static bool AreSameFile(string left, string right)
        {
            left = Path.GetFullPath(left);
            right = Path.GetFullPath(right);
            if (string.Equals(left, right, StringComparison.Ordinal)) return true;
            var caseVariant = string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
            if (!IsWindows)
            {
#if __MonoCS__
                // Tab keys are case-insensitive, while a Unix filesystem can
                // hold both names. Never silently reuse one document's buffer.
                if (caseVariant)
                    throw new IOException("These document names differ only by case. The Mono editor cannot safely combine them on a case-sensitive filesystem. Choose a different file name.");
#endif
                return caseVariant;
            }

            using (var first = OpenWindowsFile(left, 0, 7, 0x02000000))
            {
                // A new destination cannot be a physical alias of an existing
                // file. Do not open an unrelated, possibly locked background file.
                if (first == null && !caseVariant) return false;
                using (var second = OpenWindowsFile(right, 0, 7, 0x02000000))
                {
                    if (first == null && second == null) return caseVariant;
                    var sameFile = first != null && second != null &&
                        GetIdentity(first, out _).Equals(GetIdentity(second, out _));
                    // WinForms tab keys are case-insensitive. Refuse distinct
                    // case-sensitive NTFS names instead of merging their buffers.
                    if (caseVariant && !sameFile)
                        throw new IOException("These names differ only by case but do not identify the same file. This editor cannot safely combine case-sensitive filename spellings. Choose a different file name.");
                    return sameFile;
                }
            }
        }

        // The optional callbacks provide deterministic filesystem-fault tests.
        // Production uses the complete, flushed staged writer and File.Replace.
        internal static DocumentFileSnapshot Write(string path, string contents, DocumentFileSnapshot expected,
            Action<Stream, string> stageWriter = null, Action beforeCommit = null,
            Action<string, string, string> replaceFile = null)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            path = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(path);
            var stagedPath = Path.Combine(directory, ".computator-save-" + Guid.NewGuid().ToString("N") + ".tmp");
            var backupPath = Path.Combine(directory, ".computator-backup-" + Guid.NewGuid().ToString("N") + ".tmp");
            var replacedExisting = false;
            try
            {
                using (var staged = new FileStream(stagedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    if (stageWriter == null)
                    {
                        using (var writer = new StreamWriter(staged, new UTF8Encoding(false), 4096, true))
                            writer.Write(contents);
                    }
                    else stageWriter(staged, contents);
                    staged.Flush(true);
                }

                // Capture bytes/text and the future file ID before committing.
                // No fallible read is needed after a successful replacement.
                var saved = Capture(stagedPath);
                beforeCommit?.Invoke();

                // Deny in-place writers during comparison/commit, but allow the
                // atomic replacement. An external rename can still race this
                // operation: File.Replace does not offer compare-and-swap.
                using (var destination = OpenExisting(path, FileShare.Read | FileShare.Delete))
                {
                    var current = destination == null ? DocumentFileSnapshot.Missing : ReadSnapshot(path, destination);
                    if (!expected.Matches(current)) throw new IOException(ChangedMessage);
                    if ((current.Attributes & FileAttributes.ReparsePoint) != 0 || current.LinkCount > 1)
                        throw new IOException("This destination is a filesystem reparse point or has multiple hard links. Choose a new file name with Save As to preserve the linked files.");
                    if ((current.Attributes & FileAttributes.ReadOnly) != 0)
                        throw new UnauthorizedAccessException("The destination is read-only. Your unsaved document has been kept.");

                    if (current.Exists)
                    {
                        try
                        {
                            if (replaceFile == null) File.Replace(stagedPath, path, backupPath);
                            else replaceFile(stagedPath, path, backupPath);
                            replacedExisting = true;
                        }
                        catch (Exception failure) when (failure is IOException || failure is UnauthorizedAccessException)
                        {
                            // ReplaceFile can move the original before failing
                            // (Win32 errors 1176/1177). Never discard that copy.
                            RestoreBackupAfterFailure(backupPath, path, failure);
                            throw;
                        }
                    }
                    else
                        File.Move(stagedPath, path);
                }
                // Another process can replace the destination between our check
                // and File.Replace. Its newer version is then the backup: retain
                // it rather than deleting bytes that the user has not reviewed.
                if (replacedExisting && !TryDeleteVerifiedBackup(backupPath, expected))
                    saved.RetainedBackupPath = backupPath;
                return saved;
            }
            finally
            {
                TryDelete(stagedPath);
            }
        }

        private static bool TryDeleteVerifiedBackup(string path, DocumentFileSnapshot expected)
        {
            try
            {
                if (!IsWindows)
                {
                    if (!expected.Matches(Capture(path))) return false;
                    File.Delete(path);
                    return true;
                }

                // GENERIC_READ | DELETE, FILE_SHARE_READ, BACKUP_SEMANTICS |
                // OPEN_REPARSE_POINT. Exclude replacement of this backup while
                // checking it, then delete the verified object by its own handle.
                using (var handle = OpenWindowsFile(path, 0x80010000, 1, 0x02200000))
                {
                    if (handle == null) return false;
                    using (var stream = new FileStream(handle, FileAccess.Read))
                    {
                        if (!expected.Matches(ReadSnapshot(path, stream))) return false;
                        var delete = (byte)1; // FILE_DISPOSITION_INFO.DeleteFile.
                        return SetFileInformationByHandle(handle, 4, ref delete, 1);
                    }
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                                          error is ArgumentException || error is NotSupportedException)
            {
                // The save has already succeeded. The caller can report this
                // retained/unverified backup after committing the tab identity.
                return false;
            }
        }

        private static void RestoreBackupAfterFailure(string backupPath, string path, Exception failure)
        {
            if (!File.Exists(backupPath)) return;
            try { File.Move(backupPath, path); }
            catch (Exception restoreFailure) when (restoreFailure is IOException || restoreFailure is UnauthorizedAccessException)
            {
                throw new IOException("The save failed. The previous file has been preserved at '" + backupPath +
                    "'. Your unsaved document has been kept; the destination was not overwritten during recovery.", failure);
            }
        }

        private static void TryDelete(string path)
        {
            // Cleanup must not disguise the real save error or report failure
            // after a completed replacement. A blocked backup is left intact.
            try { File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private static FileStream OpenExisting(string path, FileShare sharing)
        {
            try { return new FileStream(path, FileMode.Open, FileAccess.Read, sharing); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        private static SafeFileHandle OpenWindowsFile(string path, uint access, uint sharing, uint flags)
        {
            // Metadata access (access=0) does not request file contents and is
            // compatible with an otherwise exclusively locked open document.
            var nativePath = path;
            if (!path.StartsWith(@"\\?\", StringComparison.Ordinal) && !path.StartsWith(@"\\.\", StringComparison.Ordinal))
                nativePath = path.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + path.Substring(2) : @"\\?\" + path;
            var handle = CreateFile(nativePath, access, sharing, IntPtr.Zero, 3, flags, IntPtr.Zero);
            if (!handle.IsInvalid) return handle;
            var error = Marshal.GetLastWin32Error();
            handle.Dispose();
            if (error == 2 || error == 3) return null; // Missing file/directory.
            throw new IOException("Cannot establish a safe filesystem identity for this document.", new Win32Exception(error));
        }

        private static DocumentFileSnapshot ReadSnapshot(string path, FileStream stream)
        {
            var links = 1U;
            DocumentFileIdentity? identity = IsWindows
                ? GetIdentity(stream.SafeFileHandle, out links) : null;
            using (var bytes = new MemoryStream())
            {
                stream.CopyTo(bytes);
                return new DocumentFileSnapshot(bytes.ToArray(), identity, File.GetAttributes(path), links);
            }
        }

        private static DocumentFileIdentity GetIdentity(SafeFileHandle handle, out uint links)
        {
            if (!GetFileInformationByHandle(handle, out var information))
                throw new IOException("Cannot establish a safe filesystem identity for this document.", new Win32Exception(Marshal.GetLastWin32Error()));
            links = information.NumberOfLinks;

            // FileIdInfo supports ReFS 128-bit IDs. Older filesystems can expose
            // only the legacy 64-bit file index instead.
            if (GetFileInformationByHandleEx(handle, 18, out var identity, (uint)Marshal.SizeOf<FileIdInformation>()) &&
                (identity.Low != 0 || identity.High != 0))
                return new DocumentFileIdentity(identity.VolumeSerialNumber, identity.Low, identity.High);
            if (information.FileIndexHigh == 0 && information.FileIndexLow == 0)
                throw new IOException("This filesystem does not expose a reliable document identity. Choose another folder with Save As.");
            return new DocumentFileIdentity(information.VolumeSerialNumber,
                ((ulong)information.FileIndexHigh << 32) | information.FileIndexLow, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileInformation
        {
            internal uint Attributes;
            internal System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            internal uint VolumeSerialNumber;
            internal uint FileSizeHigh;
            internal uint FileSizeLow;
            internal uint NumberOfLinks;
            internal uint FileIndexHigh;
            internal uint FileIndexLow;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileIdInformation
        {
            internal ulong VolumeSerialNumber;
            internal ulong Low;
            internal ulong High;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileInformation information);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(SafeFileHandle handle, int informationClass,
            out FileIdInformation information, uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(string path, uint access, uint sharing, IntPtr securityAttributes,
            uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFileInformationByHandle(SafeFileHandle handle, int informationClass, ref byte information, uint bufferSize);
    }
}
