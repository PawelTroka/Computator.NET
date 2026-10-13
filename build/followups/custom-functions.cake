Task("Web-CustomFunctions").IsDependentOn("ApiTests").IsDependentOn("Publish-Api")
    .IsDependentOn("Web-Publish").IsDependentOn("Web-Browsers").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("Real custom-function workflow validation requires the Windows API.");
    NodeCommand("npx", "playwright test --config playwright.custom-functions.config.ts");
});
