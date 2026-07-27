using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink;

/// <summary>
/// Contains metadata information for the upsert artist social link route.
/// </summary>
public static class AdminUpsertArtistSocialLinkMetaField
{
    public static readonly RouteMetadata UpsertArtistSocialLink = new(
        "UpsertArtistSocialLink",
        "Set or replace an artist's social link for a platform",
        """
            Sets or replaces the artist's social link for a single platform. Creates a new
            link row if none exists yet for the given artist and platform, otherwise
            replaces the existing URL.
            \n
            The URL must be an absolute https URL — other schemes are rejected.
            \n
            **Authentication Requirements:**\n
            - Requires authentication with an active account\n
            - Requires Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the upserted link's id on success\n
            - Returns 400 Bad Request if the URL is missing, too long, or not https\n
            - Returns 401 Unauthorized if not authenticated\n
            - Returns 403 Forbidden if lacking the required role\n
            - Returns 404 Not Found if the artist profile does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
