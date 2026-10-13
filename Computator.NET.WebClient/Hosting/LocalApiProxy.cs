using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Features;

namespace Computator.NET.WebClient.Hosting;

/// <summary>A deliberately local, opt-in bridge, not a general-purpose public reverse proxy.</summary>
internal static class LocalApiProxy
{
    private const int MaximumBodyBytes = 256 * 1024;

    public static void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("MathematicalApi", client => client.Timeout = TimeSpan.FromSeconds(60))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false, UseCookies = false, UseProxy = false
            });
    }

    public static void Map(WebApplication app)
    {
        var address = app.Configuration["MathematicalApi:BaseUrl"];
        if (string.IsNullOrWhiteSpace(address)) return;
        if (!Uri.TryCreate(address, UriKind.Absolute, out var origin) || origin.Scheme != "http" ||
            !origin.IsLoopback || origin.AbsolutePath != "/" || origin.Query.Length != 0 ||
            origin.Fragment.Length != 0 || origin.UserInfo.Length != 0)
            throw new InvalidOperationException("MathematicalApi:BaseUrl must be a loopback HTTP origin, with no path or credentials.");
        var token = app.Configuration["MathematicalApi:Token"];
        if (string.IsNullOrWhiteSpace(token) || token.Length < 32)
            throw new InvalidOperationException("The local proxy requires a random MathematicalApi:Token of at least 32 characters, also configured on the API.");

        app.MapMethods("/api/{**path}", new[] { "GET", "POST" }, async (HttpContext context, IHttpClientFactory clients) =>
        {
            if (context.Connection.RemoteIpAddress is not { } remote || !IPAddress.IsLoopback(remote) ||
                !IsLoopbackHost(context.Request.Host.Host))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            var browserOrigin = context.Request.Headers.Origin.ToString();
            var sameOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
            if ((browserOrigin.Length != 0 && !string.Equals(browserOrigin, sameOrigin, StringComparison.OrdinalIgnoreCase)) ||
                context.Request.Headers["Sec-Fetch-Site"] == "cross-site")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            // Forward the original escaping, not Path's partially decoded value:
            // turning %2532 into %32 would change the source expression 100%32
            // (modulo) into 1002 on the API. UriBuilder keeps the fixed authority.
            var target = context.Features.Get<IHttpRequestFeature>()?.RawTarget;
            var destination = new Uri(origin, context.Request.Path.ToUriComponent() + context.Request.QueryString.ToUriComponent());
            if (!string.IsNullOrEmpty(target) && target.StartsWith('/'))
            {
                var query = target.IndexOf('?');
                destination = new UriBuilder(origin)
                {
                    Path = query < 0 ? target : target[..query],
                    Query = query < 0 ? "" : target[(query + 1)..]
                }.Uri;
            }
            using var outgoing = new HttpRequestMessage(new HttpMethod(context.Request.Method), destination);
            outgoing.Headers.Add("X-Computator-Session", token);
            if (HttpMethods.IsPost(context.Request.Method))
            {
                if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var contentType) ||
                    !string.Equals(contentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return;
                }
                using var body = new MemoryStream();
                var buffer = new byte[8192];
                int read;
                while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) != 0)
                {
                    if (body.Length + read > MaximumBodyBytes)
                    {
                        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                        return;
                    }
                    await body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
                }
                outgoing.Content = new ByteArrayContent(body.ToArray());
                outgoing.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            try
            {
                using var response = await clients.CreateClient("MathematicalApi").SendAsync(outgoing,
                    HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
            catch (HttpRequestException) when (!context.Response.HasStarted)
            {
                await Results.Problem(statusCode: 502, title: "The local mathematical API is unavailable.").ExecuteAsync(context);
            }
            catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested && !context.Response.HasStarted)
            {
                await Results.Problem(statusCode: 504, title: "The mathematical API request timed out.").ExecuteAsync(context);
            }
        });
    }

    private static bool IsLoopbackHost(string host) => string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        (IPAddress.TryParse(host.Trim('[', ']'), out var address) && IPAddress.IsLoopback(address));
}
