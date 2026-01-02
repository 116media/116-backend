using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;

/// <summary>
/// Query used to retrieve session metrics and statistics (admin only).
/// </summary>
public record AdminGetSessionMetricsQuery : IQuery<AdminGetSessionMetricsResult>;

/// <summary>
/// Metrics for sessions grouped by client platform.
/// </summary>
/// <param name="IosMobile">Count of active sessions from iOS mobile app.</param>
/// <param name="AndroidMobile">Count of active sessions from Android mobile app.</param>
/// <param name="BrowserWeb">Count of active sessions from web browser application.</param>
/// <param name="PwaBrowser">Count of active sessions from PWA (Progressive Web App).</param>
/// <param name="Unknown">Count of active sessions without client platform information.</param>
public record ClientPlatformMetrics(
    int IosMobile,
    int AndroidMobile,
    int BrowserWeb,
    int PwaBrowser,
    int Unknown
);

/// <summary>
/// Metrics for sessions grouped by device type (classified from user agent).
/// </summary>
/// <param name="Mobile">Count of active sessions from mobile devices (phones).</param>
/// <param name="Desktop">Count of active sessions from desktop/laptop computers.</param>
/// <param name="Tablet">Count of active sessions from tablets.</param>
/// <param name="Other">Count of active sessions from unclassified devices.</param>
public record DeviceTypeMetrics(
    int Mobile,
    int Desktop,
    int Tablet,
    int Other
);

/// <summary>
/// The result of executing an <see cref="AdminGetSessionMetricsQuery" />.
/// </summary>
/// <param name="ClientPlatforms">Session counts grouped by client platform (from Client-Platform header).</param>
/// <param name="DeviceTypes">Session counts grouped by device type (classified from user agent).</param>
/// <param name="TotalActiveSessions">Total count of all active sessions.</param>
/// <param name="TotalActiveUsers">Total count of unique users with at least one active session.</param>
public record AdminGetSessionMetricsResult(
    ClientPlatformMetrics ClientPlatforms,
    DeviceTypeMetrics DeviceTypes,
    int TotalActiveSessions,
    int TotalActiveUsers
);
