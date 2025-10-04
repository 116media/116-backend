using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Contains metadata information for the admin sign-out route.
/// </summary>
public static class AdminSignOutMetaField
{
    public static readonly RouteMetadata AdminSignOut = new(
        name: "AdminSignOut",
        summary: "Sign out the authenticated admin user",
        description: """
             Signs out the currently authenticated admin user by updating their login status.
             After successful sign-out, the client should discard the JWT token.

             This endpoint performs secure sign-out by:
             - Validating JWT token authentication
             - Verifying account is active (not suspended/banned)
             - Ensuring user has admin or super admin role
             - Updating user login status in the database
             - Allowing unverified accounts to sign out

             **Authentication Requirements:**
             - Valid JWT Bearer token
             - Account must be active (not suspended)
             - User must have Admin or SuperAdmin role
             - Verification status is not required for sign-out

             **Security Features:**
             - Only active admin accounts can perform sign-out
             - Prevents unnecessary database updates if already logged out
             - Always returns success for consistent UX

             **Response Codes:**
             - Returns 200 OK with success status
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for inactive accounts or insufficient permissions

             **Process Flow:**
             1. Extracts admin user ID from JWT token
             2. Validates account is active
             3. Verifies admin/super admin role authorization
             4. Updates login status if currently logged in
             5. Returns success response.
         """
    );
}
