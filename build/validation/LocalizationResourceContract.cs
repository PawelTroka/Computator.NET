using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xml.Linq;

namespace Computator.NET.TestSupport
{
    // Shared by native Windows and Mono acceptance. Reading source XML alone
    // cannot detect missing satellites, incorrect manifest names or fallback.
    internal static class LocalizationResourceContract
    {
        public static int Verify(Assembly assembly, string sourceDirectory, string culture, string regionalCulture)
        {
            var root = Path.GetFullPath(sourceDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var neutralFiles = Directory.GetFiles(root, "*.resx", SearchOption.AllDirectories)
                .Where(path => !Path.GetFileNameWithoutExtension(path).Contains(".")).ToArray();
            Require(neutralFiles.Length > 0, "No source resources were copied to " + root);

            var language = CultureInfo.GetCultureInfo(culture);
            var region = CultureInfo.GetCultureInfo(regionalCulture);
            Require(region.Parent.Name == language.Name, "The regional culture must inherit from " + culture);
            var satellite = assembly.GetSatelliteAssembly(language);
            Require(satellite.GetName().CultureName == culture, "Wrong satellite culture for " + culture);

            var checkedStrings = 0;
            foreach (var neutralFile in neutralFiles)
            {
                var relative = neutralFile.Substring(root.Length + 1);
                var baseName = assembly.GetName().Name + "." + relative.Substring(0, relative.Length - ".resx".Length)
                    .Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
                var expected = ReadStrings(Path.ChangeExtension(neutralFile, culture + ".resx"));
                var manager = new ResourceManager(baseName, assembly);
                try
                {
                    // A fresh manager and tryParents:false prevent a cached
                    // fallback resource set from disguising a missing one.
                    var translatedSet = manager.GetResourceSet(language, true, false);
                    Require(translatedSet != null, "Missing exact resource set: " + baseName + "/" + culture);
                    foreach (var key in ReadStrings(neutralFile).Keys)
                    {
                        string translation;
                        Require(expected.TryGetValue(key, out translation), "Missing source translation: " + baseName + "/" + culture + "/" + key);
                        Require(translatedSet.GetString(key) == translation,
                            "Compiled satellite differs from source: " + baseName + "/" + culture + "/" + key);
                        Require(manager.GetString(key, region) == translation,
                            "Regional fallback differs from source: " + baseName + "/" + regionalCulture + "/" + key);
                        checkedStrings++;
                    }
                }
                finally { manager.ReleaseAllResources(); }
            }
            return checkedStrings;
        }

        private static Dictionary<string, string> ReadStrings(string path) =>
            XDocument.Load(path).Root.Elements("data")
                .Where(element => element.Attribute("type") == null && element.Attribute("mimetype") == null)
                .ToDictionary(element => (string)element.Attribute("name"), element => (string)element.Element("value") ?? "");

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
