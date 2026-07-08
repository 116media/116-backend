using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Contains metadata information for the delete lyrics route.
/// </summary>
public static class AdminDeleteLyricsMetaField
{
    public static readonly RouteMetadata DeleteLyrics = new(
        "DeleteLyrics",
        "Permanently delete a lyrics page",
        """
            Permanently deletes a lyrics page from the database.
            \n
            If the lyrics page is linked to a video, the video's HasLyrics flag
            is cleared automatically before deletion.
            \n
            This operation is <b>irreversible</b>.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the lyrics record does not exist\n
        """
    );
}
