using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Contains metadata information for the update video tags route.
/// </summary>
public static class UpdateVideoTagsMetaField
{
    public static readonly RouteMetadata UpdateVideoTags = new(
        "UpdateVideoTags",
        "Replace all tags on a video",
        """
            Replaces the full set of tags associated with a video.
            All existing tag associations are removed and replaced with the provided tag set.
            \n
            All provided tag identifiers must exist.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video or any tag does not exist\n
        """
    );
}
