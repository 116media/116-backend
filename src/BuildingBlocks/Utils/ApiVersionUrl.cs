using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.BuildingBlocks.Utils;

/// <summary>
/// Provides utilities for constructing versioned API URLs without hardcoded version numbers.
/// Automatically extracts the API version from the current request context to ensure
/// consistency between the request and response URLs.
/// </summary>
public static class ApiVersionUrl
{
    /// <summary>
    /// Builds a versioned URL using the API version from the current HTTP request.
    /// The version is extracted from the route data (e.g., /api/v1/... → version = 1).
    /// </summary>
    /// <param name="context">The HTTP context containing the current request information.</param>
    /// <param name="path">The resource path relative to the API version (e.g., "public/users/123").</param>
    /// <returns>A complete versioned URL matching the request's API version
    /// (e.g., "/api/v1/public/users/123").
    /// </returns>
    public static string Build(HttpContext context, string path)
    {
        int version = GetVersionFromContext(context);
        return Build(version, path);
    }

    /// <summary>
    /// Builds a versioned URL with an explicitly specified API version.
    /// Use this when you need to generate URLs for a specific version regardless of the current request.
    /// </summary>
    /// <param name="version">The API version number (e.g., 1, 2).</param>
    /// <param name="path">The resource path relative to the API version (e.g., "public/users/123").</param>
    /// <returns>A complete versioned URL (e.g., "/api/v1/public/users/123").</returns>
    private static string Build(int version, string path)
    {
        path = path.TrimStart('/');
        return $"/api/v{version}/{path}";
    }

    /// <summary>
    /// Extracts the API version from the HTTP request's route values.
    /// The version is captured from the route pattern "api/v{version:apiVersion}".
    /// </summary>
    /// <param name="context">The HTTP context containing route data.</param>
    /// <returns>The API version number extracted from the route, or 1 as the default fallback.</returns>
    private static int GetVersionFromContext(HttpContext context)
    {
        RouteData? routeData = context.GetRouteData();

        if (routeData?.Values.TryGetValue("version", out object? versionValue) != true)
        {
            return 1;
        }

        return int.TryParse(versionValue?.ToString(), out int version) ? version : 1;
    }
}
