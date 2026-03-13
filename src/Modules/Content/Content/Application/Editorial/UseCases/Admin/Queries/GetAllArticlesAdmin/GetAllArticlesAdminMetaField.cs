using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticlesAdmin;

/// <summary>
/// Contains metadata information for the get all articles admin route.
/// </summary>
public static class GetAllArticlesAdminMetaField
{
    public static readonly RouteMetadata GetAllArticlesAdmin = new(
        "GetAllArticlesAdmin",
        "List all articles",
        """
            Retrieves a paginated list of all articles for admin management.
            \n
            Supports optional filtering by content status (e.g., Draft, PendingReview, Published)
            and by category. Results are returned as a paginated list with summary information
            suitable for list and management views.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated article list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
