using System.Text;
using Microsoft.Extensions.Options;

namespace PetFinder.Api.Logging;

public sealed class SafeRequestBodyLoggingMiddleware(RequestDelegate next, ILogger<SafeRequestBodyLoggingMiddleware> logger, IOptions<RequestBodyLoggingOptions> options)
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "X-API-Key"
    };

    private readonly RequestBodyLoggingOptions _options = options.Value;

    public async Task Invoke(HttpContext context)
    {
        if (!_options.Enabled || !CanLogRequest(context.Request))
        {
            await next(context);
            return;
        }

        context.Request.EnableBuffering();

        var originalPosition = context.Request.Body.Position;
        var body = await ReadBodyAsync(context.Request, context.RequestAborted);
        context.Request.Body.Position = originalPosition;

        logger.LogInformation(
            "RequestBody {Method} {Path} ({ContentType}) Size={BodySize} Body={Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.ContentType,
            context.Request.ContentLength,
            body);

        await next(context);
    }

    private bool CanLogRequest(HttpRequest request)
    {
        if (request.ContentLength is null or <= 0)
        {
            return false;
        }

        if (request.ContentLength > _options.MaxBodyLength)
        {
            return false;
        }

        if (request.Headers.Keys.Any(header => SensitiveHeaders.Contains(header)))
        {
            return false;
        }

        if (_options.AllowedPathPrefixes.Length > 0 &&
            !_options.AllowedPathPrefixes.Any(prefix => request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            return false;
        }

        return _options.AllowedContentTypes.Any(contentType =>
            request.ContentType.StartsWith(contentType, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> ReadBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);

        if (text.Length <= _options.MaxBodyLength)
        {
            return text;
        }

        return text[.._options.MaxBodyLength];
    }
}
