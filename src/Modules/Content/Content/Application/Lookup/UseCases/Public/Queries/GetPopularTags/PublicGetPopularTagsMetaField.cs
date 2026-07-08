using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Contains metadata information for the public get popular tags route.
/// </summary>
public static class PublicGetPopularTagsMetaField
{
    public static readonly RouteMetadata GetPopularTags = new(
        "PublicGetPopularTags",
        "Get popular tags",
        """
            Returns the most-used tags ranked by their combined usage across articles
            and videos, most popular first.
            \n
            Results are cached server-side for 10 minutes to avoid running the
            aggregation query on every request.
            \n
            **Query Parameters:**\n
            - `limit` (optional, default 10): maximum number of tags to return\n
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of popular tags on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
