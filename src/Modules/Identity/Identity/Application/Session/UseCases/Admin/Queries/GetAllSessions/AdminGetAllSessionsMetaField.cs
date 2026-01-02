using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions;

/// <summary>
/// Contains metadata information for the admin get all sessions route.
/// </summary>
public static class AdminGetAllSessionsMetaField
{
    public static readonly RouteMetadata AdminGetAllSessions = new(
        "AdminGetAllSessions",
        "Retrieve all sessions with pagination and filtering",
        """
            Retrieves a paginated list of all user sessions with optional filtering capabilities.
            This is an admin-only operation for monitoring and managing user sessions.
            \n
            **Query Parameters:**\n
            - pageIndex: Zero-based page index (default: 0)\n
            - pageSize: Number of items per page (default: 10, max: 100)\n
            - status: Filter by status ("active" or "expired")\n
            - userId: Filter by user ID (GUID)\n
            - ipAddress: Filter by IP address (partial match)\n
            - deviceName: Filter by device name (partial match)\n
            - fromDate: Filter sessions created after this date\n
            - toDate: Filter sessions created before this date\n
            \n
            **Response Includes:**\n
            - Paginated list of sessions\n
            - Total count of matching sessions\n
            - Current page index and size\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated sessions\n
            - Returns 400 Bad Request for invalid parameters\n
            - Returns 401 Unauthorized if access token is invalid\n
            - Returns 403 Forbidden if not admin
        """
    );
}
