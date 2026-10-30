using System.Globalization;
using System.Resources;

namespace Computator.NET.Localization
{
    public static class EditorSearchStrings
    {
        private static readonly ResourceManager Resources = new ResourceManager(
            "Computator.NET.Localization.EditorSearch", typeof(EditorSearchStrings).Assembly);

        public static string Get(string name) => Resources.GetString(name, CultureInfo.CurrentUICulture) ?? name;
    }
}
