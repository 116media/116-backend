using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.ChangePassword;

/// <summary>
/// Contains metadata information for the password change route.
/// </summary>
public static class PublicChangePasswordMetaField
{
    /// <summary>
    /// Metadata describing the password change endpoint.
    /// </summary>
    public static readonly RouteMetadata ChangePassword = new(
        name: "ChangePassword",
        summary: "Change user password with current password verification",
        description: """
             Changes a user's password after verifying their current password for security.

             This endpoint performs the following operations:
             - Validates JWT token authentication and extracts user ID
             - Verifies user account is active and verified
             - Validates the current password against stored hash
             - Ensures new password is different from current password
             - Hashes the new password using secure algorithms
             - Updates the user's password in the database

             **Authentication Requirements:**
             - Valid JWT Bearer token required
             - Account must be active (not suspended/banned)
             - Account must be verified (email confirmed)
             - Only visitor role users can change their password

             **Security Features:**
             - Current password verification for authorization
             - Prevention of reusing the same password
             - Secure password hashing (PBKDF2 with SHA-256)
             - Strong password validation enforced by validator
             - Account status validation before password change

             **Request Requirements:**
             - Valid old password for verification
             - New password meeting security requirements
             - User must be authenticated with valid JWT token

             **Response Codes:**
             - Returns 200 OK with success status
             - Returns 400 Bad Request for invalid old password or same password
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for inactive or unverified accounts
             - Returns 404 Not Found for user not found
             - Returns 409 Conflict for new password same as old

             **Error Handling:**
             - BadRequestException (400): Invalid old password or inactive account
             - AuthenticationException (401): Invalid JWT token
             - AuthorizationException (403): Account not verified or insufficient permissions
             - NotFoundException (404): User not found
             - ConflictException (409): New password same as current password

             **Process Flow:**
             1. Validates JWT token and extracts user ID
             2. Validates old password and new password requirements
             3. Finds user by ID and validates account status
             4. Verifies current password matches provided old password
             5. Ensures new password is different from current password
             6. Hashes new password securely
             7. Updates user's password in database
             8. Returns success response

             After successful password change, the user continues using their existing session.
             The new password will be required for future logins.
         """
    );
}
