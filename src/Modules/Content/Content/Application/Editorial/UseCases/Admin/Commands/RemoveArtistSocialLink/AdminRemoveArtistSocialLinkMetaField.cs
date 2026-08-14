using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink;

/// <summary>
/// Contains metadata information for the remove artist social link route.
/// </summary>
public static class AdminRemoveArtistSocialLinkMetaField
{
    public static readonly RouteMetadata RemoveArtistSocialLink = new(
        "RemoveArtistSocialLink",
        "Remove an artist's social link for a platform",
        """
            Removes the artist's social link for a single platform. Removing a platform
            that has no link returns 404 rather than a silent success.
            \n
            **Authentication Requirements:**\n
            - Requires authentication with an active account\n
            - Requires Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if not authenticated\n
            - Returns 403 Forbidden if lacking the required role\n
            - Returns 404 Not Found if no link exists for that platform\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
