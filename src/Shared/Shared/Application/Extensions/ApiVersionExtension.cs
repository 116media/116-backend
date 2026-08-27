using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Holds the root versioned route group for one host. Registered as a per-host singleton so two
/// hosts in one process — the integration fixtures — never share routing state.
/// </summary>
public sealed class RootVersionedGroupHolder
{
    private RouteGroupBuilder? _group;

    /// <summary>
    /// The root versioned route group, set once by <c>UseApiVersioning</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when versioning was never initialized.</exception>
    public RouteGroupBuilder Group
    {
        get =>
            _group
            ?? throw new InvalidOperationException(
                "API versioning has not been initialized. Call app.UseApiVersioning() in Program.cs."
            );
        set => _group = value;
    }
}

/// <summary>
/// Extension methods for configuring API versioning
/// and creating versioned route groups.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Registers the per-host holder the root versioned group lives in.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddApiVersionGroupHolder(this IServiceCollection services)
    {
        services.AddSingleton<RootVersionedGroupHolder>();
        return services;
    }

    /// <summary>
    /// Configures API versioning and creates the root versioned group.
    /// Call this once in Program.cs after building the app.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static void UseApiVersioning(this WebApplication app)
    {
        // Create the API version set with v1 and v2
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder group = app.MapGroup("api/v{version:apiVersion}").WithApiVersionSet(versionSet);
        app.Services.GetRequiredService<RootVersionedGroupHolder>().Group = group;
    }

    /// <summary>
    /// Maps a version-specific group of endpoints.
    /// Use this to define endpoints for a specific API version.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="version">The API major version (e.g., 1 or 2).</param>
    /// <param name="isDeprecated">Whether this version is deprecated.</param>
    /// <returns>A route group builder for the specific version.</returns>
    public static RouteGroupBuilder MapApiVersionGroup(
        this IEndpointRouteBuilder app,
        int version,
        bool isDeprecated = false
    )
    {
        RouteGroupBuilder root = app.ServiceProvider.GetRequiredService<RootVersionedGroupHolder>().Group;
        RouteGroupBuilder versionGroup = root.MapGroup(string.Empty).HasApiVersion(version);

        return isDeprecated ? versionGroup.HasDeprecatedApiVersion(version) : versionGroup;
    }
}
