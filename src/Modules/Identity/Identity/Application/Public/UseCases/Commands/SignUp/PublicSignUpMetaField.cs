using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Public.UseCases.Commands.SignUp;

/// <summary>
/// Contains metadata information for the public user signup route.
/// </summary>
public static class PublicSignUpMetaField
{
    /// <summary>
    /// Metadata describing the public user signup endpoint.
    /// </summary>
    public static readonly RouteMetadata PublicSignUp = new(
        name: "PublicSignUp",
        summary: "Register a new public user account",
        description: """
             Registers a new public user by creating an account with the provided details.\n
             The created user account will initially have the Visitor role and related permissions,
             granting basic public access until further elevated by admins.
             \n
             This endpoint performs the following operations:\n
             - Validates signup data (email, username, password, etc.)\n
             - Ensures the email/username is unique\n
             - Hashes the password using secure algorithms (bcrypt)\n
             - Creates a new public user account in the system\n
             - Triggers optional account verification (email/SMS)\n
             \n
             **Authentication Requirements:**\n
             - No authentication required; open to the public for account creation\n
             \n
             **Security Features:**\n
             - Password securely hashed before storage\n
             - Uniqueness checks on email and username\n
             - Optional verification workflow (e.g., email confirmation)\n
             \n
             **Response Codes:**\n
             - Returns 201 Created with newly created user info (excluding sensitive data)\n
             - Returns 400 Bad Request for invalid input or weak password\n
             - Returns 409 Conflict if email/username already exists\n
             \n
             **Error Handling:**\n
             - BadRequestException (400): Invalid signup data (missing/invalid fields, weak password)\n
             - ConflictException (409): Email or username already in use.
         """
    );
}
