using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using Computator.NET.PortableWeb;

if (args is ["--execution-worker"]) return WorkerProcess.RunWorker();
if (args is ["--help"])
{
    Console.WriteLine("Computator.NET portable local proof of concept. Run only code you trust.\nUsage: Computator.NET.PortableWeb [--port 5099] [--execution-timeout-seconds 10]\nBinds 127.0.0.1 only. Port 0 selects a free port. This is not full desktop parity.");
    return 0;
}
var port = 5099;
var timeout = 10;
var seen = new HashSet<string>(StringComparer.Ordinal);
for (var index = 0; index < args.Length; index += 2)
{
    if (index + 1 >= args.Length || !seen.Add(args[index]) || !int.TryParse(args[index + 1], out var value))
        throw new ArgumentException("Use --port <0–65535> and --execution-timeout-seconds <1–60> once each.");
    switch (args[index])
    {
        case "--port" when value >= 0 && value <= 65535: port = value; break;
        case "--execution-timeout-seconds" when value >= 1 && value <= 60: timeout = value; break;
        default: throw new ArgumentException("Unknown option or value. Use --help for supported options.");
    }
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = [], ContentRootPath = AppContext.BaseDirectory, WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
});
// Explicit Kestrel listener prevents environment/configuration URL overrides from
// accidentally publishing trusted local execution on a network interface.
// PreferHostingUrls is separately configurable and otherwise wins over Listen.
builder.WebHost.PreferHostingUrls(false);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(new ConfigurationBuilder().Build());
    options.Listen(IPAddress.Loopback, port);
    options.Limits.MaxRequestBodySize = Execution.MaximumBodyBytes;
});
builder.Services.AddSingleton(services => new WorkerProcess(TimeSpan.FromSeconds(timeout),
    services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping));
builder.Services.AddProblemDetails();
var app = builder.Build();
app.Use(async (context, next) =>
{
    var request = context.Request;
    var origin = request.Headers.Origin.ToString();
    var sameOrigin = request.Headers.Origin.Count == 0 ||
        (Uri.TryCreate(origin, UriKind.Absolute, out var source) && source.Scheme == "http" &&
         source.Host == "127.0.0.1" && source.Port == context.Connection.LocalPort &&
         source.AbsolutePath == "/" && source.Query.Length == 0 && source.Fragment.Length == 0 && source.UserInfo.Length == 0);
    // Validate every request, including static assets, before routing. Host must
    // match the numeric bound address and actual port (also for --port 0).
    if (context.Connection.RemoteIpAddress is not { } remote || !IPAddress.IsLoopback(remote) ||
        request.Host.Host != "127.0.0.1" || (request.Host.Port ?? 80) != context.Connection.LocalPort || !sameOrigin ||
        request.Headers["Sec-Fetch-Site"].ToString().Equals("cross-site", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'none'";
    if (request.Path.StartsWithSegments("/api")) context.Response.Headers.CacheControl = "no-store";
    await next(context);
});
app.UseStaticFiles();
app.MapGet("/api/portable/status", (WorkerProcess workers) => new
{
    implementation = "portable-local-poc", os = RuntimeInformation.OSDescription,
    architecture = RuntimeInformation.ProcessArchitecture.ToString(), activeWorkerCount = workers.ActiveCount,
    operations = Execution.Operations, chart = false, nativeGsl = false
});
app.MapGet("/api/numerical-calculations/integral/list-methods", () => new[] { Execution.IntegralMethod });
app.MapGet("/api/numerical-calculations/derivative/list-methods", () => new[] { Execution.DerivativeMethod });
app.MapGet("/api/numerical-calculations/function-root/list-methods", () => new[] { Execution.RootMethod });
app.MapGet("/api/autocomplete/expression", () => PortableCatalog.Items(false));
app.MapGet("/api/autocomplete/scripting", () => PortableCatalog.Items(true));
app.MapPost("/api/v2/chart", () => Results.Problem(statusCode: 422,
    title: "Charts are not implemented in this portable proof of concept.",
    detail: "The portable local application currently supports calculations, scripts, custom functions and selected numerical methods. Desktop and 3D integration remain separate work."));
app.MapPost("/api/v2/execute", async (HttpContext context, WorkerProcess workers) =>
{
    if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var contentType) ||
        !string.Equals(contentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        return Results.Problem(statusCode: 415, title: "Execution requires application/json.");
    try
    {
        using var body = new MemoryStream();
        var buffer = new byte[8192]; int count;
        while ((count = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) != 0)
        {
            if (body.Length + count > Execution.MaximumBodyBytes)
                return Results.Problem(statusCode: 413, title: "Execution request exceeds 256 KiB.");
            await body.WriteAsync(buffer.AsMemory(0, count), context.RequestAborted);
        }
        var request = JsonSerializer.Deserialize<ExecutionRequest>(body.ToArray(), Execution.JsonOptions)
            ?? throw new ArgumentException("A JSON request is required.");
        Execution.Validate(request);
        var reply = await workers.RunAsync(request, context.RequestAborted);
        return reply.Status == 200 ? Results.Ok(new { result = reply.Result })
            : Results.Problem(statusCode: reply.Status, title: "Execution failed.", detail: reply.Detail,
                extensions: reply.Diagnostics == null ? null : new Dictionary<string, object?> { ["diagnostics"] = reply.Diagnostics });
    }
    catch (Exception exception) when (exception is JsonException or ArgumentException)
    { return Results.Problem(statusCode: 400, title: "Invalid execution request.", detail: exception.Message); }
    catch (BadHttpRequestException exception)
    { return Results.Problem(statusCode: exception.StatusCode, title: "Invalid execution request."); }
});
app.Map("/api/{**path}", () => Results.NotFound());
app.MapGet("/", () => Results.Redirect("/calculate"));
app.MapFallbackToFile("index.html");
await app.RunAsync();
return 0;
