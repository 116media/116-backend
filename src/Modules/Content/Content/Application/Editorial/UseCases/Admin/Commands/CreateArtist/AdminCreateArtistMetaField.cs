using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Contains metadata information for the create artist profile route.
/// </summary>
public static class AdminCreateArtistMetaField
{
    public static readonly RouteMetadata CreateArtist = new(
        "CreateArtist",
        "Create a new artist profile",
        """
            Creates a new, unclaimed artist profile — typically staff-curated from an existing
            lyrics or video record's free-text artist name.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the artist profile on success\n
            - Returns 400 Bad Request if the payload fails validation\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 409 Conflict if the slug is already in use\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
