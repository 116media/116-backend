using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions;

/// <summary>
/// Contains metadata information for the admin get own sessions route.
/// </summary>
public static class AdminGetOwnSessionsMetaField
{
    public static readonly RouteMetadata AdminGetOwnSessions = new(
        "AdminGetOwnSessions",
        "Retrieve all sessions for the authenticated admin user",
        """
            Retrieves a list of all sessions (devices) for the currently authenticated admin user.
            Supports filtering by session status (active/inactive).
            \n
            This endpoint provides session management by:\n
            - Listing all user sessions across different devices\n
            - Showing device information (IP address, device name, user agent)\n
            - Indicating session status (active or expired)\n
            - Displaying session creation and expiration times\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            - User can only view their own sessions\n
            \n
            **Query Parameters:**\n
            - isActive (optional): Filter sessions by status\n
              - true: Only active sessions\n
              - false: Only expired/inactive sessions\n
              - null/omitted: All sessions\n
            \n
            **Use Cases:**\n
            - View all active login sessions\n
            - Identify unrecognized devices\n
            - Manage active sessions before revoking specific ones\n
            - Security audit of login history\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with list of sessions\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required permissions\n
            \n
            **Session Information Includes:**\n
            - Session ID for revoking specific sessions\n
            - IP address of the device\n
            - Device name and user agent string\n
            - Creation timestamp\n
            - Expiration timestamp\n
            - Active status (computed from expiration time and deletion status)
        """
    );
}
