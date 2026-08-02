using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink;

/// <summary>
/// Contains metadata information for the remove album streaming link route.
/// </summary>
public static class AdminRemoveAlbumStreamingLinkMetaField
{
    public static readonly RouteMetadata RemoveAlbumStreamingLink = new(
        "RemoveAlbumStreamingLink",
        "Remove an album's curated streaming link for a platform",
        """
            Removes the curated deep link URL for an album's streaming platform slot, reverting
            that platform's public link back to the generated search-query fallback. A no-op if
            no curated link exists for the given album and platform.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
