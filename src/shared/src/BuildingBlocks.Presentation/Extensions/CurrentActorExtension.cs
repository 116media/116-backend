using _116.BuildingBlocks.Application.Services;
using _116.BuildingBlocks.Presentation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace _116.BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Registers the HTTP-backed <see cref="ICurrentActor" /> the audit interceptors read.
/// </summary>
public static class CurrentActorExtension
{
    /// <summary>
    /// Registers <see cref="IHttpContextAccessor" /> and <see cref="HttpCurrentActor" /> as the
    /// process-wide <see cref="ICurrentActor" />.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for method chaining.</returns>
    public static IServiceCollection AddHttpCurrentActor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<ICurrentActor, HttpCurrentActor>();
        return services;
    }
}
