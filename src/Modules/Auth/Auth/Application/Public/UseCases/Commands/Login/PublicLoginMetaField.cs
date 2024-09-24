using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.Login;

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

             This endpoint performs enhanced authentication by:
             - Validating credentials and password
             - Verifying the account is active and verified
             - Generating JWT token with appropriate user claims
             - Recording the login activity

             **Authentication Requirements:**
             - Valid email/userName and password combination
             - Account must be active and verified

             **Security Features:**
             - Password verification using secure hashing (bcrypt)
             - Login activity tracking
             - Basic JWT claims for public users operations

             **Response Codes:**
             - Returns 200 OK with user info and JWT token on successful authentication
             - Returns 400 Bad Request for invalid email/userName or incorrect password
             - Returns 403 Forbidden when user account is inactive or disabled
             - Returns 404 Not Found when no user exists with the provided email/userName

             **Error Handling:**
             - AuthorizationException (403): Account inactive - user exists but account is disabled/suspended
             - BadRequestException (400): Invalid password - email/userName exists but password is incorrect
             - NotFoundException (404): User not found - no account exists with the provided email/userName

             The returned JWT token includes claims for accessing public user's endpoints.
         """
    );
}

