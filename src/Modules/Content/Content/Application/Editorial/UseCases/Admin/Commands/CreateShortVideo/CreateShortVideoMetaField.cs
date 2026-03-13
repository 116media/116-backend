using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Contains metadata information for the create short video route.
/// </summary>
public static class CreateShortVideoMetaField
{
    public static readonly RouteMetadata CreateShortVideo = new(
        "CreateShortVideo",
        "Upload and create a new short video clip",
        """
            Uploads a video file to Cloudinary and creates a new short video record.
            \n
            Short videos are standalone loopable clips (gossip, reels, quick previews) uploaded
            directly to cloud storage — not YouTube. They bypass the editorial approval workflow.
            \n
            Optionally, a short video can be linked to a full video by providing a <c>videoId</c>,
            making it a teaser clip for the parent production.
            \n
            The video file must be submitted as <c>multipart/form-data</c>.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with short video details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
