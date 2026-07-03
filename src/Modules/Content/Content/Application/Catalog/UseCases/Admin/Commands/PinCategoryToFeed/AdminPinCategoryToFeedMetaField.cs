using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Contains metadata information for the pin category to feed route.
/// </summary>
public static class AdminPinCategoryToFeedMetaField
{
    public static readonly RouteMetadata PinCategoryToFeed = new(
        "AdminPinCategoryToFeed",
        "Pin a category to the content feed",
        """
            Pins a category to the content feed so it appears as a section on the homepage,
            displaying its latest published videos.
            \n
            At most five categories per content type can be pinned at a time. Pinning a sixth
            category automatically unpins the oldest pinned category (FIFO). A category must be
            active, of the Video content type, and have at least the minimum number of published
            videos before it can be pinned.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated category details on success\n
            - Returns 400 Bad Request if the category is inactive, not a video category, or has too few published videos\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
