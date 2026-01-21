using System;
using System.Linq;
using System.Net.Http.Headers;

namespace Ateleris.NET.Shared.Extensions;

public static class HttpResponseHeadersExtensions
{
    public static string? GetHeaderValueSafe(this HttpResponseHeaders headers, string headerName)
        => headers.GetHeaderValueSafe(headerName, val => val);

    public static T? GetHeaderValueSafe<T>(this HttpResponseHeaders headers, string headerName, Func<string, T?> parseFunc)
    {
        if (!headers.Contains(headerName))
        {
            return default;
        }

        var value = headers.GetValues(headerName).FirstOrDefault();
        return value is not null ? parseFunc(value) : default;
    }
}
