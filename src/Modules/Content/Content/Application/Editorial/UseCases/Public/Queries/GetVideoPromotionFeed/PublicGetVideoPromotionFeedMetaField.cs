using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;

/// <summary>
/// Contains metadata information for the video promotion feed route.
/// </summary>
public static class PublicGetVideoPromotionFeedMetaField
{
    public static readonly RouteMetadata GetVideoPromotionFeed = new(
        "GetVideoPromotionFeed",
        "Video homepage promotion feed",
        """
            Returns the homepage video promotion feed, grouping promoted published videos by
            spot priority (1, 2, 3).
            \n
            Each spot maps to a visual region on the homepage grid. Spot 3 distributes videos
            across two columns (a and b). Empty spots are filled with randomly selected free
            published videos (videos with no associated customer).
            \n
            A free video strip of up to 3 randomly selected videos is included below the grid.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the full promotion feed\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
