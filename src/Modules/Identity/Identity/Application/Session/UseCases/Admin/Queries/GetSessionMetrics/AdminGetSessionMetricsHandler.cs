using _116.Identity.Application.Session.Repositories;
using _116.Identity.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;

/// <summary>
/// Handles the <see cref="AdminGetSessionMetricsQuery" /> to retrieve session metrics and statistics.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
public class AdminGetSessionMetricsHandler(ISessionRepository sessionRepository)
    : IQueryHandler<AdminGetSessionMetricsQuery, AdminGetSessionMetricsResult>
{
    /// <summary>
    /// Handles the query by fetching session metrics grouped by browser, device, platform, and client type.
    /// </summary>
    /// <param name="query">The metrics query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetSessionMetricsResult" /> containing session metrics.</returns>
    public async Task<AdminGetSessionMetricsResult> Handle(
        AdminGetSessionMetricsQuery query,
        CancellationToken cancellationToken
    )
    {
        Dictionary<EnumBrowser, int> browserCounts = await sessionRepository.GetActiveSessionCountByBrowserAsync(
            cancellationToken: cancellationToken
        );

        Dictionary<EnumDevice, int> deviceCounts = await sessionRepository.GetActiveSessionCountByDeviceAsync(
            cancellationToken: cancellationToken
        );

        Dictionary<EnumPlatform, int> platformCounts = await sessionRepository.GetActiveSessionCountByPlatformAsync(
            cancellationToken: cancellationToken
        );

        Dictionary<EnumClient, int> clientCounts = await sessionRepository.GetActiveSessionCountByClientAsync(
            cancellationToken: cancellationToken
        );

        int totalActiveSessions = await sessionRepository.GetTotalActiveSessionsCountAsync(
            cancellationToken: cancellationToken
        );

        int totalActiveUsers = await sessionRepository.GetTotalActiveUsersCountAsync(
            cancellationToken: cancellationToken
        );

        // Build browser metrics
        var browsers = new BrowserMetrics(
            Chrome: browserCounts.GetValueOrDefault(key: EnumBrowser.Chrome, defaultValue: 0),
            Firefox: browserCounts.GetValueOrDefault(key: EnumBrowser.Firefox, defaultValue: 0),
            Safari: browserCounts.GetValueOrDefault(key: EnumBrowser.Safari, defaultValue: 0),
            Edge: browserCounts.GetValueOrDefault(key: EnumBrowser.Edge, defaultValue: 0),
            Opera: browserCounts.GetValueOrDefault(key: EnumBrowser.Opera, defaultValue: 0),
            InternetExplorer: browserCounts.GetValueOrDefault(key: EnumBrowser.InternetExplorer, defaultValue: 0),
            GoogleSearchApp: browserCounts.GetValueOrDefault(key: EnumBrowser.GoogleSearchApp, defaultValue: 0),
            Samsung: browserCounts.GetValueOrDefault(key: EnumBrowser.Samsung, defaultValue: 0),
            Unknown: browserCounts.GetValueOrDefault(key: EnumBrowser.Unknown, defaultValue: 0)
        );

        // Build device metrics
        var devices = new DeviceMetrics(
            Desktop: deviceCounts.GetValueOrDefault(key: EnumDevice.Desktop, defaultValue: 0),
            Mobile: deviceCounts.GetValueOrDefault(key: EnumDevice.Mobile, defaultValue: 0),
            Tablet: deviceCounts.GetValueOrDefault(key: EnumDevice.Tablet, defaultValue: 0),
            Watch: deviceCounts.GetValueOrDefault(key: EnumDevice.Watch, defaultValue: 0),
            Tv: deviceCounts.GetValueOrDefault(key: EnumDevice.Tv, defaultValue: 0),
            Console: deviceCounts.GetValueOrDefault(key: EnumDevice.Console, defaultValue: 0),
            Car: deviceCounts.GetValueOrDefault(key: EnumDevice.Car, defaultValue: 0),
            IoT: deviceCounts.GetValueOrDefault(key: EnumDevice.IoT, defaultValue: 0),
            Unknown: deviceCounts.GetValueOrDefault(key: EnumDevice.Unknown, defaultValue: 0)
        );

        // Build platform metrics
        var platforms = new PlatformMetrics(
            Windows: platformCounts.GetValueOrDefault(key: EnumPlatform.Windows, defaultValue: 0),
            Mac: platformCounts.GetValueOrDefault(key: EnumPlatform.Mac, defaultValue: 0),
            Ios: platformCounts.GetValueOrDefault(key: EnumPlatform.Ios, defaultValue: 0),
            IpadOs: platformCounts.GetValueOrDefault(key: EnumPlatform.IpadOs, defaultValue: 0),
            Linux: platformCounts.GetValueOrDefault(key: EnumPlatform.Linux, defaultValue: 0),
            Android: platformCounts.GetValueOrDefault(key: EnumPlatform.Android, defaultValue: 0),
            ChromeOs: platformCounts.GetValueOrDefault(key: EnumPlatform.ChromeOs, defaultValue: 0),
            Unknown: platformCounts.GetValueOrDefault(key: EnumPlatform.Unknown, defaultValue: 0)
        );

        // Build client metrics
        var clients = new ClientMetrics(
            MobileApp: clientCounts.GetValueOrDefault(key: EnumClient.MobileApp, defaultValue: 0),
            WebApp: clientCounts.GetValueOrDefault(key: EnumClient.WebApp, defaultValue: 0),
            Dashboard: clientCounts.GetValueOrDefault(key: EnumClient.Dashboard, defaultValue: 0),
            Unknown: clientCounts.GetValueOrDefault(key: EnumClient.Unknown, defaultValue: 0)
        );

        return new AdminGetSessionMetricsResult(
            Clients: clients,
            Devices: devices,
            Browsers: browsers,
            Platforms: platforms,
            TotalActiveUsers: totalActiveUsers,
            TotalActiveSessions: totalActiveSessions
        );
    }
}
