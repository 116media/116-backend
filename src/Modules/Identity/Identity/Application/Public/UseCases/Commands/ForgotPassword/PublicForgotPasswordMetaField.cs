using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.ForgotPassword;

/// <summary>
/// Contains metadata information for the forgot password route.
/// </summary>
public static class PublicForgotPasswordMetaField
{
    public static readonly RouteMetadata ForgotPassword = new(
        name: "PublicForgotPassword",
        summary: "Initiate password reset process for existing users",
        description: """
             Initiates the password reset process by generating an OTP for the specified email address.
             The generated OTP can be used with the verify-otp endpoint to proceed with password reset.
             \n
             This endpoint follows security best practices by:\n
             - Always returning success to prevent user enumeration attacks\n
             - Only generating OTP for valid, active, and verified accounts\n
             - Silently handling cases where email doesn't exist or account is inactive\n
             \n
             **Request Requirements:**\n
             - Valid email address format\n
             - Email must belong to an existing, active, and verified account\n
             \n
             **Security Features:**\n
             - User enumeration protection (consistent response regardless of email existence)\n
             - Account status validation (active and verified)\n
             - OTP generation with expiration time\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with success status (always true for security) and the email address\n
             - Returns 400 Bad Request for invalid email format\n
             \n
             **Process Flow:**\n
             1. Validates email format\n
             2. Checks if user exists and is active/verified\n
             3. Generates OTP for password reset\n
             4. Returns success response (regardless of actual outcome).
         """
    );
}
