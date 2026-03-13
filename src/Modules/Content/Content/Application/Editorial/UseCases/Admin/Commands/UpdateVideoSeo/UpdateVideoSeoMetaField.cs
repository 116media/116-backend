using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Contains metadata information for the update video SEO route.
/// </summary>
public static class UpdateVideoSeoMetaField
{
    public static readonly RouteMetadata UpdateVideoSeo = new(
        "UpdateVideoSeo",
        "Update video SEO metadata",
        """
            Updates a video's SEO metadata fields (meta title and meta description).
            \n
            If <c>metaTitle</c> is null, the video's display title is used as the SEO title.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated video details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
