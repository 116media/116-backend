using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Contains metadata information for the update lyrics metadata route.
/// </summary>
public static class AdminUpdateLyricsMetadataMetaField
{
    public static readonly RouteMetadata UpdateLyricsMetadata = new(
        "UpdateLyricsMetadata",
        "Update song-credit metadata for a lyrics page",
        """
            Updates the song-credit metadata fields of the specified lyrics page: album,
            release year, record label, songwriter, and producer.
            \n
            Passing <c>null</c> for any field clears its value. This endpoint does not affect
            the lyrics content itself — use <c>PUT /api/v1/admin/lyrics/{id}</c> for that, or
            the cover image — use <c>POST /api/v1/admin/lyrics/{id}/cover</c> for that.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated lyrics details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the lyrics record does not exist\n
            - Returns 400 Bad Request if the release year is out of bounds or a credit field
              exceeds its maximum length\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
