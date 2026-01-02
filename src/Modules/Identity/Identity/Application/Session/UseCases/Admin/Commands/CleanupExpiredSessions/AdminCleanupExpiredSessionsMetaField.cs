using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions;

/// <summary>
/// Contains metadata information for the admin cleanup expired sessions route.
/// </summary>
public static class AdminCleanupExpiredSessionsMetaField
{
    public static readonly RouteMetadata AdminCleanupExpiredSessions = new(
        "AdminCleanupExpiredSessions",
        "Cleanup all expired sessions",
        """
            Performs a cleanup operation to soft delete all expired sessions from the database.
            This is an admin-only maintenance operation used for database hygiene.
            \n
            This endpoint performs cleanup by:\n
            - Identifying all sessions that have expired (past their ExpiresAt timestamp)\n
            - Soft deleting those expired sessions to maintain audit trail\n
            - Returning the count of sessions that were cleaned up\n
            \n
            **Authentication Requirements:**\n
            - Admin must be authenticated with a valid access token\n
            - Requires admin role and appropriate permissions\n
            \n
            **Use Cases:**\n
            - Regular maintenance to keep session data clean\n
            - Database optimization and cleanup\n
            - Scheduled cleanup operations (can be triggered manually or via cron)\n
            - Removing stale session data that won't be used again\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with deleted count on successful cleanup\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if admin lacks required permissions\n
            \n
            **Important Notes:**\n
            - Sessions are soft deleted, not permanently removed\n
            - This operation only affects expired sessions, not active ones\n
            - The operation is safe to run multiple times\n
            - Consider running this periodically as part of maintenance tasks\n
        """
    );
}
