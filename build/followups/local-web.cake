// Deployment profile is explicit in this graph, not inferred from the outer
// target name. An aggregate target gets the same relative-API production assets.
Task("Local-Web-Assets").IsDependentOn("Web-Restore").Does(() =>
    NodeCommand("npm", "run build", new Dictionary<string, string> { { "VITE_API_BASE_URL", "/api" } }));
Task("Local-Web-Stage").IsDependentOn("Build").IsDependentOn("Native-GSL").IsDependentOn("Local-Web-Assets").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("The chart-backed local package currently supports Windows x64.");
    CleanDirectory("artifacts/local-web");
    foreach (var entry in new[] { new[] { "Computator.NET.WebApi/Computator.NET.WebApi.csproj", "api" }, new[] { webProject, "webclient" } })
    {
        DotNetPublish(entry[0], new DotNetPublishSettings
        {
            Configuration = configuration, Runtime = "win-x64", SelfContained = true,
            OutputDirectory = "artifacts/local-web/" + entry[1],
            ArgumentCustomization = a => a.Append("-p:SkipFrontendBuild=true -p:PublishTrimmed=false -p:PublishSingleFile=false -p:PublishAot=false")
                .Append("-p:WarningsAsErrors=NU1900%3BNU1901%3BNU1902%3BNU1903%3BNU1904")
        });
        if (entry[1] == "api") StageGslPayload("artifacts/local-web/api");
    }
    RequirePayload("artifacts/local-web/api", new[] { "Computator.NET.WebApi.exe", "coreclr.dll", "Static/functions.xml" });
    RequirePayload("artifacts/local-web/webclient", new[] { "Computator.NET.WebClient.exe", "coreclr.dll", "dist/index.html" });
    PowerShell("build/Test-PublishedNet10Api.ps1", "-PublishDirectory artifacts/local-web/api");
    System.IO.File.WriteAllText("artifacts/local-web/webclient/appsettings.json",
        "{\"LocalWeb\":{\"Enabled\":true,\"OpenBrowser\":true},\"Logging\":{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}}");
    System.IO.File.WriteAllText("artifacts/local-web/Start-Computator.cmd", "@echo off\r\ncd /d \"%~dp0webclient\"\r\nComputator.NET.WebClient.exe\r\n");
    System.IO.File.WriteAllText("artifacts/local-web/README.txt", "Run Start-Computator.cmd. Both hosts are local and the API token is generated automatically. Closing the launcher stops its owned API. This runs trusted code with your permissions; it is not a sandbox or a public server. No SDK or Node installation is required by this self-contained package.\r\n");
});
Task("Local-Web-Archive").IsDependentOn("Local-Web-Stage").Does(() =>
{
    EnsureDirectoryExists("publish");
    PackagePayload("artifacts/local-web", "publish/Computator.NET.LocalWeb.win-x64.zip", "Start-Computator.cmd");
    PowerShell("build/local-web/Expand-Candidate.ps1");
});
Task("Local-Web-Validate").IsDependentOn("Local-Web-Archive").IsDependentOn("Web-Browsers").Does(() =>
{
    NodeCommand("npx", "playwright test --config playwright.local-package.config.ts");
    PowerShell("build/local-web/Assert-Stopped.ps1");
});
Task("Local-Web-Package").IsDependentOn("Local-Web-Validate");
