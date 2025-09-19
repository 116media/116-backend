using _116.Shared.Application.Metadata;

namespace _116.User.Application.Admin.UseCases.Commands.ResetPassword;

/// <summary>
/// Contains metadata information for the admin password reset route.
/// </summary>
public static class AdminResetPasswordMetaField
{
    /// <summary>
    /// Metadata describing the admin password reset endpoint.
    /// </summary>
    public static readonly RouteMetadata ResetPassword = new(
        name: "AdminResetPassword",
        summary: "Reset admin user password using OTP verification",
        description: """
             Resets an admin user's password after validating the OTP code sent during the forgot password process.

             This endpoint performs the following operations:
             - Validates the OTP code format and authenticity
             - Checks if the admin user exists and is active
             - Validates the OTP against the database (not expired, not used, under attempt limit)
             - Hashes the new password using secure algorithms
             - Updates the admin user's password in the database
             - Invalidates all remaining password reset OTPs for the user

             **Authentication Requirements:**
             - No authentication required; open to admin users with valid OTP codes
             - Admin user account must be active

             **Security Features:**
             - OTP expiration (60 minutes)
             - Maximum 3 verification attempts per OTP
             - Single-use OTP codes
             - Secure password hashing (PBKDF2 with SHA-256)
             - Automatic cleanup of expired/used OTPs
             - Password validation enforced by validator

             **Request Requirements:**
             - Valid email address format
             - Valid OTP code (6-digit numeric)
             - New password meeting security requirements

             **Response Codes:**
             - Returns 200 OK with success status
             - Returns 400 Bad Request for invalid input or inactive account
             - Returns 401 Unauthorized for expired OTP
             - Returns 403 Forbidden for max attempts reached
             - Returns 404 Not Found for no valid OTP found or user not found

             **Error Handling:**
             - BadRequestException (400): Invalid input format, inactive account, or invalid OTP
             - AuthenticationException (401): OTP has expired
             - AuthorizationException (403): Maximum verification attempts reached
             - NotFoundException (404): No valid OTP found or user not found

             **Process Flow:**
             1. Validates email format and password requirements
             2. Finds admin user by email address
             3. Validates account is active
             4. Validates OTP code for password reset purpose
             5. Hashes new password securely
             6. Updates admin user's password
             7. Marks OTP as used and invalidates remaining OTPs
             8. Returns success response

             After successful password reset, the admin user can login with their new password.
         """
    );
}
