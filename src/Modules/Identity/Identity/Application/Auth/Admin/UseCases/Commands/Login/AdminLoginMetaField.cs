using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.Login;

/// <summary>
/// Contains metadata information for the admin login route.
/// </summary>
public static class AdminLoginMetaField
{
    public static readonly RouteMetadata AdminLogin = new(
        name: "AdminLogin",
        summary: "Authenticate admin and return JWT token with admin claims",
        description: """
             Authenticates an admin user using email and password credentials.
             The returned JWT token includes admin-specific claims for accessing administrative endpoints.
             \n
             This endpoint performs enhanced authentication by:\n
             - Validating email and password\n
             - Verifying the account is active and verified\n
             - Checking for admin role privileges (Admin or SuperAdmin)\n
             - Generating JWT token with appropriate admin claims\n
             - Recording the login activity\n
             \n
             **Authentication Requirements:**\n
             - Valid email and password combination\n
             - Account must be active and verified\n
             - User must have Admin or SuperAdmin role assigned\n
             \n
             **Security Features:**\n
             - Password verification using secure hashing (bcrypt)\n
             - Role-based access validation\n
             - Login activity tracking\n
             - Enhanced JWT claims for admin operations\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with user info and JWT token on successful authentication\n
             - Returns 400 Bad Request for invalid email format or incorrect password\n
             - Returns 401 Unauthorized when user lacks admin privileges (Admin/SuperAdmin role required)\n
             - Returns 403 Forbidden when user account is inactive or disabled\n
             - Returns 404 Not Found when no user exists with the provided email\n
             \n
             **Error Handling:**\n
             - AuthenticationException (401): Missing admin role - user authenticated but lacks Admin/SuperAdmin privileges\n
             - AuthorizationException (403): Account inactive - user exists but account is disabled/suspended\n
             - BadRequestException (400): Invalid password - email exists but password is incorrect\n
             - NotFoundException (404): User not found - no account exists with the provided email.
         """
    );
}
