namespace _116.Identity.Application.Session.Constants;

/// <summary>
/// Contains route path constants for session-related API endpoints.
/// Provides centralized string constants for URL segments used in session management routes.
/// </summary>
public static class SessionRouteConstants
{
    /// <summary>
    /// Route segment for refresh token endpoints.
    /// Used to refresh access tokens using a valid refresh token.
    /// Example: /api/v1/public/sessions/refresh-token.
    /// </summary>
    public const string RefreshToken = "refresh-token";

    /// <summary>
    /// Route segment for revoking a specific session.
    /// Used to log out from a specific device/session.
    /// Example: /api/v1/public/sessions/{id}/revoke.
    /// </summary>
    public const string Revoke = "revoke";

    /// <summary>
    /// Route segment for force logout endpoints (admin only).
    /// Used by admins to forcefully log out a user from all devices.
    /// Example: /api/v1/admin/sessions/force-logout.
    /// </summary>
    public const string ForceLogout = "force-logout";

    /// <summary>
    /// Route segment for session metrics endpoints (admin only).
    /// Used to retrieve session analytics and statistics.
    /// Example: /api/v1/admin/sessions/metrics.
    /// </summary>
    public const string Metrics = "metrics";

    /// <summary>
    /// Route segment for session data export endpoints (admin only).
    /// Used to export session data in various formats.
    /// Example: /api/v1/admin/sessions/export.
    /// </summary>
    public const string Export = "export";

    /// <summary>
    /// Route segment for cleanup expired sessions endpoints (admin only).
    /// Used to perform manual cleanup of expired sessions.
    /// Example: /api/v1/admin/sessions/cleanup.
    /// </summary>
    public const string Cleanup = "cleanup";

    /// <summary>
    /// The base endpoint path for all session routes.
    /// Combined with public/admin prefixes: /api/v1/public/sessions or /api/v1/admin/sessions.
    /// </summary>
    public const string Endpoint = "sessions";
}
