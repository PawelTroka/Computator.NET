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

Task("Mono-Mac-Prepare")
    .Description("macOS: prepare the pinned private X11 drawing stack without editing global Mono configuration.")
    .Does(() =>
{
    if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        throw new Exception("Mono-Mac-Prepare requires a native macOS host.");
    var arguments = new ProcessArgumentBuilder().AppendQuoted("build/macos/prepare.sh");
    if (Argument("mono-install-dependencies", false)) arguments.Append("--install-dependencies");
    CheckedProcess("bash", new ProcessSettings { Arguments = arguments });
});

Task("Mono-Mac-Validate")
    .Description("macOS: exercise the production Mono application and full native X11 desktop acceptance.")
    .IsDependentOn("Mono-Mac-Prepare").IsDependentOn("Mono-Build")
    .Does(() => CheckedProcess("bash", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/macos/validate.sh")
            .Append(configuration).Append("--no-build")
    }));
