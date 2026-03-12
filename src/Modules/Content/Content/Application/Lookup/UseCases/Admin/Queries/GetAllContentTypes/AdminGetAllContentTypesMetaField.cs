using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes;

/// <summary>
/// Contains metadata information for the get all content types route.
/// </summary>
public static class AdminGetAllContentTypesMetaField
{
    public static readonly RouteMetadata AdminGetAllContentTypes = new(
        "AdminGetAllContentTypes",
        "List all content types",
        """
            Returns the complete list of content types available in the system.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of content types on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
