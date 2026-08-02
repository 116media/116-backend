using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink;

/// <summary>
/// Contains metadata information for the upsert album streaming link route.
/// </summary>
public static class AdminUpsertAlbumStreamingLinkMetaField
{
    public static readonly RouteMetadata UpsertAlbumStreamingLink = new(
        "UpsertAlbumStreamingLink",
        "Set or replace an album's curated streaming link for a platform",
        """
            Sets the curated deep link URL for an album's streaming platform slot. Creates a
            new streaming link row if none exists yet for the given album and platform,
            otherwise replaces the existing curated URL.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the streaming link id on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the album does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
