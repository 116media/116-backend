using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Contains metadata information for the create package route.
/// </summary>
public static class AdminCreatePackageMetaField
{
    public static readonly RouteMetadata AdminCreatePackage = new(
        "AdminCreatePackage",
        "Create a package",
        """
            Creates a new content package with a flat price.
            \n
            A package groups multiple content slots into a named bundle deal (e.g., "Artist Starter Pack").
            Slots must be added separately after the package is created.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the created package details on success\n
            - Returns 400 Bad Request if the request body is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
        """
    );
}
