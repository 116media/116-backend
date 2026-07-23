using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;

/// <summary>
/// Contains metadata information for the update artist profile route.
/// </summary>
public static class AdminUpdateArtistMetaField
{
    public static readonly RouteMetadata UpdateArtist = new(
        "UpdateArtist",
        "Update an artist profile",
        """
            Updates an artist profile's display name and biography.
            \n
            The URL slug is immutable after creation and cannot be changed here.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated artist profile on success\n
            - Returns 400 Bad Request if the payload fails validation\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the artist profile does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
