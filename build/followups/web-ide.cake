Task("Web-IDE-Validate").IsDependentOn("Build").IsDependentOn("Web-Browsers").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("This developer workflow starts the real Windows mathematical API.");
    // A published/previously built dist must not conceal the reported fresh-clone 404.
    CleanDirectory(webDirectory + "/dist");
    var stamp = webDirectory + "/obj/frontend-inputs.sha256";
    if (FileExists(stamp)) DeleteFile(stamp);
    NodeCommand("npx", "playwright test --config playwright.ide.config.ts");
});
