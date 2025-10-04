using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.ResendOtp;

/// <summary>
/// Contains metadata information for the public resend OTP route.
/// </summary>
public static class PublicResendOtpMetaField
{
    public static readonly RouteMetadata ResendOtp = new(
        name: "PublicResendOtp",
        summary: "Resend OTP verification code for public users",
        description: """
             Resends a new OTP verification code for public users by invalidating existing OTPs and generating a fresh one.
             \n
             This endpoint enables users to request a new verification code when:\n
             - The original OTP wasn't received\n
             - The previous OTP has expired\n
             - There were issues with email delivery\n
             - Maximum attempts were reached on the previous OTP\n
             \n
             **Request Requirements:**\n
             - Valid email address format\n
             - Valid OTP purpose (EmailVerification, PasswordReset, TwoFactorAuthentication, AccountRecovery)\n
             - Account must be active\n
             \n
             **Security Features:**\n
             - Account active status validation\n
             - Automatic invalidation of existing OTPs for the specified purpose\n
             - New OTP generation with fresh expiration time\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with success status when OTP is resent\n
             - Returns 400 Bad Request for invalid email format or purpose\n
             - Returns 404 Not Found when user doesn't exist\n
             - Returns 403 Forbidden when user account is inactive\n
             \n
             **Process Flow:**\n
             1. Validates email format and OTP purpose\n
             2. Verifies user exists\n
             3. Checks account is active and verified\n
             4. Invalidates all existing OTPs for the specified purpose\n
             5. Generates new OTP with fresh expiration\n
             6. Returns success response\n
             \n
             **Supported OTP Purposes:**\n
             - EmailVerification: For email address verification\n
             - PasswordReset: For password reset requests\n
             - TwoFactorAuthentication: For 2FA setup/verification\n
             - AccountRecovery: For account recovery processes
         """
    );
}
