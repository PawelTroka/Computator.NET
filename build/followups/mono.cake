// Explicit compatibility tasks; the default .NET 10 graph never depends on Mono.
string MonoMacPackageRid()
{
    if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        throw new Exception("Mono Mac packaging requires a native macOS host.");
    switch (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)
    {
        case System.Runtime.InteropServices.Architecture.Arm64: return "osx-arm64";
        case System.Runtime.InteropServices.Architecture.X64: return "osx-x64";
        default: throw new Exception("Use a native Intel or Apple Silicon SDK/runtime for Mono Mac packaging.");
    }
}

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

Task("Mono-Linux-Package")
    .Description("Linux: stage the full Mono desktop, pinned native payload, runtime preflight and release inventory.")
    .IsDependentOn("Mono-Build")
    .Does(() =>
    {
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            throw new Exception("Mono-Linux-Package requires a native Linux host.");
        CheckedProcess("bash", new ProcessSettings
        { Arguments = new ProcessArgumentBuilder().AppendQuoted("build/mono/package-linux.sh").Append(configuration) });
    });

Task("Mono-Linux-Package-Validate")
    .Description("Linux: exercise the actual extracted release archive with its own production DLLs and native libraries.")
    .IsDependentOn("Mono-Linux-Package")
    .Does(() =>
{
    var baseline = Argument("mono-linux-baseline", "auto");
    if (baseline == "auto")
    {
        var osRelease = System.IO.File.ReadAllLines("/etc/os-release");
        var id = osRelease.Single(line => line.StartsWith("ID=")).Substring(3).Trim('"');
        var version = osRelease.Single(line => line.StartsWith("VERSION_ID=")).Substring(11).Trim('"');
        baseline = id + "-" + version;
    }
    if (baseline != "ubuntu-22.04" && baseline != "ubuntu-24.04") throw new Exception("Use a tested Mono Linux baseline: ubuntu-22.04 or ubuntu-24.04.");
    CheckedProcess("bash", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/mono/validate-package.sh")
            .AppendQuoted("artifacts/mono-package/" + configuration + "/Computator.NET-" + baseline + "-linux-x64.tar.gz")
            .AppendQuoted("artifacts/mono-package/unpacked-" + configuration)
            .AppendQuoted("artifacts/mono-package/evidence-" + configuration)
    });
});

Task("Mono-Mac-Package")
    .Description("macOS: package the accepted native Mono desktop as a runtime-dependent application bundle and ZIP.")
    .IsDependentOn("Mono-Mac-Validate")
    .Does(() =>
{
    var rid = MonoMacPackageRid();
    var output = "artifacts/macos/package/" + configuration;
    CheckedProcess("python3", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/macos/package.py")
            .Append("--configuration").Append(configuration)
            .Append("--output").AppendQuoted(output + "/" + rid + "/Computator.NET.app")
            .Append("--archive").AppendQuoted(output + "/Computator.NET-mono-" + rid + ".zip")
    });
});

Task("Mono-Mac-Package-Validate")
    .Description("macOS: relocate and exercise the actual native application ZIP, preflight and full packaged desktop suite.")
    .IsDependentOn("Mono-Mac-Package")
    .Does(() => CheckedProcess("bash", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/macos/validate-package.sh")
            .Append(configuration).AppendQuoted("artifacts/macos/package/" + configuration +
                "/Computator.NET-mono-" + MonoMacPackageRid() + ".zip")
    }));

Task("Mono-Mac-Archive-Validate")
    .Description("macOS: test an existing Mono ZIP on this native host; pass --mono-mac-archive without rebuilding its payload.")
    .IsDependentOn("Mono-Mac-Prepare").IsDependentOn("Mono-Build")
    .Does(() =>
{
    MonoMacPackageRid();
    var archive = Argument("mono-mac-archive", "");
    if (string.IsNullOrWhiteSpace(archive) || !FileExists(archive))
        throw new Exception("Pass an existing Mac application ZIP with --mono-mac-archive=<path>.");
    CheckedProcess("bash", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder().AppendQuoted("build/macos/validate-package.sh")
            .Append(configuration).AppendQuoted(archive)
    });
});
