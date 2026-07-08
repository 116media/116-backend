using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Contains metadata information for the public get popular videos route.
/// </summary>
public static class PublicGetPopularVideosMetaField
{
    public static readonly RouteMetadata GetPopularVideos = new(
        "GetPopularVideos",
        "Get popular videos",
        """
            Returns published videos ranked by a weighted engagement score
            (rating volume weighted by rating quality, plus shares),
            tie-broken by publish date descending.
            \n
            YouTube view/like/comment figures are not used; ranking is based only on the
            platform's own engagement (ratings and shares).
            \n
            Results are cached server-side for 10 minutes to avoid running the
            ranking query on every request.
            \n
            **Query Parameters:**\n
            - `limit` (optional, default 10, max 50): maximum number of videos to return\n
            - `categoryId` (optional): rank only videos in this category\n
            - `excludeId` (optional): video id to omit, e.g. the video currently being viewed\n
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of popular videos on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
