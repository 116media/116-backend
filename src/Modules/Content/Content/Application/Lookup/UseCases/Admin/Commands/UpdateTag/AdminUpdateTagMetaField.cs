using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Contains metadata information for the update tag route.
/// </summary>
public static class AdminUpdateTagMetaField
{
    public static readonly RouteMetadata AdminUpdateTag = new(
        "AdminUpdateTag",
        "Update a content tag",
        """
            Updates an existing content tag's name and slug.
            \n
            This endpoint updates a tag by:\n
            - Validating the new name and slug format\n
            - Checking that no other tag with the same slug already exists\n
            - Applying the name and slug changes and returning the updated tag\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Request Body:**\n
            - name: The new display name for the tag (max 50 characters)\n
            - slug: The new URL-safe identifier — lowercase letters, numbers, and hyphens only (max 60 characters)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated tag details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the tag does not exist\n
            - Returns 409 Conflict if another tag with the same slug already exists\n
        """
    );
}
