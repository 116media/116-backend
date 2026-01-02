using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;

/// <summary>
/// Contains metadata information for the admin get session metrics route.
/// </summary>
public static class AdminGetSessionMetricsMetaField
{
    public static readonly RouteMetadata AdminGetSessionMetrics = new(
        "AdminGetSessionMetrics",
        "Get session metrics and statistics",
        """
            Retrieves comprehensive session metrics including platform distribution and user activity.
            This is an admin-only operation for monitoring system usage and platform analytics.
            \n
            **Metrics Provided:**\n
            - Client Platform Counts: Sessions grouped by client platform (iOS app, Android app, Web browser, PWA)\n
            - Device Type Counts: Sessions grouped by device type (Mobile, Desktop, Tablet)\n
            - Total Active Sessions: Count of all currently active sessions\n
            - Total Active Users: Count of unique users with at least one active session\n
            \n
            **Use Cases:**\n
            - Monitor platform adoption (mobile app vs web app usage)\n
            - Track device type distribution for responsive design priorities\n
            - Measure concurrent user activity\n
            - Generate dashboards showing real-time system usage\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with metrics data\n
            - Returns 401 Unauthorized if access token is invalid\n
            - Returns 403 Forbidden if not admin
        """
    );
}
