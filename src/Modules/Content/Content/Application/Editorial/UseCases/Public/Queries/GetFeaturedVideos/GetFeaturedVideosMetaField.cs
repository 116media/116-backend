using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;

/// <summary>
/// Contains metadata information for the get featured videos route.
/// </summary>
public static class GetFeaturedVideosMetaField
{
    public static readonly RouteMetadata GetFeaturedVideos = new(
        "GetFeaturedVideos",
        "List featured videos",
        """
            Retrieves the list of currently featured published videos.
            \n
            A video is featured when an admin stamps it with a future expiry date after a
            Commerce promotion purchase. Only videos where is_featured = true,
            featured_until > now(), and status = Published are returned.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with featured video list on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
