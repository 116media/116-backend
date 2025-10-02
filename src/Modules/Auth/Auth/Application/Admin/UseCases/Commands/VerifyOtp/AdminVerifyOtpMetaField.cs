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

             **Supported OTP Purposes:**
             - **Email Verification**: During admin account registration
             - **Account Recovery**: For account recovery processes

             This endpoint performs the following operations:
             - Validates the OTP code format (6-digit numeric)
             - Checks if the admin user exists and is not already verified
             - Validates the OTP against the database (not expired, not used, under attempt limit)
             - Marks the admin user account as verified upon successful validation
             - Invalidates all remaining OTPs for the admin user

             **Authentication Requirements:**
             - No authentication required; open to admin users with unverified accounts

             **Security Features:**
             - OTP expiration (60 minutes)
             - Maximum 3 verification attempts per OTP
             - Single-use OTP codes
             - Automatic cleanup of expired/used OTPs
             - Admin role verification

             **Response Codes:**
             - Returns 200 OK with verification success status
             - Returns 400 Bad Request for invalid OTP code format
             - Returns 401 Unauthorized for expired OTP
             - Returns 403 Forbidden for maximum attempts reached
             - Returns 404 Not Found for no valid OTP found
             - Returns 409 Conflict if account is already verified

             **Error Handling:**
             - BadRequestException (400): Invalid OTP code format or value
             - AuthenticationException (401): OTP has expired
             - AuthorizationException (403): Maximum verification attempts reached
             - NotFoundException (404): No valid OTP found for the admin user

             The admin user must verify their account within the OTP expiration window to gain full access.
         """
    );
}
