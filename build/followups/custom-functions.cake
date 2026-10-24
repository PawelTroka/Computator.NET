suites["Computator.NET.WebApi.IntegrationTests"] = 117;
Task("Web-CustomFunctions")
    .Description("Windows: exercise real API and browser custom-function workflows.")
    .IsDependentOn("ApiTests").IsDependentOn("Publish-Api")
    .IsDependentOn("Web-Real-Publish").IsDependentOn("Web-Browsers").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("Real custom-function workflow validation requires the Windows API.");
    NodeCommand("npx", "playwright test --config playwright.custom-functions.config.ts");
});
