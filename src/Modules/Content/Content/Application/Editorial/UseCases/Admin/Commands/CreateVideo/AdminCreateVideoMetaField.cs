using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Contains metadata information for the create video route.
/// </summary>
public static class AdminCreateVideoMetaField
{
    public static readonly RouteMetadata AdminCreateVideo = new(
        "CreateVideo",
        "Create a new video",
        """
            Creates a new video shell (step 1 of the video creation flow).
            \n
            The video is created with a title, slug, and category only. The YouTube ID
            and thumbnail must be attached via subsequent endpoints before the video
            can be published.
            \n
            For paid (commissioned) videos, both <c>customerId</c> and <c>orderItemId</c>
            must be provided together. For free editorial content, both must be omitted.
            \n
            An optional <c>shootingScheduledAt</c> date can be provided for pre-booked
            productions where the client pays before the shoot takes place.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with video details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the specified category does not exist\n
            - Returns 409 Conflict if a video with the same slug already exists\n
        """
    );
}
