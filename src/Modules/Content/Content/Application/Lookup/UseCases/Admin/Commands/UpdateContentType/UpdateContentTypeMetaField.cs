using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Contains metadata information for the update content type route.
/// </summary>
public static class UpdateContentTypeMetaField
{
    public static readonly RouteMetadata UpdateContentType = new(
        "UpdateContentType",
        "Update a content type",
        """
            Updates the name of an existing content type.
            \n
            **Note:** Renaming a content type affects the label of all categories currently assigned to it.
            Ensure the new name accurately reflects the content format before saving.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated content type details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the content type does not exist\n
            - Returns 409 Conflict if the new name is already taken\n
        """
    );
}
