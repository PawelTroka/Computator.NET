using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Xml;
using System.Xml.Linq;
using Computator.NET.TestSupport;
var source = args[0];
var assembly = Assembly.GetExecutingAssembly();
int failed = 0;
foreach (var culture in new[] { "de", "cs" })
{
    if (args.Length < 2 || args[1] != "contract-only")
    {
        var manager = new ResourceManager("LocalizationNewlineFixture.Strings", assembly);
        var actual = manager.GetString("featuresInclude", CultureInfo.GetCultureInfo(culture));
        var path = Path.Combine(source, "Strings." + culture + ".resx");
        var ordinary = (string)XDocument.Load(path).Root.Elements("data").Single(e => (string)e.Attribute("name") == "featuresInclude").Element("value");
        using (var reader = new XmlTextReader(path) { Normalization = false, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null })
        {
            var preserved = (string)XDocument.Load(reader, LoadOptions.PreserveWhitespace).Root.Elements("data").Single(e => (string)e.Attribute("name") == "featuresInclude").Element("value");
            Console.WriteLine($"{culture}: compiled len={actual.Length}, CR={actual.Count(c=>c=='\r')}, LF={actual.Count(c=>c=='\n')}; normal XML len={ordinary.Length}; preserved exact match={preserved==actual}");
        }
    }
    try { Console.WriteLine($"PASS {culture}: {LocalizationResourceContract.Verify(assembly, source, culture, culture == "de" ? "de-DE" : "cs-CZ")} exact strings and regional fallback"); }
    catch (Exception error) { failed++; Console.WriteLine("FAIL " + error.GetType().Name + ": " + error.Message); }
}
Console.WriteLine($"Contract failures: {failed}");
return failed == 0 ? 0 : 1;
