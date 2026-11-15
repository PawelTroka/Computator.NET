#if __MonoCS__
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Win32.SafeHandles;

namespace Computator.NET.Desktop.Controls.CodeEditors
{
    // Use the runtime's own portable POSIX adapter. Native struct stat has
    // different layouts on Linux x64, Darwin x64 and Darwin arm64; guessing its
    // offsets in managed code would corrupt memory or identify the wrong file.
    internal static class MonoFileIdentity
    {
        private static readonly Lazy<Reader> Instance = new Lazy<Reader>(() => new Reader());

        internal static DocumentFileIdentity Read(SafeFileHandle handle, out uint links) => Instance.Value.Read(handle, out links);
        internal static FileStream CreatePrivateFile(string path) => Instance.Value.CreatePrivateFile(path);
        internal static uint GetPermissions(SafeFileHandle handle) => Instance.Value.GetPermissions(handle);
        internal static void SetPermissions(SafeFileHandle handle, uint permissions) => Instance.Value.SetPermissions(handle, permissions);

        private sealed class Reader
        {
            private readonly Type stat;
            private readonly MethodInfo fstat, fchmod, open, close;
            private readonly FieldInfo device, inode, count, mode;
            private readonly object createFlags;

            internal Reader()
            {
                try
                {
                    var assembly = Assembly.Load("Mono.Posix, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
                    stat = assembly.GetType("Mono.Unix.Native.Stat", true);
                    var syscall = assembly.GetType("Mono.Unix.Native.Syscall", true);
                    fstat = syscall.GetMethod("fstat", new[] { typeof(int), stat.MakeByRefType() });
                    device = stat.GetField("st_dev"); inode = stat.GetField("st_ino"); count = stat.GetField("st_nlink");
                    mode = stat.GetField("st_mode");
                    if (mode != null) fchmod = syscall.GetMethod("fchmod", new[] { typeof(int), mode.FieldType });
                    var flags = assembly.GetType("Mono.Unix.Native.OpenFlags", true);
                    if (mode != null) open = syscall.GetMethod("open", new[] { typeof(string), flags, mode.FieldType });
                    createFlags = Enum.Parse(flags, "O_WRONLY, O_CREAT, O_EXCL, O_CLOEXEC");
                    close = syscall.GetMethod("close", new[] { typeof(int) });
                    if (fstat == null || fchmod == null || open == null || close == null ||
                        device == null || inode == null || count == null || mode == null)
                        throw new MissingMemberException("Mono.Posix lacks the required portable document file APIs.");
                }
                catch (Exception error)
                {
                    throw new IOException("Safe document handling requires the matching Mono.Posix runtime component. Install the documented Mono runtime prerequisites.", error);
                }
            }

            internal DocumentFileIdentity Read(SafeFileHandle handle, out uint links)
            {
                return ReadIdentity(ReadStatus(handle), out links);
            }

            private DocumentFileIdentity ReadIdentity(object value, out uint links)
            {
                var linkCount = Convert.ToUInt64(count.GetValue(value), CultureInfo.InvariantCulture);
                if (linkCount == 0) throw new IOException("The document was unlinked while its filesystem identity was being read.");
                links = linkCount > uint.MaxValue ? uint.MaxValue : (uint)linkCount;
                return new DocumentFileIdentity(Convert.ToUInt64(device.GetValue(value), CultureInfo.InvariantCulture),
                    Convert.ToUInt64(inode.GetValue(value), CultureInfo.InvariantCulture), 0);
            }

            internal uint GetPermissions(SafeFileHandle handle) =>
                Convert.ToUInt32(mode.GetValue(ReadStatus(handle)), CultureInfo.InvariantCulture) & 0x1ff;

            internal FileStream CreatePrivateFile(string path)
            {
                try
                {
                    var descriptor = (int)open.Invoke(null, new[] { (object)path, createFlags, Enum.ToObject(mode.FieldType, 0x180) });
                    if (descriptor < 0) throw new IOException("Cannot create a private temporary document file.");
                    try
                    {
                        SetPermissions(descriptor, 0x180); // restore owner access after any umask
                        var identity = ReadIdentity(ReadStatus(descriptor), out _);
                        // Mono cannot adopt an arbitrary POSIX descriptor into
                        // FileStream. Keep the native inode open while creating
                        // a managed stream and verify both still name the same
                        // private file before any document bytes are written.
                        var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
                        try
                        {
                            if (!identity.Equals(Read(stream.SafeFileHandle, out _)))
                                throw new IOException("The temporary document file changed while it was being opened.");
                            return stream;
                        }
                        catch { stream.Dispose(); throw; }
                    }
                    finally { close.Invoke(null, new object[] { descriptor }); }
                }
                catch (TargetInvocationException error)
                {
                    throw new IOException("The Mono POSIX runtime could not create a private document file.", error.InnerException ?? error);
                }
            }

            internal void SetPermissions(SafeFileHandle handle, uint permissions)
            {
                var retained = false;
                try
                {
                    handle.DangerousAddRef(ref retained);
                    SetPermissions(handle.DangerousGetHandle().ToInt32(), permissions);
                }
                catch (TargetInvocationException error)
                {
                    throw new IOException("The Mono POSIX runtime could not set Unix document permissions.", error.InnerException ?? error);
                }
                finally { if (retained) handle.DangerousRelease(); }
            }

            private void SetPermissions(int descriptor, uint permissions)
            {
                var arguments = new[] { (object)descriptor, Enum.ToObject(mode.FieldType, permissions & 0x1ff) };
                if ((int)fchmod.Invoke(null, arguments) != 0)
                    throw new IOException("Cannot preserve Unix document permissions. The original file has not been replaced.");
            }

            private object ReadStatus(SafeFileHandle handle)
            {
                var retained = false;
                try
                {
                    handle.DangerousAddRef(ref retained);
                    return ReadStatus(handle.DangerousGetHandle().ToInt32());
                }
                catch (TargetInvocationException error)
                {
                    throw new IOException("The Mono POSIX runtime could not identify this open document.", error.InnerException ?? error);
                }
                finally { if (retained) handle.DangerousRelease(); }
            }

            private object ReadStatus(int descriptor)
            {
                var arguments = new[] { (object)descriptor, Activator.CreateInstance(stat) };
                if ((int)fstat.Invoke(null, arguments) != 0) throw new IOException("Cannot establish a safe Unix filesystem identity for this document.");
                return arguments[1];
            }
        }
    }
}
#endif
