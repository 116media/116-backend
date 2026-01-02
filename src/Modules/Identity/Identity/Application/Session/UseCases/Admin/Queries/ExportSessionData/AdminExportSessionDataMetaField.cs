using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Contains metadata information for the admin export session data route.
/// </summary>
public static class AdminExportSessionDataMetaField
{
    public static readonly RouteMetadata AdminExportSessionData = new(
        "AdminExportSessionData",
        "Export session data with optional filtering",
        """
            Exports session data with optional filtering by status and date range.
            This is an admin-only operation for extracting session data for reporting and analysis.
            \n
            **Filter Parameters:**\n
            - Status: Filter by session status ("active" or "expired").\n
            - FromDate: Include only sessions created after this date.\n
            - ToDate: Include only sessions created before this date.\n
            \n
            **Exported Fields:**\n
            - Session ID, User ID\n
            - IP Address, Device Name, User Agent, Client Platform\n
            - Created At, Expires At, Is Active, Deleted At\n
            \n
            **Use Cases:**\n
            - Generate session activity reports\n
            - Export data for compliance and auditing\n
            - Analyze session patterns and user behavior\n
            - Create backups of session data\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with session export data\n
            - Returns 400 Bad Request if filter parameters are invalid\n
            - Returns 401 Unauthorized if access token is invalid\n
            - Returns 403 Forbidden if not admin
        """
    );
}
