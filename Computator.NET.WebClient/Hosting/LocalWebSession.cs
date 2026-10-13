using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Computator.NET.WebClient.Hosting;

/// <summary>Owns the API lifetime for an explicitly packaged trusted-local application.</summary>
internal sealed class LocalWebSession : IHostedService, IDisposable
{
    private readonly string _apiDirectory;
    private readonly string _address;
    private readonly string _token;
    private readonly bool _openBrowser;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServer _server;
    private readonly ILogger<LocalWebSession> _logger;
    private Process? _process;
    private OwnedProcessJob? _job;
    private CancellationTokenRegistration _started;

    private LocalWebSession(string apiDirectory, string address, string token, bool openBrowser,
        IHostApplicationLifetime lifetime, IServer server, ILogger<LocalWebSession> logger)
    {
        _apiDirectory = apiDirectory; _address = address; _token = token; _openBrowser = openBrowser;
        _lifetime = lifetime; _server = server; _logger = logger;
    }

    public static void Configure(WebApplicationBuilder builder)
    {
        if (!builder.Configuration.GetValue<bool>("LocalWeb:Enabled")) return;
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("The packaged mathematical API currently requires Windows. The independent frontend and portable CLI remain cross-platform.");
        var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "api"));
        if (!File.Exists(Path.Combine(directory, "Computator.NET.WebApi.exe")))
            throw new InvalidOperationException("The local application is incomplete: the sibling api directory is missing. Build the complete Cake Local-Web-Package, not only the frontend.");
        var port = builder.Configuration.GetValue<int>("LocalWeb:ApiPort");
        if (port < 0 || port > 65535) throw new InvalidOperationException("LocalWeb:ApiPort must be zero or a valid TCP port.");
        if (port == 0)
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            port = ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        var address = $"http://127.0.0.1:{port}";
        var urls = builder.Configuration["urls"];
        if (string.IsNullOrWhiteSpace(urls)) builder.WebHost.UseUrls("http://127.0.0.1:0");
        else if (urls.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(value =>
            !Uri.TryCreate(value, UriKind.Absolute, out var uri) || !uri.IsLoopback || uri.Scheme != "http"))
            throw new InvalidOperationException("The local application must bind its frontend to a loopback HTTP address.");
        // The token lives in server memory/environment only, never in a VITE value,
        // browser URL, readiness document or persisted application settings.
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        builder.Configuration["MathematicalApi:BaseUrl"] = address;
        builder.Configuration["MathematicalApi:Token"] = token;
        var open = builder.Configuration.GetValue("LocalWeb:OpenBrowser", true);
        builder.Services.AddSingleton<IHostedService>(services => new LocalWebSession(directory, address, token, open,
            services.GetRequiredService<IHostApplicationLifetime>(), services.GetRequiredService<IServer>(),
            services.GetRequiredService<ILogger<LocalWebSession>>()));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo(Path.Combine(_apiDirectory, "Computator.NET.WebApi.exe"))
        {
            WorkingDirectory = _apiDirectory, UseShellExecute = false, CreateNoWindow = true
        };
        info.ArgumentList.Add("--urls"); info.ArgumentList.Add(_address);
        info.Environment["ExecutionAccess__Token"] = _token;
        info.Environment["ExecutionAccess__AllowUnauthenticatedDevelopment"] = "false";
        info.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        info.Environment["DOTNET_ENVIRONMENT"] = "Production";
        try
        {
            _job = new OwnedProcessJob();
            _process = Process.Start(info) ?? throw new InvalidOperationException("The local mathematical API did not start.");
            _job.Assign(_process);
            _process.EnableRaisingEvents = true;
            _process.Exited += (_, _) => _lifetime.StopApplication();
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(45));
            using var client = new HttpClient(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false })
                { Timeout = TimeSpan.FromSeconds(3) };
            client.DefaultRequestHeaders.Add("X-Computator-Session", _token);
            while (true)
            {
                deadline.Token.ThrowIfCancellationRequested();
                if (_process.HasExited) throw new InvalidOperationException($"The local API exited during startup ({_process.ExitCode}).");
                try
                {
                    using var response = await client.GetAsync(_address + "/api/numerical-calculations/integral/list-methods", deadline.Token);
                    if (response.IsSuccessStatusCode) break;
                    throw new InvalidOperationException($"The configured local API rejected readiness ({(int)response.StatusCode}); refusing to reuse another process.");
                }
                catch (HttpRequestException) { await Task.Delay(150, deadline.Token); }
            }
            _started = _lifetime.ApplicationStarted.Register(() =>
            {
                var address = _server.Features.Get<IServerAddressesFeature>()?.Addresses.SingleOrDefault();
                if (address == null) { _logger.LogError("Cannot determine the local frontend address."); _lifetime.StopApplication(); return; }
                _logger.LogInformation("Local workspace ready at {Address}", address);
                if (!_openBrowser) return;
                try { Process.Start(new ProcessStartInfo(address) { UseShellExecute = true }); }
                catch (Exception exception) { _logger.LogWarning(exception, "Open the local workspace address in your browser."); }
            });
        }
        catch { Dispose(); throw; }
    }

    public Task StopAsync(CancellationToken cancellationToken) { Dispose(); return Task.CompletedTask; }
    public void Dispose()
    {
        _started.Dispose();
        // Closing the job terminates only this owned API/process tree, including if
        // the frontend is terminated abruptly and Windows closes its handles.
        _job?.Dispose(); _job = null;
        if (_process != null)
        {
            try { if (!_process.HasExited) { _process.Kill(entireProcessTree: true); _process.WaitForExit(5000); } }
            catch (InvalidOperationException) { }
            _process.Dispose(); _process = null;
        }
    }
}
