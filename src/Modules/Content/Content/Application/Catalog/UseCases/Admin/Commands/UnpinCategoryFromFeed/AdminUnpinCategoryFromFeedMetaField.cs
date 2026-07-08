using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;

/// <summary>
/// Contains metadata information for the unpin category from feed route.
/// </summary>
public static class AdminUnpinCategoryFromFeedMetaField
{
    public static readonly RouteMetadata UnpinCategoryFromFeed = new(
        "AdminUnpinCategoryFromFeed",
        "Unpin a category from the content feed",
        """
            Removes a category from the content feed so it no longer appears as a section
            on the homepage. Unpinning a category that is not currently pinned succeeds as a no-op.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated category details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
