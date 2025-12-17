using _116.Shared.Application.Middleware;

using Microsoft.AspNetCore.Builder;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for Swagger middleware configuration.
/// </summary>
public static class SwaggerMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware to process Swagger JSON descriptions for UI rendering.
    /// </summary>
    public static IApplicationBuilder UseSwaggerFormatting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SwaggerDescriptionMiddleware>();
    }
}
