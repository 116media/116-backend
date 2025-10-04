using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Commands.VerifyOtp;

/// <summary>
/// Contains metadata information for the admin OTP verification route.
/// </summary>
public static class AdminVerifyOtpMetaField
{
    /// <summary>
    /// Metadata describing the admin OTP verification endpoint.
    /// </summary>
    public static readonly RouteMetadata VerifyOtp = new(
        name: "AdminVerifyOtp",
        summary: "Verify OTP code for admin account activation",
        description: """
             Verifies the OTP (One-Time Password) code sent to the admin user's email for various purposes.
             The admin user must verify their account within the OTP expiration window to gain full access.
             \n
             **Supported OTP Purposes:**\n
             - **Email Verification**: During admin account registration\n
             - **Account Recovery**: For account recovery processes\n
             \n
             This endpoint performs the following operations: \n
             - Validates the OTP code format (6-digit numeric)\n
             - Checks if the admin user exists and is not already verified\n
             - Validates the OTP against the database (not expired, not used, under attempt limit)\n
             - Marks the admin user account as verified upon successful validation\n
             - Invalidates all remaining OTPs for the admin user\n
             \n
             **Authentication Requirements:**\n
             - No authentication required; open to admin users with unverified accounts\n
             \n
             **Security Features:**\n
             - OTP expiration (60 minutes)\n
             - Maximum 3 verification attempts per OTP\n
             - Single-use OTP codes\n
             - Automatic cleanup of expired/used OTPs\n
             - Admin role verification\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with verification success status\n
             - Returns 400 Bad Request for invalid OTP code format\n
             - Returns 401 Unauthorized for expired OTP\n
             - Returns 403 Forbidden for maximum attempts reached\n
             - Returns 404 Not Found for no valid OTP found\n
             - Returns 409 Conflict if account is already verified\n
             \n
             **Error Handling:**\n
             - BadRequestException (400): Invalid OTP code format or value\n
             - AuthenticationException (401): OTP has expired\n
             - AuthorizationException (403): Maximum verification attempts reached\n
             - NotFoundException (404): No valid OTP found for the admin user.
         """
    );
}
