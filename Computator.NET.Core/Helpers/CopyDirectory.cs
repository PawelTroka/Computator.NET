using System.IO;

namespace Computator.NET.Core.Helpers
{
    internal class CopyDirectory
    {
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourceDirectory, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourceDirectory, targetDirectory));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourceDirectory, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourceDirectory, targetDirectory), true);
        }
    }
}