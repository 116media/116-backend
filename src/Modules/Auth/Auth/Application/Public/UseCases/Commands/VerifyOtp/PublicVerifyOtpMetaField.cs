using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.VerifyOtp;

/// <summary>
/// Contains metadata information for the OTP verification route.
/// </summary>
public static class PublicVerifyOtpMetaField
{
    /// <summary>
    /// Metadata describing the OTP verification endpoint.
    /// </summary>
    public static readonly RouteMetadata VerifyOtp = new(
        name: "VerifyOtp",
        summary: "Verify OTP code for account activation",
        description: """
             Verifies the OTP (One-Time Password) code sent to the user's email for various purposes.

             **Supported OTP Purposes:**
             - **Email Verification**: During user account registration
             - **Account Recovery**: For account recovery processes

             This endpoint performs the following operations:
             - Validates the OTP code format (6-digit numeric)
             - Checks if the user exists and is not already verified
             - Validates the OTP against the database (not expired, not used, under attempt limit)
             - Marks the user account as verified upon successful validation
             - Invalidates all remaining OTPs for the user

             **Authentication Requirements:**
             - No authentication required; open to users with unverified accounts

             **Security Features:**
             - OTP expiration (60 minutes)
             - Maximum 3 verification attempts per OTP
             - Single-use OTP codes
             - Automatic cleanup of expired/used OTPs

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
             - NotFoundException (404): No valid OTP found for the user
             - ConflictException (409): User account is already verified

             The user must verify their account within the OTP expiration window to gain full access.
         """
    );
}
