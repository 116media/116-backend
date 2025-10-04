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
             Changes a user's password after verifying their current password for security.\n
             After successful password change, the user continues using their existing session.
             The new password will be required for future logins.
             \n
             This endpoint performs the following operations:\n
             - Validates JWT token authentication and extracts user ID\n
             - Verifies user account is active and verified\n
             - Validates the current password against stored hash\n
             - Ensures new password is different from current password\n
             - Hashes the new password using secure algorithms\n
             - Updates the user's password in the database\n
             \n
             **Authentication Requirements:**\n
             - Valid JWT Bearer token required\n
             - Account must be active (not suspended/banned)\n
             - Account must be verified (email confirmed)\n
             - Only visitor role users can change their password\n
             \n
             **Security Features:**\n
             - Current password verification for authorization\n
             - Prevention of reusing the same password\n
             - Secure password hashing (PBKDF2 with SHA-256)\n
             - Strong password validation enforced by validator\n
             - Account status validation before password change\n
             \n
             **Request Requirements:**\n
             - Valid old password for verification\n
             - New password meeting security requirements\n
             - User must be authenticated with valid JWT token\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with success status\n
             - Returns 400 Bad Request for invalid old password or same password\n
             - Returns 401 Unauthorized for invalid/missing JWT token\n
             - Returns 403 Forbidden for inactive or unverified accounts\n
             - Returns 404 Not Found for user not found\n
             - Returns 409 Conflict for new password same as old\n
             \n
             **Error Handling:**\n
             - BadRequestException (400): Invalid old password or inactive account\n
             - AuthenticationException (401): Invalid JWT token\n
             - AuthorizationException (403): Account not verified or insufficient permissions\n
             - NotFoundException (404): User not found\n
             - ConflictException (409): New password same as current password\n
             \n
             **Process Flow:**\n
             1. Validates JWT token and extracts user ID\n
             2. Validates old password and new password requirements\n
             3. Finds user by ID and validates account status\n
             4. Verifies current password matches provided old password\n
             5. Ensures new password is different from current password\n
             6. Hashes new password securely\n
             7. Updates user's password in database\n
             8. Returns success response.
         """
    );
}
