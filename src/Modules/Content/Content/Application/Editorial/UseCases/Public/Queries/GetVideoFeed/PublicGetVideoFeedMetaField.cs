using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Contains metadata information for the public video feed route.
/// </summary>
public static class PublicGetVideoFeedMetaField
{
    public static readonly RouteMetadata GetVideoFeed = new(
        "PublicGetVideoFeed",
        "Get the public video feed",
        """
            Returns the homepage video feed as an ordered list of sections, one per category
            pinned to the feed (most recently pinned first). Each section contains the category
            metadata and its latest published videos. Sections whose category has no published
            videos are omitted.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of feed sections\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
