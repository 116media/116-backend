using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Contains metadata information for the rate video route.
/// </summary>
public static class PublicRateVideoMetaField
{
    public static readonly RouteMetadata PublicRateVideo = new(
        "PublicRateVideo",
        "Rate a video",
        """
            Submits or updates the authenticated user's star rating (1–5) for a video.
            If the user has already rated the video, the existing rating is updated.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if stars is not between 1 and 5\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
