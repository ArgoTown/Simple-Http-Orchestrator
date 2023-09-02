using Simple.Http.Orchestrator.Enums;

namespace Simple.Http.Orchestrator.Utils;

public static class Extensions
{
    public static HttpMethod GetHttpMethod(this CallType callType)
    {
        var method = callType.ToString();
        if (method.Equals(nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Post;
        }

        if (method.Equals(nameof(HttpMethod.Get), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Get;
        }

        if (method.Equals(nameof(HttpMethod.Delete), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Delete;
        }

        if (method.Equals(nameof(HttpMethod.Patch), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Patch;
        }

        if (method.Equals(nameof(HttpMethod.Put), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Put;
        }

        if (method.Equals(nameof(HttpMethod.Head), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Head;
        }

        if (method.Equals(nameof(HttpMethod.Trace), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Trace;
        }

        if (method.Equals(nameof(HttpMethod.Options), StringComparison.OrdinalIgnoreCase))
        {
            return HttpMethod.Options;
        }

        throw new ArgumentOutOfRangeException(nameof(method));
    }

    public static string RemoveCurlyBrackets(this string value) => value.Replace("{", string.Empty).Replace("}", string.Empty);
}
