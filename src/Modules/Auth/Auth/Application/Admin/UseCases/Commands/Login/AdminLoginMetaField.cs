using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Commands.Login;

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

             This endpoint performs enhanced authentication by:
             - Validating email and password
             - Verifying the account is active and verified
             - Checking for admin role privileges (Admin or SuperAdmin)
             - Generating JWT token with appropriate admin claims
             - Recording the login activity

             **Authentication Requirements:**
             - Valid email and password combination
             - Account must be active and verified
             - User must have Admin or SuperAdmin role assigned

             **Security Features:**
             - Password verification using secure hashing (bcrypt)
             - Role-based access validation
             - Login activity tracking
             - Enhanced JWT claims for admin operations

             **Response Codes:**
             - Returns 200 OK with user info and JWT token on successful authentication
             - Returns 400 Bad Request for invalid email format or incorrect password
             - Returns 401 Unauthorized when user lacks admin privileges (Admin/SuperAdmin role required)
             - Returns 403 Forbidden when user account is inactive or disabled
             - Returns 404 Not Found when no user exists with the provided email

             **Error Handling:**
             - AuthenticationException (401): Missing admin role - user authenticated but lacks Admin/SuperAdmin privileges
             - AuthorizationException (403): Account inactive - user exists but account is disabled/suspended
             - BadRequestException (400): Invalid password - email exists but password is incorrect
             - NotFoundException (404): User not found - no account exists with the provided email.
         """
    );
}

