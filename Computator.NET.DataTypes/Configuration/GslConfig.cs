namespace Computator.NET.DataTypes.Configuration
{
    public static class GslConfig
    {
        public static readonly string Location = RuntimeInformation.IsUnix
            ? AppInformation.Directory//hack - for Unix we just copy .so files into app directory because otherwise it doesn't seem to work. On Windows we must use temp directory because we don't have admin rights.
            : AppInformation.TempDirectory;

        public const string GslDllName = "gsl.dll";

        private static readonly string OSPrefix = RuntimeInformation.IsUnix ? "lib" : string.Empty;

        private static readonly string OSExtension = RuntimeInformation.IsUnix
            ? (RuntimeInformation.IsMacOS ? ".dylib" : ".so")
            : ".dll";

        private static string GetOSSpecificLibraryName(string baseName)
        {
            return $"{OSPrefix}{baseName}{OSExtension}";
        }

        public static readonly string GslLibraryName = GetOSSpecificLibraryName("gsl");
    }
}