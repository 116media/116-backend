using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;

/// <summary>
/// Contains metadata information for the request artist claim route.
/// </summary>
public static class PublicRequestArtistClaimMetaField
{
    public static readonly RouteMetadata RequestArtistClaim = new(
        "RequestArtistClaim",
        "Request ownership of an artist profile",
        """
            Records a request from the authenticated user to claim ownership of an artist
            profile. This does not grant ownership — staff review the request and confirm it
            separately through the admin verify-owner endpoint.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            \n
            **Response Codes:**\n
            - Returns 200 OK once the request has been recorded\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the artist profile does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
