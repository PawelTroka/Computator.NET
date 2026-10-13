// Opt-in real-stack validation; the existing mocked UI suite remains unchanged.
if (target == "Web-E2E" || target == "Web-CustomFunctions")
    System.Environment.SetEnvironmentVariable("VITE_API_BASE_URL", "/api");
Task("Web-E2E").IsDependentOn("Publish-Api").IsDependentOn("Web-Publish")
    .IsDependentOn("Web-Browsers").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("The real chart-backed API requires Windows.");
    NodeCommand("npx", "playwright test --config playwright.e2e.config.ts");
});
