using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Contains metadata information for the create lyrics route.
/// </summary>
public static class AdminCreateLyricsMetaField
{
    public static readonly RouteMetadata CreateLyrics = new(
        "CreateLyrics",
        "Create a lyrics page",
        """
            Creates a new SEO-optimised lyrics page for a song.
            \n
            A lyrics page can be:
            - Standalone with no parent content
            - Linked to a full video by providing a <c>videoId</c> (e.g., lyric video, "Behind the Lyrics" episode)
            \n
            For paid (commissioned) lyrics pages, both <c>customerId</c> and <c>orderItemId</c>
            must be provided together. For free editorial content, both must be omitted.
            \n
            Returns a conflict error if a lyrics page with the same slug already exists.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with lyrics details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the specified category does not exist\n
            - Returns 409 Conflict if a lyrics page with the same slug already exists\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
