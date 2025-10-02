using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Commands.ForgotPassword;

/// <summary>
/// Contains metadata information for the admin forgot password route.
/// </summary>
public static class AdminForgotPasswordMetaField
{
    public static readonly RouteMetadata ForgotPassword = new(
        name: "AdminForgotPassword",
        summary: "Initiate password reset process for existing admin users",
        description: """
             Initiates the password reset process by generating an OTP for the specified admin email address.

             This endpoint follows security best practices by:
             - Always returning success to prevent user enumeration attacks
             - Only generating OTP for valid and active admin accounts
             - Silently handling cases where email doesn't exist or account is inactive

             **Request Requirements:**
             - Valid email address format
             - Email must belong to an existing and active admin account

             **Security Features:**
             - User enumeration protection (consistent response regardless of email existence)
             - Account status validation (active admin accounts only)
             - OTP generation with expiration time

             **Response Codes:**
             - Returns 200 OK with success status (always true for security)
             - Returns 400 Bad Request for invalid email format

             **Process Flow:**
             1. Validates email format
             2. Checks if admin user exists and is active
             3. Generates OTP for password reset
             4. Returns success response (regardless of actual outcome)

             The generated OTP can be used with the verify-otp endpoint to proceed with password reset.
         """
    );
}
