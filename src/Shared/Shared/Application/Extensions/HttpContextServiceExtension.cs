using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Resolves services from the current request's container, for code handed an
/// <see cref="HttpContext" /> rather than constructor injection.
/// </summary>
public static class HttpContextServiceExtension
{
    /// <summary>
    /// Resolves a required service from the request's container.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The resolved service.</returns>
    public static T Resolve<T>(this HttpContext context)
        where T : notnull
    {
        return context.RequestServices.GetRequiredService<T>();
    }
}
