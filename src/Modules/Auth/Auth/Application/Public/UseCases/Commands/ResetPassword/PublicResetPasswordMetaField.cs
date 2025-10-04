using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.ResetPassword;

/// <summary>
/// Contains metadata information for the password reset route.
/// </summary>
public static class PublicResetPasswordMetaField
{
    /// <summary>
    /// Metadata describing the password reset endpoint.
    /// </summary>
    public static readonly RouteMetadata ResetPassword = new(
        name: "ResetPassword",
        summary: "Reset user password using OTP verification",
        description: """
             Resets a user's password after validating the OTP code sent during the forgot password process.
             After successful password reset, the user can login with their new password.
             \n
             This endpoint performs the following operations:\n
             - Validates the OTP code format and authenticity\n
             - Checks if the user exists and is active/verified\n
             - Validates the OTP against the database (not expired, not used, under attempt limit)\n
             - Hashes the new password using secure algorithms\n
             - Updates the user's password in the database\n
             - Invalidates all remaining password reset OTPs for the user\n
             \n
             **Authentication Requirements:**\n
             - No authentication required; open to users with valid OTP codes\n
             - User account must be active and verified\n
             \n
             **Security Features:**\n
             - OTP expiration (60 minutes)\n
             - Maximum 3 verification attempts per OTP\n
             - Single-use OTP codes\n
             - Secure password hashing (PBKDF2 with SHA-256)\n
             - Automatic cleanup of expired/used OTPs\n
             - Password validation enforced by validator\n
             \n
             **Request Requirements:**\n
             - Valid email address format\n
             - Valid OTP code (6-digit numeric)\n
             - New password meeting security requirements\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with success status\n
             - Returns 400 Bad Request for invalid input or inactive account\n
             - Returns 401 Unauthorized for expired OTP\n
             - Returns 403 Forbidden for max attempts reached or unverified account\n
             - Returns 404 Not Found for no valid OTP found or user not found\n
             \n
             **Error Handling:**\n
             - BadRequestException (400): Invalid input format, inactive account, or invalid OTP\n
             - AuthenticationException (401): OTP has expired\n
             - AuthorizationException (403): Maximum verification attempts reached or account not verified\n
             - NotFoundException (404): No valid OTP found or user not found\n
             \n
             **Process Flow:**\n
             1. Validates email format and password requirements\n
             2. Finds user by email address\n
             3. Validates account is active and verified\n
             4. Validates OTP code for password reset purpose\n
             5. Hashes new password securely\n
             6. Updates user's password\n
             7. Marks OTP as used and invalidates remaining OTPs\n
             8. Returns success response.
         """
    );
}
