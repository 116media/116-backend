using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Contains metadata information for the social login route.
/// </summary>
public static class PublicSocialLoginMetaField
{
    /// <summary>
    /// Metadata describing the social login endpoint.
    /// </summary>
    public static readonly RouteMetadata SocialLogin = new(
        "PublicSocialLogin",
        "Authenticate user via social provider",
        """
            Authenticates a user through external social providers (Google or Facebook).\n
            Social users are automatically verified and granted visitor role permissions.
            Avatar images from social providers are downloaded and stored locally.
            \n
            This endpoint performs the following operations:\n
            - Validates social provider data (email, username, avatar URL, provider)\n
            - Checks for existing local account conflicts\n
            - Creates new user account or updates existing social user\n
            - Downloads and stores avatar from social provider URL\n
            - Assigns visitor role to new users\n
            - Marks social users as verified and active\n
            \n
            **Authentication Requirements:**\n
            - No authentication required; open to the public for social login\n
            \n
            **Supported Providers:**\n
            - Google OAuth\n
            - Facebook OAuth\n
            \n
            **Security Features:**\n
            - Prevents social login if local account exists with same email\n
            - Downloads external avatars to prevent hotlinking\n
            - Automatically verifies social accounts (trusted providers)\n
            - Updates user login status\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with user info and JWT token\n
            - Returns 400 Bad Request for invalid provider or malformed data\n
            - Returns 409 Conflict if local account exists with same email\n
            \n
            **Error Handling:**\n
            - BadRequestException (400): Invalid provider or malformed social data\n
            - ConflictException (409): Local account already exists with email.
        """
    );
}
