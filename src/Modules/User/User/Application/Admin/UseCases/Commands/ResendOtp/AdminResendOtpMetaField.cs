using _116.Shared.Application.Metadata;

namespace _116.User.Application.Admin.UseCases.Commands.ResendOtp;

/// <summary>
/// Contains metadata information for the admin resend OTP route.
/// </summary>
public static class AdminResendOtpMetaField
{
    public static readonly RouteMetadata ResendOtp = new(
        name: "AdminResendOtp",
        summary: "Resend OTP verification code for admin users",
        description: """
             Resends a new OTP verification code for admin users by invalidating existing OTPs and generating a fresh one.

             This endpoint enables admins to request a new verification code when:
             - The original OTP wasn't received
             - The previous OTP has expired
             - There were issues with email delivery
             - Maximum attempts were reached on the previous OTP

             **Request Requirements:**
             - Valid admin email address format
             - Valid OTP purpose (EmailVerification, PasswordReset, TwoFactorAuthentication, AccountRecovery)
             - User must have admin privileges
             - Account must be active

             **Security Features:**
             - Admin role verification
             - Account active status validation
             - Automatic invalidation of existing OTPs for the specified purpose
             - New OTP generation with fresh expiration time

             **Response Codes:**
             - Returns 200 OK with success status when OTP is resent
             - Returns 400 Bad Request for invalid email format or purpose
             - Returns 404 Not Found when admin user doesn't exist
             - Returns 403 Forbidden when user lacks admin privileges

             **Process Flow:**
             1. Validates email format and OTP purpose
             2. Verifies admin user exists and has admin role
             3. Checks account is active and verified
             4. Invalidates all existing OTPs for the specified purpose
             5. Generates new OTP with fresh expiration
             6. Returns success response

             **Supported OTP Purposes:**
             - EmailVerification: For email address verification
             - PasswordReset: For password reset requests
             - TwoFactorAuthentication: For 2FA setup/verification
             - AccountRecovery: For account recovery processes
         """
    );
}
