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

        private sealed class Reader
        {
            private readonly Type stat;
            private readonly MethodInfo fstat;
            private readonly FieldInfo device, inode, count;

            internal Reader()
            {
                try
                {
                    var assembly = Assembly.Load("Mono.Posix, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
                    stat = assembly.GetType("Mono.Unix.Native.Stat", true);
                    fstat = assembly.GetType("Mono.Unix.Native.Syscall", true).GetMethod("fstat", new[] { typeof(int), stat.MakeByRefType() });
                    device = stat.GetField("st_dev"); inode = stat.GetField("st_ino"); count = stat.GetField("st_nlink");
                    if (fstat == null || device == null || inode == null || count == null)
                        throw new MissingMemberException("Mono.Posix lacks the required portable file-identity API.");
                }
                catch (Exception error)
                {
                    throw new IOException("Safe document handling requires the matching Mono.Posix runtime component. Install the documented Mono runtime prerequisites.", error);
                }
            }

            internal DocumentFileIdentity Read(SafeFileHandle handle, out uint links)
            {
                var retained = false;
                try
                {
                    handle.DangerousAddRef(ref retained);
                    var arguments = new[] { (object)handle.DangerousGetHandle().ToInt32(), Activator.CreateInstance(stat) };
                    if ((int)fstat.Invoke(null, arguments) != 0) throw new IOException("Cannot establish a safe Unix filesystem identity for this document.");
                    var value = arguments[1];
                    var linkCount = Convert.ToUInt64(count.GetValue(value), CultureInfo.InvariantCulture);
                    if (linkCount == 0) throw new IOException("The document was unlinked while its filesystem identity was being read.");
                    links = linkCount > uint.MaxValue ? uint.MaxValue : (uint)linkCount;
                    return new DocumentFileIdentity(Convert.ToUInt64(device.GetValue(value), CultureInfo.InvariantCulture),
                        Convert.ToUInt64(inode.GetValue(value), CultureInfo.InvariantCulture), 0);
                }
                catch (TargetInvocationException error)
                {
                    throw new IOException("The Mono POSIX runtime could not identify this open document.", error.InnerException ?? error);
                }
                finally { if (retained) handle.DangerousRelease(); }
            }
        }
    }
}
#endif
