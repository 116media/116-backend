using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Contains metadata information for the add package slot route.
/// </summary>
public static class AddPackageSlotMetaField
{
    public static readonly RouteMetadata AddPackageSlot = new(
        "AddPackageSlot",
        "Add a slot to a package",
        """
            Adds a new content slot to an existing package.
            \n
            A slot defines what content must be delivered as part of the package.
            Specify a category to lock the slot to a specific type of content,
            or leave the category empty to create an open slot where the client may choose any category.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the updated package details on success\n
            - Returns 400 Bad Request if the request body is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the package or category does not exist\n
        """
    );
}
