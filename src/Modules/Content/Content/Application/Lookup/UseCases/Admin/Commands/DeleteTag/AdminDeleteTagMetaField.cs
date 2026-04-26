using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Contains metadata information for the delete tag route.
/// </summary>
public static class AdminDeleteTagMetaField
{
    public static readonly RouteMetadata AdminDeleteTag = new(
        "AdminDeleteTag",
        "Permanently delete a content tag",
        """
            Permanently and irreversibly deletes a content tag from the system.
            \n
            This endpoint deletes a tag by:\n
            - Validating the tag ID format\n
            - Locating the tag or throwing a 404 if it does not exist\n
            - Hard deleting the record from the database\n
            \n
            **Warning:** This operation is irreversible. Once deleted, the tag cannot be restored.\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success confirmation on deletion\n
            - Returns 400 Bad Request if the ID format is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the tag does not exist\n
        """
    );
}
