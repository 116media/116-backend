using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Contains metadata information for the get promoted videos route.
/// </summary>
public static class PublicGetPromotedVideosMetaField
{
    public static readonly RouteMetadata GetPromotedVideos = new(
        "GetPromotedVideos",
        "List promoted videos",
        """
            Retrieves the list of currently promoted published videos.
            \n
            A video is promoted when a Commerce promotion purchase is verified.
            Only videos where is_promoted = true,
            promoted_until > now(), and status = Published are returned.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with promoted video list on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
