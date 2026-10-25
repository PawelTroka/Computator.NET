// The profile belongs to the task, including Worker-Validate and aggregate runs.
// A separate output keeps a later direct-development publish from replacing it.
Task("Web-Real-Publish")
    .Description("Build the relative-API frontend profile to artifacts/webclient-real.")
    .IsDependentOn("Web-Restore").Does(() => PublishWeb("artifacts/webclient-real", "/api"));
Task("Web-E2E")
    .Description("Windows: exercise published frontend routes against the real mathematical API.")
    .IsDependentOn("Publish-Api").IsDependentOn("Web-Real-Publish")
    .IsDependentOn("Web-Browsers").Does(() =>
{
    if (!IsRunningOnWindows()) throw new Exception("The real chart-backed API requires Windows.");
    NodeCommand("npx", "playwright test --config playwright.e2e.config.ts");
});
