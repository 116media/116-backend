using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// Contains metadata information for the public randomized shorts feed route.
/// </summary>
public static class PublicGetShortsFeedMetaField
{
    public static readonly RouteMetadata GetShortsFeed = new(
        "GetShortsFeed",
        "Get the randomized short videos feed",
        """
            Returns a cursor-paginated, seeded pseudo-random feed of active short videos.
            The ordering is stable for a given cursor session, so paging never drifts or repeats items.
            \n
            **Pagination:**\n
            - Omit the cursor to start a fresh randomized session; the first page returns a `nextCursor`.\n
            - Pass the returned `nextCursor` to fetch the following page.\n
            - A null `nextCursor` means the feed is exhausted.\n
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint).\n
            - When authenticated, each item carries the caller's `isLiked` / `isBookmarked` flags.\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the feed page on success.\n
            - Returns 429 Too Many Requests if rate limit is exceeded.\n
        """
    );
}
