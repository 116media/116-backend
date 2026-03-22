using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById;

/// <summary>
/// Contains metadata information for the admin get own session by ID route.
/// </summary>
public static class AdminGetOwnSessionByIdMetaField
{
    public static readonly RouteMetadata AdminGetOwnSessionById = new(
        "AdminGetOwnSessionById",
        "Retrieve a specific session by ID",
        """
            Retrieves detailed information about a specific session identified by its ID.
            The session must belong to the authenticated admin user.
            \n
            This endpoint provides session details by:\n
            - Validating the session ID from the route parameter\n
            - Verifying the session belongs to the authenticated admin user\n
            - Returning complete session metadata and status\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            - User can only view their own sessions\n
            \n
            **Use Cases:**\n
            - View detailed information about a specific session\n
            - Check session status before revoking\n
            - Verify device information for security auditing\n
            - Display session details in admin dashboard\n
            \n
            **Security Features:**\n
            - Session ownership verification prevents viewing other users' sessions\n
            - Returns 404 (not 403) for unauthorized access to prevent session enumeration\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with session details on success\n
            - Returns 400 Bad Request if session ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required permissions\n
            - Returns 404 Not Found if session doesn't exist or doesn't belong to user\n
            \n
            **Session Information Includes:**\n
            - Session ID\n
            - IP address of the device\n
            - Device name and user agent string\n
            - Creation timestamp\n
            - Expiration timestamp\n
            - Active status (computed from expiration time and deletion status)\n
            \n
            **Error Handling:**\n
            - NotFoundException (404): Session not found or doesn't belong to user
        """
    );
}
