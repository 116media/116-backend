using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;

/// <summary>
/// Contains metadata information for the upsert single streaming link route.
/// </summary>
public static class AdminUpsertSingleStreamingLinkMetaField
{
    public static readonly RouteMetadata UpsertSingleStreamingLink = new(
        "UpsertSingleStreamingLink",
        "Set or replace a standalone single's curated streaming link for a platform",
        """
            Sets the curated deep link URL for a standalone single's streaming platform slot.
            Creates a new streaming link row if none exists yet for the given lyrics page and
            platform, otherwise replaces the existing curated URL.
            \n
            Rejected with a conflict if the target lyrics page belongs to an album — a track
            that belongs to an album gets its streaming links through the album's own endpoint,
            not per-track.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the streaming link id on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 409 Conflict if the lyrics page belongs to an album\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
