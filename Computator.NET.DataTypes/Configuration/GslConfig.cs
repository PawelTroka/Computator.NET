namespace Computator.NET.DataTypes
{
    public static class GslConfig
    {
        public const string GslDllName = "gsl.dll";
        public static readonly string GslLibraryName = GetOSSpecificLibraryName("gsl");

        public static readonly string CblasLibraryName = RuntimeInformation.IsUnix
            ? GetOSSpecificLibraryName("gslcblas")
            : GetOSSpecificLibraryName("cblas");

        private static string GetOSSpecificLibraryName(string baseName)
        {
            return $"{OSPrefix}{baseName}{OSBitSuffix}{OSExtension}";
        }

        private static readonly string OSBitSuffix = (RuntimeInformation.Is64Bit && RuntimeInformation.IsUnix) ? "64" : string.Empty;

        private static readonly string OSPrefix = RuntimeInformation.IsUnix ? "lib" : string.Empty;

        private static readonly string OSExtension = RuntimeInformation.IsUnix
            ? (RuntimeInformation.IsMacOS ? ".dylib" : ".so")
            : ".dll";
    }
}