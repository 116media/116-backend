using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.SignUp;

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
             Registers a new public user by creating an account with the provided details.

             This endpoint performs the following operations:
             - Validates signup data (email, username, password, etc.)
             - Ensures the email/username is unique
             - Hashes the password using secure algorithms (bcrypt)
             - Creates a new public user account in the system
             - Triggers optional account verification (email/SMS)

             **Authentication Requirements:**
             - No authentication required; open to the public for account creation

             **Security Features:**
             - Password securely hashed before storage
             - Uniqueness checks on email and username
             - Optional verification workflow (e.g., email confirmation)

             **Response Codes:**
             - Returns 201 Created with newly created user info (excluding sensitive data)
             - Returns 400 Bad Request for invalid input or weak password
             - Returns 409 Conflict if email/username already exists

             **Error Handling:**
             - BadRequestException (400): Invalid signup data (missing/invalid fields, weak password)
             - ConflictException (409): Email or username already in use

             The created user account will initially have the Visitor role and related permissions,
             granting basic public access until further elevated by admins.
         """
    );
}
