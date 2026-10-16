using System;
using System.Net;

namespace Computator.NET.Core.Autocompletion
{
    /// <summary>Document shell for the trusted, checked-in HTML reference fragments.</summary>
    public static class FunctionReferenceDocument
    {
        public static string Render(FunctionInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            // Description is the locally shipped reference, verified by content
            // tests. This helper is not a sanitizer for arbitrary user HTML.
            // Keep the stylesheet compatible with the offline Windows browser;
            // no remote fonts, scripts or equation-image services are required.
            return "<!doctype html><html><head><meta charset=\"utf-8\">" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>" + WebUtility.HtmlEncode(info.Title) +
                "</title><style>body{font:18px/1.6 'Segoe UI',Arial,sans-serif;margin:0;padding:16px;color:#24262b;background:#fff;word-wrap:break-word;}" +
                "h1{font-size:24px;line-height:1.3;margin:0 0 16px;}p{margin:12px 0;}" +
                "code{font-size:1em;font-family:Consolas,'Cambria Math',monospace;white-space:normal;word-wrap:break-word;}" +
                ".reference-signature{color:#454958;}.reference-note{border-left:3px solid #b07934;padding-left:12px;}" +
                ".reference-formula{padding:12px;background:#f4f6f8;border-left:3px solid #75879b;}" +
                ".reference-formula code{font-family:'Cambria Math','Segoe UI',serif;font-size:1.125em;}" +
                ".reference-source{font-size:15px;}a{color:#305c85;text-decoration:underline;}</style></head><body><h1>" +
                WebUtility.HtmlEncode(info.Title) + "</h1>" + info.Description + "</body></html>";
        }
    }
}
