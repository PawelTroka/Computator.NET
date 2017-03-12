using System;
using System.IO;
using System.Reflection;
using Computator.NET.DataTypes.Localization;

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