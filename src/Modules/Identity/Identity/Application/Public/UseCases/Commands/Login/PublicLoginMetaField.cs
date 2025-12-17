using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Public.UseCases.Commands.Login;

/// <summary>
/// Contains metadata information for the public users login route.
/// </summary>
public static class PublicLoginMetaField
{
    public static readonly RouteMetadata PublicLogin = new(
        name: "PublicLogin",
        summary: "Authenticate public user and return JWT token with user claims",
        description: """
             Authenticates a public user using email/userName and password credentials.
             The returned JWT token includes claims for accessing public user's endpoints.
             \n
             This endpoint performs enhanced authentication by:\n
             - Validating credentials and password\n
             - Verifying the account is active and verified\n
             - Generating JWT token with appropriate user claims\n
             - Recording the login activity\n
             \n
             **Authentication Requirements:**\n
             - Valid email/userName and password combination\n
             - Account must be active and verified\n
             \n
             **Security Features:**\n
             - Password verification using secure hashing (bcrypt)\n
             - Login activity tracking\n
             - Basic JWT claims for public users operations\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with user info and JWT token on successful authentication\n
             - Returns 400 Bad Request for invalid email/userName or incorrect password\n
             - Returns 403 Forbidden when user account is inactive or disabled\n
             - Returns 404 Not Found when no user exists with the provided email/userName\n
             \n
             **Error Handling:**\n
             - AuthorizationException (403): Account inactive - user exists but account is disabled/suspended\n
             - BadRequestException (400): Invalid password - email/userName exists but password is incorrect\n
             - NotFoundException (404): User not found - no account exists with the provided email/userName.
         """
    );
}

