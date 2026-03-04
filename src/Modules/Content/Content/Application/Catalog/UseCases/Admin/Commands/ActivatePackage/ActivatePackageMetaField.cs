using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Contains metadata information for the activate package route.
/// </summary>
public static class ActivatePackageMetaField
{
    public static readonly RouteMetadata ActivatePackage = new(
        "ActivatePackage",
        "Activate a package",
        """
            Activates a package, making it available for new orders.
            \n
            An inactive package cannot be added to new client orders.
            This operation restores it to active status.
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
            - Returns 409 Conflict if the package is already active\n
        """
    );
}
