using Microsoft.AspNetCore.Http;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Shared test helpers for HTTP context and request/response tests.
/// </summary>
public static class HttpTestHelpers
{
    /// <summary>
    /// Creates a default HttpContext for testing exception handlers and middleware.
    /// </summary>
    public static DefaultHttpContext CreateDefaultHttpContext()
    {
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        context.TraceIdentifier = "test-trace-id";
        context.Response.Body = new MemoryStream();
        return context;
    }
}
