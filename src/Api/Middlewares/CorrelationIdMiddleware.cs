using Serilog.Context;

namespace _116.Api.Middlewares;

/// <summary>
/// Stamps every request with a correlation id — the inbound <c>X-Correlation-Id</c> header when
/// present, a new one otherwise — pushed into the log context and echoed on the response, so
/// Serilog lines and traces join on one id.
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    /// <summary>
    /// Resolves the request's correlation id and scopes it over the rest of the pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var inbound) && !string.IsNullOrWhiteSpace(inbound)
                ? inbound.ToString()
                : Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
