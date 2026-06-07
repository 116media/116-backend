using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Shared test helpers for HTTP context and request/response tests.
/// </summary>
public static class HttpTestHelpers
{
    /// <summary>
    /// Creates a default HttpContext for testing exception handlers and middleware.
    /// Includes a service provider with localization and shared exception messages.
    /// </summary>
    public static DefaultHttpContext CreateDefaultHttpContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddScoped<SharedExceptionMessage>();
        ServiceProvider provider = services.BuildServiceProvider();

        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        context.TraceIdentifier = "test-trace-id";
        context.Response.Body = new MemoryStream();
        context.RequestServices = provider;
        return context;
    }
}
