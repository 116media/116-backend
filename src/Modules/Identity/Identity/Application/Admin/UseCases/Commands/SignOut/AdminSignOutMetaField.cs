using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Admin.UseCases.Commands.SignOut;

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
             \n
             This endpoint performs secure sign-out by:\n
             - Validating JWT token authentication\n
             - Verifying account is active (not suspended/banned)\n
             - Ensuring user has admin or super admin role\n
             - Updating user login status in the database\n
             - Allowing unverified accounts to sign out\n
             \n
             **Authentication Requirements:**\n
             - Valid JWT Bearer token\n
             - Account must be active (not suspended)\n
             - User must have Admin or SuperAdmin role\n
             - Verification status is not required for sign-out\n
             \n
             **Security Features:**\n
             - Only active admin accounts can perform sign-out\n
             - Prevents unnecessary database updates if already logged out\n
             - Always returns success for consistent UX\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with success status\n
             - Returns 401 Unauthorized for invalid/missing JWT token\n
             - Returns 403 Forbidden for inactive accounts or insufficient permissions\n
             \n
             **Process Flow:**\n
             1. Extracts admin user ID from JWT token\n
             2. Validates account is active\n
             3. Verifies admin/super admin role authorization\n
             4. Updates login status if currently logged in\n
             5. Returns success response.
         """
    );
}
