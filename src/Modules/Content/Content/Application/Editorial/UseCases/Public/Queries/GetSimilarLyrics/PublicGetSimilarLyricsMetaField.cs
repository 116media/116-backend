using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;

/// <summary>
/// Contains metadata information for the get similar lyrics route.
/// </summary>
public static class PublicGetSimilarLyricsMetaField
{
    public static readonly RouteMetadata GetSimilarLyrics = new(
        "GetSimilarLyrics",
        "Get similar lyrics pages",
        """
            Retrieves lyrics pages similar to the given lyrics page, via a three-way waterfall:
            published pages linked to a video in the same category, then published pages
            sharing at least one tag, then the most recent standalone published pages. The
            first non-empty branch wins.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with a list of similar lyrics pages, which is empty when no
              branch of the waterfall yields any matches\n
            - Returns 404 Not Found if the given lyrics page id does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
