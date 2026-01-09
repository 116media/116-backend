using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;

/// <summary>
/// Query used to retrieve session metrics and statistics (admin only).
/// </summary>
public record AdminGetSessionMetricsQuery : IQuery<AdminGetSessionMetricsResult>;

/// <summary>
/// Metrics for sessions grouped by browser type.
/// </summary>
/// <param name="Chrome">Count of active sessions from Chrome browser.</param>
/// <param name="Firefox">Count of active sessions from Firefox browser.</param>
/// <param name="Safari">Count of active sessions from Safari browser.</param>
/// <param name="Edge">Count of active sessions from Edge browser.</param>
/// <param name="Opera">Count of active sessions from Opera browser.</param>
/// <param name="InternetExplorer">Count of active sessions from Internet Explorer.</param>
/// <param name="GoogleSearchApp">Count of active sessions from Google Search App.</param>
/// <param name="Samsung">Count of active sessions from Samsung browser.</param>
/// <param name="Unknown">Count of active sessions from unknown browsers.</param>
public record BrowserMetrics(
    int Chrome,
    int Firefox,
    int Safari,
    int Edge,
    int Opera,
    int InternetExplorer,
    int GoogleSearchApp,
    int Samsung,
    int Unknown
);

/// <summary>
/// Metrics for sessions grouped by device type.
/// </summary>
/// <param name="Desktop">Count of active sessions from desktop computers.</param>
/// <param name="Mobile">Count of active sessions from mobile phones.</param>
/// <param name="Tablet">Count of active sessions from tablets.</param>
/// <param name="Watch">Count of active sessions from smart watches.</param>
/// <param name="Tv">Count of active sessions from TV devices.</param>
/// <param name="Console">Count of active sessions from gaming consoles.</param>
/// <param name="Car">Count of active sessions from automotive devices.</param>
/// <param name="IoT">Count of active sessions from IoT devices.</param>
/// <param name="Unknown">Count of active sessions from unknown devices.</param>
public record DeviceMetrics(
    int Desktop,
    int Mobile,
    int Tablet,
    int Watch,
    int Tv,
    int Console,
    int Car,
    int IoT,
    int Unknown
);

/// <summary>
/// Metrics for sessions grouped by platform/operating system.
/// </summary>
/// <param name="Windows">Count of active sessions from Windows.</param>
/// <param name="Mac">Count of active sessions from macOS.</param>
/// <param name="Ios">Count of active sessions from iOS (iPhone).</param>
/// <param name="IpadOs">Count of active sessions from iPadOS.</param>
/// <param name="Linux">Count of active sessions from Linux.</param>
/// <param name="Android">Count of active sessions from Android.</param>
/// <param name="ChromeOs">Count of active sessions from ChromeOS.</param>
/// <param name="Unknown">Count of active sessions from unknown platforms.</param>
public record PlatformMetrics(
    int Windows,
    int Mac,
    int Ios,
    int IpadOs,
    int Linux,
    int Android,
    int ChromeOs,
    int Unknown
);

/// <summary>
/// Metrics for sessions grouped by client application type.
/// </summary>
/// <param name="MobileApp">Count of active sessions from mobile applications.</param>
/// <param name="WebApp">Count of active sessions from web applications.</param>
/// <param name="Dashboard">Count of active sessions from admin dashboard.</param>
/// <param name="Unknown">Count of active sessions from unknown client types.</param>
public record ClientMetrics(int MobileApp, int WebApp, int Dashboard, int Unknown);

/// <summary>
/// The result of executing an <see cref="AdminGetSessionMetricsQuery" />.
/// </summary>
/// <param name="Browsers">Session counts grouped by browser type.</param>
/// <param name="Devices">Session counts grouped by device type.</param>
/// <param name="Platforms">Session counts grouped by platform/OS.</param>
/// <param name="Clients">Session counts grouped by client application type.</param>
/// <param name="TotalActiveSessions">Total count of all active sessions.</param>
/// <param name="TotalActiveUsers">Total count of unique users with at least one active session.</param>
public record AdminGetSessionMetricsResult(
    BrowserMetrics Browsers,
    DeviceMetrics Devices,
    PlatformMetrics Platforms,
    ClientMetrics Clients,
    int TotalActiveSessions,
    int TotalActiveUsers
);
