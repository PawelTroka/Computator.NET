// Independent managed browser application; no Windows-only task dependencies.
var portableWebRid = Argument("portable-web-rid", "");
var portableWebOutput = Argument("portable-web-output", "artifacts/portable-web/" + portableWebRid);
var portableWebEvidence = "artifacts/portable-web-evidence/" + portableWebRid;
void RequirePortableWebRid()
{
    if (!new[] { "osx-x64", "osx-arm64", "linux-x64", "linux-arm64" }.Contains(portableWebRid))
        throw new Exception("Specify --portable-web-rid=osx-x64|osx-arm64|linux-x64|linux-arm64.");
}
Task("PortableWeb-Publish")
    .Description("Any OS: publish the independent portable local browser preview for an explicit RID.")
    .IsDependentOn("Web-Restore").Does(() =>
{
    RequirePortableWebRid();
    NodeCommand("npm", "run build -- --outDir dist-portable", new Dictionary<string, string>
    { { "VITE_API_BASE_URL", "/api" }, { "VITE_PORTABLE_LOCAL", "true" } });
    DotNetPublish("Computator.NET.PortableWeb/Computator.NET.PortableWeb.csproj", new DotNetPublishSettings
    {
        Configuration = configuration, Runtime = portableWebRid, SelfContained = false,
        OutputDirectory = portableWebOutput,
        ArgumentCustomization = a => a.Append("-p:WarningsAsErrors=NU1900%3BNU1901%3BNU1902%3BNU1903%3BNU1904")
    });
});
Task("PortableWeb-HTTP")
    .Description("Linux/macOS: exercise the actual published portable apphost and worker lifetimes.")
    .IsDependentOn("PortableWeb-Publish").Does(() =>
{
    var arguments = new ProcessArgumentBuilder().Append("build/portable-web/smoke.py")
        .Append("--publish-directory").AppendQuoted(MakeAbsolute(Directory(portableWebOutput)).FullPath)
        .Append("--expected-rid").Append(portableWebRid)
        .Append("--expected-architecture").Append(portableWebRid.EndsWith("arm64") ? "Arm64" : "X64")
        .Append("--output").AppendQuoted(portableWebEvidence);
    CheckedProcess("python3", new ProcessSettings { Arguments = arguments });
});
Task("PortableWeb-Browsers")
    .Description("Install Chromium/WebKit for native portable local application acceptance.")
    .IsDependentOn("Web-Restore").Does(() =>
        NodeCommand("npx", "playwright install " + (IsRunningOnWindows() ? "" : "--with-deps ") + "chromium webkit"));
Task("PortableWeb-Validate")
    .Description("Linux/macOS: audited publish, real HTTP/process tests, and real Chromium/WebKit workflows.")
    .IsDependentOn("PortableWeb-HTTP").IsDependentOn("PortableWeb-Browsers").Does(() =>
        NodeCommand("npx", "playwright test --config playwright.portable.config.ts", new Dictionary<string, string>
        {
            { "COMPUTATOR_PORTABLE_PUBLISH_DIRECTORY", MakeAbsolute(Directory(portableWebOutput)).FullPath },
            { "COMPUTATOR_EXPECTED_ARCHITECTURE", portableWebRid.EndsWith("arm64") ? "Arm64" : "X64" }
        }));
