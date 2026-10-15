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
            return "<!doctype html><html><head><meta charset=\"utf-8\">" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><title>" + WebUtility.HtmlEncode(info.Title) +
                "</title><style>body{font:14px/1.45 'Segoe UI',Arial,sans-serif;margin:12px;color:#24262b;background:#fff;}" +
                "h1{font-size:18px;margin:0 0 10px;}p{margin:8px 0;}code{font-family:Consolas,'Cambria Math',monospace;word-wrap:break-word;}" +
                ".reference-signature{color:#454958;}.reference-note{border-left:3px solid #b07934;padding-left:8px;}" +
                ".reference-source{font-size:12px;}a{color:#305c85;}</style></head><body><h1>" +
                WebUtility.HtmlEncode(info.Title) + "</h1>" + info.Description + "</body></html>";
        }
    }
}
