using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Contains metadata information for the create tag route.
/// </summary>
public static class AdminCreateTagMetaField
{
    public static readonly RouteMetadata AdminCreateTag = new(
        "AdminCreateTag",
        "Create a new content tag",
        """
            Creates a new content discovery tag (e.g. "Fally Ipupa", "Kinshasa", "Afrobeats").
            \n
            This endpoint creates a tag by:\n
            - Validating the name and slug format\n
            - Checking that no tag with the same slug already exists\n
            - Creating the tag and returning its details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Request Body:**\n
            - name: The display name for the tag (max 50 characters)\n
            - slug: URL-safe identifier — lowercase letters, numbers, and hyphens only (max 60 characters)\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with tag details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 409 Conflict if tag slug already exists\n
        """
    );
}
