using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Contains metadata information for the update lyrics route.
/// </summary>
public static class AdminUpdateLyricsMetaField
{
    public static readonly RouteMetadata UpdateLyrics = new(
        "UpdateLyrics",
        "Update a lyrics page",
        """
            Replaces all editable fields of the specified lyrics page: category, song title,
            artist name, slug, lyrics text, language, linked video, and commerce fields.
            \n
            To update SEO metadata, use the <c>PATCH /api/v1/admin/lyrics/{id}/seo</c> endpoint.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated lyrics details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the lyrics record or category does not exist\n
            - Returns 409 Conflict if another lyrics page already uses the given slug\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
