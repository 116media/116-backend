using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;

/// <summary>
/// Contains metadata information for the archive lyrics route.
/// </summary>
public static class AdminArchiveLyricsMetaField
{
    public static readonly RouteMetadata ArchiveLyrics = new(
        "ArchiveLyrics",
        "Archive a lyrics page",
        """
            Archives a lyrics page, removing it from all public feeds without permanently deleting it.
            \n
            Archiving is a reversible operation. Use archiving instead of deletion when you want
            to temporarily hide content without losing it permanently.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the lyrics page does not exist\n
        """
    );
}
