using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Contains metadata information for the deactivate package route.
/// </summary>
public static class AdminDeactivatePackageMetaField
{
    public static readonly RouteMetadata AdminDeactivatePackage = new(
        "AdminDeactivatePackage",
        "Deactivate a package",
        """
            Deactivates a package, removing it from available bundles for new orders.
            \n
            Existing orders that reference this package are not affected.
            The package can be restored later using the activate endpoint.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated package details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the package does not exist\n
            - Returns 409 Conflict if the package is already inactive\n
        """
    );
}
