// Computator.NET: one Cake task graph for local builds and every CI provider.
// Original target names and Task/IsDependentOn/Does pattern are retained.
using System.Xml.Linq;
using System.Text.RegularExpressions;
#load "./build/frontend-toolchain.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", EnvironmentVariable("configuration") ?? EnvironmentVariable("build_config") ?? "Release");
var suite = Argument("suite", "All");
var coverage = Argument("coverage", target == "Calculate-Coverage" || target == "Upload-Coverage");
var solution = "Computator.NET.slnx";
var webDirectory = "Computator.NET.WebClient";
var webProject = webDirectory + "/Computator.NET.WebClient.csproj";
var framework = XDocument.Load("build/ModernDesktop.props").Descendants("TargetFramework").Single().Value;
var modernRuntime = framework.StartsWith("net10.", StringComparison.Ordinal);
var suites = new Dictionary<string, int>
{
    { "Computator.NET.Core.Tests", modernRuntime ? 2118 : 225 },
    { "Computator.NET.Core.IntegrationTests", 947 },
    { "Computator.NET.WebApi.IntegrationTests", modernRuntime ? 130 : 52 },
    { "Computator.NET.Desktop.Tests", modernRuntime ? 551 : 9 }
};
if (configuration != "Debug" && configuration != "Release") throw new Exception("Configuration must be Debug or Release.");
if (suite != "All" && !suites.ContainsKey(suite)) throw new Exception("Unknown test suite: " + suite);
string temporaryProfile = null;
var previousDataDirectory = EnvironmentVariable("COMPUTATOR_DATA_DIRECTORY");
var previousFixture = EnvironmentVariable("COMPUTATOR_LEGACY_FIXTURE");
string frontendCommandPrefix = null;

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
string FrontendCommandPrefix()
{
    if (frontendCommandPrefix != null) return frontendCommandPrefix;
    var node = System.IO.File.ReadAllText(".nvmrc").Trim();
    var npm = Regex.Match(System.IO.File.ReadAllText(webDirectory + "/package.json"),
        "\"packageManager\"\\s*:\\s*\"npm@(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)\"").Groups["version"].Value;
    var installedNode = FrontendToolVersion("node", webDirectory);
    var installedNpm = FrontendToolVersion("npm", webDirectory);
    var prefix = SelectFrontendCommandPrefix(node, npm, installedNode, installedNpm);
    if (prefix.Length != 0)
        Information("Using pinned Node {0} and npm {1} from npm's local tool cache (PATH has Node {2}, npm {3}).", node, npm, installedNode, installedNpm);
    // npm exec adds both exact-version tools to the child PATH. Lifecycle scripts,
    // Vite and nested IDE builds therefore use the same Node/npm without a global
    // install, a relaxed engine policy or changes to the application lockfile.
    return frontendCommandPrefix = prefix;
}
void NodeCommand(string executable, string arguments, IDictionary<string, string> environment = null)
{
    var prefix = FrontendCommandPrefix();
    if (prefix.Length != 0)
    {
        arguments = prefix + executable + " " + arguments;
        executable = "npm";
    }
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
    StageGslPayload("artifacts/desktop");
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
    StageGslPayload("artifacts/api");
    if (modernRuntime) PowerShell("build/Test-PublishedNet10Api.ps1", "-PublishDirectory artifacts/api");
    else RequirePayload("artifacts/api", new[] { "Computator.NET.WebApi.exe", "Static/functions.xml" });
}
void PublishWeb(string directory, string apiBase = null)
{
    // Build and copy one profile together: an aggregate target can publish both
    // the direct-development client and the protected relative-API client.
    NodeCommand("npm", "run build", apiBase == null ? null : new Dictionary<string, string> { { "VITE_API_BASE_URL", apiBase } });
    CleanDirectory(directory);
    DotNetRestore(webProject, RestoreSettings());
    DotNetPublish(webProject, new DotNetPublishSettings
    {
        Configuration = configuration, NoRestore = true, OutputDirectory = directory,
        ArgumentCustomization = a => a.Append("-p:SkipFrontendBuild=true")
    });
    RequirePayload(directory, new[] { "Computator.NET.WebClient.dll", "dist/index.html" });
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
    // Source/IDE metadata checks need neither output directories nor the legacy
    // Framework fixture compiler (including on a Windows workstation).
    if (string.Equals(target, "Solution-Check", StringComparison.OrdinalIgnoreCase)) return;
    // Cake 6.3 invokes Setup during dry runs but consumes the runner switches
    // before IsDryRun() can see them. Also inspect the exact supported switches
    // so direct `dotnet cake ... --dryrun` never prepares output or test fixtures.
    if (IsDryRun() || System.Environment.GetCommandLineArgs().Any(argument =>
        argument == "--dryrun" || argument == "--noop" || argument == "--whatif")) return;
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
Task("Clean")
    .Description("Remove generated application, artifact, test and ZIP output.")
    .Does(() =>
{
    foreach (var directory in GetDirectories("Computator.NET*/bin").Concat(GetDirectories("Computator.NET*/obj")))
        DeleteDirectory(directory, new DeleteDirectorySettings { Recursive = true });
    CleanDirectories(new[] { new DirectoryPath("artifacts"), new DirectoryPath("TestResults"), new DirectoryPath("publish") });
});
Task("Restore")
    .Description("Windows: validate the solution inventory and restore all supported projects.")
    .Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("Full solution builds require Windows. Use --target=Portable-Validate or --target=Web-Validate on other platforms.");
    PowerShell("build/Assert-Solution.ps1");
    PowerShell("build/Test-ResultGuard.ps1");
    DotNetRestore(solution, RestoreSettings());
});
Task("Build")
    .Description("Windows: restore and build the complete solution; write artifacts/build.binlog.")
    .IsDependentOn("Restore").Does(() =>
    DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration, NoRestore = true,
        ArgumentCustomization = a => a.Append("-bl:artifacts/build.binlog")
    }));
Task("CoreTests")
    .Description("Windows: build and run the core unit-test suite with discovery guards.")
    .IsDependentOn("Build").Does(() => RunSuite("Computator.NET.Core.Tests"));
Task("DesktopTests")
    .Description("Windows: build and run the native desktop workflow suite.")
    .IsDependentOn("Build").Does(() => RunSuite("Computator.NET.Desktop.Tests"));
Task("CoreIntegrationTests")
    .Description("Windows: build and run the mathematical/native integration suite.")
    .IsDependentOn("Build").Does(() => RunSuite("Computator.NET.Core.IntegrationTests"));
Task("ApiTests")
    .Description("Windows: build and run the mathematical API integration suite.")
    .IsDependentOn("Build").Does(() => RunSuite("Computator.NET.WebApi.IntegrationTests"));
Task("UnitTests")
    .Description("Windows: run CoreTests and DesktopTests (the default developer target).")
    .IsDependentOn("CoreTests").IsDependentOn("DesktopTests");
Task("IntegrationTests")
    .Description("Windows: run core/native and API integration suites.")
    .IsDependentOn("CoreIntegrationTests").IsDependentOn("ApiTests");
Task("AllTests")
    .Description("Windows: build and run all four complete desktop/core/API suites.")
    .IsDependentOn("UnitTests").IsDependentOn("IntegrationTests");
Task("WebTests")
    .Description("Historical alias for ApiTests; use Web-Tests for frontend browser tests.")
    .IsDependentOn("ApiTests");
Task("SelectedTests")
    .Description("Windows: run --suite=<project> or all four suites; complete-suite guards apply.")
    .IsDependentOn("Build").Does(() =>
{
    foreach (var project in suite == "All" ? suites.Keys.ToArray() : new[] { suite }) RunSuite(project);
});
Task("Publish-Desktop")
    .Description("Windows: publish and smoke-test the desktop payload with native GSL.")
    .IsDependentOn("Build").IsDependentOn("Native-GSL").Does(PublishDesktop);
Task("Publish-Api")
    .Description("Windows: publish and smoke-test the mathematical API with native GSL.")
    .IsDependentOn("Build").IsDependentOn("Native-GSL").Does(PublishApi);
var ci = Task("CI")
    .Description("Windows: validate the selected suite and its applicable published payloads.")
    .IsDependentOn("SelectedTests");
if (suite == "All" || suite == "Computator.NET.Desktop.Tests") ci.IsDependentOn("Publish-Desktop");
if (suite == "All" || suite == "Computator.NET.WebApi.IntegrationTests") ci.IsDependentOn("Publish-Api");

//////////////////////////////////////////////////////////////////////
// FRONTEND: npm is a tool, Cake owns the task graph
//////////////////////////////////////////////////////////////////////
Task("Web-Restore")
    .Description("Install locked frontend dependencies with npm ci and audit them.")
    .Does(() =>
{
    NodeCommand("npm", "ci");
    NodeCommand("npm", "audit --audit-level=low");
});
Task("Web-Build")
    .Description("Restore frontend dependencies, type-check TypeScript and build Vite assets.")
    .IsDependentOn("Web-Restore").Does(() => NodeCommand("npm", "run build"));
Task("Web-Publish")
    .Description("Build and publish the frontend host to artifacts/webclient.")
    .IsDependentOn("Web-Restore").Does(() => PublishWeb("artifacts/webclient"));
Task("Web-Browsers")
    .Description("Restore frontend tools and install Playwright Chromium/Firefox browsers.")
    .IsDependentOn("Web-Restore").Does(() =>
    NodeCommand("npx", "playwright install " + (IsRunningOnWindows() ? "" : "--with-deps ") + "chromium firefox"));
Task("Web-Tests")
    .Description("Publish the frontend and run Chromium/Firefox UI tests with API fixtures.")
    .IsDependentOn("Web-Publish").IsDependentOn("Web-Browsers").Does(() =>
{
    var directory = MakeAbsolute(Directory("artifacts/webclient")).FullPath;
    NodeCommand("npm", "test", new Dictionary<string, string>
    {
        { "PLAYWRIGHT_SERVER_COMMAND", "dotnet \"" + directory + "/Computator.NET.WebClient.dll\" --contentRoot \"" + directory + "\" --urls http://127.0.0.1:4173" }
    });
});
Task("Web-Validate")
    .Description("Validate the published frontend through the Web-Tests browser suite.")
    .IsDependentOn("Web-Tests");

//////////////////////////////////////////////////////////////////////
// PORTABLE PACKAGING / COVERAGE
//////////////////////////////////////////////////////////////////////
Task("Package")
    .Description("Windows: publish and package desktop, API and frontend ZIP payloads.")
    .IsDependentOn("Publish-Desktop").IsDependentOn("Publish-Api").IsDependentOn("Web-Publish").Does(() =>
{
    EnsureDirectoryExists("publish");
    var declaration = Regex.Match(System.IO.File.ReadAllText("SharedAssemblyInfo.cs"),
        @"^\s*\[assembly:\s*AssemblyVersion\(""(?<version>\d+\.\d+\.\d+\.\d+)""\)\]", RegexOptions.Multiline);
    if (!declaration.Success) throw new Exception("An explicit four-part AssemblyVersion declaration was not found.");
    var version = declaration.Groups["version"].Value;
    PackagePayload("artifacts/desktop", "publish/Computator.NET.v" + version + "." + framework + ".zip", "Computator.NET.exe");
    PackagePayload("artifacts/api", "publish/Computator.NET.WebApi.v" + version + "." + framework + ".zip", "Static/functions.xml");
    PackagePayload("artifacts/webclient", "publish/Computator.NET.WebClient.v" + version + ".net10.0.zip", "dist/index.html");
});
Task("Validate")
    .Description("Windows: all application suites, frontend browser tests and ZIP packaging.")
    .IsDependentOn("AllTests").IsDependentOn("Web-Tests").IsDependentOn("Package");
Task("Publish")
    .Description("Windows: validate and prepare local ZIP packages; does not publish a release.")
    .IsDependentOn("Validate");
Task("Calculate-Coverage")
    .Description("Windows: run complete selected suites and require fresh Cobertura reports.")
    .IsDependentOn("SelectedTests").Does(() =>
{
    if (!GetFiles("TestResults/**/*.cobertura.xml").Any()) throw new Exception("No Cobertura coverage reports were produced.");
});
Task("Upload-Coverage")
    .Description("AppVeyor: run coverage and upload reports as build artifacts.")
    .IsDependentOn("Calculate-Coverage").Does(() =>
{
    if (!AppVeyor.IsRunningOnAppVeyor)
        throw new Exception("Coverage is in TestResults. GitHub uploads artifacts; external Coveralls/Codecov publication requires configured credentials.");
    foreach (var report in GetFiles("TestResults/**/*.cobertura.xml")) AppVeyor.UploadArtifact(report);
});
if (!FileExists("build/followups/installer.cake"))
    Task("Build-Installer")
        .Description("Windows: build unsigned MSI/EXE; requires explicit WiX terms.")
        .Does(() => { throw new Exception("The MSI migration module is not present. Use --target=Publish for portable ZIPs."); });
if (!FileExists("build/followups/msix.cake"))
    Task("Build-Uwp")
        .Description("Historical alias for Build-MSIX; creates the unsigned desktop MSIX.")
        .Does(() => { throw new Exception("The MSIX migration module is not present. Use --target=Publish for portable ZIPs."); });
Task("Install-Linux")
    .Description("Unsupported machine-wide install; use the explicit Mono-Build/Mono-Validate lane.")
    .Does(() => { throw new Exception("Machine-wide installation is not provided. Use Mono-Build / Mono-Validate for the reduced desktop; see docs/MONO-DESKTOP.md."); });
Task("Default")
    .Description("Windows: build the solution and run CoreTests plus DesktopTests.")
    .IsDependentOn("UnitTests");
#load "./build/followups/*.cake"
RunTarget(target);
