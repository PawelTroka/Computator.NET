using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace Computator.NET.WebClient.Hosting;

/// <summary>Make an ordinary IDE project launch serve the same assets Cake publishes.</summary>
internal static class DevelopmentAssets
{
    public static async Task<PhysicalFileProvider> PrepareAsync(WebApplicationBuilder builder)
    {
        var root = builder.Environment.WebRootPath;
        if (builder.Environment.IsDevelopment())
        {
            var source = FindSourceRoot(builder.Environment.ContentRootPath) ?? FindSourceRoot(AppContext.BaseDirectory);
            if (source != null)
            {
                root = Path.Combine(source, "dist");
                var intermediate = Path.Combine(source, "obj");
                Directory.CreateDirectory(intermediate);
                using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                await using var gate = await AcquireLockAsync(Path.Combine(intermediate, "frontend-build.lock"), deadline.Token);
                var stamp = Path.Combine(intermediate, "frontend-inputs.sha256");
                var fingerprint = Fingerprint(source);
                if (!File.Exists(Path.Combine(root, "index.html")) || !File.Exists(stamp) ||
                    await File.ReadAllTextAsync(stamp, deadline.Token) != fingerprint + "\n" + OutputFingerprint(root))
                {
                    Console.WriteLine("Preparing the web interface through Cake Web-Build…");
                    var repository = Path.GetDirectoryName(source)!;
                    await RunAsync(repository, deadline.Token, "tool", "restore");
                    await RunAsync(repository, deadline.Token, "cake", "build.cake", "--target=Web-Build");
                    if (!File.Exists(Path.Combine(root, "index.html")))
                        throw new InvalidOperationException("Cake completed without producing dist/index.html.");
                    await File.WriteAllTextAsync(stamp, fingerprint + "\n" + OutputFingerprint(root), deadline.Token);
                }
            }
        }
        if (string.IsNullOrWhiteSpace(root) || !File.Exists(Path.Combine(root, "index.html")))
            throw new InvalidOperationException("Web interface assets are missing. In a source checkout use the Development launch profile; for deployment run Cake Web-Publish. The application will not start an empty site returning 404.");
        // A builder created while dist did not exist has a NullFileProvider. Replace
        // it after the first build; setting WebRootPath alone does not repair that.
        var files = new PhysicalFileProvider(Path.GetFullPath(root));
        builder.Environment.WebRootPath = Path.GetFullPath(root);
        builder.Environment.WebRootFileProvider = files;
        return files;
    }

    private static string FindSourceRoot(string start)
    {
        for (var directory = new DirectoryInfo(start); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Computator.NET.WebClient.csproj"))) return directory.FullName;
            var nested = Path.Combine(directory.FullName, "Computator.NET.WebClient");
            if (File.Exists(Path.Combine(nested, "Computator.NET.WebClient.csproj"))) return nested;
        }
        return null;
    }

    private static string Fingerprint(string root)
    {
        var paths = new List<string>();
        foreach (var directory in new[] { "ClientApp", "wwwroot" })
            if (Directory.Exists(Path.Combine(root, directory)))
                paths.AddRange(Directory.EnumerateFiles(Path.Combine(root, directory), "*", SearchOption.AllDirectories));
        foreach (var name in new[] { "package.json", "package-lock.json", "index.html", "vite.config.ts", "tsconfig.json", ".env", ".env.local", ".env.production", ".env.production.local" })
            if (File.Exists(Path.Combine(root, name))) paths.Add(Path.Combine(root, name));
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        // This is a public build-time endpoint, never the execution token.
        hash.AppendData(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("VITE_API_BASE_URL") ?? "<default>"));
        foreach (var path in paths.OrderBy(path => path, StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes("\0" + Path.GetRelativePath(root, path).Replace('\\', '/') + "\0"));
            using var stream = File.OpenRead(path);
            hash.AppendData(SHA256.HashData(stream));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string OutputFingerprint(string root)
    {
        // Cake/package builds can replace dist with another API profile without
        // changing source inputs or the IDE stamp. Missing/corrupt assets count too.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes("\0" + Path.GetRelativePath(root, path).Replace('\\', '/') + "\0"));
            using var stream = File.OpenRead(path);
            hash.AppendData(SHA256.HashData(stream));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static async Task<FileStream> AcquireLockAsync(string path, CancellationToken cancellation)
    {
        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            try { return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) { await Task.Delay(200, cancellation); }
        }
    }

    private static async Task RunAsync(string directory, CancellationToken cancellation, params string[] arguments)
    {
        var info = new ProcessStartInfo("dotnet") { WorkingDirectory = directory, UseShellExecute = false };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = Process.Start(info) ?? throw new InvalidOperationException("Cannot start the .NET SDK. Install the versions pinned by global.json and .nvmrc.");
        try
        {
            await process.WaitForExitAsync(cancellation);
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Frontend preparation failed (exit {process.ExitCode}). See the Cake output above; the pinned Node/npm and .NET SDK are required for source development.");
        }
        catch
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            throw;
        }
    }
}
