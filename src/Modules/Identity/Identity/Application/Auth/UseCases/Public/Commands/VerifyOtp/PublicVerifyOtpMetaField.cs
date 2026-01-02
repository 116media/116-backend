using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Contains metadata information for the OTP verification route.
/// </summary>
public static class PublicVerifyOtpMetaField
{
    /// <summary>
    /// Metadata describing the OTP verification endpoint.
    /// </summary>
    public static readonly RouteMetadata VerifyOtp = new(
        "PublicVerifyOtp",
        "Verify OTP code for account activation",
        """
            Verifies the OTP (One-Time Password) code sent to the user's email for various purposes.
            The user must verify their account within the OTP expiration window to gain full access.
            \n
            **Supported OTP Purposes:**\n
            - **Email Verification**: During user account registration\n
            - **Account Recovery**: For account recovery processes\n
            \n
            This endpoint performs the following operations:\n
            - Validates the OTP code format (6-digit numeric)\n
            - Checks if the user exists and is not already verified\n
            - Validates the OTP against the database (not expired, not used, under attempt limit)\n
            - Marks the user account as verified upon successful validation\n
            - Invalidates all remaining OTPs for the user\n
            \n
            **Authentication Requirements:**\n
            - No authentication required; open to users with unverified accounts\n
            \n
            **Security Features:**\n
            - OTP expiration (60 minutes)\n
            - Maximum 3 verification attempts per OTP\n
            - Single-use OTP codes\n
            - Automatic cleanup of expired/used OTPs\n
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
            - NotFoundException (404): No valid OTP found for the user\n
            - ConflictException (409): User account is already verified.
        """
    );
}
