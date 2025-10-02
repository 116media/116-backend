using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for configuring API versioning
/// and creating versioned route groups.
/// </summary>
public static class ApiVersioningExtensions
{
    private static RouteGroupBuilder? _rootVersionedGroup;

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

        _rootVersionedGroup = app
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);
    }

    /// <summary>
    /// Gets the root versioned route group.
    /// Throws if versioning has not been initialized.
    /// </summary>
    private static RouteGroupBuilder RootVersionedGroup(this IEndpointRouteBuilder app) =>
        _rootVersionedGroup ?? throw new InvalidOperationException(
            "API versioning has not been initialized. Call app.UseApiVersioning() in Program.cs."
        );

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
        RouteGroupBuilder versionGroup = app.RootVersionedGroup()
            .MapGroup(string.Empty)
            .HasApiVersion(version);

        return isDeprecated
            ? versionGroup.HasDeprecatedApiVersion(version)
            : versionGroup;
    }
}
