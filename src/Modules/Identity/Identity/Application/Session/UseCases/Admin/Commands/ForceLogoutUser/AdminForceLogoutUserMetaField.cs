using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Contains metadata information for the admin force logout user route.
/// </summary>
public static class AdminForceLogoutUserMetaField
{
    public static readonly RouteMetadata AdminForceLogoutUser = new(
        "AdminForceLogoutUser",
        "Force logout a user from all devices",
        """
            Forces a user to log out from all their active sessions across all devices.
            This is an admin-only operation used for security purposes or account management.
            \n
            This endpoint performs force logout by:\n
            - Validating the target user ID from the route parameter\n
            - Soft deleting all sessions associated with that user\n
            - Invalidating all tokens for those sessions\n
            \n
            **Authentication Requirements:**\n
            - Admin must be authenticated with a valid access token\n
            - Requires admin role and appropriate permissions\n
            \n
            **Use Cases:**\n
            - Security response to compromised accounts\n
            - Account suspension or termination\n
            - Policy enforcement (e.g., forced password reset)\n
            - Emergency access revocation\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success flag on successful logout\n
            - Returns 400 Bad Request if user ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if admin lacks required permissions\n
            \n
            **Important Notes:**\n
            - This operation affects all user sessions, not just one device\n
            - Sessions are soft deleted for audit trail purposes\n
            - User will need to log in again on all devices
        """
    );
}
