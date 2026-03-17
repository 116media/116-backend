using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Contains metadata information for the remove package slot route.
/// </summary>
public static class AdminRemovePackageSlotMetaField
{
    public static readonly RouteMetadata AdminRemovePackageSlot = new(
        "AdminRemovePackageSlot",
        "Remove a slot from a package",
        """
            Permanently removes a slot from the specified package.
            \n
            This operation cannot be undone. The slot and its category assignment will be deleted.
            Existing orders that referenced this package before the slot was removed are not affected.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the package or slot does not exist\n
        """
    );
}
