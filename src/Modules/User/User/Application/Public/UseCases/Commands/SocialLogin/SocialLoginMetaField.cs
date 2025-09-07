using _116.Shared.Application.Metadata;

namespace _116.User.Application.Public.UseCases.Commands.SocialLogin;

/// <summary>
/// Contains metadata information for the social login route.
/// </summary>
public static class SocialLoginMetaField
{
    /// <summary>
    /// Metadata describing the social login endpoint.
    /// </summary>
    public static readonly RouteMetadata SocialLogin = new(
        name: "SocialLogin",
        summary: "Authenticate user via social provider",
        description: """
             Authenticates a user through external social providers (Google or Facebook).

             This endpoint performs the following operations:
             - Validates social provider data (email, username, avatar URL, provider)
             - Checks for existing local account conflicts
             - Creates new user account or updates existing social user
             - Downloads and stores avatar from social provider URL
             - Assigns visitor role to new users
             - Marks social users as verified and active

             **Authentication Requirements:**
             - No authentication required; open to the public for social login

             **Supported Providers:**
             - Google OAuth
             - Facebook OAuth

             **Security Features:**
             - Prevents social login if local account exists with same email
             - Downloads external avatars to prevent hotlinking
             - Automatically verifies social accounts (trusted providers)
             - Updates user login status

             **Response Codes:**
             - Returns 200 OK with user info and JWT token
             - Returns 400 Bad Request for invalid provider or malformed data
             - Returns 409 Conflict if local account exists with same email

             **Error Handling:**
             - BadRequestException (400): Invalid provider or malformed social data
             - ConflictException (409): Local account already exists with email

             Social users are automatically verified and granted visitor role permissions.
             Avatar images from social providers are downloaded and stored locally.
         """
    );
}
