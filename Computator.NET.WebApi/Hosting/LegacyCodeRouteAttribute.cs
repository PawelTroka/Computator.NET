using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Computator.NET.WebApi.Hosting
{
    // Apply only to legacy endpoints that transport source code in route segments.
    // JSON execution does not need URL decoding and remains the preferred contract.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LegacyCodeRouteAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint() as RouteEndpoint;
            var target = context.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
            if (endpoint == null || string.IsNullOrEmpty(target)) return;

            // ASP.NET leaves encoded slashes in route values, but decodes %25.
            // Decode the original segment once, so %2F becomes / while %252F
            // stays the literal text %2F. Never unescape the bound value again.
            var query = target.IndexOf('?');
            var path = query < 0 ? target : target.Substring(0, query);
            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                if (!Uri.TryCreate(target, UriKind.Absolute, out var absolute)) return;
                path = absolute.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
            }
            var segments = path.Trim('/').Split('/');
            var pattern = endpoint.RoutePattern.PathSegments;
            // A host can mount the application below PathBase. Routing's matched
            // pattern excludes that prefix, which remains present in RawTarget.
            var offset = segments.Length - pattern.Count;
            if (offset < 0) return;
            for (var index = 0; index < pattern.Count; index++)
            {
                var parts = pattern[index].Parts;
                if (parts.Count != 1 || !(parts[0] is RoutePatternParameterPart parameter)) continue;
                if (parameter.Name != "equation" && parameter.Name != "tslCode" && parameter.Name != "customFunctionsCode") continue;
                context.RouteData.Values[parameter.Name] = Uri.UnescapeDataString(segments[offset + index]);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context) { }
    }
}
