using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Contains metadata information for the create lyrics route.
/// </summary>
public static class CreateLyricsMetaField
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
            - Linked to an article by providing an <c>articleId</c> (e.g., a dedicated Lyrics Page article)
            \n
            Returns a conflict error if lyrics for the same song title and artist already exist.
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
            - Returns 409 Conflict if lyrics for the same song and artist already exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
