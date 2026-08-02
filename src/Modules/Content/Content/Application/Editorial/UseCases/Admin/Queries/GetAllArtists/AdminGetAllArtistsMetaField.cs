using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists;

/// <summary>
/// Contains metadata information for the get all artists route.
/// </summary>
public static class AdminGetAllArtistsMetaField
{
    public static readonly RouteMetadata GetAllArtists = new(
        "GetAllArtists",
        "List artist profiles",
        """
            Returns a paginated list of artist profiles, optionally filtered by a search term
            matched against the artist's name and biography.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the paginated artist list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
