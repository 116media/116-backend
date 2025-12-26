using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices;

/// <summary>
/// Contains metadata information for the admin sign-out from all devices route.
/// </summary>
public static class AdminSignOutFromAllDevicesMetaField
{
    public static readonly RouteMetadata AdminSignOutFromAllDevices = new(
        "AdminSignOutFromAllDevices",
        "Sign out the authenticated admin user from all devices",
        """
            Signs out the currently authenticated admin user from all devices by invalidating all active sessions.
            After successful sign-out, all refresh tokens will be revoked and the admin must re-authenticate on all devices.
            \n
            This endpoint is commonly used:\n
            - After password changes (security best practice)\n
            - When admin suspects account compromise\n
            - When enabling two-factor authentication\n
            - As a "Sign Out Everywhere" feature\n
            \n
            **Authentication Requirements:**\n
            - Valid JWT Bearer token with admin privileges\n
            - Account must be active (not suspended)\n
            \n
            **Security Features:**\n
            - Invalidates all active sessions across all devices\n
            - Uses soft delete for session tracking and analytics\n
            - Prevents token reuse after sign-out\n
            - Idempotent operation (safe to call multiple times)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success status\n
            - Returns 401 Unauthorized for invalid/missing JWT token\n
            - Returns 403 Forbidden for non-admin users or inactive accounts\n
            \n
            **Process Flow:**\n
            1. Extracts admin user ID from JWT token\n
            2. Validates account is active and has admin privileges\n
            3. Soft deletes all active sessions for the admin user\n
            4. Returns success response.
        """
    );
}
