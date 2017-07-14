using System.IO;
using Computator.NET.DataTypes.Configuration;

namespace Computator.NET.DataTypes.Utility
{
    public static class PathUtility
    {
        public static string GetFullPath(params string[] foldersAndFile)
        {
            return Path.Combine(AppInformation.Directory, Path.Combine(foldersAndFile));
        }
    }
}