using _116.Shared.Application.Middleware;

using Microsoft.AspNetCore.Builder;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for configuring the ResourceNotFoundMiddleware.
/// </summary>
public static class ResourceNotFoundExtension
{
    /// <summary>
    /// Adds the ResourceNotFoundMiddleware to the application pipeline to handle non-existing endpoints and wrong HTTP methods.
    /// This middleware should be added after routing but before the exception handler.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseResourceNotFoundHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ResourceNotFoundMiddleware>();
    }
}
