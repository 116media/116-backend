using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Contains metadata information for the update lyrics SEO route.
/// </summary>
public static class AdminUpdateLyricsSeoMetaField
{
    public static readonly RouteMetadata AdminUpdateLyricsSeo = new(
        "UpdateLyricsSeo",
        "Update SEO metadata for a lyrics page",
        """
            Updates the SEO metadata fields of the specified lyrics page, including meta title,
            meta description, meta keywords, and Schema.org JSON-LD structured data.
            \n
            Passing <c>null</c> for any field clears its value. This endpoint does not affect
            the lyrics text itself — use <c>PUT /api/v1/admin/lyrics/{id}</c> for that.
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
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
