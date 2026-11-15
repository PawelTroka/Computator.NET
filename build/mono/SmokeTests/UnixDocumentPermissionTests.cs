using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Computator.NET.Desktop.Controls.CodeEditors;

internal static partial class Program
{
    // Invoked by the existing Unix document acceptance case on Linux and macOS.
    // UnixFileInfo independently observes the resulting path; production uses
    // open-descriptor fstat/fchmod through the Mono POSIX adapter.
    private static void UnixDocumentPermissions()
    {
        var safety = typeof(TextEditorCodeEditor).Assembly.GetType("Computator.NET.Desktop.Controls.CodeEditors.DocumentFileSafety", true);
        var capture = safety.GetMethod("Capture", BindingFlags.Static | BindingFlags.NonPublic);
        var write = safety.GetMethod("Write", BindingFlags.Static | BindingFlags.NonPublic);
        var directory = Path.Combine(Path.GetTempPath(), "computator-mono-permissions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var originalMask = PermissionUmask(0x12); // 0022
        try
        {
            Action<Stream, string> privateWriter = (stream, contents) =>
            {
                var staged = Directory.GetFiles(directory, ".computator-save-*.tmp");
                Check(staged.Length == 1 && PermissionMode(staged[0]) == 0x180,
                    "Document text must be staged with owner-only read/write permissions.");
                var bytes = Encoding.UTF8.GetBytes(contents);
                stream.Write(bytes, 0, bytes.Length);
            };
            foreach (var octal in new[] { "600", "640", "660", "750" })
            {
                var path = Path.Combine(directory, "mode-" + octal + ".tsl");
                File.WriteAllText(path, "old source");
                var permissions = Convert.ToUInt32(octal, 8);
                SetPermissionMode(path, permissions);
                var baseline = capture.Invoke(null, new object[] { path });
                baseline = write.Invoke(null, new object[] { path, "saved π", baseline, privateWriter, null, null });
                Check(PermissionMode(path) == permissions, "Saving changed Unix mode " + octal + ".");
                Equal(File.ReadAllText(path), "saved π");
                write.Invoke(null, new object[] { path, "second save", baseline, null, null, null });
                Check(PermissionMode(path) == permissions, "A repeated save changed Unix mode " + octal + ".");
            }

            // Save As captures its chosen destination. Keep that destination's
            // mode instead of the mode of the source document being copied.
            var source = Path.Combine(directory, "source.tsl");
            var destination = Path.Combine(directory, "overwrite.tslf");
            File.WriteAllText(source, "source document");
            File.WriteAllText(destination, "old destination");
            SetPermissionMode(source, 0x180); // 0600
            SetPermissionMode(destination, 0x1b4); // 0664
            write.Invoke(null, new object[] { destination, "saved copy", capture.Invoke(null, new object[] { destination }), null, null, null });
            Check(PermissionMode(destination) == 0x1b4, "Save As overwrite lost the destination's group permissions.");
            Equal(File.ReadAllText(source), "source document");
            Check(PermissionMode(source) == 0x180, "Save As changed the original file's permissions.");

            var changed = Path.Combine(directory, "external-chmod.tsl");
            File.WriteAllText(changed, "original");
            SetPermissionMode(changed, 0x1a4); // 0644
            var original = capture.Invoke(null, new object[] { changed });
            Action chmod = () => SetPermissionMode(changed, 0x1a0); // 0640
            original = write.Invoke(null, new object[] { changed, "saved", original, null, chmod, null });
            Check(PermissionMode(changed) == 0x1a0, "Save restored a stale mode over an external chmod.");

            Action makeReadOnly = () => SetPermissionMode(changed, 0x120); // 0440
            var failure = Expect<TargetInvocationException>(() =>
                write.Invoke(null, new object[] { changed, "must not overwrite", original, null, makeReadOnly, null }));
            Check(failure.InnerException is UnauthorizedAccessException, "Making the destination read-only must reject the save.");
            Equal(File.ReadAllText(changed), "saved");
            Check(PermissionMode(changed) == 0x120, "A refused save changed read-only permissions.");

            foreach (var mask in new uint[] { 0x12, 0x3f, 0x13f, 0x1ff }) // 0022, 0077, 0477, 0777
            {
                PermissionUmask(mask);
                var path = Path.Combine(directory, "new-" + mask + ".tsl");
                write.Invoke(null, new object[] { path, "new document", capture.Invoke(null, new object[] { path }), privateWriter, null, null });
                Check(PermissionMode(path) == (0x1b6 & ~mask), "A new document must retain the process's umask defaults.");
            }
            PermissionUmask(0x12);

            foreach (var octal in new[] { "6750", "1750" })
            {
                var path = Path.Combine(directory, "special-" + octal + ".tsl");
                File.WriteAllText(path, "original");
                SetPermissionMode(path, Convert.ToUInt32(octal, 8));
                write.Invoke(null, new object[] { path, "edited content", capture.Invoke(null, new object[] { path }), null, null, null });
                Check(PermissionMode(path) == 0x1e8, "Edited contents must retain ordinary 0750 permissions without special bits.");
            }

            var interrupted = Path.Combine(directory, "interrupted.tsl");
            File.WriteAllText(interrupted, "keep me");
            SetPermissionMode(interrupted, 0x1a0); // 0640
            Action<Stream, string> failStaging = (stream, contents) =>
            {
                privateWriter(stream, contents);
                throw new IOException("Simulated interrupted write.");
            };
            failure = Expect<TargetInvocationException>(() => write.Invoke(null, new object[]
            {
                interrupted, "unfinished", capture.Invoke(null, new object[] { interrupted }), failStaging, null, null
            }));
            Check(failure.InnerException is IOException, "Staging failure was not propagated.");
            Equal(File.ReadAllText(interrupted), "keep me");
            Check(PermissionMode(interrupted) == 0x1a0, "An interrupted save changed the original mode.");
            Check(Directory.GetFiles(directory, ".computator-*.tmp").Length == 0, "Permission tests left temporary save or backup files.");
            Console.WriteLine("  verified Unix save modes, private staging, overwrite, external chmod, umask, special bits and failure cleanup");
        }
        finally
        {
            PermissionUmask(originalMask);
            foreach (var path in Directory.GetFiles(directory)) SetPermissionMode(path, 0x180);
            Directory.Delete(directory, true);
        }
    }

    private static uint PermissionMode(string path)
    {
        var posix = Assembly.Load("Mono.Posix, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
        var type = posix.GetType("Mono.Unix.UnixFileInfo", true);
        var info = Activator.CreateInstance(type, path);
        return Convert.ToUInt32(type.GetProperty("Protection").GetValue(info, null)) & 0xfff;
    }

    private static void SetPermissionMode(string path, uint mode)
    {
        if (PermissionChmod(path, mode) != 0)
            throw new IOException("Cannot set Unix permissions for the isolated document fixture.");
    }

    [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
    private static extern int PermissionChmod(string path, uint mode);

    [DllImport("libc", EntryPoint = "umask")]
    private static extern uint PermissionUmask(uint mask);
}
