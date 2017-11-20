using System;
using System.Runtime.InteropServices;

namespace Computator.NET.DataTypes.Configuration
{
    public static class RuntimeInformation
    {
        private const uint PRODUCT_CLOUD = 0x000000B2; // Windows 10S
        private const uint PRODUCT_CLOUDN = 0x000000B3; // Windows 10S N edition

        [DllImport("Kernel32.dll")]
        public static extern bool GetProductInfo([In] uint dwOSMajorVersion, [In] uint dwOSMinorVersion,
            [In] uint dwSpMajorVersion, [In] uint dwSpMinorVersion, [Out] out uint pdwReturnedProductType);

        public static bool IsWindows10S
        {
            get
            {
                if (IsUnix)
                    return false;

                if (Environment.OSVersion.Version.Major < 10)
                    return false;

                GetProductInfo((uint) Environment.OSVersion.Version.Major, (uint) Environment.OSVersion.Version.Minor,
                    (uint) Environment.OSVersion.Version.MajorRevision,
                    (uint) Environment.OSVersion.Version.MinorRevision, out var productType);
                return productType == PRODUCT_CLOUD || productType == PRODUCT_CLOUDN;
            }
        }

        public static bool IsUnix
        {
            get
            {
                var platform = Environment.OSVersion.Platform;             
                var p = (int)platform;

                if (p == 4 || p == 6 || p == 128)
                    return true;
                return platform == PlatformID.MacOSX || platform == PlatformID.Unix;
            }
        }

        public static bool IsMacOS => Environment.OSVersion.Platform == PlatformID.MacOSX;
        public static bool IsLinux => IsUnix && !IsMacOS;
        public static bool IsWindows => !IsUnix;

        public static bool Is64Bit => Environment.Is64BitProcess && IntPtr.Size == 8;
        public static bool Is32Bit => !Environment.Is64BitProcess && IntPtr.Size == 4;

    }
}