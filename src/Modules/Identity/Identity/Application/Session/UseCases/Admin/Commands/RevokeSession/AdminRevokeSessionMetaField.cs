using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;

/// <summary>
/// Contains metadata information for the admin revoke session route.
/// </summary>
public static class AdminRevokeSessionMetaField
{
    public static readonly RouteMetadata AdminRevokeSession = new(
        "AdminRevokeSession",
        "Revoke a specific session (log out from a device)",
        """
            Revokes (logs out from) a specific session identified by its ID.
            This allows admin users to remotely log out from other devices.
            \n
            This endpoint performs session revocation by:\n
            - Validating the session ID from the route parameter\n
            - Verifying the session belongs to the authenticated admin user\n
            - Soft deleting the session (marking it as inactive)\n
            - Invalidating all tokens associated with that session\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            - User can only revoke their own sessions\n
            \n
            **Use Cases:**\n
            - Log out from a specific device remotely\n
            - Remove unrecognized or suspicious sessions\n
            - Clean up old sessions after viewing session list\n
            - Security response to potential account compromise\n
            \n
            **Security Features:**\n
            - Session ownership verification prevents revoking other users' sessions\n
            - Soft delete ensures session history is maintained for audit purposes\n
            - Immediate invalidation prevents further use of associated tokens\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success flag on successful revocation\n
            - Returns 400 Bad Request if session ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required permissions\n
            - Returns 404 Not Found if session doesn't exist or doesn't belong to user\n
            \n
            **Error Handling:**\n
            - NotFoundException (404): Session not found or doesn't belong to user\n
            - Attempting to revoke another user's session returns 404 (not 403) to prevent session enumeration attacks
        """
    );
}
