// Computator.NET: one Cake task graph for local builds and every CI provider.
// Original target names and Task/IsDependentOn/Does pattern are retained.
using System.Xml.Linq;
using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", EnvironmentVariable("configuration") ?? EnvironmentVariable("build_config") ?? "Release");
var suite = Argument("suite", "All");
var coverage = Argument("coverage", target == "Calculate-Coverage" || target == "Upload-Coverage");
var solution = "Computator.NET.sln";
var webDirectory = "Computator.NET.WebClient";
var webProject = webDirectory + "/Computator.NET.WebClient.csproj";
var framework = XDocument.Load("build/ModernDesktop.props").Descendants("TargetFramework").Single().Value;
var modernRuntime = framework.StartsWith("net10.", StringComparison.Ordinal);
var suites = new Dictionary<string, int>
{
    { "Computator.NET.Core.Tests", modernRuntime ? 252 : 225 },
    { "Computator.NET.Core.IntegrationTests", 947 },
    { "Computator.NET.WebApi.IntegrationTests", modernRuntime ? 53 : 52 },
    { "Computator.NET.Desktop.Tests", modernRuntime ? 20 : 9 }
};
if (configuration != "Debug" && configuration != "Release") throw new Exception("Configuration must be Debug or Release.");
if (suite != "All" && !suites.ContainsKey(suite)) throw new Exception("Unknown test suite: " + suite);
string temporaryProfile = null;
var previousDataDirectory = EnvironmentVariable("COMPUTATOR_DATA_DIRECTORY");
var previousFixture = EnvironmentVariable("COMPUTATOR_LEGACY_FIXTURE");

void CheckedProcess(string executable, ProcessSettings settings)
{
    var exitCode = StartProcess(executable, settings);
    if (exitCode != 0) throw new Exception(executable + " failed with exit code " + exitCode + ".");
}
void PowerShell(string script, string arguments = "", string executable = "pwsh")
{
    var args = new ProcessArgumentBuilder().Append("-NoProfile").Append("-File").AppendQuoted(script).Append(arguments);
    CheckedProcess(executable, new ProcessSettings { Arguments = args });
}
void NodeCommand(string executable, string arguments, IDictionary<string, string> environment = null)
{
    // Windows npm/npx launchers are command scripts. Arguments are repository-owned.
    CheckedProcess(IsRunningOnWindows() ? "cmd.exe" : executable, new ProcessSettings
    {
        Arguments = IsRunningOnWindows() ? "/d /c " + executable + " " + arguments : arguments,
        WorkingDirectory = webDirectory, EnvironmentVariables = environment
    });
}
DotNetRestoreSettings RestoreSettings() => new DotNetRestoreSettings
{
    ArgumentCustomization = a => a.Append("-p:WarningsAsErrors=NU1900%3BNU1901%3BNU1902%3BNU1903%3BNU1904")
};
void RunSuite(string project)
{
    var report = "TestResults/" + project + ".trx";
    if (FileExists(report)) DeleteFile(report);
    var previousCoverage = GetFiles("TestResults/**/*.cobertura.xml").Select(p => p.FullPath).ToArray();
    DotNetTest(project + "/" + project + ".csproj", new DotNetTestSettings
    {
        Configuration = configuration, NoBuild = true, NoRestore = true,
        ResultsDirectory = "TestResults",
        Loggers = new[] { "console;verbosity=normal", "trx;LogFileName=" + project + ".trx" },
        ArgumentCustomization = a =>
        {
            a.Append("--blame-hang-timeout 120s --blame-hang-dump-type none");
            if (coverage) a.Append("--collect").AppendQuoted("Code Coverage;Format=Cobertura");
            return a;
        }
    });
    PowerShell("build/Assert-SuiteResults.ps1", "-Suite " + project + " -MinimumTests " + suites[project]);
    if (coverage && !GetFiles("TestResults/**/*.cobertura.xml").Any(p => !previousCoverage.Contains(p.FullPath)))
        throw new Exception("The coverage collector did not produce a new Cobertura report for " + project + ".");
}
void RequirePayload(string directory, IEnumerable<string> paths)
{
    foreach (var path in paths)
        if (!FileExists(directory + "/" + path)) throw new Exception("Missing published file: " + directory + "/" + path);
}
void PublishDesktop()
{
    CleanDirectory("artifacts/desktop");
    DotNetPublish("Computator.NET/Computator.NET.csproj", new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = "artifacts/desktop"
    });
    RequirePayload("artifacts/desktop", new[] { "Computator.NET.exe", "Microsoft.CodeAnalysis.CSharp.dll", "MathNet.Numerics.dll",
        "Meta.Numerics.dll", "Static/functions.xml", "Static/TSL-Syntax.xshd", "pl/Computator.NET.Localization.resources.dll", "CHANGELOG" });
    if (modernRuntime)
    {
        RequirePayload("artifacts/desktop", new[] { "Computator.NET.dll", "Computator.NET.runtimeconfig.json", "Computator.NET.deps.json" });
        PowerShell("build/Test-PublishedNet10Desktop.ps1", "-PublishDirectory artifacts/desktop");
    }
    else RequirePayload("artifacts/desktop", new[] { "Computator.NET.exe.config" });
}
void PublishApi()
{
    CleanDirectory("artifacts/api");
    DotNetPublish("Computator.NET.WebApi/Computator.NET.WebApi.csproj", new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = "artifacts/api"
    });
    if (modernRuntime) PowerShell("build/Test-PublishedNet10Api.ps1", "-PublishDirectory artifacts/api");
    else RequirePayload("artifacts/api", new[] { "Computator.NET.WebApi.exe", "Static/functions.xml" });
}
void PackagePayload(string directory, string fileName, string requiredEntry)
{
    if (FileExists(fileName)) DeleteFile(fileName);
    Zip(directory, fileName);
    using (var archive = System.IO.Compression.ZipFile.OpenRead(fileName))
    {
        if (!archive.Entries.Any(entry => entry.FullName.Replace('\\', '/') == requiredEntry))
            throw new Exception("The ZIP is missing its root payload entry: " + requiredEntry);
    }
}

Setup(context =>
{
    EnsureDirectoryExists("artifacts");
    EnsureDirectoryExists("TestResults");
    if (IsRunningOnWindows())
    {
        temporaryProfile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Computator-Cake-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(temporaryProfile);
        System.Environment.SetEnvironmentVariable("COMPUTATOR_DATA_DIRECTORY", System.IO.Path.Combine(temporaryProfile, "profile"));
        if (modernRuntime)
        {
            var fixture = System.IO.Path.Combine(temporaryProfile, "fixture/settings.dat");
            System.Environment.SetEnvironmentVariable("COMPUTATOR_LEGACY_FIXTURE", fixture);
            PowerShell("build/Generate-LegacySettingsFixture.ps1", "-OutputPath \"" + fixture + "\"", "powershell.exe");
        }
    }
});
Teardown(context =>
{
    System.Environment.SetEnvironmentVariable("COMPUTATOR_DATA_DIRECTORY", previousDataDirectory);
    System.Environment.SetEnvironmentVariable("COMPUTATOR_LEGACY_FIXTURE", previousFixture);
    if (temporaryProfile != null && System.IO.Directory.Exists(temporaryProfile))
    {
        try { System.IO.Directory.Delete(temporaryProfile, true); }
        catch (System.IO.IOException ex) { Warning("Temporary test profile could not be removed: " + ex.Message); }
    }
});

//////////////////////////////////////////////////////////////////////
// DESKTOP / API (original target names)
//////////////////////////////////////////////////////////////////////
Task("Clean").Does(() =>
{
    foreach (var directory in GetDirectories("Computator.NET*/bin").Concat(GetDirectories("Computator.NET*/obj")))
        DeleteDirectory(directory, new DeleteDirectorySettings { Recursive = true });
    CleanDirectories(new[] { new DirectoryPath("artifacts"), new DirectoryPath("TestResults"), new DirectoryPath("publish") });
});
Task("Restore").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("Desktop/API require Windows. Use --target=Web-Validate for the independent web client.");
    var xmlProjects = XDocument.Load("Computator.NET.Modern.slnx").Descendants("Project")
        .Select(p => p.Attribute("Path").Value.Replace('\\', '/')).OrderBy(p => p).ToArray();
    var slnProjects = Regex.Matches(System.IO.File.ReadAllText(solution), "\"([^\"\\r\\n]+\\.csproj)\"")
        .Cast<Match>().Select(m => m.Groups[1].Value.Replace('\\', '/')).OrderBy(p => p).ToArray();
    if (!xmlProjects.SequenceEqual(slnProjects)) throw new Exception("The .sln and .slnx project lists differ.");
    PowerShell("build/Test-ResultGuard.ps1");
    DotNetRestore(solution, RestoreSettings());
});
Task("Build").IsDependentOn("Restore").Does(() =>
    DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration, NoRestore = true,
        ArgumentCustomization = a => a.Append("-bl:artifacts/build.binlog")
    }));
Task("CoreTests").IsDependentOn("Build").Does(() => RunSuite("Computator.NET.Core.Tests"));
Task("DesktopTests").IsDependentOn("Build").Does(() => RunSuite("Computator.NET.Desktop.Tests"));
Task("CoreIntegrationTests").IsDependentOn("Build").Does(() => RunSuite("Computator.NET.Core.IntegrationTests"));
Task("ApiTests").IsDependentOn("Build").Does(() => RunSuite("Computator.NET.WebApi.IntegrationTests"));
Task("UnitTests").IsDependentOn("CoreTests").IsDependentOn("DesktopTests");
Task("IntegrationTests").IsDependentOn("CoreIntegrationTests").IsDependentOn("ApiTests");
Task("AllTests").IsDependentOn("UnitTests").IsDependentOn("IntegrationTests");
Task("WebTests").IsDependentOn("ApiTests");
Task("SelectedTests").IsDependentOn("Build").Does(() =>
{
    foreach (var project in suite == "All" ? suites.Keys.ToArray() : new[] { suite }) RunSuite(project);
});
Task("Publish-Desktop").IsDependentOn("Build").Does(PublishDesktop);
Task("Publish-Api").IsDependentOn("Build").Does(PublishApi);
var ci = Task("CI").IsDependentOn("SelectedTests");
if (suite == "All" || suite == "Computator.NET.Desktop.Tests") ci.IsDependentOn("Publish-Desktop");
if (suite == "All" || suite == "Computator.NET.WebApi.IntegrationTests") ci.IsDependentOn("Publish-Api");

//////////////////////////////////////////////////////////////////////
// FRONTEND: npm is a tool, Cake owns the task graph
//////////////////////////////////////////////////////////////////////
Task("Web-Restore").Does(() =>
{
    NodeCommand("npm", "ci");
    NodeCommand("npm", "audit --audit-level=low");
});
Task("Web-Build").IsDependentOn("Web-Restore").Does(() => NodeCommand("npm", "run build"));
Task("Web-Publish").IsDependentOn("Web-Build").Does(() =>
{
    CleanDirectory("artifacts/webclient");
    DotNetRestore(webProject, RestoreSettings());
    DotNetPublish(webProject, new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = "artifacts/webclient",
        ArgumentCustomization = a => a.Append("-p:SkipFrontendBuild=true")
    });
    RequirePayload("artifacts/webclient", new[] { "Computator.NET.WebClient.dll", "dist/index.html" });
});
Task("Web-Browsers").IsDependentOn("Web-Restore").Does(() =>
    NodeCommand("npx", "playwright install " + (IsRunningOnWindows() ? "" : "--with-deps ") + "chromium firefox"));
Task("Web-Tests").IsDependentOn("Web-Publish").IsDependentOn("Web-Browsers").Does(() =>
{
    var directory = MakeAbsolute(Directory("artifacts/webclient")).FullPath;
    NodeCommand("npm", "test", new Dictionary<string, string>
    {
        { "PLAYWRIGHT_SERVER_COMMAND", "dotnet \"" + directory + "/Computator.NET.WebClient.dll\" --contentRoot \"" + directory + "\" --urls http://127.0.0.1:4173" }
    });
});
Task("Web-Validate").IsDependentOn("Web-Tests");

//////////////////////////////////////////////////////////////////////
// PORTABLE PACKAGING / COVERAGE
//////////////////////////////////////////////////////////////////////
Task("Package").IsDependentOn("Publish-Desktop").IsDependentOn("Publish-Api").IsDependentOn("Web-Publish").Does(() =>
{
    EnsureDirectoryExists("publish");
    // Anchored declaration: never read the commented-out wildcard example above it.
    var declaration = Regex.Match(System.IO.File.ReadAllText("SharedAssemblyInfo.cs"),
        @"^\s*\[assembly:\s*AssemblyVersion\(""(?<version>\d+\.\d+\.\d+\.\d+)""\)\]", RegexOptions.Multiline);
    if (!declaration.Success) throw new Exception("An explicit four-part AssemblyVersion declaration was not found.");
    var version = declaration.Groups["version"].Value;
    PackagePayload("artifacts/desktop", "publish/Computator.NET.v" + version + "." + framework + ".zip", "Computator.NET.exe");
    PackagePayload("artifacts/api", "publish/Computator.NET.WebApi.v" + version + "." + framework + ".zip", "Static/functions.xml");
    PackagePayload("artifacts/webclient", "publish/Computator.NET.WebClient.v" + version + ".net10.0.zip", "dist/index.html");
});
Task("Validate").IsDependentOn("AllTests").IsDependentOn("Web-Tests").IsDependentOn("Package");
Task("Publish").IsDependentOn("Validate");
Task("Calculate-Coverage").IsDependentOn("SelectedTests").Does(() =>
{
    if (!GetFiles("TestResults/**/*.cobertura.xml").Any()) throw new Exception("No Cobertura coverage reports were produced.");
});
Task("Upload-Coverage").IsDependentOn("Calculate-Coverage").Does(() =>
{
    if (!AppVeyor.IsRunningOnAppVeyor)
        throw new Exception("Coverage is in TestResults. GitHub uploads artifacts; external Coveralls/Codecov publication requires configured credentials.");
    foreach (var report in GetFiles("TestResults/**/*.cobertura.xml")) AppVeyor.UploadArtifact(report);
});
// Keep unsupported historical targets explicit: never silently ship old prerequisites.
Task("Build-Installer").Does(() => { throw new Exception("The old WiX installer needs migrated prerequisites/payload validation. Use --target=Publish for tested portable ZIPs; see docs/BUILD.md."); });
Task("Build-Uwp").Does(() => { throw new Exception("Historical UWP/Windows S packaging is not compatible with this runtime. Use --target=Publish; see docs/BUILD.md."); });
Task("Install-Linux").Does(() => { throw new Exception("Machine-wide Mono installation is retired. Install the pinned SDK/Node and use --target=Web-Validate."); });
Task("Default").IsDependentOn("UnitTests");
RunTarget(target);
