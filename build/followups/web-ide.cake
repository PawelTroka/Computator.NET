Task("Web-IDE-Validate").IsDependentOn("Build").IsDependentOn("Web-Browsers").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("This developer workflow starts the real Windows mathematical API.");
    // A published/previously built dist must not conceal the reported fresh-clone 404.
    CleanDirectory(webDirectory + "/dist");
    var stamp = webDirectory + "/obj/frontend-inputs.sha256";
    if (FileExists(stamp)) DeleteFile(stamp);
    // Playwright itself uses the pinned tools. Its source hosts must inherit
    // the original IDE PATH so the old-toolchain case exercises host -> Cake
    // preparation, rather than accidentally receiving an already-fixed PATH.
    NodeCommand("npx", "playwright test --config playwright.ide.config.ts", new Dictionary<string, string>
    {
        { "COMPUTATOR_IDE_PATH", EnvironmentVariable("PATH") ?? "" }
    });
});
