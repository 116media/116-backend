using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Contains metadata information for the archive video route.
/// </summary>
public static class AdminArchiveVideoMetaField
{
    public static readonly RouteMetadata AdminArchiveVideo = new(
        "ArchiveVideo",
        "Archive a video",
        """
            Archives a video, removing it from all public feeds without permanently deleting it.
            \n
            Archiving is reversible — Cloudinary thumbnail assets are <b>not</b> deleted.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
