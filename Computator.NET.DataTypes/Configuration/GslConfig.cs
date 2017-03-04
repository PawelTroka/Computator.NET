namespace Computator.NET.DataTypes
{
    public static class GslConfig
    {
        public static readonly string GslDllName = RuntimeInformation.IsUnix
            ? (RuntimeInformation.IsMacOS ? "libgsl.dylib" : "libgsl.so")
            : "gsl.dll";

        public static readonly string GslCblasDllName = RuntimeInformation.IsUnix
            ? (RuntimeInformation.IsMacOS ? "libgslcblas.dylib" : "libgslcblas.so")
            : "cblas.dll";
    }
}