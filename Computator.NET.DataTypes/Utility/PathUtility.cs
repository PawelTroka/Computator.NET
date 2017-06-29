using System.IO;

namespace Computator.NET.DataTypes
{
    public static class PathUtility
    {
        public static string GetFullPath(params string[] foldersAndFile)
        {
            return Path.Combine(AppInformation.Directory, Path.Combine(foldersAndFile));
        }
    }
}