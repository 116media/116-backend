using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner;

/// <summary>
/// Contains metadata information for the verify artist owner route.
/// </summary>
public static class AdminVerifyArtistOwnerMetaField
{
    public static readonly RouteMetadata VerifyArtistOwner = new(
        "VerifyArtistOwner",
        "Verify and confirm an artist profile's owner",
        """
            Confirms an artist profile's ownership claim, linking it to the verified identity
            user. This is the only path by which an artist profile's ownership is actually set.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the claimed artist profile on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the artist profile does not exist\n
            - Returns 409 Conflict if the profile has already been claimed\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
