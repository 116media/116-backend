using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;

/// <summary>
/// Contains metadata information for the admin get short video by id route.
/// </summary>
public static class AdminGetShortByIdMetaField
{
    public static readonly RouteMetadata AdminGetShortById = new(
        "AdminGetShortById",
        "Get short video details",
        """
            Retrieves the full details of a single short video clip by its unique identifier.
            \n
            Returns the complete short video information including video URL, thumbnail, activity status,
            engagement counters, and parent video link if applicable.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with short video details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
