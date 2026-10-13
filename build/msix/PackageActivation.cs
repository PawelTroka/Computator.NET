using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Computator.Acceptance
{
    public static class PackageActivation
    {
        [ComImport, Guid("2E941141-7F97-4756-BA1D-9DECDE894A3D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IActivationManager
        {
            [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string id,
                [MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint processId);
            [PreserveSig] int ActivateForFile([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr items,
                [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
            [PreserveSig] int ActivateForProtocol([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr items, out uint processId);
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPackageFullName(IntPtr process, ref uint length, StringBuilder name);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(string path, IntPtr context, ref Guid iid, out IntPtr item);
        [DllImport("shell32.dll", PreserveSig = true)]
        private static extern int SHCreateShellItemArrayFromShellItem(IntPtr item, ref Guid iid, out IntPtr items);

        public static void ValidateIdentity(string applicationId, string expectedPackage)
        {
            var parts = (expectedPackage ?? "").Split('_');
            Version version;
            if (parts.Length != 5 || parts[0] != "TROKASoftware.Computator.NET" ||
                !Version.TryParse(parts[1], out version) || version.Revision < 0 ||
                parts[2] != "x64" || parts[3] != "" || parts[4].Length != 13 ||
                !string.Equals(applicationId, parts[0] + "_" + parts[4] + "!Computator.NET", StringComparison.Ordinal))
                throw new ArgumentException("Expected the installed Computator.NET package and application identity.");
        }
        public static void ValidateDocument(string document)
        {
            if (document == null) return;
            var extension = Path.GetExtension(document);
            if (!Path.IsPathFullyQualified(document) || !File.Exists(document) ||
                (!string.Equals(extension, ".tsl", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(extension, ".tslf", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("The activation document must be an existing absolute TSL or TSLF path.");
        }
        public static uint LaunchAndVerify(string applicationId, string expectedPackage, string document)
        {
            ValidateIdentity(applicationId, expectedPackage);
            ValidateDocument(document);
            var type = Type.GetTypeFromCLSID(new Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C"), true);
            var manager = (IActivationManager)Activator.CreateInstance(type);
            IntPtr item = IntPtr.Zero, items = IntPtr.Zero;
            uint pid;
            try
            {
                if (document == null) Marshal.ThrowExceptionForHR(manager.ActivateApplication(applicationId, "", 0, out pid));
                else
                {
                    var shellItem = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
                    var shellArray = new Guid("B63EA76D-1F85-456F-A19C-48159EFA858B");
                    Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(document, IntPtr.Zero, ref shellItem, out item));
                    Marshal.ThrowExceptionForHR(SHCreateShellItemArrayFromShellItem(item, ref shellArray, out items));
                    Marshal.ThrowExceptionForHR(manager.ActivateForFile(applicationId, items, "open", out pid));
                }
            }
            finally
            {
                if (items != IntPtr.Zero) Marshal.Release(items);
                if (item != IntPtr.Zero) Marshal.Release(item);
                Marshal.ReleaseComObject(manager);
            }
            // A returned PID alone is not proof of package-identity activation.
            using (var process = Process.GetProcessById(checked((int)pid)))
            {
                uint length = 0;
                var status = GetPackageFullName(process.Handle, ref length, null);
                if (status != 122 || length == 0 || length > 4096) throw new Win32Exception(status);
                var fullName = new StringBuilder((int)length);
                status = GetPackageFullName(process.Handle, ref length, fullName);
                if (status != 0) throw new Win32Exception(status);
                if (!string.Equals(fullName.ToString(), expectedPackage, StringComparison.Ordinal))
                    throw new InvalidOperationException("Activated process has an unexpected package identity.");
            }
            return pid;
        }
    }
}
