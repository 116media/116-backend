using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Contains metadata information for the refresh token route.
/// </summary>
public static class PublicRefreshTokenMetaField
{
    public static readonly RouteMetadata PublicRefreshToken = new(
        "PublicRefreshToken",
        "Refresh access token using a valid refresh token",
        """
            Validates and rotates a refresh token to obtain a new access token.
            Implements token rotation for enhanced security - the old refresh token is invalidated.
            \n
            This endpoint performs token refresh by:\n
            - Validating the provided refresh token\n
            - Verifying the session is still active and not expired\n
            - Generating a new access token\n
            - Rotating the refresh token (old token becomes invalid)\n
            - Returning new authentication credentials\n
            \n
            **Authentication Requirements:**\n
            - Valid, non-expired refresh token\n
            - Session must be active (not logged out or revoked)\n
            \n
            **Security Features:**\n
            - Automatic token rotation prevents token reuse\n
            - Refresh token hashing for secure storage\n
            - Session validation ensures only active sessions can refresh\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with new tokens on successful refresh\n
            - Returns 403 Forbidden for invalid or expired refresh tokens\n
            \n
            **Error Handling:**\n
            - AuthorizationException (403): Invalid/expired refresh token or session revoked\n
            - Token rotation ensures old refresh tokens cannot be reused after successful refresh.
        """
    );
}
