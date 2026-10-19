// Explicit compatibility tasks; the default .NET 10 graph never depends on Mono.
Task("Mono-Build")
    .Description("Compile the reduced Framework Mono desktop and its acceptance probes.")
    .Does(() => CheckedProcess("bash", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/mono/build.sh").Append(configuration)
    }));

Task("Mono-Validate")
    .Description("Exercise the Mono desktop startup, calculations, editors and supported charts.")
    .IsDependentOn("Mono-Build")
    .Does(() => CheckedProcess("bash", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/mono/validate.sh").Append(configuration).Append("--no-build")
    }));
