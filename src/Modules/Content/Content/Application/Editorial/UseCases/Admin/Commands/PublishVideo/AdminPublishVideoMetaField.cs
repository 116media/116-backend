using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Contains metadata information for the publish video route.
/// </summary>
public static class AdminPublishVideoMetaField
{
    public static readonly RouteMetadata AdminPublishVideo = new(
        "PublishVideo",
        "Publish an approved video",
        """
            Publishes a video that is currently in <c>Approved</c> status,
            transitioning it to <c>Published</c> and making it visible to all public visitors.
            \n
            Only videos in <c>Approved</c> status can be published.
            Attempting to publish a video in any other status will return a 400 Bad Request.
            \n
            A YouTube video ID must be attached before publishing.
            The domain method enforces this gate — if no YouTube ID is attached,
            a 400 Bad Request will be returned.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the video is not in Approved status or has no YouTube ID\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
