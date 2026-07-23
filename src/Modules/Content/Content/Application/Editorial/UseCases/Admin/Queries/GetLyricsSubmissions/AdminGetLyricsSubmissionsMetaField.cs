using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions;

/// <summary>
/// Contains metadata information for the admin get lyrics submissions route.
/// </summary>
public static class AdminGetLyricsSubmissionsMetaField
{
    public static readonly RouteMetadata GetLyricsSubmissions = new(
        "GetLyricsSubmissions",
        "List the community lyrics submission moderation queue",
        """
            Retrieves a paginated view of community-submitted new songs awaiting moderation.
            \n
            Supports optional filtering by moderation status (Pending, Approved, Rejected, or
            NeedsRevision).
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the paginated submission list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
