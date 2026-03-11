using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Contains metadata information for the update lyrics route.
/// </summary>
public static class AdminUpdateLyricsMetaField
{
    public static readonly RouteMetadata AdminUpdateLyrics = new(
        "UpdateLyrics",
        "Update the lyrics text",
        """
            Replaces the lyrics text of the specified lyrics page.
            \n
            Only the lyrics text body can be updated via this endpoint. To update SEO metadata,
            use the <c>PATCH /api/v1/admin/lyrics/{id}/seo</c> endpoint.
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
            - Returns 404 Not Found if the lyrics record does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
