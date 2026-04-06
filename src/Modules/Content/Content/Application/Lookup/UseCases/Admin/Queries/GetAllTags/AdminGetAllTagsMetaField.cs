using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags;

/// <summary>
/// Contains metadata information for the admin get all tags route.
/// </summary>
public static class AdminGetAllTagsMetaField
{
    public static readonly RouteMetadata AdminGetAllTags = new(
        "AdminGetAllTags",
        "List all tags",
        """
            Returns the complete list of tags available in the system,
            with optional search filtering by name or slug.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of tags on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
