using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using WixSharp;
using WixSharp.Bootstrapper;
using WixSharp.CommonTasks;
using IO = System.IO;

namespace Computator.NET.Installer
{
    internal static class ModernInstaller
    {
        // Existing MSI/Burn product family. Never generate a new upgrade identity per build.
        private static readonly Guid UpgradeCode = new Guid("86D89770-B31B-4467-BC5E-B54B0FF263A1");
        private static int Main(string[] args)
        {
            try
            {
                var options = Parse(args);
                var root = IO.Path.GetFullPath(Required(options, "root"));
                var payload = IO.Path.GetFullPath(Required(options, "payload"));
                var output = IO.Path.GetFullPath(Required(options, "output"));
                var mode = Required(options, "mode");
                var version = Version.Parse(Required(options, "version"));
                if (version.Major < 0 || version.Major > 255 || version.Minor < 0 || version.Minor > 255 ||
                    version.Build < 0 || version.Build > 65535 || version.Revision > 0)
                    throw new ArgumentException("MSI version must be major.minor.build (255.255.65535 maximum), with no nonzero fourth field.");
                if (mode != "source" && mode != "msi" && mode != "bundle") throw new ArgumentException("Unknown installer mode.");
                if (mode != "source" && (!options.TryGetValue("accept-wix7-eula", out var accepted) || accepted != "true"))
                    throw new InvalidOperationException("WiX 7 requires explicit owner acceptance of its current EULA/maintenance terms. Review docs/followups/INSTALLER.md before passing --accept-wix7-eula=true.");
                IO.Directory.CreateDirectory(output);
                ValidatePayload(payload);
                var project = CreateProject(root, payload, output, version);
                var source = project.BuildWxs();
                VerifySource(source, payload);
                var sourceDocument = XDocument.Load(source);
                sourceDocument.Save(IO.Path.Combine(output, "Computator.NET.reviewed.wxs"));
                WriteInventory(payload, output, version);
                if (mode == "source")
                {
                    VerifyIdentityRules();
                    Console.WriteLine("WixSharp source/payload/upgrade-identity checks passed; no MSI was built and no EULA was accepted.");
                    return 0;
                }
                WixTools.AcceptEulaFor = "wix7"; // Reached only through the explicit owner-controlled argument above.
                WixExtension.UI.PreferredVersion = "7.0.0";
                WixExtension.Bal.PreferredVersion = "7.0.0";
                var msi = project.BuildMsi();
                if (!IO.File.Exists(msi)) throw new IO.FileNotFoundException("The MSI compiler did not produce its output.", msi);
                if (mode == "bundle")
                {
                    var bundle = new Bundle("Computator.NET", new MsiPackage(msi) { DisplayInternalUI = true, Visible = false })
                    {
                        Version = version, UpgradeCode = UpgradeCode, Manufacturer = "TROKA Software",
                        IconFile = IO.Path.Combine(root, "Graphics/computator.net-icon-16x16-to-256x256.ico"),
                        Application = new LicenseBootstrapperApplication { LicensePath = IO.Path.Combine(root, "docs/eula.rtf") },
                        OutDir = output, OutFileName = "Computator.NET.Installer"
                    };
                    bundle.Build(IO.Path.Combine(output, "Computator.NET.Installer.exe"));
                }
                return 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        }

        private static Project CreateProject(string root, string payload, string output, Version version)
        {
            var product = new Project("Computator.NET", new Dir(new Id("INSTALLDIR"), @"%ProgramFiles%\Computator.NET", ReadDirectory(payload, payload)))
            {
                Version = version, GUID = UpgradeCode, UpgradeCode = UpgradeCode,
                ProductId = ProductId(version), Platform = Platform.x64, Scope = InstallScope.perMachine,
                UI = WUI.WixUI_Minimal, LicenceFile = IO.Path.Combine(root, "docs/eula.rtf"),
                OutDir = output, OutFileName = "Computator.NET",
                MajorUpgrade = new MajorUpgrade
                {
                    AllowDowngrades = false, AllowSameVersionUpgrades = false,
                    DowngradeErrorMessage = "A newer Computator.NET version is already installed.",
                    IgnoreRemoveFailure = false, Schedule = UpgradeSchedule.afterInstallInitialize
                }
            };
            product.ControlPanelInfo.Manufacturer = "TROKA Software";
            product.ControlPanelInfo.ProductIcon = IO.Path.Combine(root, "Graphics/computator.net-icon-16x16-to-256x256.ico");
            product.ControlPanelInfo.UrlInfoAbout = "https://github.com/PawelTroka/Computator.NET";
            product.ControlPanelInfo.HelpLink = "https://github.com/PawelTroka/Computator.NET/issues";
            product.ControlPanelInfo.NoModify = true;
            // Runtime is inside the staged product: no obsolete NetFx prerequisite inference.
            return product;
        }

        private static WixEntity[] ReadDirectory(string directory, string root)
        {
            if ((IO.File.GetAttributes(directory) & IO.FileAttributes.ReparsePoint) != 0)
                throw new IO.InvalidDataException("Refusing a redirected installer directory: " + directory);
            var entries = new List<WixEntity>();
            foreach (var path in IO.Directory.GetFiles(directory).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                if ((IO.File.GetAttributes(path) & IO.FileAttributes.ReparsePoint) != 0) throw new IO.InvalidDataException("Refusing linked installer input.");
                var file = new WixSharp.File(path);
                if (directory == root && IO.Path.GetFileName(path) == "Computator.NET.exe")
                {
                    file.Shortcuts = new[] { new FileShortcut("Computator.NET", "%ProgramMenu%"), new FileShortcut("Computator.NET", "%Desktop%") };
                    file.Associations = new[]
                    {
                        new FileAssociation("tsl") { Description = "TROKA Scripting Language script", ContentType = "text/tsl" },
                        new FileAssociation("tslf") { Description = "TROKA Scripting Language functions", ContentType = "text/tslf" }
                    };
                }
                entries.Add(file);
            }
            foreach (var child in IO.Directory.GetDirectories(directory).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                entries.Add(new Dir(IO.Path.GetFileName(child), ReadDirectory(child, root)));
            return entries.ToArray();
        }

        private static void ValidatePayload(string payload)
        {
            foreach (var name in new[] { "Computator.NET.exe", "Computator.NET.dll", "Computator.NET.runtimeconfig.json", "Computator.NET.deps.json",
                "coreclr.dll", "hostfxr.dll", "Microsoft.CodeAnalysis.CSharp.dll", "Static/functions.xml", "pl/Computator.NET.Localization.resources.dll", "CHANGELOG", "LICENSE" })
                if (!IO.File.Exists(IO.Path.Combine(payload, name))) throw new IO.FileNotFoundException("Missing self-contained installer payload entry: " + name);
            if (IO.Directory.EnumerateFiles(payload, "*", IO.SearchOption.AllDirectories).Any(path =>
                new[] { ".pfx", ".p12", ".snk", ".pem" }.Contains(IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)))
                throw new IO.InvalidDataException("Signing material must never be part of an installer payload.");
        }

        private static void VerifySource(string source, string payload)
        {
            var document = XDocument.Load(source);
            var files = document.Descendants().Where(element => element.Name.LocalName == "File")
                .Select(element => (string)element.Attribute("Source")).Where(path => path != null).Select(IO.Path.GetFullPath).ToArray();
            var expected = IO.Directory.GetFiles(payload, "*", IO.SearchOption.AllDirectories).Select(IO.Path.GetFullPath).ToArray();
            if (expected.Any(path => !files.Contains(path, StringComparer.OrdinalIgnoreCase)))
                throw new IO.InvalidDataException("WiX source lost a nested staged file.");
            var package = document.Descendants().Single(element => element.Name.LocalName == "Package");
            if (!Guid.TryParse((string)package.Attribute("UpgradeCode"), out var upgrade) || upgrade != UpgradeCode)
                throw new IO.InvalidDataException("The existing product upgrade identity was not preserved.");
            if (document.Descendants().Any(element => element.Name.LocalName.Contains("NetFx")))
                throw new IO.InvalidDataException("An obsolete Framework prerequisite leaked into the self-contained package.");
            foreach (var extension in new[] { "tsl", "tslf" })
                if (!document.Descendants().Any(element => element.Name.LocalName == "Extension" && (string)element.Attribute("Id") == extension))
                    throw new IO.InvalidDataException("Missing file association: " + extension);
        }

        private static Guid ProductId(Version version)
        {
            using (var hash = SHA256.Create()) return new Guid(hash.ComputeHash(Encoding.UTF8.GetBytes(UpgradeCode + ":x64:" + version)).Take(16).ToArray());
        }
        private static void VerifyIdentityRules()
        {
            var first = new Version(2, 4, 0);
            if (ProductId(first) != ProductId(first) || ProductId(first) == ProductId(new Version(2, 4, 1)))
                throw new IO.InvalidDataException("MSI repair/major-upgrade identity rules failed.");
        }
        private static void WriteInventory(string payload, string output, Version version)
        {
            using (var hash = SHA256.Create())
            {
                var inventory = new XElement("Payload", new XAttribute("Version", version), new XAttribute("UpgradeCode", UpgradeCode),
                    new XAttribute("ProductCode", ProductId(version)), IO.Directory.GetFiles(payload, "*", IO.SearchOption.AllDirectories).OrderBy(path => path)
                        .Select(path => new XElement("File", new XAttribute("Path", path.Substring(payload.Length).TrimStart('\\', '/').Replace('\\', '/')),
                            new XAttribute("SHA256", BitConverter.ToString(hash.ComputeHash(IO.File.ReadAllBytes(path))).Replace("-", "")))));
                inventory.Save(IO.Path.Combine(output, "payload.xml"));
            }
        }
        private static string Required(Dictionary<string, string> options, string key) =>
            options.TryGetValue(key, out var value) && value.Length != 0 ? value : throw new ArgumentException("Missing --" + key + "=value");
        private static Dictionary<string, string> Parse(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var argument in args)
            {
                var index = argument.IndexOf('=');
                if (!argument.StartsWith("--") || index < 3) throw new ArgumentException("Expected --name=value arguments.");
                result.Add(argument.Substring(2, index - 2), argument.Substring(index + 1));
            }
            return result;
        }
    }
}
