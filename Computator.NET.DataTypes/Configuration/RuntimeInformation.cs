using System;

namespace Computator.NET.DataTypes.Configuration
{
    public static class RuntimeInformation
    {
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